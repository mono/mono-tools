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
using System.Collections.Generic;
using System.Web.Services.Protocols;
using System.Xml;

using Mono.Options;

#if MACOS
using OSXIntegration.Framework;
#endif

namespace Monodoc {
class Driver {
	  
	public static string[] engines = {"WebKit", "Dummy"};
	  
	static int Main (string [] args)
	{
		string topic = null;
		bool remote_mode = false;
		
		string engine = engines[0];
		string basedir = null;
		string mergeConfigFile = null;
		bool show_help = false, show_version = false;
		bool show_gui = true;
		var sources = new List<string> ();
		
		int r = 0;

		var p = new OptionSet () {
			{ "docrootdir=",
				"Load documentation tree & sources from {DIR}.  The default directory is $libdir/monodoc.",
				v => {
					basedir = v != null && v.Length > 0 ? v : null;
					string md;
					if (basedir != null && !File.Exists (Path.Combine (basedir, "monodoc.xml"))) {
						Error ("Missing required file monodoc.xml.");
						r = 1;
					}
				} },
			{ "docdir=",
				"Load documentation from {DIR}.",
				v => sources.Add (v) },
			{ "engine=",
				"Specify which HTML rendering {ENGINE} to use:\n" + 
					"  " + string.Join ("\n  ", engines) + "\n" +
					"If the chosen engine isn't available " + 
					"(or you\nhaven't chosen one), monodoc will fallback to the next " +
					"one on the list until one is found.",
				v => engine = v },
			{ "html=",
				"Write to stdout the HTML documentation for {CREF}.",
				v => {
					show_gui = false;
					Node n;
					RootTree help_tree = LoadTree (basedir, sources);
					string res = help_tree.RenderUrl (v, out n);
					if (res != null)
						Console.WriteLine (res);
					else {
						Error ("Could not find topic: {0}", v);
						r = 1;
					}
				} },
			{ "make-index",
				"Generate a documentation index.  Requires write permission to $libdir/monodoc.",
				v => {
					show_gui = false;
					RootTree.MakeIndex ();
				} },
			{ "make-search-index",
				"Generate a search index.  Requires write permission to $libdir/monodoc.",
				v => {
					show_gui = false;
					RootTree.MakeSearchIndex () ;
				} },
			{ "merge-changes=",
				"Merge documentation changes found within {FILE} and target directories.",
				v => {
					show_gui = false;
					if (v != null)
						mergeConfigFile = v;
					else {
						Error ("Missing config file for --merge-changes.");
						r = 1;
					}
				} },
			{ "remote-mode",
				"Accept CREFs from stdin to display in the browser.\n" +
					"For MonoDevelop integration.",
				v => remote_mode = v != null },
			{ "about|version",
				"Write version information and exit.",
				v => show_version = v != null },
			{ "h|?|help",
				"Show this message and exit.",
				v => show_help = v != null },
		};

		List<string> topics = p.Parse (args);

		if (basedir == null)
			basedir = Directory.GetParent (System.Reflection.Assembly.GetExecutingAssembly ().Location).FullName;

		if (show_version) {
			Console.WriteLine ("Mono Documentation Browser");
			Version ver = Assembly.GetExecutingAssembly ().GetName ().Version;
			if (ver != null)
				Console.WriteLine (ver.ToString ());
			return r;
		}
		if (show_help) {
			Console.WriteLine ("usage: monodoc [--html TOPIC] [--make-index] [--make-search-index] [--merge-changes CHANGE_FILE TARGET_DIR+] [--about]  [--remote-mode] [--engine engine] [TOPIC]");
			p.WriteOptionDescriptions (Console.Out);
			return r;
		}

		/*if (mergeConfigFile != null) {
			ArrayList targetDirs = new ArrayList ();
			
			for (int i = 0; i < topics.Count; i++)
				targetDirs.Add (topics [i]);
			
			EditMerger e = new EditMerger (
				GlobalChangeset.LoadFromFile (mergeConfigFile),
				targetDirs
			);

			e.Merge ();
			return 0;
		}*/
		
		if (r != 0 || !show_gui)
			return r;

		SettingsHandler.CheckUpgrade ();
		
		Settings.RunningGUI = true;
		Application.Init ();
		Browser browser = new Browser (basedir, sources, engine);
		
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

	static void Error (string format, params object[] args)
	{
		Console.Error.Write("monodoc: ");
		Console.Error.WriteLine (format, args);
	}

	public static RootTree LoadTree (string basedir, IEnumerable<string> sourcedirs)
	{
		var root = RootTree.LoadTree (basedir);
		foreach (var s in sourcedirs)
			root.AddSource (s);
		return root;
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
	[Glade.Widget] public MenuItem print;
	[Glade.Widget] public MenuItem close_tab;
	public Notebook tabs_nb;
	public Tab CurrentTab;
	bool HoldCtrl;
	public string engine;

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
	ArrayList searchResults = new ArrayList (20);
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
		Gtk.TreeModel model;

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

	//
	// Invoked by Close Tab menu entry.
	//
	public void OnCloseTab (object sender, EventArgs a)
	{
		CloseTab();
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
			string res = Browser.GetHtml (url, n.tree.HelpSource, browser.help_tree);
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
			
			string url = n.PublicUrl;
			Node match;
			string s;

			if (n.tree.HelpSource != null)
			{
				//
				// Try the tree-based urls first.
				//
				
				s = Browser.GetHtml (url, n.tree.HelpSource, help_tree);
				if (s != null){
					((Browser)browser).Render (s, n, url);
					browser.CurrentTab.history.AppendHistory (new NodePageVisit (browser, n, url));
					return;
				}
			}
			
			//
			// Try the url resolver next
			//
			s = Browser.GetHtml (url, null, help_tree);
			if (s != null){
				((Browser)browser).Render (s, n, url);
				browser.CurrentTab.history.AppendHistory (new Browser.LinkPageVisit (browser, url));
				return;
			}

			((Browser)browser).Render ("<h1>Unhandled URL</h1>" + "<p>Functionality to view the resource <i>" + n.PublicUrl + "</i> is not available on your system or has not yet been implemented.</p>", null, url);
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
	// Where we render the contents
	public IHtmlRender html;
	
	public History history;
	private Browser browser;
	private Label titleLabel;
	public HBox TabLabel;
	
	public string Title {
		get { return titleLabel.Text; }
		set { titleLabel.Text = value; }
	}
	
	public Node CurrentNode;
	
	void FocusOut (object sender, FocusOutEventArgs args)
	{	
	}


	private static IHtmlRender LoadRenderer (string dll, Browser browser) {
		if (!System.IO.File.Exists (dll))
			return null;
		
		try {
			Assembly ass = Assembly.LoadFile (dll);		
			System.Type type = ass.GetType ("Monodoc." + ass.GetName ().Name, false, false);
			if (type == null)
				return null;
			return (IHtmlRender) Activator.CreateInstance (type, new object[1] { browser.help_tree });
		} catch (Exception ex) {
			Console.Error.WriteLine (ex);
		}
		return null;
	}
	
	public static IHtmlRender GetRenderer (string engine, Browser browser)
	{
		IHtmlRender renderer = LoadRenderer (System.IO.Path.Combine (AppDomain.CurrentDomain.BaseDirectory, engine + "HtmlRender.dll"), browser);
		if (renderer != null) {
			try {
				if (renderer.Initialize ()) {
					Console.WriteLine ("using " + renderer.Name);
					return renderer;
				}
			} catch (Exception ex) {
				Console.Error.WriteLine (ex);
			}
		}
		
		foreach (string backend in Driver.engines) {
			if (backend != engine) {
				renderer = LoadRenderer (System.IO.Path.Combine (AppDomain.CurrentDomain.BaseDirectory, backend + "HtmlRender.dll"), browser);
				if (renderer != null) {
					try {
						if (renderer.Initialize ()) {
							Console.WriteLine ("using " + renderer.Name);
							return renderer;
						}
					} catch (Exception ex) {
						Console.Error.WriteLine (ex);
					}
				}			
			}
		}
		
		return null;		
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

		html = GetRenderer (browser.engine, browser);
		if (html == null)
			throw new Exception ("Couldn't find html renderer!");

		browser.capabilities = html.Capabilities;

		HelpSource.FullHtml = false;
		HelpSource.UseWebdocCache = true;
		if ((html.Capabilities & Capabilities.Css) != 0)
			HelpSource.use_css = true;

		//Prepare Font for css (TODO: use GConf?)
		if ((html.Capabilities & Capabilities.Fonts) != 0 && SettingsHandler.Settings.preferred_font_size == 0) { 
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
		tabClose.Clicked += new EventHandler (browser.OnCloseTab);

		TabLabel.PackStart (titleLabel, true, true, 0);
		TabLabel.PackStart (tabClose, false, false, 2);
		
		// needed, otherwise even calling show_all on the notebook won't
		// make the hbox contents appear.
		TabLabel.ShowAll();
	
	}

	public static string GetNiceUrl (Node node) {
		if (node.Element.StartsWith("N:"))
			return node.Element;
		string name, full;
		int bk_pos = node.Caption.IndexOf (' ');
		// node from an overview
		if (bk_pos != -1) {
			name = node.Caption.Substring (0, bk_pos);
			full = node.Parent.Caption + "." + name.Replace ('.', '+');
			return "T:" + full;
		}
		// node that lists constructors, methods, fields, ...
		if ((node.Caption == "Constructors") || (node.Caption == "Fields") || (node.Caption == "Events") 
			|| (node.Caption == "Members") || (node.Caption == "Properties") || (node.Caption == "Methods")
			|| (node.Caption == "Operators")) {
			bk_pos = node.Parent.Caption.IndexOf (' ');
			name = node.Parent.Caption.Substring (0, bk_pos);
			full = node.Parent.Parent.Caption + "." + name.Replace ('.', '+');
			return "T:" + full + "/" + node.Element; 
		}
		int pr_pos = node.Caption.IndexOf ('(');
		// node from a constructor
		if (node.Parent.Element == "C") {
			name = node.Parent.Parent.Parent.Caption;
			int idx = node.PublicUrl.IndexOf ('/');
			return node.PublicUrl[idx+1] + ":" + name + "." + node.Caption.Replace ('.', '+');
		// node from a method with one signature, field, property, operator
		} else if (pr_pos == -1) {
			bk_pos = node.Parent.Parent.Caption.IndexOf (' ');
			name = node.Parent.Parent.Caption.Substring (0, bk_pos);
			full = node.Parent.Parent.Parent.Caption + "." + name.Replace ('.', '+');
			int idx = node.PublicUrl.IndexOf ('/');
			return node.PublicUrl[idx+1] + ":" + full + "." + node.Caption;
		// node from a method with several signatures
		} else {
			bk_pos = node.Parent.Parent.Parent.Caption.IndexOf (' ');
			name = node.Parent.Parent.Parent.Caption.Substring (0, bk_pos);
			full = node.Parent.Parent.Parent.Parent.Caption + "." + name.Replace ('.', '+');
			int idx = node.PublicUrl.IndexOf ('/');
			return node.PublicUrl[idx+1] + ":" + full + "." + node.Caption;
		}
	}
}
}
