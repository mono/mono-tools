using System;
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
		// This is used if the user click on different urls while some are still loading so that only the most recent content is displayed
		long loadUrlTimestamp = long.MinValue;

		History history;
		SearchableIndex searchIndex;
		IndexSearcher mdocSearch;

		Node match;
		string currentUrl;
		string currentTitle;

		object placeholderTreeNode = new Object ();
		bool loadedFromString;

		public MainWindow ()
		{
			InitializeComponent();
		}

		void MainWindow_Load(object sender, EventArgs e)
		{
			var indexManager = Program.IndexUpdateManager;
			indexManager.UpdaterChange += IndexUpdaterCallback;
			indexManager.CheckIndexIsFresh ().ContinueWith (t => {
				if (t.IsFaulted)
					Console.WriteLine ("Error while checking indexes: {0}", t.Exception);
				else if (!t.Result)
					indexManager.PerformSearchIndexCreation ();
				else
					indexManager.AdvertiseFreshIndex ();
			}).ContinueWith (t => Console.WriteLine ("Error while creating indexes: {0}", t.Exception), TaskContinuationOptions.OnlyOnFaulted);

			SetupSearch ();
			SetupDocTree ();
			SetupBookmarks ();
			history = new History (backButton, forwardButton);
			docBrowser.DocumentTitleChanged += (s, _) => currentTitle = docBrowser.DocumentTitle;
			docBrowser.DocumentCompleted += (s, _) => loadedFromString = false;
			docBrowser.Navigating += (s, nav) => {
				if (loadedFromString)
					return;
				LoadUrl (nav.Url.IsFile ? nav.Url.Segments.LastOrDefault () : nav.Url.OriginalString, true);
				nav.Cancel = true;
			};
		}

		void SetupSearch ()
		{
			searchIndex = Program.Root.GetSearchIndex ();
			mdocSearch = new IndexSearcher (Program.IndexUpdateManager.IsFresh ? Program.Root.GetIndex () : null);
		}

		void SetupDocTree ()
		{
			var node = (Node)Program.Root;
			var rootTreeNode = new TreeNode ();

			foreach (Node child in node.Nodes)
				AppendDocTreeNode (child, rootTreeNode);

			docTree.Nodes.AddRange (rootTreeNode.Nodes.Cast<TreeNode> ().ToArray ());
			// Our hack to allow lazy loading is to setup a dummy node as a child of a node we just added
			// if it supposed to have children. Then when we are about to expand, we remove that node and
			// correctly populate the subtree
			docTree.BeforeExpand += (s, e) => {
				var tn = e.Node;
				if (tn.Nodes.Count != 1 || tn.Nodes[0].Tag != placeholderTreeNode)
					return;
				var mn = e.Node.Tag as Node;
				if (mn == null)
					return;
				docTree.BeginUpdate ();
				tn.Nodes.Clear ();
				foreach (Node child in mn.Nodes)
					AppendDocTreeNode (child, tn, GetParentImageKeyFromNode (mn));
				docTree.EndUpdate ();
			};
			docTree.AfterSelect += (s, e) => {
				var treeNode = e.Node;
				var n = treeNode.Tag as Node;
				LoadUrl (n.PublicUrl, true, n.tree.HelpSource);
			};
			docTree.DrawNode += DrawNodeText;
			docTree.DrawMode = TreeViewDrawMode.OwnerDrawText;
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
			if (url.StartsWith ("#")) {
				Console.WriteLine ("FIXME: Anchor jump");
				return;
			}
			// In case user click on an external link e.g. [Android documentation] link at bottom of MonoDroid docs
			if (url.StartsWith ("http://") || url.StartsWith ("https://")) {
				UrlLauncher.Launch (url);
				return;
			}
			Console.WriteLine ("Loading {0}", url);
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
							tabContainer.SelectedIndex = 0;
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
			docBrowser.DocumentText = html;
		}

		void AppendDocTreeNode (Node node, TreeNode treeNode, string imageKey = null)
		{
			var root = new TreeNode (node.Caption, 20, 20);
			root.Tag = node;
			root.ImageKey = root.SelectedImageKey = imageKey ?? GetImageKeyFromNode (node);
			if (!node.IsLeaf)
				root.Nodes.Add (new TreeNode () { Tag = placeholderTreeNode });
			treeNode.Nodes.Add (root);
		}

		string GetImageKeyFromNode (Node node)
		{
			if (node.Caption.EndsWith (" Class"))
				return "class.png";
			if (node.Caption.EndsWith (" Interface"))
				return "interface.png";
			if (node.Caption.EndsWith (" Structure"))
				return "structure.png";
			if (node.Caption.EndsWith (" Enumeration"))
				return "enumeration.png";
			if (node.Caption.EndsWith (" Delegate"))
				return "delegate.png";
			var url = node.PublicUrl;
			if (!string.IsNullOrEmpty (url) && url.StartsWith ("N:"))
				return "namespace.png";
			return null;
		}

		string GetParentImageKeyFromNode (Node node)
		{
			switch (node.Caption) {
				case "Methods":
				case "Constructors":
					return "method.png";
				case "Properties":
					return "property.png";
				case "Events":
					return "event.png";
				case "Members":
					return "members.png";
				case "Fields":
					return "field.png";
			}

			return null;
		}

		void IndexUpdaterCallback (object sender, EventArgs e)
		{
			var manager = (IndexUpdateManager)sender;

			if (!manager.IsCreatingSearchIndex) {
				Invoke (new Action (delegate {
					indexesLabel.Visible = false;
					indexesProgressBar.Visible = false;
					searchIndex = Program.Root.GetSearchIndex ();
					searchBox.Enabled = false;
					mdocSearch.Index = Program.Root.GetIndex ();
				}));
			} else {
				Invoke (new Action (delegate {
					indexesLabel.Visible = true;
					indexesProgressBar.Visible = true;
					searchBox.Enabled = false;
				}));
			}
		}

		// This method is here because by default TreeView try to display icon in every case
		// i.e. even when we have no icon to show it's going to put a blank space. So here we 
		// detect when that happen and "shift" the text back in the right position 16px to the left
		void DrawNodeText (object sender, DrawTreeNodeEventArgs e)
		{
			if (!string.IsNullOrEmpty (e.Node.ImageKey)) {
				e.DrawDefault = true;
				return;
			}
			// Retrieve the node font. If the node font has not been set,
            // use the TreeView font.
            Font nodeFont = e.Node.NodeFont;
            if (nodeFont == null)
				nodeFont = ((TreeView)sender).Font;

            // Draw the node text.
			var clip = new Rectangle (e.Bounds.X - 16, e.Bounds.Y, e.Bounds.Width + 16, e.Bounds.Height);
			e.Graphics.SetClip (clip);
			if ((e.State & TreeNodeStates.Selected) != 0) {
				e.Graphics.Clear (SystemColors.Highlight);
				using (var pen = new Pen (Color.Black)) {
					pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
					e.Graphics.DrawRectangle (pen, new Rectangle (clip.Location, new Size (clip.Width - 1, clip.Height - 1)));
				}
				e.Graphics.DrawString (e.Node.Text, nodeFont, SystemBrushes.HighlightText, clip);
			} else {
				e.Graphics.Clear (Color.White);
				e.Graphics.DrawString (e.Node.Text, nodeFont, Brushes.Black, clip);
			}
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

		void bkAdd_Click(object sender, EventArgs e)
		{
			Program.BookmarkManager.AddBookmark (new BookmarkManager.Entry { Name = currentTitle, Url = currentTitle, Notes = string.Empty });
		}

		private void bkRemove_Click (object sender, EventArgs e)
		{
			var manager = Program.BookmarkManager;
			var idx = manager.FindIndexOfBookmarkFromUrl (currentUrl);
			var bks = manager.GetAllBookmarks ();
			if (idx > -1 && idx < bks.Count)
				manager.DeleteBookmark (bks[idx]);
		}
	}
}
