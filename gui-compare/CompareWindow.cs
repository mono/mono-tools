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

public class CompareWindow : Window
{

		
		public CompareWindow ()
			: base ("Mono GuiCompare")
		{
			SetDefaultSize (500, 400);

			vbox = new Gtk.VBox ();
			tree = new Gtk.TreeView ();

			treeStore = new Gtk.TreeStore (typeof (string), typeof (Gdk.Pixbuf), typeof (Gdk.Pixbuf),
						       typeof (Gdk.Pixbuf), typeof (string),
						       typeof (Gdk.Pixbuf), typeof (string),
						       typeof (Gdk.Pixbuf), typeof (string));

			tree.Model = treeStore;

			// Create a column for the node name
			Gtk.TreeViewColumn nameColumn = new Gtk.TreeViewColumn ();
			nameColumn.Title = "Name";
			nameColumn.Resizable = true;

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

			// Create a column for the status counts
			Gtk.TreeViewColumn countsColumn = new Gtk.TreeViewColumn ();
			countsColumn.Title = "Counts";
			countsColumn.Resizable = true;

			Gtk.CellRendererPixbuf missingPixbufCell = new Gtk.CellRendererPixbuf ();
			Gtk.CellRendererText missingTextCell = new Gtk.CellRendererText ();
			Gtk.CellRendererPixbuf extraPixbufCell = new Gtk.CellRendererPixbuf ();
			Gtk.CellRendererText extraTextCell = new Gtk.CellRendererText ();
			Gtk.CellRendererPixbuf errorPixbufCell = new Gtk.CellRendererPixbuf ();
			Gtk.CellRendererText errorTextCell = new Gtk.CellRendererText ();

			countsColumn.PackStart (missingPixbufCell, false);
			countsColumn.PackStart (missingTextCell, false);
			countsColumn.PackStart (extraPixbufCell, false);
			countsColumn.PackStart (extraTextCell, false);
			countsColumn.PackStart (errorPixbufCell, false);
			countsColumn.PackStart (errorTextCell, false);

			tree.AppendColumn (countsColumn);

			countsColumn.AddAttribute (missingPixbufCell, "pixbuf", 3);
			countsColumn.AddAttribute (missingTextCell, "text", 4);
			countsColumn.AddAttribute (extraPixbufCell, "pixbuf", 5);
			countsColumn.AddAttribute (extraTextCell, "text", 6);
			countsColumn.AddAttribute (errorPixbufCell, "pixbuf", 7);
			countsColumn.AddAttribute (errorTextCell, "text", 8);

			
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