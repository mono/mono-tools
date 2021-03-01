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
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Web.Services.Protocols;
using System.Xml;

using Mono.Options;

#if MACOS
using OSXIntegration.Framework;
#endif

namespace Monodoc {
public class Browser {
	Builder ui;
	public Gtk.Window MainWindow;

	public Window window1;
	TreeView reference_tree;
	public Statusbar statusbar;
	public Button back_button, forward_button;
	public Entry index_entry;
	CheckMenuItem showinheritedmembers;
	public MenuItem print;
	public MenuItem close_tab;
	public Notebook tabs_nb;
	public Tab CurrentTab;
	bool HoldCtrl;
	public string engine;

	public MenuItem bookmarksMenu;
	MenuItem view1;
	MenuItem textLarger;
	MenuItem textSmaller;
	MenuItem textNormal;

	VBox help_container;
	
	EventBox bar_eb, index_eb;
	Label subtitle_label;
	Notebook nb;

	Box title_label_box;
	ELabel title_label;

	// Bookmark Manager
	BookmarkManager bookmark_manager;

	//
	// Accessed from the IndexBrowser class
	//
	internal VBox search_box;
	internal Frame matches;
	internal VBox index_vbox;
	
	Gdk.Pixbuf monodoc_pixbuf;

	//
	// Used for searching
	//
	Entry search_term;
	TreeView search_tree;
	TreeStore search_store;
	SearchableIndex search_index;
	ArrayList searchResults = new ArrayList (20);
	string highlight_text;
	VBox search_vbox;
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

	public Capabilities capabilities;

	public Browser (string basedir, IEnumerable<string> sources, string engine)
	{
#if MACOS
		try {
			InitMacAppHandlers();
		} catch (Exception ex) {
			Console.Error.WriteLine ("Installing Mac AppleEvent handlers failed. Skipping.\n" + ex);
		}
#endif
	
		this.engine = engine;		
		ui = new Builder();
		ui.AddFromFile("Browser.glade");
		ui.Autoconnect (this);

		MainWindow = (Gtk.Window) ui.GetObject("window1");
		window1 = MainWindow;

		// Glade did this via attribs, Builder doesn't; we need to initialize everything
		help_container = (Gtk.VBox) ui.GetObject("help_container");
		search_vbox = (Gtk.VBox) ui.GetObject("search_vbox");
		index_vbox = (Gtk.VBox) ui.GetObject("index_vbox");
		title_label_box = (Gtk.Box) ui.GetObject("title_label_box");
		bar_eb = (Gtk.EventBox) ui.GetObject("bar_eb");
		index_eb = (Gtk.EventBox) ui.GetObject("index_eb");
		back_button = (Button) ui.GetObject("back_button");
		forward_button = (Button) ui.GetObject("forward_button");
		reference_tree = (TreeView) ui.GetObject("reference_tree");
		subtitle_label = (Label) ui.GetObject("subtitle_label");
		nb = (Notebook) ui.GetObject("nb");
		statusbar = (Statusbar) ui.GetObject("statusbar");
		showinheritedmembers = (CheckMenuItem) ui.GetObject("showinheritedmembers");
		print = (MenuItem) ui.GetObject("print");
		view1 = (MenuItem) ui.GetObject("view1");
		close_tab = (MenuItem) ui.GetObject("close_tab");
		bookmarksMenu = (MenuItem) ui.GetObject("bookmarksMenu");
		bookmarksMenu.Submenu = new Menu(); // sigh; null now...

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

		help_tree = Driver.LoadTree (basedir, sources);
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

		AddTab();
			
			
		if ((capabilities & Capabilities.Fonts) != 0) {
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
		MainWindow.ShowAll();
		
#if MACOS
		try {
			InstallMacMainMenu ();
			((MenuBar)ui["menubar1"]).Hide ();
		} catch (Exception ex) {
			Console.Error.WriteLine ("Installing Mac IGE Main Menu failed. Skipping.\n" + ex);
		}
#endif
	}

#if MACOS
		void InstallMacMainMenu ()
		{
			IgeMacIntegration.IgeMacMenu.GlobalKeyHandlerEnabled = true;
			IgeMacIntegration.IgeMacMenu.MenuBar = (MenuBar) ui["menubar1"];
			IgeMacIntegration.IgeMacMenu.QuitMenuItem = (MenuItem) ui["quit1"];
			var appGroup = IgeMacIntegration.IgeMacMenu.AddAppMenuGroup ();
			appGroup.AddMenuItem ((MenuItem)ui["quit1"], null);
			appGroup.AddMenuItem ((MenuItem)ui["about1"], null);
		}

		void InitMacAppHandlers ()
		{
			ApplicationEvents.Quit += delegate (object sender, ApplicationEventArgs e) {
				Application.Quit ();
				e.Handled = true;
			};
			
			ApplicationEvents.Reopen += delegate (object sender, ApplicationEventArgs e) {
				if (MainWindow != null) {
					MainWindow.Deiconify ();
					MainWindow.Visible = true;
					e.Handled = true;
				}
			};
			
			ApplicationEvents.OpenUrls += delegate (object sender, ApplicationUrlEventArgs e) {
				if (e.Urls == null || e.Urls.Count == 0)
					return;
				string url = e.Urls[0];
				if (string.IsNullOrEmpty (url) || !url.StartsWith ("monodoc://"))
					return;
				url = url.Substring ("monodoc://".Length);
				if (url.Length == 0)
					return;
				url = System.Web.HttpUtility.UrlDecode (url);
				LoadUrl (url);
				if (MainWindow != null)
					MainWindow.Present ();
				e.Handled = true;
			};
		}
#endif

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
		search_vbox.PackStart (vbox1, true, true, 0);
		
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
		search_tree.FocusOutEvent += new FocusOutEventHandler(LostFocus);

		vbox1.ShowAll ();
		search_vbox.ShowAll ();
	}	
			
	// Adds a Tab and Activates it
	void AddTab() 
	{
		CurrentTab = new Tab (this);
		tabs_nb.AppendPage (CurrentTab, CurrentTab.TabLabel);
		tabs_nb.ShowTabs = (tabs_nb.NPages > 1);
		close_tab.Sensitive = (tabs_nb.NPages > 1);
		tabs_nb.ShowAll (); //Needed to show the new tab
		tabs_nb.CurrentPage = tabs_nb.PageNum (CurrentTab);
		//Show root node
		Node match;
		string s = Browser.GetHtml ("root:", null, help_tree, out match);
		if (s != null){
			Render (s, match, "root:");
			CurrentTab.history.AppendHistory (new Browser.LinkPageVisit (this, "root:"));
		}
		
	}

	void CloseTab ()
	{
		tabs_nb.RemovePage(tabs_nb.CurrentPage);
		bool multiple_tabs = (tabs_nb.NPages > 1);
		tabs_nb.ShowTabs = multiple_tabs;
		close_tab.Sensitive = multiple_tabs;
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
		
		CurrentTab.history.ActivateCurrent();
		
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
		searchResults.Add (r);
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
	// Invoked when the search results panel losts focus
	//
	void LostFocus(object sender, FocusOutEventArgs a)
	{
		search_tree.Selection.UnselectAll();
	}

	//
	// Invoked when the user click on one of the search results
	//
	void ShowSearchResult (object sender, EventArgs a)
	{
		Gtk.TreeIter iter;
		Gtk.ITreeModel model;

		bool selected = search_tree.Selection.GetSelected (out model, out iter);
		if (!selected)
			return;

		TreePath p = model.GetPath (iter);
		if (p.Depth < 2)
			return;
		int i_0 = p.Indices [0];
		int i_1 = p.Indices [1];
		Result res = (Result) searchResults [i_0];
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
		//HelpSource.CssCode = null;
		Reload ();
		SettingsHandler.Save ();
	}
	void TextSmaller (object obj, EventArgs args)
	{
		SettingsHandler.Settings.preferred_font_size -= 10;
		//HelpSource.CssCode = null;
		Reload ();
		SettingsHandler.Save ();
	}
	void TextNormal (object obj, EventArgs args)
	{
		SettingsHandler.Settings.preferred_font_size = 100;
		//HelpSource.CssCode = null;
		Reload ();
		SettingsHandler.Save ();
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
			
			string res = Browser.GetHtml (url, null, browser.help_tree, out n);
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

	public void LoadUrl (string url)
	{
		if (url.StartsWith("#"))
		{
			// FIXME: This doesn't deal with whether anchor jumps should go in the history
			CurrentTab.html.JumpToAnchor(url.Substring(1));
			return;
		}
		
		Node node;
		
		/*
		 * The webkit library converts the url titles (N:, T:, etc.) to lower case (n:, t:, etc.)
		 * when clicking on a link. Therefore we need to convert them to upper case, since the
		 * monodoc backend only understands upper case titles (except for root:, afaik).
		 */
		string[] urlParts = url.Split (':');
		if (urlParts [0].Length == 1)
			url = urlParts [0].ToUpper () + url.Substring (1);
			
		Console.Error.WriteLine ("Trying: {0}", url);
		try {
			string res = Browser.GetHtml (url, null, help_tree, out node);
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

		// Comment out, thta routine is completely broken, someone needs to redo it
		// it crashes randomly
		//if (highlight_text != null)
		//text = DoHighlightText (text);

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
			string[] parts = matched_node.PublicUrl.Split('/', '#');
			if(matched_node.PublicUrl != null && matched_node.PublicUrl.StartsWith("ecma:")) {
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
		args.RetVal = true;
	}
	void on_print_activate (object sender, EventArgs e) 
	{
		 // desactivate css temporary
		 if ((capabilities & Capabilities.Css) != 0)
		 	HelpSource.use_css = false;
		 
		string html = GetHtml (CurrentUrl, CurrentTab.CurrentNode.tree.HelpSource, help_tree);

		// sending Html to be printed. 
		if (html != null)
			CurrentTab.html.Print (html);

		if ((capabilities & Capabilities.Css) != 0)
			HelpSource.use_css = true;
	}

	public static string GetHtml (string url, HelpSource help_source, RootTree help_tree)
	{
		Node _;
		return GetHtml (url, help_source, help_tree, out _);
	}

	public static string GetHtml (string url, HelpSource help_source, RootTree help_tree, out Node match)
	{
		match = null;
		string html_content = null;
		if (help_source != null)
			html_content = help_source.GetText (url, out match);
		if (html_content == null && help_tree != null) {
			html_content = help_tree.RenderUrl (url, out match);
			if (html_content != null && match != null && match.tree != null)
				help_source = match.tree.HelpSource;
		}

		if (html_content == null)
			return null;

		var html = new StringWriter ();
		html.Write ("<html>\n");
		html.Write ("  <head>\n");
		html.Write ("    <title>");
		html.Write (url);
		html.Write ("</title>\n");

		if (help_source != null && help_source.InlineCss != null) {
			html.Write ("    <style type=\"text/css\">\n");
			html.Write (help_source.InlineCss);
			html.Write ("    </style>\n");
		}
		if (help_source != null && help_source.InlineJavaScript != null) {
			html.Write ("    <script type=\"text/JavaScript\">\n");
			html.Write (help_source.InlineJavaScript);
			html.Write ("    </script>\n");
		}

		html.Write ("  </head>\n");
		html.Write ("  <body>\n");
		html.Write (html_content);
		html.Write ("  </body>\n");
		html.Write ("</html>");
		return html.ToString ();
	}
	
	void OnInheritedMembersActivate (object o, EventArgs args)
	{
		SettingsHandler.Settings.ShowInheritedMembers = showinheritedmembers.Active;
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
	// Invoked by Edit/Copy menu entry.
	//
	void OnCopyActivate (object sender, EventArgs a)
	{
		CurrentTab.html.Copy ();
	}

	//
	// Hooked up from Glade
	//
	void OnAboutActivate (object sender, EventArgs a)
	{
		// TODO: Use a standard Gtk about dialog instead (copy data from old glade file)
	}

	class Lookup {
		Window lookup;
		Entry entry;
		static Lookup LookupBox;
		Browser parent;
		
		Lookup (Browser browser)
		{
			var ui = new Builder();
			ui.AddFromFile("Lookup.glade");
			ui.Autoconnect (this);

			lookup = (Window) ui.GetObject ("lookup");
			entry = (Entry) ui.GetObject ("entry");

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

	//
	// Invoked by Close Tab menu entry.
	//
	public void OnCloseTab (object sender, EventArgs a)
	{
		CloseTab();
	}
}
}
