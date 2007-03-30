//
// browser.cs: Mono documentation browser
//
// Author:
//   Miguel de Icaza
//
// (C) 2003 Ximian, Inc.
//
// TODO:
//
using Gtk;
using Glade;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Web.Services.Protocols;
using System.Xml;

namespace Monodoc {
class Driver {
	static int Main (string [] args)
	{
		string topic = null;
		bool useGecko = true;
		bool remote_mode = false;
		
		for (int i = 0; i < args.Length; i++){
			switch (args [i]){
			case "--html":
				if (i+1 == args.Length){
					Console.WriteLine ("--html needed argument");
					return 1; 
				}

				Node n;
				RootTree help_tree = RootTree.LoadTree ();
				string res = help_tree.RenderUrl (args [i+1], out n);
				if (res != null){
					Console.WriteLine (res);
					return 0;
				} else {
					return 1;
				}
			case "--make-index":
				RootTree.MakeIndex ();
				return 0;
				
			case "--make-search-index":
				RootTree.MakeSearchIndex ();
				return 0;
				
			case "--about":
				Console.WriteLine ("Mono Documentation Browser");
				Version ver = Assembly.GetExecutingAssembly ().GetName ().Version;
				if (ver != null)
					Console.WriteLine (ver.ToString ());
				return 0;

			case "--help":
				Console.WriteLine ("Options are:\n"+
						   "browser [--html TOPIC] [--make-index] [TOPIC] [--merge-changes CHANGE_FILE TARGET_DIR+] [--about]");
				return 0;
			
			case "--merge-changes":
				if (i+2 == args.Length) {
					Console.WriteLine ("--merge-changes 2+ args");
					return 1; 
				}
				
				ArrayList targetDirs = new ArrayList ();
				
				for (int j = i+2; j < args.Length; j++)
					targetDirs.Add (args [j]);
				
				EditMerger e = new EditMerger (
					GlobalChangeset.LoadFromFile (args [i+1]),
					targetDirs
				);

				e.Merge ();
				
				return 0;
			
			case "--edit":
				if (i+1 == args.Length) {
					Console.WriteLine ("Usage: --edit path, where path is to the location of Monodoc-format XML documentation files.");
					return 1; 
				}
				RootTree.UncompiledHelpSources.Add(args[i+1]);
				i++;
				break;

			case "--remote-mode":
				//In this mode, monodoc will accept urls on stdin
				//Used for integeration with monodevelop
				remote_mode = true;
				break;
				
			case "--no-gecko":
				useGecko = false;
				break;
			default:
				topic = args [i];
				break;
			}
			
		}
		
		SettingsHandler.CheckUpgrade ();
		
		Settings.RunningGUI = true;
		Application.Init ();
		Browser browser = new Browser (useGecko);
		
		if (topic != null)
			browser.LoadUrl (topic);

		Thread in_thread = null;
		if (remote_mode) {
			in_thread = new Thread (delegate () {
						while (true) {
							string url = Console.ReadLine ();
							if (url == null)
								return;

							Gtk.Application.Invoke (delegate {
								browser.LoadUrl (url);
								browser.MainWindow.Present ();
							});
						}
					});

			in_thread.Start ();
		}

		Application.Run ();
		if (in_thread != null)
			in_thread.Abort ();						

		return 0;
	}
}

public class Browser {
	Glade.XML ui;
	public Gtk.Window MainWindow;
	Style bar_style;

	[Glade.Widget] public Window window1;
	[Glade.Widget] TreeView reference_tree;
	[Glade.Widget] TreeView bookmark_tree;
	[Glade.Widget] public Statusbar statusbar;
	[Glade.Widget] public Button back_button, forward_button;
	public Entry index_entry;
	[Glade.Widget] CheckMenuItem editing1;
	[Glade.Widget] CheckMenuItem showinheritedmembers;
	[Glade.Widget] CheckMenuItem comments1;
	[Glade.Widget] MenuItem postcomment;
	[Glade.Widget] public MenuItem cut1;
	[Glade.Widget] public MenuItem paste1;
	[Glade.Widget] public MenuItem print;
	public Notebook tabs_nb;
	public Tab CurrentTab;
	bool HoldCtrl;
	public bool UseGecko;

	[Glade.Widget] public MenuItem bookmarksMenu;
	[Glade.Widget] MenuItem view1;
	MenuItem textLarger;
	MenuItem textSmaller;
	MenuItem textNormal;

	[Glade.Widget] VBox help_container;
	
	[Glade.Widget] EventBox bar_eb, index_eb;
	[Glade.Widget] Label subtitle_label;
	[Glade.Widget] Notebook nb;

	[Glade.Widget] Box title_label_box;
	ELabel title_label;

	// Bookmark Manager
	BookmarkManager bookmark_manager;

	//
	// Accessed from the IndexBrowser class
	//
	internal VBox search_box;
	internal Frame matches;
	[Glade.Widget] internal VBox index_vbox;
	
	Gdk.Pixbuf monodoc_pixbuf;

	//
	// Used for searching
	//
	Entry search_term;
	TreeView search_tree;
	TreeStore search_store;
	SearchableIndex search_index;
	string highlight_text;
	[Glade.Widget] VBox search_vbox;
	ProgressPanel ppanel;
	
        //
	// Left-hand side Browsers
	//
	public TreeBrowser tree_browser;
	IndexBrowser index_browser;
	public string CurrentUrl;
	
	internal RootTree help_tree;

	// For the status bar.
	public uint context_id;

	// Control of Bookmark
	struct BookLink
        {
		public string Text, Url;

		public BookLink (string text, string url)
                {
			this.Text = text;
			this.Url = url;
		}
	}

	public ArrayList bookList;

	public Browser (bool UseGecko)
	{
		this.UseGecko = UseGecko;
		ui = new Glade.XML (null, "browser.glade", "window1", null);
		ui.Autoconnect (this);

		MainWindow = (Gtk.Window) ui["window1"];
		MainWindow.DeleteEvent += new DeleteEventHandler (delete_event_cb);
                
		MainWindow.KeyPressEvent += new KeyPressEventHandler (keypress_event_cb);
		MainWindow.KeyReleaseEvent += new KeyReleaseEventHandler (keyrelease_event_cb);
                
		Stream icon = GetResourceImage ("monodoc.png");

		if (icon != null) {
			monodoc_pixbuf = new Gdk.Pixbuf (icon);
			MainWindow.Icon = monodoc_pixbuf;
		}

		//ellipsizing label for the title
		title_label = new ELabel ("");
		title_label.Xalign = 0;
		Pango.FontDescription fd = new Pango.FontDescription ();
		fd.Weight = Pango.Weight.Bold;
		title_label.ModifyFont (fd);
		title_label.Layout.FontDescription = fd;
		title_label_box.Add (title_label);
		title_label.Show ();
		
		//colour the bar according to the current style
		bar_style = bar_eb.Style.Copy ();
		bar_eb.Style = bar_style;
		MainWindow.StyleSet += new StyleSetHandler (BarStyleSet);
		BarStyleSet (null, null);

		help_tree = RootTree.LoadTree ();
		tree_browser = new TreeBrowser (help_tree, reference_tree, this);
		
		// Bookmark Manager init;
		bookmark_manager = new BookmarkManager(this);
		
		//
		// Tab Notebook and first tab
		//
		tabs_nb = new Notebook(); //the Notebook that holds tabs
		tabs_nb.Scrollable = true;
		tabs_nb.SwitchPage += new SwitchPageHandler(ChangeTab);
		help_container.Add(tabs_nb);

		if (UseGecko) {
			// Add Menu entries for changing the font
			Menu aux = (Menu) view1.Submenu;
			MenuItem sep = new SeparatorMenuItem ();
			sep.Show ();
			aux.Append (sep);
			AccelGroup accel = new AccelGroup ();
			MainWindow.AddAccelGroup (accel);

			textLarger = new MenuItem ("_Larger text");
			textLarger.Activated += new EventHandler (TextLarger);
			textLarger.Show ();
			aux.Append (textLarger);
			AccelKey ak = new AccelKey (Gdk.Key.plus, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			textLarger.AddAccelerator ("activate", accel, ak);
		
			textSmaller = new MenuItem ("_Smaller text");
			textSmaller.Activated += new EventHandler (TextSmaller);
			textSmaller.Show ();
			aux.Append (textSmaller);
			ak = new AccelKey (Gdk.Key.minus, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			textSmaller.AddAccelerator ("activate", accel, ak);
	
			textNormal = new MenuItem ("_Original size");
			textNormal.Activated += new EventHandler (TextNormal);
			textNormal.Show ();
			aux.Append (textNormal);
			ak = new AccelKey (Gdk.Key.Key_0, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			textNormal.AddAccelerator ("activate", accel, ak);
		}

		// restore the editing setting
		editing1.Active = SettingsHandler.Settings.EnableEditing;

		comments1.Active = SettingsHandler.Settings.ShowComments;

		cut1.Sensitive = false;
		paste1.Sensitive = false;

		//
		// Other bits
		//
		search_index = help_tree.GetSearchIndex();
		if (search_index == null) {
			ppanel = new ProgressPanel ("<b>No Search index found</b>", "Generate", RootTree.MakeSearchIndex, CreateSearchPanel); 
			search_vbox.Add (ppanel);
			search_vbox.Show ();
		} else {
			CreateSearchPanel ();
		}
		bookList = new ArrayList ();

		index_browser = IndexBrowser.MakeIndexBrowser (this);
		
		AddTab();
		MainWindow.ShowAll();
	}

	// Initianlizes the search index
	void CreateSearchPanel ()
	{
		//get the search index
		if (search_index == null) {
			search_index = help_tree.GetSearchIndex();
			//restore widgets
			search_vbox.Remove (ppanel);
		}
		//
		// Create the search panel
		//
		VBox vbox1 = new VBox (false, 0);
		search_vbox.PackStart (vbox1);
		
		// title
		HBox hbox1 = new HBox (false, 3);
		hbox1.BorderWidth = 3;
		Image icon = new Image (Stock.Find, IconSize.Menu);
		Label look_for_label = new Label ("Search for:");
		look_for_label.Justify = Justification.Left;
		look_for_label.Xalign = 0;
		hbox1.PackEnd (look_for_label, true, true, 0);
		hbox1.PackEnd (icon, false, true, 0);
		hbox1.ShowAll ();
		vbox1.PackStart (hbox1, false, true, 0);

		// entry
		search_term = new Entry ();
		search_term.Activated += OnSearchActivated;
		vbox1.PackStart (search_term, false, true, 0);
		
		// treeview
		ScrolledWindow scrolledwindow_search = new ScrolledWindow ();
		scrolledwindow_search.HscrollbarPolicy = PolicyType.Automatic;
		scrolledwindow_search.VscrollbarPolicy = PolicyType.Always;
		vbox1.PackStart (scrolledwindow_search, true, true, 0);
		search_tree = new TreeView ();
		search_tree.HeadersVisible = false;
		scrolledwindow_search.AddWithViewport (search_tree);
		
		//prepare the treeview
		search_store = new TreeStore (typeof (string));
		search_tree.Model = search_store;
		search_tree.AppendColumn ("Searches", new CellRendererText(), "text", 0);
		search_tree.Selection.Changed += new EventHandler (ShowSearchResult);

		vbox1.ShowAll ();
		search_vbox.ShowAll ();
	}	
			
	// Adds a Tab and Activates it
	void AddTab() 
	{
		CurrentTab = new Tab (this);
		tabs_nb.AppendPage (CurrentTab, CurrentTab.TabLabel);
		tabs_nb.ShowTabs = (tabs_nb.NPages > 1);
		tabs_nb.ShowAll (); //Needed to show the new tab
		tabs_nb.CurrentPage = tabs_nb.PageNum (CurrentTab);
		//Show root node
		Node match;
		string s = help_tree.RenderUrl ("root:", out match);
		if (s != null){
			Render (s, match, "root:");
			CurrentTab.history.AppendHistory (new Browser.LinkPageVisit (this, "root:"));
		}
		
	}
	
	//Called when the user changes the active Tab
	void ChangeTab(object o, SwitchPageArgs args) 
	{
		
		//Deactivate the old history
		CurrentTab.history.Active = false;
		
		//Get the new Tab		
		CurrentTab = (Tab) tabs_nb.GetNthPage ((int) args.PageNum);
		title_label.Text = CurrentTab.Title;
		
		//Activate the new history
		CurrentTab.history.Active = true;
		
		if (CurrentTab.Tab_mode == Mode.Viewer) {
			CurrentTab.history.ActivateCurrent();
			paste1.Sensitive = false;
			print.Sensitive = true;
		} else {
			paste1.Sensitive = true;
			print.Sensitive = false;
		}
		
		if (tree_browser.SelectedNode != CurrentTab.CurrentNode)
			tree_browser.ShowNode (CurrentTab.CurrentNode);
	}
	
	//
	// Invoked when the user presses enter on the search_entry
	// 
	void OnSearchActivated (object sender, EventArgs a)
	{
		string term = search_term.Text;
		if (term == "")
			return; //Search cannot handle empty string
		search_tree.Model = null;
		search_term.Editable = false;
		//search in the index
		Result r = search_index.Search (term);
		if (r == null)
			return; //There was a problem with the index
		//insert the results in the tree
		TreeIter iter;
					
		int max = r.Count > 500? 500:r.Count;
		iter = search_store.AppendValues (r.Term + " (" + max + " hits)");
		for (int i = 0; i < max; i++) 
			search_store.AppendValues (iter, r.GetTitle(i));

		// Show the results
		search_tree.Model = search_store;
		search_tree.CollapseAll();
		TreePath p = search_store.GetPath (iter);
		search_tree.ExpandToPath (p);
		search_tree.Selection.SelectPath (p);
		search_term.Editable = true;	
	}
	//
	// Invoked when the user click on one of the search results
	//
	void ShowSearchResult (object sender, EventArgs a)
	{
		CurrentTab.SetMode (Mode.Viewer);
		
		Gtk.TreeIter iter;
		Gtk.TreeModel model;

		bool selected = search_tree.Selection.GetSelected (out model, out iter);
		if (!selected)
			return;

		TreePath p = model.GetPath (iter);
		if (p.Depth < 2)
			return;
		int i_0 = p.Indices [0];
		int i_1 = p.Indices [1];
		Result res = (Result) search_index.Results [i_0];
		TreeIter parent;
		model.IterParent (out parent, iter);
		string term = (string) search_store.GetValue (parent, 0);
		highlight_text = term.Substring (0, term.IndexOf ("(")-1);
		LoadUrl (res.GetUrl (i_1));
	}

	//
	// Reload current page
	//
	void Reload ()
	{
		if (CurrentTab.history == null) // catch the case when we are currently loading
			return;
		if (CurrentTab.history.Count == 0)
			LoadUrl ("root:");
		else
			CurrentTab.history.ActivateCurrent ();
	}
	//
	// Changing font size menu entries
	// 
	void TextLarger (object obj, EventArgs args)
	{
		SettingsHandler.Settings.preferred_font_size += 10;
		HelpSource.CssCode = null;
		Reload ();
		SettingsHandler.Save ();
	}
	void TextSmaller (object obj, EventArgs args)
	{
		SettingsHandler.Settings.preferred_font_size -= 10;
		HelpSource.CssCode = null;
		Reload ();
		SettingsHandler.Save ();
	}
	void TextNormal (object obj, EventArgs args)
	{
		SettingsHandler.Settings.preferred_font_size = 100;
		HelpSource.CssCode = null;
		Reload ();
		SettingsHandler.Save ();
	}

	void BarStyleSet (object obj, StyleSetArgs args)
	{
		bar_style.SetBackgroundGC (StateType.Normal, MainWindow.Style.BackgroundGCs[1]);
	}

	public Stream GetResourceImage (string name)
	{
		Assembly assembly = System.Reflection.Assembly.GetCallingAssembly ();
		System.IO.Stream s = assembly.GetManifestResourceStream (name);
		
		return s;
	}

	public class LinkPageVisit : PageVisit {
		Browser browser;
		string url;
		
		public LinkPageVisit (Browser browser, string url)
		{
			this.browser = browser;
			this.url = url;
		}

		public override void Go ()
		{
			Node n;
			
			string res = browser.help_tree.RenderUrl (url, out n);
			browser.Render (res, n, url);
		}
	}
	
	public void LinkClicked (object o, EventArgs args)
	{
		string url = CurrentTab.html.Url;
			
		if (HoldCtrl)
			AddTab ();

		LoadUrl (url);
	}

	private System.Xml.XmlNode edit_node;
	private string edit_url;

	public void LoadUrl (string url)
	{
		if (url.StartsWith("#"))
		{
			// FIXME: This doesn't deal with whether anchor jumps should go in the history
			CurrentTab.html.JumpToAnchor(url.Substring(1));
			return;
		}

		if (url.StartsWith ("edit:"))
		{
			Console.WriteLine ("Node is: " + url);
			CurrentTab.edit_node = EditingUtils.GetNodeFromUrl (url, help_tree);
			CurrentTab.edit_url = url;
			CurrentTab.SetMode (Mode.Editor);
			CurrentTab.text_editor.Buffer.Text = CurrentTab.edit_node.InnerXml;
			return;
		}
		
		Node node;
		
		Console.Error.WriteLine ("Trying: {0}", url);
		try {
			string res = help_tree.RenderUrl (url, out node);
			if (res != null){
				Render (res, node, url);
				CurrentTab.history.AppendHistory (new LinkPageVisit (this, url));
				return;
			}
		} catch (Exception e){
			Console.WriteLine("#########");
			Console.WriteLine("Error loading url {0} - excpetion below:",url);
			Console.WriteLine("#########");
			Console.WriteLine(e);
		}
		
		Console.Error.WriteLine ("+----------------------------------------------+");
		Console.Error.WriteLine ("| Here we should locate the provider for the   |");
		Console.Error.WriteLine ("| link.  Maybe using this document as a base?  |");
		Console.Error.WriteLine ("| Maybe having a locator interface?   The short|");
		Console.Error.WriteLine ("| urls are not very useful to locate types     |");
		Console.Error.WriteLine ("+----------------------------------------------+");
		Render (url, null, url);
	}

	//
	// Renders the HTML text in `text' which was computed from `url'.
	// The Node matching is `matched_node'.
	//
	// `url' is only used for debugging purposes
	//
	public void Render (string text, Node matched_node, string url)
	{
		CurrentUrl = url;
		CurrentTab.CurrentNode = matched_node;
		if (highlight_text != null)
			text = DoHighlightText (text);

		CurrentTab.html.Render(text);

		if (matched_node != null) {
			if (tree_browser.SelectedNode != matched_node)
				tree_browser.ShowNode (matched_node);
			title_label.Text = matched_node.Caption;
		
			//
			//Try to find a better name for the Tab
			//
			string tabTitle;
			tabTitle = matched_node.Caption; //Normal title
			string[] parts = matched_node.URL.Split('/', '#');
			if(matched_node.URL != null && matched_node.URL.StartsWith("ecma:")) {
				if(parts.Length == 3 && parts[2] != String.Empty) { //List of Members, properties, events, ...
					tabTitle = parts[1] + ": " + matched_node.Caption;
				} else if(parts.Length >= 4) { //Showing a concrete Member, property, ...					
						tabTitle = parts[1] + "." + matched_node.Caption;
				} else {
					tabTitle = matched_node.Caption;
				}
			}
			//Trim too large titles
			if(tabTitle.Length > 35) {
				CurrentTab.Title = tabTitle.Substring(0,35) + " ...";
			} else {
				CurrentTab.Title = tabTitle;
			}
				
			if (matched_node.Nodes != null) {
				int count = matched_node.Nodes.Count;
				string term;

				if (count == 1)
					term = "subpage";
				else
					term = "subpages";

				subtitle_label.Text = count + " " + term;
			} else
				subtitle_label.Text = "";
		} else {
			title_label.Text = "Error";
			subtitle_label.Text = "";
		}
	}
	
	//
	// Highlights the text of the search
	//
	// we have to highligh everything that is not inside < and >
	string DoHighlightText (string text) {
		System.Text.StringBuilder sb = new System.Text.StringBuilder (text);
		
		//search for the term to highlight in a lower case version of the text
		string text_low = text.ToLower();
		string term_low = highlight_text.ToLower();
		
		//search for < and > so we dont substitute text of html tags
		ArrayList lt = new ArrayList();
		ArrayList gt = new ArrayList();
		int ini = 0;
		ini = text_low.IndexOf ('<', ini, text_low.Length);
		while (ini != -1) {
			lt.Add (ini);
			ini = text_low.IndexOf ('<', ini+1, text_low.Length-ini-1);
		}
		ini = 0;
		ini = text_low.IndexOf ('>', ini, text_low.Length);
		while (ini != -1) {
			gt.Add (ini);
			ini = text_low.IndexOf ('>', ini+1, text_low.Length-ini-1);
		}
		//start searching for the term
		int offset = 0; 
		int p = 0;
		ini = 0;
		ini = text_low.IndexOf (term_low, ini, text_low.Length);
		while (ini != -1) {
			bool beforeLt = ini < (int) lt [p];
			//look if term is inside any html tag
			while (!beforeLt) {
				bool afterGt = ini > (int) gt [p];
				if (afterGt) {
					p++;
					beforeLt = ini < (int) lt [p];
					continue;
				} else {
				    goto ExtLoop;
				}
			}
			string t = sb.ToString (ini + offset, term_low.Length);
			sb.Remove (ini + offset, term_low.Length);
			sb.Insert (ini + offset, "<span style=\"background: yellow\">" + t + "</span>");
			offset += 40; //due to the <span> tag inserted
			
ExtLoop:
			ini = text_low.IndexOf (term_low, ini+1, text_low.Length-ini-1);
		}

		highlight_text = null; //only highlight when a search result is clicked
		return sb.ToString();
	}
	//
	// Invoked when the mouse is over a link
	//
	string last_url = "";
	public void OnUrlMouseOver (object o, EventArgs args)
	{
		string new_url = CurrentTab.html.Url;

		if (new_url == null)
			new_url = "";
		
		if (new_url != last_url){
			statusbar.Pop (context_id);
			statusbar.Push (context_id, new_url);
			last_url = new_url;
		}
	}
	
	void keypress_event_cb (object o, KeyPressEventArgs args)
	{
		switch (args.Event.Key) {
		case Gdk.Key.Left:
			if (((Gdk.ModifierType) args.Event.State &
			Gdk.ModifierType.Mod1Mask) !=0)
			CurrentTab.history.BackClicked (this, EventArgs.Empty);
			args.RetVal = true;
			break;
		case Gdk.Key.Right:
			if (((Gdk.ModifierType) args.Event.State &
			Gdk.ModifierType.Mod1Mask) !=0)
			CurrentTab.history.ForwardClicked (this, EventArgs.Empty);
			args.RetVal = true;
			break;
		case Gdk.Key.Control_L:
		case Gdk.Key.Control_R:
			HoldCtrl = true;
			break;
		case Gdk.Key.Page_Up:
			if (HoldCtrl)
				tabs_nb.PrevPage();
			break;
		case Gdk.Key.Page_Down:
			if (HoldCtrl)
				tabs_nb.NextPage();
			break;
		}
	}
	
	void keyrelease_event_cb (object o, KeyReleaseEventArgs args)
	{
		switch (args.Event.Key) {
		case Gdk.Key.Control_L:
		case Gdk.Key.Control_R:
			HoldCtrl = false;
			break;
		}
	}
	
	void delete_event_cb (object o, DeleteEventArgs args)
	{
		Application.Quit ();
	}
	void on_print_activate (object sender, EventArgs e) 
	{
		 // desactivate css temporary
		 if (UseGecko)
		 	HelpSource.use_css = false;
		 
		string url = CurrentUrl;
		string html;
		Node cur = CurrentTab.CurrentNode;
		Node n; 

		// deal with the two types of urls
		if (cur.tree.HelpSource != null) {
			html = cur.tree.HelpSource.GetText (url, out n);
			if (html == null)
				html = help_tree.RenderUrl (url, out n);
		} else {
			html = help_tree.RenderUrl (url, out n);
		}

		// sending Html to be printed. 
		if (html != null)
			CurrentTab.html.Print (html);

		if (UseGecko)
			HelpSource.use_css = true;
	}

	void OnCommentsActivate (object o, EventArgs args)
	{
		SettingsHandler.Settings.ShowComments = comments1.Active;

		// postcomment.Sensitive = comments1.Active;

		// refresh, so we can see the comments
		if (CurrentTab != null && CurrentTab.history != null) // catch the case when we are currently loading
			CurrentTab.history.ActivateCurrent ();
	}
	
	void OnInheritedMembersActivate (object o, EventArgs args)
	{
		SettingsHandler.Settings.ShowInheritedMembers = showinheritedmembers.Active;
		if (CurrentTab != null && CurrentTab.history != null) // catch the case when we are currently loading
			CurrentTab.history.ActivateCurrent ();
	}

	void OnEditingActivate (object o, EventArgs args)
	{
		SettingsHandler.Settings.EnableEditing = editing1.Active;

		// refresh, so we can see the [edit] things
		if (CurrentTab != null && CurrentTab.history != null) // catch the case when we are currently loading
			CurrentTab.history.ActivateCurrent ();
	}
	
	void OnCollapseActivate (object o, EventArgs args)
	{
		reference_tree.CollapseAll ();
		reference_tree.ExpandRow (new TreePath ("0"), false);
	}

	//
	// Invoked when the index_entry Entry line content changes
	//
	public void OnIndexEntryChanged (object sender, EventArgs a)
	{
		if (index_browser != null)
			index_browser.SearchClosest (index_entry.Text);
	}

	//
	// Invoked when the user presses enter on the index_entry
	//
	public void OnIndexEntryActivated (object sender, EventArgs a)
	{
		if (index_browser != null)
			index_browser.LoadSelected ();
	}

	//
	// Invoked when the user presses a key on the index_entry
	//
	public void OnIndexEntryKeyPress (object o, KeyPressEventArgs args)
	{
		args.RetVal = true;

		switch (args.Event.Key) {
			case Gdk.Key.Up:

				if (matches.Visible == true && index_browser.match_list.Selected != 0)
				{
					index_browser.match_list.Selected--;
				} else {
					index_browser.index_list.Selected--;
					if (matches.Visible == true)
						index_browser.match_list.Selected = index_browser.match_model.Rows - 1;
				}
				break;

			case Gdk.Key.Down:

				if (matches.Visible == true && index_browser.match_list.Selected + 1 != index_browser.match_model.Rows) {
					index_browser.match_list.Selected++;
				} else {
					index_browser.index_list.Selected++;
					if (matches.Visible == true)
						index_browser.match_list.Selected = 0;
				}
				break;

			default:
				args.RetVal = false;
				break;
		}
	}

	//
	// For the accel keystroke
	//
	public void OnIndexEntryFocused (object sender, EventArgs a)
	{
		nb.Page = 1;
	}

	//
	// Invoked from File/Quit menu entry.
	//
	void OnQuitActivate (object sender, EventArgs a)
	{
		Application.Quit ();
	}

	//
	// Invoked by Edit/Cut menu entry.
	//
	void OnCutActivate (object sender, EventArgs a)
	{
		if (CurrentTab.Tab_mode == Mode.Editor) {
			Clipboard cb = Clipboard.Get (Gdk.Selection.Clipboard);
			CurrentTab.text_editor.Buffer.CutClipboard (cb, true);
		}
	}

	//
	// Invoked by Edit/Copy menu entry.
	//
	void OnCopyActivate (object sender, EventArgs a)
	{
		if (CurrentTab.Tab_mode == Mode.Viewer)
			CurrentTab.html.Copy ();
		else {
			Clipboard cb = Clipboard.Get (Gdk.Selection.Clipboard);
			CurrentTab.text_editor.Buffer.CopyClipboard (cb);
		}
	}

	//
	// Invoked by Edit/Paste menu entry.
	//
	void OnPasteActivate (object sender, EventArgs a)
	{
		Clipboard cb = Clipboard.Get (Gdk.Selection.Clipboard);
		
		if (!cb.WaitIsTextAvailable ())
			return;

		//string text = cb.WaitForText ();

		//CurrentTab.text_editor.Buffer.InsertAtCursor (text);

		CurrentTab.text_editor.Buffer.PasteClipboard (cb);
	}

	class About {
		[Glade.Widget] Window about;
		[Glade.Widget] Image logo_image;
		[Glade.Widget] Label label_version;

		static About AboutBox;
		Browser parent;

		About (Browser parent)
		{
			Glade.XML ui = new Glade.XML (null, "browser.glade", "about", null);
			ui.Autoconnect (this);
			this.parent = parent;

			about.TransientFor = parent.window1;

			Gdk.Pixbuf icon = new Gdk.Pixbuf (null, "monodoc.png");

			if (icon != null) {
				about.Icon = icon;
				logo_image.Pixbuf = icon;
			}

			Assembly assembly = Assembly.GetExecutingAssembly ();
			label_version.Markup = String.Format ("<b>Version:</b> {0}", assembly.GetName ().Version.ToString ());
		}

		void OnOkClicked (object sender, EventArgs a)
		{
			about.Hide ();
		}

                //
		// Called on the Window delete icon clicked
		//
		void OnDelete (object sender, DeleteEventArgs a)
		{
                        AboutBox = null;
		}

		static public void Show (Browser parent)
		{
			if (AboutBox == null)
				AboutBox = new About (parent);
			AboutBox.about.Show ();
		}
	}

	//
	// Hooked up from Glade
	//
	void OnAboutActivate (object sender, EventArgs a)
	{
		About.Show (this);
	}

	void OnUpload (object sender, EventArgs a)
	{
		string key = SettingsHandler.Settings.Key;
		if (key == null || key == "")
			ConfigWizard.Run (this);
		else
			DoUpload ();
	}

	void DoUpload ()
	{
		Upload.Run (this);
	}

	class Upload {
		enum State {
			GetSerial,
			PrepareUpload,
			SerialError,
			VersionError,
			SubmitError,
			NetworkError,
			Done
		}
		
		[Glade.Widget] Dialog upload_dialog;
		[Glade.Widget] Label status;
		[Glade.Widget] Button cancel;
		State state;
		ThreadNotify tn;
		WebClientAsyncResult war;
		ContributionsSoap d;
		int serial;
		
		public static void Run (Browser browser)
		{
			new Upload (browser);
		}

		Upload (Browser browser)
		{
			tn = new ThreadNotify (new ReadyEvent (Update));
			Glade.XML ui = new Glade.XML (null, "browser.glade", "upload_dialog", null);
			ui.Autoconnect (this);
			d = new ContributionsSoap ();
			if (Environment.GetEnvironmentVariable ("MONODOCTESTING") == null)
				d.Url = "http://www.go-mono.com/docs/server.asmx";
			
			status.Text = "Checking Server version";
			war = (WebClientAsyncResult) d.BeginCheckVersion (1, new AsyncCallback (VersionChecked), null);
		}

		void Update ()
		{
			Console.WriteLine ("In Update: " + state);
			switch (state){
			case State.NetworkError:
				status.Text = "A network error ocurred";
				cancel.Label = "Close";
				return;
			case State.VersionError:
				status.Text = "Server has a different version, upgrade your MonoDoc";
				cancel.Label = "Close";
				return;
			case State.GetSerial:
				war = (WebClientAsyncResult) d.BeginGetSerial (
					SettingsHandler.Settings.Email, SettingsHandler.Settings.Key,
					new AsyncCallback (GetSerialDone), null);
				return;
			case State.SerialError:
				status.Text = "Error obtaining serial number from server for this account";
				cancel.Label = "Close";
				return;
			case State.SubmitError:
				status.Text = "There was a problem with the documentation uploaded";
				cancel.Label = "Close";
				return;
				
			case State.PrepareUpload:
				GlobalChangeset cs = EditingUtils.GetChangesFrom (serial);
				if (cs == null){
					status.Text = "No new contributions";
					cancel.Label = "Close";
					return;
				}
				
				CopyXmlNodeWriter w = new CopyXmlNodeWriter ();
				GlobalChangeset.serializer.Serialize (w, cs);
				Console.WriteLine ("Uploading...");
				status.Text = String.Format ("Uploading {0} contributions", cs.Count);
				XmlDocument dd = (XmlDocument) w.Document;
				war = (WebClientAsyncResult) d.BeginSubmit (
					SettingsHandler.Settings.Email, SettingsHandler.Settings.Key,
					((XmlDocument) w.Document).DocumentElement,
					new AsyncCallback (UploadDone), null);
				return;
			case State.Done:
				status.Text = "All contributions uploaded";
				cancel.Label = "Close";
				SettingsHandler.Settings.SerialNumber = serial;
				SettingsHandler.Save ();
				return;
			}
		}

		void UploadDone (IAsyncResult iar)
		{
			try {
				int result = d.EndSubmit (iar);
				war = null;
				if (result < 0)
					state = State.SubmitError;
				else {
					state = State.Done;
					serial = result;
				}
			} catch (Exception e) {
				state = State.NetworkError;
				Console.WriteLine ("Upload: " + e);
			}
			if (tn != null)
				tn.WakeupMain ();
		}
		
		void GetSerialDone (IAsyncResult iar)
		{
			try {
				serial = d.EndGetSerial (iar);
				war = null;
				if (serial < 0)
					state = State.SerialError;
				else
					state = State.PrepareUpload;
			} catch (Exception e) {
				Console.WriteLine ("Serial: " + e);
				state = State.NetworkError;
			}
			if (tn != null)
				tn.WakeupMain ();
		}
		
		void VersionChecked (IAsyncResult iar)
		{
			try {
				int ver = d.EndCheckVersion (iar);
				war = null;
				if (ver != 0)
					state = State.VersionError;
				else
					state = State.GetSerial;
			} catch (Exception e) {
				Console.WriteLine ("Version: " + e);
				state = State.NetworkError;
			}
			if (tn != null)
				tn.WakeupMain ();
		}

		void Cancel_Clicked (object sender, EventArgs a)
		{
			if (war != null)
				war.Abort ();
			war = null;
			state = State.Done;

			upload_dialog.Destroy ();
			upload_dialog = null;
			tn = null;
		}
	}
	
	class ConfigWizard {
		static ConfigWizard config_wizard;
		
		[Glade.Widget] Window window_config_wizard;
		[Glade.Widget] Notebook notebook;
		[Glade.Widget] Button button_email_ok;
		[Glade.Widget] Entry entry_email;
		[Glade.Widget] Entry entry_password;
		
		Browser parent;
		ContributionsSoap d;
		WebClientAsyncResult war;
		ThreadNotify tn;
		int new_page;
		
		public static void Run (Browser browser)
		{
			if (config_wizard == null)
				config_wizard = new ConfigWizard (browser);
			return;
		}
			
		ConfigWizard (Browser browser)
		{
			tn = new ThreadNotify (new ReadyEvent (UpdateNotebookPage));
			Glade.XML ui = new Glade.XML (null, "browser.glade", "window_config_wizard", null);
			ui.Autoconnect (this);
			//notebook.ShowTabs = false;
			parent = browser;
			window_config_wizard.TransientFor = browser.window1;

			d = new ContributionsSoap ();
			if (Environment.GetEnvironmentVariable ("MONODOCTESTING") == null)
				d.Url = "http://www.go-mono.com/docs/server.asmx";
			notebook.Page = 8;
			
			war = (WebClientAsyncResult) d.BeginCheckVersion (1, new AsyncCallback (VersionChecked), null);
		}

		void NetworkError ()
		{
			new_page = 9;
			tn.WakeupMain ();
		}
		
		void VersionChecked (IAsyncResult iar)
		{
			int ver = -1;
			
			try {
				if (notebook.Page != 8)
					return;

				ver = d.EndCheckVersion (iar);
				if (ver != 0)
					new_page = 10;
				else 
					new_page = 0;
				tn.WakeupMain ();
			} catch (Exception e){
				Console.WriteLine ("Error" + e);
				NetworkError ();
			}
		}
		
		//
		// Called on the Window delete icon clicked
		//
		void OnDelete (object sender, DeleteEventArgs a)
		{
			config_wizard = null;
		}

		//
		// called when the license is approved
		//
		void OnPage1_Clicked (object sender, EventArgs a)
		{
			button_email_ok.Sensitive = false;
			notebook.Page = 1;
		}

		//
		// Request the user registration.
		//
		void OnPage2_Clicked (object sender, EventArgs a)
		{
			notebook.Page = 2;
			SettingsHandler.Settings.Email = entry_email.Text;
			war = (WebClientAsyncResult) d.BeginRegister (entry_email.Text, new AsyncCallback (RegisterDone), null);
		}

		void UpdateNotebookPage ()
		{
			notebook.Page = new_page;
		}
		
		void RegisterDone (IAsyncResult iar)
		{
			int code;
			
			try {
				Console.WriteLine ("Registration done");
				code = d.EndRegister (iar);
				if (code != 0 && code != -2){
					NetworkError ();
					return;
				}
				new_page = 4;
			} catch {
				new_page = 3;
			}
			tn.WakeupMain ();
		}

		void PasswordContinue_Clicked (object sender, EventArgs a)
		{
			notebook.Page = 5;
			SettingsHandler.Settings.Key = entry_password.Text;
			war = (WebClientAsyncResult) d.BeginGetSerial (entry_email.Text, entry_password.Text, new AsyncCallback (GetSerialDone), null); 
		}

		void GetSerialDone (IAsyncResult iar)
		{
			try {
				int last = d.EndGetSerial (iar);
				if (last == -1){
					SettingsHandler.Settings.Key = "";
					new_page = 11;
					tn.WakeupMain ();
					return;
				}
				
				SettingsHandler.Settings.SerialNumber = last;
				new_page = 6;
				tn.WakeupMain ();
			} catch {
				NetworkError ();
			}
		}
		
		void AccountRequestCancel_Clicked (object sender, EventArgs a)
		{
			war.Abort ();
			notebook.Page = 7;
		}

		void SerialRequestCancel_Clicked (object sender, EventArgs a)
		{
			war.Abort ();
			notebook.Page = 7;
		}

		void LoginRequestCancel_Clicked (object sender, EventArgs a)
		{
			war.Abort ();
			notebook.Page = 7;
		}

		//
		// Called when the user clicks `ok' on a terminate page
		//
		void Terminate_Clicked (object sender, EventArgs a)
		{
			window_config_wizard.Destroy ();
			config_wizard = null;
		}

		// Called when the registration process has been successful
		void Completed_Clicked (object sender, EventArgs a)
		{
			window_config_wizard.Destroy ();
			config_wizard = null;
			try {
				Console.WriteLine ("Saving");
				SettingsHandler.Save ();
				parent.DoUpload ();
			} catch (Exception e) {
				MessageDialog md = new MessageDialog (null, 
								      DialogFlags.DestroyWithParent,
								      MessageType.Error, 
								      ButtonsType.Close, "Error Saving settings\n" +
								      e.ToString ());
			}
		}
		
		void OnEmail_Changed (object sender, EventArgs a)
		{
			string text = entry_email.Text;
			
			if (text.IndexOf ("@") != -1 && text.Length > 3)
				button_email_ok.Sensitive = true;
		}
	}

	void OnContributionStatistics (object sender, EventArgs a)
	{
		string email = SettingsHandler.Settings.Email;
		string key = SettingsHandler.Settings.Key;
		
		if (key == null || key == "") {
			MessageDialog md = new MessageDialog (null, 
							      DialogFlags.DestroyWithParent,
							      MessageType.Info, 
							      ButtonsType.Close, 
				                  "You have not obtained or used a contribution key yet.");
			md.Title = "No contribution key";
			
			md.Run();
			md.Destroy();
		}
		else
			ContributionStatus.GetStatus (email, key);
	}
	
	class ContributionStatus {
		enum State {
			GetStatusError,
			NetworkError,
			Done
		}

		State state;
		Status status;
		string contributoremail;
		
		ThreadNotify tn;
		WebClientAsyncResult war;
		ContributionsSoap d;
		
		public static void GetStatus (string email, string key)
		{
			new ContributionStatus(email, key);
		}
		
		ContributionStatus (string email, string key)
		{
			tn = new ThreadNotify (new ReadyEvent (Update));
			
			d = new ContributionsSoap ();
			if (Environment.GetEnvironmentVariable ("MONODOCTESTING") == null)
				d.Url = "http://www.go-mono.com/docs/server.asmx";
				
			war = (WebClientAsyncResult) d.BeginGetStatus (email, key, new AsyncCallback (GetStatusDone), null);
			contributoremail = email;
		}
		
		void Update ()
		{
			MessageDialog md = null;
			
			switch (state) {
			case State.GetStatusError:
				md = new MessageDialog (null, 
					              DialogFlags.DestroyWithParent,
							      MessageType.Error, ButtonsType.Close, 
				                  "Server returned error while requesting contributor statistics");
				md.Title = "Contribution Statistics Error Occurred";
				break;
			case State.NetworkError:
				md = new MessageDialog (null, 
					              DialogFlags.DestroyWithParent,
							      MessageType.Error, ButtonsType.Close, 
				                  "Network error occurred while requesting contributor statistics");
				md.Title = "Contribution Statistics Error Occurred";
				break;
			case State.Done:
				md = new MessageDialog (null, 
					              DialogFlags.DestroyWithParent,
							      MessageType.Info, ButtonsType.Close, 
				                  "Contribution statistics for " + contributoremail +
					              "\n\nTotal contributions: " + status.Contributions +
					              "\nContributions committed: " + status.Commited +
					              "\nContributions pending: " + status.Pending);
				md.Title = "Contribution Statistics";
				break;
			}

			md.Run();
			md.Destroy();
		}
				
		void GetStatusDone (IAsyncResult iar)
		{
			try {
				status = d.EndGetStatus (iar);
				war = null;

				if (status == null)
					state = State.GetStatusError;
				else
					state = State.Done;

			} catch (Exception e) {
				state = State.NetworkError;
				Console.WriteLine ("Error getting status: " + e);
			}
			if (tn != null)
				tn.WakeupMain ();
		}	
	}

	class NewComment {
		[Glade.Widget] Window newcomment;
		[Glade.Widget] Entry entry;
		static NewComment NewCommentBox;
		Browser parent;
		
		NewComment (Browser browser)
		{
			Glade.XML ui = new Glade.XML (null, "browser.glade", "newcomment", null);
			ui.Autoconnect (this);
			parent = browser;
			newcomment.TransientFor = browser.window1;
		}

		void OnOkClicked (object sender, EventArgs a)
		{
			CommentService service = new CommentService();
			// todo
			newcomment.Hide ();
		}

		void OnCancelClicked (object sender, EventArgs a)
		{
			newcomment.Hide ();
		}

                //
		// Called on the Window delete icon clicked
		//
		void OnDelete (object sender, DeleteEventArgs a)
		{
                        NewCommentBox = null;
		}

		static public void Show (Browser browser)
		{
			if (NewCommentBox == null)
				NewCommentBox = new NewComment (browser);
			NewCommentBox.newcomment.Show ();
		}
	}

	void OnNewComment (object sender, EventArgs a)
	{
		NewComment.Show (this);
	}



	class Lookup {
		[Glade.Widget] Window lookup;
		[Glade.Widget] Entry entry;
		static Lookup LookupBox;
		Browser parent;
		
		Lookup (Browser browser)
		{
			Glade.XML ui = new Glade.XML (null, "browser.glade", "lookup", null);
			ui.Autoconnect (this);
			parent = browser;
			lookup.TransientFor = browser.window1;
		}

		void OnOkClicked (object sender, EventArgs a)
		{
			string text = entry.Text;
			if (text != "")
				parent.LoadUrl (entry.Text);
			lookup.Hide ();
		}

                //
		// Called on the Window delete icon clicked
		//
		void OnDelete(object sender, DeleteEventArgs a)
		{
                        LookupBox = null;
		}

                static public void Show (Browser browser)
		{
			if (LookupBox == null)
				LookupBox = new Lookup (browser);
			LookupBox.lookup.Show ();
		}
	}

	//
	// Invoked by File/LookupURL menu entry.
	//
	void OnLookupURL (object sender, EventArgs a)
	{
		Lookup.Show (this);
	}

	//
	// Invoked by Edit/Select All menu entry.
	//
	void OnSelectAllActivate (object sender, EventArgs a)
	{
		CurrentTab.html.SelectAll ();
	}
	
	//
	// Invoked by New Tab menu entry.
	//
	void OnNewTab (object sender, EventArgs a)
	{
		AddTab();
	}
	
}

//
// This class implements the tree browser
//
public class TreeBrowser {
	Browser browser;

	TreeView tree_view;
	
	TreeStore store;
	RootTree help_tree;
	TreeIter root_iter;

	//
	// This hashtable maps an iter to its node.
	//
	Hashtable iter_to_node;

	//
	// This hashtable maps the node to its iter
	//
	Hashtable node_to_iter;

	//
	// Maps a node to its TreeIter parent
	//
	Hashtable node_parent;

	public TreeBrowser (RootTree help_tree, TreeView reference_tree, Browser browser)
	{
		this.browser = browser;
		tree_view = reference_tree;
		iter_to_node = new Hashtable ();
		node_to_iter = new Hashtable ();
		node_parent = new Hashtable ();

		// Setup the TreeView
		tree_view.AppendColumn ("name_col", new CellRendererText (), "text", 0);

		// Bind events
		tree_view.RowExpanded += new Gtk.RowExpandedHandler (RowExpanded);
		tree_view.Selection.Changed += new EventHandler (RowActivated);
		tree_view.RowActivated += new Gtk.RowActivatedHandler (RowClicked);

		// Setup the model
		this.help_tree = help_tree;
		store = new TreeStore (typeof (string));

		root_iter = store.AppendValues ("Mono Documentation");
		iter_to_node [root_iter] = help_tree;
		node_to_iter [help_tree] = root_iter;
		PopulateNode (help_tree, root_iter);

		reference_tree.Model = store;
	}

	void PopulateNode (Node node, TreeIter parent)
	{
		if (node.Nodes == null)
			return;

		TreeIter iter;
		foreach (Node n in node.Nodes){
			iter = store.AppendValues (parent, n.Caption);
			iter_to_node [iter] = n;
			node_to_iter [n] = iter;
		}
	}

	Hashtable populated = new Hashtable ();
	
	void RowExpanded (object o, Gtk.RowExpandedArgs args)
	{
		Node result = iter_to_node [args.Iter] as Node;

		Open (result);
	}

	void RowClicked (object o, Gtk.RowActivatedArgs args)
	{
		Gtk.TreeModel model;
		Gtk.TreeIter iter;	
		Gtk.TreePath path = args.Path;	

		tree_view.Selection.GetSelected (out model, out iter);

		Node result = iter_to_node [iter] as Node;

		if (!tree_view.GetRowExpanded (path)) {
			tree_view.ExpandRow (path, false);
			Open (result);
		} else {
			tree_view.CollapseRow (path);
		}
			
	}

	void Open (Node node)
	{
		if (node == null){
			Console.Error.WriteLine ("Expanding something that I do not know about");
			return;
		}

		if (populated.Contains (node))
			return;
		
		//
		// We need to populate data on a second level
		//
		if (node.Nodes == null)
			return;

		foreach (Node n in node.Nodes){
			PopulateNode (n, (TreeIter) node_to_iter [n]);
		}
		populated [node] = true;
	}
	
	void PopulateTreeFor (Node n)
	{
		if (populated [n] == null){
			if (n.Parent != null) {
				OpenTree (n.Parent);
			}
		} 
		Open (n);
	}

	public void OpenTree (Node n)
	{
		PopulateTreeFor (n);

		TreeIter iter = (TreeIter) node_to_iter [n];
		TreePath path = store.GetPath (iter);
	}

	public Node SelectedNode
	{
		get {
	                Gtk.TreeIter iter;
	                Gtk.TreeModel model;

	                if (tree_view.Selection.GetSelected (out model, out iter))
	                        return (Node) iter_to_node [iter];
			else
				return null;
		}
	}
	
	public void ShowNode (Node n)
	{
		if (node_to_iter [n] == null){
			OpenTree (n);
			if (node_to_iter [n] == null){
				Console.Error.WriteLine ("Internal error: no node to iter mapping");
				return;
			}
		}
		
		TreeIter iter = (TreeIter) node_to_iter [n];
		TreePath path = store.GetPath (iter);

		tree_view.ExpandToPath (path);

		IgnoreRowActivated = true;
		tree_view.Selection.SelectPath (path);
		IgnoreRowActivated = false;
		tree_view.ScrollToCell (path, null, false, 0.5f, 0.0f);
	}
	
	class NodePageVisit : PageVisit {
		Browser browser;
		Node n;
		string url;

		public NodePageVisit (Browser browser, Node n, string url)
		{
			if (n == null)
				throw new Exception ("N is null");
			
			this.browser = browser;
			this.n = n;
			this.url = url;
		}

		public override void Go ()
		{
			string res;
			Node x;
			
			// The root tree has no help source
			if (n.tree.HelpSource != null)
				res = n.tree.HelpSource.GetText (url, out x);
			else
				res = ((RootTree)n.tree).RenderUrl (url, out x);

			browser.Render (res, n, url);
		}
	}

	bool IgnoreRowActivated = false;
	
	//
	// This has to handle two kinds of urls: those encoded in the tree
	// file, which are used to quickly lookup information precisely
	// (things like "ecma:0"), and if that fails, it uses the more expensive
	// mechanism that walks trees to find matches
	//
	void RowActivated  (object sender, EventArgs a)
	{

		//browser.CurrentTab.SetMode (Mode.Viewer);

		if (IgnoreRowActivated)
			return;
		
		Gtk.TreeIter iter;
		Gtk.TreeModel model;

		if (tree_view.Selection.GetSelected (out model, out iter)){
			Node n = (Node) iter_to_node [iter];
			
			string url = n.URL;
			Node match;
			string s;

			if (n.tree.HelpSource != null)
			{
				//
				// Try the tree-based urls first.
				//
				
				s = n.tree.HelpSource.GetText (url, out match);
				if (s != null){
					((Browser)browser).Render (s, n, url);
					browser.CurrentTab.history.AppendHistory (new NodePageVisit (browser, n, url));
					return;
				}
			}
			
			//
			// Try the url resolver next
			//
			s = help_tree.RenderUrl (url, out match);
			if (s != null){
				((Browser)browser).Render (s, n, url);
				browser.CurrentTab.history.AppendHistory (new Browser.LinkPageVisit (browser, url));
				return;
			}

			((Browser)browser).Render ("<h1>Unhandled URL</h1>" + "<p>Functionality to view the resource <i>" + n.URL + "</i> is not available on your system or has not yet been implemented.</p>", null, url);
		}
	}
}

//
// The index browser
//
class IndexBrowser {
	Browser browser;

	IndexReader index_reader;
	public BigList index_list;
	public MatchModel match_model;
	public BigList match_list;
	IndexEntry current_entry = null;
	

	public static IndexBrowser MakeIndexBrowser (Browser browser)
	{
		IndexReader ir = browser.help_tree.GetIndex ();
		if (ir == null) {
			return new IndexBrowser (browser);
		}

		return new IndexBrowser (browser, ir);
	}

	ProgressPanel ppanel;
	IndexBrowser (Browser parent)
	{
			browser = parent;
			ppanel = new ProgressPanel ("<b>No index found</b>", "Generate", RootTree.MakeIndex, NewIndexCreated); 
			browser.index_vbox.Add (ppanel);
			browser.index_vbox.Show ();
	}

	void NewIndexCreated ()
	{
		index_reader = browser.help_tree.GetIndex ();
		//restore widgets
		browser.index_vbox.Remove (ppanel);
		CreateWidget ();
		browser.index_vbox.ShowAll ();
	}
	
	IndexBrowser (Browser parent, IndexReader ir)
	{
		browser = parent;
		index_reader = ir;

		CreateWidget ();
	}

	void CreateWidget () {
		//
		// Create the widget
		//
		Frame frame1 = new Frame ();
		VBox vbox1 = new VBox (false, 0);
		frame1.Add (vbox1);

		// title
		HBox hbox1 = new HBox (false, 3);
		hbox1.BorderWidth = 3;
		Image icon = new Image (Stock.Index, IconSize.Menu);
		Label look_for_label = new Label ("Look for:");
		look_for_label.Justify = Justification.Left;
		look_for_label.Xalign = 0;
		hbox1.PackEnd (look_for_label, true, true, 0);
		hbox1.PackEnd (icon, false, true, 0);
		hbox1.ShowAll ();
		vbox1.PackStart (hbox1, false, true, 0);

		// entry
		vbox1.PackStart (new HSeparator (), false, true, 0);
		browser.index_entry = new Entry ();
		browser.index_entry.Activated += browser.OnIndexEntryActivated;
		browser.index_entry.Changed += browser.OnIndexEntryChanged;
		browser.index_entry.FocusInEvent += browser.OnIndexEntryFocused;
		browser.index_entry.KeyPressEvent += browser.OnIndexEntryKeyPress;
		vbox1.PackStart (browser.index_entry, false, true, 0);
		vbox1.PackStart (new HSeparator (), false, true, 0);

		//search results
		browser.search_box = new VBox ();
		vbox1.PackStart (browser.search_box, true, true, 0);
		vbox1.ShowAll ();

		
		//
		// Setup the widget
		//
		index_list = new BigList (index_reader);
		//index_list.SetSizeRequest (100, 400);

		index_list.ItemSelected += new ItemSelected (OnIndexSelected);
		index_list.ItemActivated += new ItemActivated (OnIndexActivated);
		HBox box = new HBox (false, 0);
		box.PackStart (index_list, true, true, 0);
		Scrollbar scroll = new VScrollbar (index_list.Adjustment);
		box.PackEnd (scroll, false, false, 0);
		
		browser.search_box.PackStart (box, true, true, 0);
		box.ShowAll ();

		//
		// Setup the matches.
		//
		browser.matches = new Frame ();
		match_model = new MatchModel (this);
		browser.matches.Hide ();
		match_list = new BigList (match_model);
		match_list.ItemSelected += new ItemSelected (OnMatchSelected);
		match_list.ItemActivated += new ItemActivated (OnMatchActivated);
		HBox box2 = new HBox (false, 0);
		box2.PackStart (match_list, true, true, 0);
		Scrollbar scroll2 = new VScrollbar (match_list.Adjustment);
		box2.PackEnd (scroll2, false, false, 0);
		box2.ShowAll ();
		
		browser.matches.Add (box2);
		index_list.SetSizeRequest (100, 200);

		browser.index_vbox.PackStart (frame1);
		browser.index_vbox.PackEnd (browser.matches);
	}

	//
	// This class is used as an implementation of the IListModel
	// for the matches for a given entry.
	// 
	public class MatchModel : IListModel {
		IndexBrowser index_browser;
		Browser browser;
		
		public MatchModel (IndexBrowser parent)
		{
			index_browser = parent;
			browser = parent.browser;
		}
		
		public int Rows {
			get {
				if (index_browser.current_entry != null)
					return index_browser.current_entry.Count;
				else
					return 0;
			}
		}

		public string GetValue (int row)
		{
			Topic t = index_browser.current_entry [row];
			
			// Names from the ECMA provider are somewhat
			// ambigious (you have like a million ToString
			// methods), so lets give the user the full name
			
			// Filter out non-ecma
			if (t.Url [1] != ':')
				return t.Caption;
			
			switch (t.Url [0]) {
				case 'C': return t.Url.Substring (2) + " constructor";
				case 'M': return t.Url.Substring (2) + " method";
				case 'P': return t.Url.Substring (2) + " property";
				case 'F': return t.Url.Substring (2) + " field";
				case 'E': return t.Url.Substring (2) + " event";
				default:
					return t.Caption;
			}
		}

		public string GetDescription (int row)
		{
			return GetValue (row);
		}
		
	}

	void ConfigureIndex (int index)
	{
		current_entry = index_reader.GetIndexEntry (index);

		if (current_entry.Count > 1){
			browser.matches.Show ();
			match_list.Reload ();
			match_list.Refresh ();
		} else {
			browser.matches.Hide ();
		}
	}
	
	//
	// When an item is selected from the main index list
	//
	void OnIndexSelected (int index)
	{
		ConfigureIndex (index);
		if (browser.matches.Visible == true)
			match_list.Selected = 0;
	}

	void OnIndexActivated (int index)
	{
		if (browser.matches.Visible == false)
			browser.LoadUrl (current_entry [0].Url);
	}

	void OnMatchSelected (int index)
	{
	}

	void OnMatchActivated (int index)
	{
		browser.LoadUrl (current_entry [index].Url);
	}

	int FindClosest (string text)
	{
		int low = 0;
		int top = index_reader.Rows-1;
		int high = top;
		bool found = false;
		int best_rate_idx = Int32.MaxValue, best_rate = -1;
		
		while (low <= high){
			int mid = (high + low) / 2;

			//Console.WriteLine ("[{0}, {1}] -> {2}", low, high, mid);

			string s;
			int p = mid;
			for (s = index_reader.GetValue (mid); s [0] == ' ';){
				if (p == high){
					if (p == low){
						if (best_rate_idx != Int32.MaxValue){
							//Console.WriteLine ("Bestrated: "+best_rate_idx);
							//Console.WriteLine ("Bestrated: "+index_reader.GetValue(best_rate_idx));
							return best_rate_idx;
						} else {
							//Console.WriteLine ("Returning P="+p);
							return p;
						}
					}
					
					high = mid;
					break;
				}

				if (p < 0)
					return 0;

				s = index_reader.GetValue (++p);
				//Console.WriteLine ("   Advancing to ->"+p);
			}
			if (s [0] == ' ')
				continue;
			
			int c, rate;
			c = Rate (text, s, out rate);
			//Console.WriteLine ("[{0}] Text: {1} at {2}", text, s, p);
			//Console.WriteLine ("     Rate: {0} at {1}", rate, p);
			//Console.WriteLine ("     Best: {0} at {1}", best_rate, best_rate_idx);
			//Console.WriteLine ("     {0} - {1}", best_rate, best_rate_idx);
			if (rate >= best_rate){
				best_rate = rate;
				best_rate_idx = p;
			}
			if (c == 0)
				return mid;

			if (low == high){
				//Console.WriteLine ("THISPATH");
				if (best_rate_idx != Int32.MaxValue)
					return best_rate_idx;
				else
					return low;
			}

			if (c < 0){
				high = mid;
			} else {
				if (low == mid)
					low = high;
				else
					low = mid;
			}
		}

		//		Console.WriteLine ("Another");
		if (best_rate_idx != Int32.MaxValue)
			return best_rate_idx;
		else
			return high;

	}

	int Rate (string user_text, string db_text, out int rate)
	{
		int c = String.Compare (user_text, db_text, true);
		if (c == 0){
			rate = 0;
			return 0;
		}

		int i;
		for (i = 0; i < user_text.Length; i++){
			if (db_text [i] != user_text [i]){
				rate = i;
				return c;
			}
		}
		rate = i;
		return c;
	}
	
	public void SearchClosest (string text)
	{
		index_list.Selected = FindClosest (text);
	}

	public void LoadSelected ()
	{
		if (browser.matches.Visible == true) {
			if (match_list.Selected != -1)
				OnMatchActivated (match_list.Selected);
		} else {
			if (index_list.Selected != -1)
				OnIndexActivated (index_list.Selected);
		}
	}
}

public enum Mode {
		Viewer, Editor
	}
	
//
// A Tab is a Notebok with two pages, one for editing and one for visualizing
//
public class Tab : Notebook {
	
	// Our HTML preview during editing.
	public IHtmlRender html_preview;
	
	// Where we render the contents
	public IHtmlRender html;
	
	public TextView text_editor;
	public Mode Tab_mode;
	public History history;
	private Browser browser;
	private Label titleLabel;
	private Image EditImg;
	public HBox TabLabel;
	
	public string Title {
		get { return titleLabel.Text; }
		set { titleLabel.Text = value; }
	}
	
	public Node CurrentNode;
	public System.Xml.XmlNode edit_node;
	public string edit_url;
	
	void FocusOut (object sender, FocusOutEventArgs args)
	{
		if (TabMode == Mode.Editor)
			text_editor.GrabFocus ();	
	}

	static IHtmlRender GetRenderer (string file, string type, Browser browser)
	{
		try {
			
			string exeAssembly = Assembly.GetExecutingAssembly ().Location;
			string myPath = System.IO.Path.GetDirectoryName (exeAssembly);
			Assembly dll = Assembly.LoadFrom (System.IO.Path.Combine (myPath, file));
			Type t = dll.GetType (type, true);
		
			return (IHtmlRender) Activator.CreateInstance (t, new object [1] { browser.help_tree });
		} catch {
			return null;
		}
	}
	

	public Tab(Browser br) 
	{

		browser = br;
		CurrentNode = br.help_tree;
		ShowTabs = false;
		ShowBorder = false;
		TabBorder = 0;
		TabHborder = 0;
		history = new History (browser.back_button, browser.forward_button);
		
		//
		// First Page
		//
		ScrolledWindow html_container = new ScrolledWindow();
		html_container.Show();
		
		//
		// Setup the HTML rendering and preview area
		//
		if (browser.UseGecko) {
			html = GetRenderer ("GeckoHtmlRender.dll", "Monodoc.GeckoHtmlRender", browser);
			html_preview = GetRenderer ("GeckoHtmlRender.dll", "Monodoc.GeckoHtmlRender", browser);
		}
		
		if (html == null || html_preview == null) {
			html = GetRenderer ("GtkHtmlHtmlRender.dll", "Monodoc.GtkHtmlHtmlRender", browser);
			html_preview = GetRenderer ("GtkHtmlHtmlRender.dll", "Monodoc.GtkHtmlHtmlRender", browser);
			browser.UseGecko = false;
		}

		if (html == null || html_preview == null)
			throw new Exception ("Couldn't find html renderer!");
				
		//Prepare Font for css (TODO: use GConf?)
		if (browser.UseGecko && SettingsHandler.Settings.preferred_font_size == 0) { 
			Pango.FontDescription font_desc = Pango.FontDescription.FromString ("Sans 12");
			SettingsHandler.Settings.preferred_font_family = font_desc.Family;
			SettingsHandler.Settings.preferred_font_size = 100; //size: 100%
		}
		
		html_container.Add (html.HtmlPanel);
		html.UrlClicked += new EventHandler (browser.LinkClicked);
		html.OnUrl += new EventHandler (browser.OnUrlMouseOver);
		browser.context_id = browser.statusbar.GetContextId ("");
		
		AppendPage(html_container, new Label("Html"));
		
		//
		// Second Page: editing
		//
		VBox vbox1 = new VBox(false, 0);
		
		VBox MainPart = new VBox(false, 0);
		
		//
		// TextView for XML code
		//
		ScrolledWindow sw = new ScrolledWindow();
		text_editor = new TextView();
		text_editor.Buffer.Changed += new EventHandler (EditedTextChanged);
		text_editor.WrapMode = WrapMode.Word;
		sw.Add(text_editor);
		text_editor.FocusOutEvent += new FocusOutEventHandler (FocusOut);
		
		//
		// XML editing buttons
		//
		HBox EdBots = new HBox(false, 2);
		Button button1 = new Button("<e_xample>");
		EdBots.PackStart(button1);
		Button button2 = new Button("<list>");
		EdBots.PackStart(button2);
		Button button3 = new Button("<_table>");
		EdBots.PackStart(button3);
		Button button4 = new Button("<_see...>");
		EdBots.PackStart(button4);
		Button button5 = new Button("<_para>");
		EdBots.PackStart(button5);
		Button button6 = new Button("Add Note");
		EdBots.PackStart(button6);
		
		button1.Clicked += new EventHandler (OnInsertExampleClicked);
		button2.Clicked += new EventHandler (OnInsertListClicked);
		button3.Clicked += new EventHandler (OnInsertTableClicked);
		button4.Clicked += new EventHandler (OnInsertType);
		button5.Clicked += new EventHandler (OnInsertParaClicked);
		button6.Clicked += new EventHandler (OnInsertNoteClicked);
		
		Frame html_preview_frame = new Frame("Preview");
		ScrolledWindow html_preview_container = new ScrolledWindow();
		//
		// code preview panel
		//
		html_preview_container.Add (html_preview.HtmlPanel);
		html_preview_frame.Add(html_preview_container);
		
		MainPart.PackStart(sw);
		MainPart.PackStart(EdBots, false, false, 0);
		MainPart.PackStart(html_preview_frame);
		
		//
		// Close and Save buttons
		//
		HBox MainBots = new HBox(false, 3);
		HBox Filling = new HBox(false, 0);
		Button close = new Button("C_lose");
		Button save = new Button("S_ave");
		Button restore = new Button("_Restore");
		
		close.Clicked += new EventHandler (OnCancelEdits);
		save.Clicked += new EventHandler (OnSaveEdits);
		restore.Clicked += new EventHandler (OnRestoreEdits);
		
		MainBots.PackStart(Filling);
		MainBots.PackStart(close, false, false, 0);
		MainBots.PackStart(save, false, false, 0);
		MainBots.PackStart(restore, false, false, 0);
		
		vbox1.PackStart(MainPart);
		vbox1.PackStart(MainBots, false, false, 0);
		
		AppendPage(vbox1, new Label("Edit XML"));
		
		//
		//Create the Label for the Tab
		//
		TabLabel = new HBox(false, 2);
		
		titleLabel = new Label("");
		
		//Close Tab button
		Button tabClose = new Button();
		Image img = new Image(Stock.Close, IconSize.SmallToolbar);
		tabClose.Add(img);
		tabClose.Relief = Gtk.ReliefStyle.None;
		tabClose.SetSizeRequest (18, 18);
		tabClose.Clicked += new EventHandler (OnTabClose);
		
		//Icon showed when the Tab is in Edit Mode
		EditImg = new Image (Stock.Convert, IconSize.SmallToolbar);
		
		TabLabel.PackStart (EditImg, false, true, 2);
		TabLabel.PackStart (titleLabel, true, true, 0);
		TabLabel.PackStart (tabClose, false, false, 2);
		
		// needed, otherwise even calling show_all on the notebook won't
		// make the hbox contents appear.
		TabLabel.ShowAll();
		EditImg.Visible = false;
	
	}
	
	public Mode TabMode {
		get { return Tab_mode; }
		set { Tab_mode = value; }
	}

	
	public void SetMode (Mode m)
	{
		if (Tab_mode == m)
			return;
		
		if (m == Mode.Viewer) {
			this.Page = 0;
			browser.cut1.Sensitive = false;
			browser.paste1.Sensitive = false;
			browser.print.Sensitive = true;
			EditImg.Visible = false;
		} else {
			this.Page = 1;
			browser.cut1.Sensitive = true;
			browser.paste1.Sensitive = true;
			browser.print.Sensitive = false;
			EditImg.Visible = true;
		}

		Tab_mode = m;
	}
	
	// Events for the Editor
	
	void OnInsertParaClicked (object sender, EventArgs a)
	{
		text_editor.Buffer.InsertAtCursor ("\n<para>\n</para>");
	}

	void OnInsertNoteClicked (object sender, EventArgs a)
	{
		text_editor.Buffer.InsertAtCursor ("\n<block subset=\"none\" type=\"note\">\n  <para>\n  </para>\n</block>");
	}

	void OnInsertExampleClicked (object sender, EventArgs a)
	{
		text_editor.Buffer.InsertAtCursor ("\n<example>\n  <code lang=\"C#\">\n  </code>\n</example>");
	}

	void OnInsertListClicked  (object sender, EventArgs a)
	{
		text_editor.Buffer.InsertAtCursor ("\n<list type=\"bullet\">\n  <item>\n    <term>First Item</term>\n  </item>\n</list>");

	}

	void OnInsertTableClicked (object sender, EventArgs a)
	{
		text_editor.Buffer.InsertAtCursor ("\n<list type=\"table\">\n  <listheader>\n    <term>Column</term>\n" +
						   "    <description>Description</description>\n" +
						   "  </listheader>\n" +
						   "  <item>\n" +
						   "    <term>Term</term>\n" +
						   "    <description>Description</description>\n" +
						   "  </item>\n" +
						   "</list>");
	 }

	void OnInsertType (object sender, EventArgs a)
	{
		text_editor.Buffer.InsertAtCursor ("<see cref=\"T:System.Object\"/>");
	}
	void OnSaveEdits (object sender, EventArgs a)
	{
		try {
			edit_node.InnerXml = text_editor.Buffer.Text;
		} catch (Exception e) {
			browser.statusbar.Pop (browser.context_id);
			browser.statusbar.Push (browser.context_id, e.Message);
			return;
		}
		string [] uSplit = EditingUtils.ParseEditUrl (edit_url);
		
		if (uSplit[0].StartsWith ("monodoc:"))
			EditingUtils.SaveChange (edit_url, browser.help_tree, edit_node, EcmaHelpSource.GetNiceUrl (browser.CurrentTab.CurrentNode));
		else if (uSplit[0].StartsWith ("file:"))
			EditingUtils.SaveChange (edit_url, browser.help_tree, edit_node, String.Empty);
		else
			Console.WriteLine ("Edit url wrong: {0}", edit_url);
		SetMode (Mode.Viewer);
		history.ActivateCurrent ();
	}

	void OnCancelEdits (object sender, EventArgs a)
	{
		SetMode (Mode.Viewer);
		history.ActivateCurrent ();
	}

	void OnRestoreEdits (object sender, EventArgs a)
	{
		EditingUtils.RemoveChange (edit_url, browser.help_tree);
		SetMode (Mode.Viewer);
		history.ActivateCurrent ();
	}
	
	bool queued = false;

	void EditedTextChanged (object sender, EventArgs args)
	{
		if (queued)
			return;

		queued = true;
		GLib.Timeout.Add (500, delegate {
			queued = false;
			
			StringWriter sw = new StringWriter ();
			XmlWriter w = new XmlTextWriter (sw);
			
			try {
				edit_node.InnerXml = text_editor.Buffer.Text;
				EditingUtils.RenderEditPreview (edit_url, browser.help_tree, edit_node, w);
				w.Flush ();
			} catch (Exception e) {
				browser.statusbar.Pop (browser.context_id);
				browser.statusbar.Push (browser.context_id, e.Message);

				return false;
			}
			browser.statusbar.Pop (browser.context_id);
			browser.statusbar.Push (browser.context_id, "XML OK");
			string s = HelpSource.BuildHtml (EcmaHelpSource.css_ecma_code, sw.ToString ());
			html_preview.Render(s);

			return false;
		});
	}
	void OnTabClose (object sender, EventArgs a)
	{
		browser.tabs_nb.RemovePage(browser.tabs_nb.PageNum(this));
		browser.tabs_nb.ShowTabs = (browser.tabs_nb.NPages > 1);
	}
	
}
}
