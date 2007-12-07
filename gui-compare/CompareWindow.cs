//
//
// Authors:
//   Chris Toshok
//
using Gtk;
using System;
using System.IO;
using System.Reflection;

using IOPath = System.IO.Path;

namespace GuiCompare {
	public class CompareWindow : Window
	{
		static Gdk.Pixbuf classPixbuf, delegatePixbuf, enumPixbuf;
		static Gdk.Pixbuf eventPixbuf, fieldPixbuf, interfacePixbuf;
		static Gdk.Pixbuf methodPixbuf, namespacePixbuf, propertyPixbuf;
		static Gdk.Pixbuf attributePixbuf, structPixbuf, assemblyPixbuf;

		static Gdk.Pixbuf okPixbuf, errorPixbuf;
		static Gdk.Pixbuf missingPixbuf, todoPixbuf, extraPixbuf;

		static CompareWindow ()
		{
			Assembly ta = typeof (CompareWindow).Assembly;

			classPixbuf = new Gdk.Pixbuf (ta, "c.gif");
			delegatePixbuf = new Gdk.Pixbuf (ta, "d.gif");
			enumPixbuf = new Gdk.Pixbuf (ta, "en.gif");
			eventPixbuf = new Gdk.Pixbuf (ta, "e.gif");
			fieldPixbuf = new Gdk.Pixbuf (ta, "f.gif");
			interfacePixbuf = new Gdk.Pixbuf (ta, "i.gif");
			methodPixbuf = new Gdk.Pixbuf (ta, "m.gif");
			namespacePixbuf = new Gdk.Pixbuf (ta, "n.gif");
			propertyPixbuf = new Gdk.Pixbuf (ta, "p.gif");
			attributePixbuf = new Gdk.Pixbuf (ta, "r.gif");
			structPixbuf = new Gdk.Pixbuf (ta, "s.gif");
			assemblyPixbuf = new Gdk.Pixbuf (ta, "y.gif");
	
			okPixbuf = new Gdk.Pixbuf (ta, "sc.gif");
			errorPixbuf = new Gdk.Pixbuf (ta, "se.gif");
			missingPixbuf = new Gdk.Pixbuf (ta, "sm.gif");
			todoPixbuf = new Gdk.Pixbuf (ta, "st.gif");
			extraPixbuf = new Gdk.Pixbuf (ta, "sx.gif");
		}
		
		public CompareWindow ()
			: base ("Mono GuiCompare")
		{
			SetDefaultSize (500, 400);

			vbox = new Gtk.VBox ();
			tree = new Gtk.TreeView ();

			treeStore = new Gtk.TreeStore (typeof (string), typeof (Gdk.Pixbuf), typeof (Gdk.Pixbuf));

			tree.Model = treeStore;

			// Create a column for the node name
			Gtk.TreeViewColumn nameColumn = new Gtk.TreeViewColumn ();
			nameColumn.Title = "Name";
 
			Gtk.CellRendererText nameCell = new Gtk.CellRendererText ();
			Gtk.CellRendererPixbuf typeCell = new Gtk.CellRendererPixbuf ();
			Gtk.CellRendererPixbuf statusCell = new Gtk.CellRendererPixbuf ();

			nameColumn.PackStart (statusCell, false);
			nameColumn.PackStart (typeCell, false);
			nameColumn.PackStart (nameCell, true);

			tree.AppendColumn (nameColumn);

			nameColumn.AddAttribute (nameCell, "text", 0);
			nameColumn.AddAttribute (typeCell, "pixbuf", 1);
			nameColumn.AddAttribute (statusCell, "pixbuf", 2);

			scroll = new Gtk.ScrolledWindow ();

			scroll.HscrollbarPolicy = scroll.VscrollbarPolicy = PolicyType.Automatic;
			scroll.Add (tree);

			vbox.PackStart (scroll, true, true, 0);

			status = new Gtk.Statusbar ();

			vbox.PackEnd (status, false, false, 0);

			progressbar = new Gtk.ProgressBar ();

			status.PackEnd (progressbar, false, false, 0);

			Add (vbox);
		}

		string masterinfoDirectory = "./masterinfos";

		public void SetAssemblyPath (string path)
		{
			assemblyPath = path;
			PerformCompare ();
		}

		public void PerformCompare ()
		{
			string masterinfoPath = IOPath.Combine (masterinfoDirectory, IOPath.GetFileName (IOPath.ChangeExtension (assemblyPath, "xml")));

			// clear our existing content
			if (context != null)
				context.StopCompare ();

			// now generate new content asynchronously
			context = new CompareContext (masterinfoPath, assemblyPath);
			context.ProgressChanged += delegate (object sender, CompareProgressChangedEventArgs e) {
				/* update our progress bar */
				status.Pop (0);
				status.Push (0, e.Message);
				progressbar.Fraction = e.Progress;
			};
			context.Error += delegate (object sender, CompareErrorEventArgs e) {
				Console.WriteLine ("ERROR: {0}", e.Message);
				MessageDialog md = new MessageDialog (this, 0, MessageType.Error, ButtonsType.Ok, false,
								      e.Message);
				md.Response += delegate (object s, ResponseArgs ra) {
					md.Hide ();
				};
				md.Show();
				status.Pop (0);
				status.Push (0, String.Format ("Comparison failed at {0}", DateTime.Now));
				progressbar.Fraction = 0.0;
			};
			context.Finished += delegate (object sender, EventArgs e) {
				status.Pop (0);
				status.Push (0, String.Format ("Comparison completed at {0}", DateTime.Now));
				Title = IOPath.GetFileName (assemblyPath);
				PopulateTreeFromComparison (context.Comparison);
				progressbar.Fraction = 0.0;
			};
			context.Compare ();
		}

		Gdk.Pixbuf TypePixbufFromComparisonNode (ComparisonNode node)
		{
			switch (node.type) {
			case ComparisonNodeType.Assembly: return assemblyPixbuf;
			case ComparisonNodeType.Namespace: return namespacePixbuf;
			case ComparisonNodeType.Attribute: return attributePixbuf;
			case ComparisonNodeType.Class: return classPixbuf;
			case ComparisonNodeType.Struct: return structPixbuf;
			case ComparisonNodeType.Enum: return enumPixbuf;
			case ComparisonNodeType.Method: return methodPixbuf;
			case ComparisonNodeType.Property: return propertyPixbuf;
			case ComparisonNodeType.Field: return fieldPixbuf;
			}
			return null;
		}

		Gdk.Pixbuf StatusPixbufFromComparisonNode (ComparisonNode node)
		{
			switch (node.status) {
			case ComparisonStatus.None: return okPixbuf;
			case ComparisonStatus.Missing: return missingPixbuf;
			case ComparisonStatus.Extra: return extraPixbuf;
			case ComparisonStatus.Todo: return todoPixbuf;
			case ComparisonStatus.Error: return errorPixbuf;
			}
			return null;
		}

		void PopulateTreeFromComparison (ComparisonNode root)
		{
			Gtk.TreeIter iter = treeStore.AppendValues (root.name,
								    TypePixbufFromComparisonNode (root),
								    StatusPixbufFromComparisonNode (root));
			Gtk.TreePath path = treeStore.GetPath (iter);

			foreach (ComparisonNode n in root.children)
				PopulateTreeFromComparison (iter, n);

			tree.ExpandRow (path, false);
		}

		void PopulateTreeFromComparison (Gtk.TreeIter iter, ComparisonNode node)
		{
			Gtk.TreeIter citer = treeStore.AppendValues (iter,
								     node.name,
								     TypePixbufFromComparisonNode (node),
								     StatusPixbufFromComparisonNode (node));
			foreach (ComparisonNode n in node.children)
				PopulateTreeFromComparison (citer, n);
		}

		string assemblyPath;
		CompareContext context;
		Gtk.VBox vbox;
		Gtk.TreeView tree;
		Gtk.TreeStore treeStore;
		Gtk.Statusbar status;
		Gtk.ScrolledWindow scroll;
		Gtk.ProgressBar progressbar;
	}
}
