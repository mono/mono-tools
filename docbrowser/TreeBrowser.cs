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
}
