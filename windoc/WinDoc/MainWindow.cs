using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;

using Monodoc;

namespace WinDoc
{
	public partial class MainWindow : Form
	{
		readonly string initialUrl;
		// This is used if the user click on different urls while some are still loading so that only the most recent content is displayed
		long loadUrlTimestamp = long.MinValue;

		History history;
		SearchableIndex searchIndex;
		IndexSearcher indexSearch;

		Node match;
		string currentUrl;
		string currentTitle;

		object placeholderTreeNode = new Object ();
		bool loadedFromString;

		bool indexPageLoaded;
		AnimatedTreeNode indexLoadingNode;
		SearchTextBox searchInput;
		ResultDataSet dataSource = new ResultDataSet ();
		Dictionary<Node, TreeNode> nodeToTreeNodeMap = new Dictionary<Node,TreeNode> ();

		public MainWindow (string initialUrl)
		{
			InitializeComponent();
			SetStyle (ControlStyles.OptimizedDoubleBuffer, true);
			this.initialUrl = initialUrl;
		}

		void MainWindow_Load(object sender, EventArgs e)
		{
			var indexManager = Program.IndexUpdateManager;
			indexManager.UpdaterChange += IndexUpdaterCallback;
			indexManager.CheckIndexIsFresh ().ContinueWith (t => {
				if (t.IsFaulted)
					Logger.LogError ("Error while checking indexes", t.Exception);
				else if (!t.Result)
					indexManager.PerformSearchIndexCreation ();
				else
					indexManager.AdvertiseFreshIndex ();
			}).ContinueWith (t => Logger.LogError ("Error while creating indexes", t.Exception), TaskContinuationOptions.OnlyOnFaulted);

			SetupSearch ();
			SetupDocTree ();
			SetupBookmarks ();
			history = new History (backButton, forwardButton);
			docBrowser.DocumentTitleChanged += (s, _) => currentTitle = docBrowser.DocumentTitle;
			docBrowser.DocumentCompleted += (s, _) => loadedFromString = false;
			docBrowser.Navigating += (s, nav) => {
				if (loadedFromString)
					return;
				string url = nav.Url.OriginalString;
				if (nav.Url.IsFile) {
					var segs = nav.Url.Segments;
					url = segs.LastOrDefault () == "*" ? segs.Skip (segs.Length - 2).Aggregate (string.Concat) : segs.LastOrDefault ();
				}

				LoadUrl (url, true);
				nav.Cancel = true;
			};
			LoadUrl (string.IsNullOrEmpty (initialUrl) ? "root:" : initialUrl, syncTreeView: true);
		}

		void SetupSearch ()
		{
			searchIndex = Program.Root.GetSearchIndex ();
			indexSearch = new IndexSearcher (Program.IndexUpdateManager.IsFresh ? Program.Root.GetIndex () : null);

			searchInput = new SearchTextBox (searchBox.TextBox);
			searchInput.SearchTextChanged += SearchCallback;
			indexSearchBox.SearchTextChanged += IndexSearchCallback;
			indexLoadingNode = new AnimatedTreeNode (indexListResults.Nodes [0]);
			indexLoadingNode.StartAnimation ();
			tabContainer.Selected += (s, e) => {
				if (tabContainer.SelectedIndex == 1 && !indexPageLoaded) {
					FillUpIndex ();
					indexPageLoaded = true;
				}
				if (match != null && ShowNodeInTree (match)) {
					docTree.SelectedNode = nodeToTreeNodeMap[match];
					match = null;
				}
			};
			indexListResults.AfterSelect += (s, e) => {
				var entry = e.Node.Tag as IndexEntry;
				if (entry.Count == 1) {
					LoadUrl (entry[0].Url);
				} else {
					LoadMultipleMatchData (entry);
					indexSplitContainer.Panel2Collapsed = false;
					e.Node.EnsureVisible ();
				}
			};
			multipleMatchList.AfterSelect += (s, e) => {
				var topic = e.Node.Tag as Topic;
				LoadUrl (topic.Url);
				multipleMatchList.SelectedNode = e.Node;
			};
			searchListResults.DrawNode += CustomDrawing.DrawSearchResultNodeText;
			searchListResults.BeforeSelect += (s, e) => e.Cancel = e.Node.Tag == null;
			searchListResults.AfterSelect += (s, e) => {
				var entry = e.Node.Tag as ResultDataEntry;
				LoadUrl (entry.ResultSet.GetUrl (entry.Index));
			};
		}

		void SetupDocTree ()
		{
			var node = Program.Root.RootNode;
			var rootTreeNode = new TreeNode ();

			foreach (Node child in node.ChildNodes)
				AppendDocTreeNode (child, rootTreeNode);

			docTree.Nodes.AddRange (rootTreeNode.Nodes.Cast<TreeNode> ().ToArray ());
			// Our hack to allow lazy loading is to setup a dummy node as a child of a node we just added
			// if it supposed to have children. Then when we are about to expand, we remove that node and
			// correctly populate the subtree
			docTree.BeforeExpand += (s, e) => InflateTreeNode (e.Node);
			docTree.AfterSelect += (s, e) => {
				var treeNode = e.Node;
				var n = treeNode.Tag as Node;
				LoadUrl (n.PublicUrl, true, n.Tree.HelpSource);
			};
			docTree.DrawNode += CustomDrawing.DrawDocTreeNodeText;
		}

		void InflateTreeNode (TreeNode tn)
		{
			if (tn.Nodes.Count != 1 || tn.Nodes[0].Tag != placeholderTreeNode)
				return;
			var mn = tn.Tag as Node;
			if (mn == null)
				return;
			docTree.BeginUpdate ();
			tn.Nodes.Clear ();
			foreach (Node child in mn.ChildNodes)
				AppendDocTreeNode (child, tn, UIUtils.GetParentImageKeyFromNode (mn));
			docTree.EndUpdate ();
		}

		void SetupBookmarks ()
		{
			var manager = Program.BookmarkManager;
			manager.BookmarkListChanged += (sender, e) => {
				switch (e.EventType) {
				case BookmarkEventType.Modified:
					int index = manager.GetAllBookmarks ().IndexOf (e.Entry);
					bookmarkSelector.Items[index] = e.Entry.Name;
					break;
				case BookmarkEventType.Deleted:
					bookmarkSelector.Items.Remove (e.Entry.Name);
					break;
				case BookmarkEventType.Added:
					index = bookmarkSelector.Items.Add (e.Entry.Name);
					bookmarkSelector.SelectedIndex = index;
					break;
				}
			};
			bookmarkSelector.Items.AddRange (manager.GetAllBookmarks ().Select (i => i.Name).ToArray ());
			bookmarkSelector.SelectedIndexChanged += (sender, e) => {
				var bmarks = manager.GetAllBookmarks ();
				var index = bookmarkSelector.SelectedIndex;
				if (index >= 0 && index < bmarks.Count)
					LoadUrl (bmarks[index].Url, true);
			};
			bookmarkSelector.SelectedIndex = -1;
		}

		void LoadUrl (string url, bool syncTreeView = false, HelpSource source = null, bool addToHistory = true)
		{
			if (url == currentUrl)
				return;
			if (url.StartsWith ("#")) {
				Console.WriteLine ("FIXME: Anchor jump");
				return;
			}
			// In case user click on an external link e.g. [Android documentation] link at bottom of MonoDroid docs
			if (url.StartsWith ("http://") || url.StartsWith ("https://")) {
				UrlLauncher.Launch (url);
				return;
			}
			var ts = Interlocked.Increment (ref loadUrlTimestamp);
			Task.Factory.StartNew (() => {
				Node node;
				var res = DocTools.GetHtml (url, source, out node);
				return new { Node = node, Html = res };
			}).ContinueWith (t => {
				var node = t.Result.Node;
				var res = t.Result.Html;
				if (res != null) {
					BeginInvoke (new Action (() => {
						if (ts < loadUrlTimestamp)
							return;
						Text = currentUrl = node == null ? url : node.PublicUrl;
						if (addToHistory)
							history.AppendHistory (new LinkPageVisit (this, currentUrl));
						LoadHtml (res);
						this.match = node;
						if (syncTreeView) {
							if (tabContainer.SelectedIndex == 0 && match != null) {
								if (ShowNodeInTree (match))
									docTree.SelectedNode = nodeToTreeNodeMap[match];
							} else {
								tabContainer.SelectedIndex = 0;
							}
						}
						// Bookmark spinner management
						var bookmarkIndex = Program.BookmarkManager.FindIndexOfBookmarkFromUrl (url);
						if (bookmarkIndex == -1 || bookmarkIndex < bookmarkSelector.Items.Count)
							bookmarkSelector.SelectedIndex = bookmarkIndex;
					}));
				}
			});
		}

		void LoadHtml (string html)
		{
			loadedFromString = true;
			var documentUri = Path.Combine (Program.MonoDocDir, "doc.html");
			File.WriteAllText (documentUri, html);
			docBrowser.Navigate ("file://" + Path.GetFullPath (documentUri));
		}

		bool ShowNodeInTree (Node node)
		{
			if (node == null)
				return false;

			TreeNode treeNode;

			if (!nodeToTreeNodeMap.TryGetValue (node, out treeNode)) {
				if (node.Parent == null)
					return false;
				ShowNodeInTree (node.Parent);
				if (!nodeToTreeNodeMap.TryGetValue (node.Parent, out treeNode))
					return false;
				InflateTreeNode (treeNode);
				if (!nodeToTreeNodeMap.TryGetValue (node, out treeNode))
					return false;
			}

			treeNode.EnsureVisible ();
			return true;
		}

		void AppendDocTreeNode (Node node, TreeNode treeNode, string imageKey = null)
		{
			var root = new TreeNode (node.Caption, 20, 20);
			root.Tag = node;
			root.ImageKey = root.SelectedImageKey = imageKey ?? UIUtils.GetImageKeyFromNode (node);
			if (!node.IsLeaf)
				root.Nodes.Add (new TreeNode () { Tag = placeholderTreeNode });
			treeNode.Nodes.Add (root);
			nodeToTreeNodeMap[node] = root;
		}

		void IndexUpdaterCallback (object sender, EventArgs e)
		{
			var manager = (IndexUpdateManager)sender;

			if (!manager.IsCreatingSearchIndex) {
				Invoke (new Action (delegate {
					indexesLabel.Visible = false;
					indexesProgressBar.Visible = false;
					searchIndex = Program.Root.GetSearchIndex ();
					searchBox.Enabled = true;
					indexSearch.Index = Program.Root.GetIndex ();
					if (tabContainer.SelectedIndex == 1)
						FillUpIndex ();
					else
						indexPageLoaded = false;
				}));
			} else {
				Invoke (new Action (delegate {
					indexesLabel.Visible = true;
					indexesProgressBar.Visible = true;
					searchBox.Enabled = false;
					indexSearchBox.Enabled = false;
				}));
			}
		}

		void FillUpIndex ()
		{
			if (indexSearch.Index == null || indexSearch.Index.Rows == 0)
				return;

			Task.Factory.StartNew (() => Enumerable.Range (0, indexSearch.Index.Rows).Select (i => new TreeNode (indexSearch.Index.GetValue (i)) { Tag = indexSearch.GetIndexEntry (i) }).ToArray ())
				.ContinueWith (t => {
					indexLoadingNode.StopAnimation ();
					indexListResults.Nodes.Clear ();
					indexListResults.ImageList = null;
					indexListResults.Nodes.AddRange (t.Result);
					indexSearchBox.Enabled = true;
				}, TaskScheduler.FromCurrentSynchronizationContext ());
		}

		void bkAdd_Click(object sender, EventArgs e)
		{
			Program.BookmarkManager.AddBookmark (new BookmarkManager.Entry { Name = currentTitle, Url = currentTitle, Notes = string.Empty });
		}

		void bkRemove_Click (object sender, EventArgs e)
		{
			var manager = Program.BookmarkManager;
			var idx = manager.FindIndexOfBookmarkFromUrl (currentUrl);
			var bks = manager.GetAllBookmarks ();
			if (idx > -1 && idx < bks.Count)
				manager.DeleteBookmark (bks[idx]);
		}

		
		void bkModify_Click(object sender, EventArgs e)
		{
			new BookmarkEditor (Program.BookmarkManager).ShowDialog (this);
		}

		void SearchCallback (object sender, EventArgs e)
		{
			var input = sender as SearchTextBox;
			if (searchIndex == null) {
				searchIndex = Program.Root.GetSearchIndex ();
				if (searchIndex == null)
					return;
			}
			var text = input.Text;
			if (string.IsNullOrEmpty (text))
				return;
			tabContainer.SelectedIndex = 2;
			searchListResults.Tag = text; // Last searched term
			Result results = searchIndex.FastSearch (text, 5);
			dataSource.ClearResultSet ();
			dataSource.AddResultSet (results);
			Task.Factory.StartNew (() => searchIndex.Search (text, 20)).ContinueWith (t => Invoke (new Action (() => {
				var rs = t.Result;
				if (rs == null || rs.Count == 0 || text != ((string)searchListResults.Tag))
					return;
				dataSource.AddResultSet (rs);
				ReloadSearchData ();
			})), TaskScheduler.FromCurrentSynchronizationContext ());
			ReloadSearchData ();
			if (results.Count > 0) {
				var firstNode = searchListResults.Nodes[1];
				searchListResults.SelectedNode = firstNode;
				firstNode.EnsureVisible ();
			}
		}

		void ReloadSearchData ()
		{
			searchListResults.Nodes.Clear ();
			var nodes = Enumerable.Range (0, dataSource.Count)
				.Select (i => dataSource[i])
				.Select (e => dataSource.IsSection (e) ? new TreeNode (e.SectionName) : new TreeNode (e.ResultSet.GetTitle (e.Index)) { Tag = e })
				.ToArray ();
			searchListResults.Nodes.AddRange (nodes);
		}

		void LoadMultipleMatchData (IndexEntry entry)
		{
			multipleMatchList.Nodes.Clear ();
			for (int i = 0; i < entry.Count; i++)
				multipleMatchList.Nodes.Add (new TreeNode (RenderTopicMatch (entry[i])) { Tag = entry[i] });
		}

		// Names from the ECMA provider are somewhat
		// ambigious (you have like a million ToString
		// methods), so lets give the user the full name
		string RenderTopicMatch (Topic t)
		{
			// Filter out non-ecma
			if (t.Url [1] != ':')
				return t.Caption;

			switch (t.Url [0]) {
			case 'C': return t.Url.Substring (2) + " constructor";
			case 'M': return t.Url.Substring (2) + " method";
			case 'P': return t.Url.Substring (2) + " property";
			case 'F': return t.Url.Substring (2) + " field";
			case 'E': return t.Url.Substring (2) + " event";
			}
			return t.Caption;
		}

		void IndexSearchCallback (object sender, EventArgs e)
		{
			var input = sender as SearchTextBox;
			if (string.IsNullOrEmpty (input.Text))
				return;
			var index = indexSearch.FindClosest (input.Text);
			indexListResults.Nodes[index].EnsureVisible ();
			indexListResults.SelectedNode = indexListResults.Nodes[index];
		}
		
		class LinkPageVisit : PageVisit {
			MainWindow document;
			string url;
		
			public LinkPageVisit (MainWindow document, string url)
			{
				this.document = document;
				this.url = url;
			}
			
			public override void Go ()
			{
				document.LoadUrl (url, true, null, false);
			}
		}

	}
}
