// Window.cs - Toplevel Window Class
//
// Author: Mike Kestner <mkestner@novell.com>
//
// Copyright (c) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person 
// obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, 
// publish, distribute, sublicense, and/or sell copies of the Software, 
// and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be 
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN 
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN 
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
// SOFTWARE.


namespace IlContrast {

	using Gdk;
	using Gtk;
	using System;
	using System.Threading;
	using System.Xml;
	using System.Xml.Xsl;
	using Mono.AssemblyCompare;
	using Mono.AssemblyInfo;

	public class Window : Gtk.Window {

		Box main_vbox;
		Statusbar statusbar;
		Gecko.WebControl browser;
		bool first_show = false;
		Thread worker;
		ComparisonInfo info;

		public Window (ComparisonInfo info) : base ("ilContrast Assembly Comparison Tool") 
		{
			DefaultSize = new Size (450, 450);

			browser = new Gecko.WebControl ();
			main_vbox = new VBox (false, 0);
			AddActionUI ();
			main_vbox.PackStart (browser, true, true, 0);
			statusbar = new Statusbar ();
			main_vbox.PackStart (statusbar, false, false, 0);
			Add (main_vbox);
			main_vbox.ShowAll ();
			first_show = true;
			this.info = info;
		}

		protected override void OnShown ()
		{
			base.OnShown ();
			if (first_show)
				Refresh ();
			first_show = false;
		}

		protected override bool OnDeleteEvent (Gdk.Event ev)
		{
			Gtk.Application.Quit ();
			return true;
		}

		void Refresh ()
		{
			if (info == null) {
				info = RequestComparisonInfo ();
				if (info == null)
					return;
			}

			worker = new Thread (new ThreadStart (GenerateHtmlTarget));
			worker.Start ();
		}

		ComparisonInfo RequestComparisonInfo ()
		{
			uint cid = statusbar.GetContextId ("assemblies");
			statusbar.Pop (cid);
			statusbar.Push (cid, "Choosing Base Assembly");
			FileChooserDialog d = new FileChooserDialog ("Choose Base Assembly", this, FileChooserAction.Open, Stock.Cancel, ResponseType.Cancel, Stock.Open, ResponseType.Accept);
			FileFilter filter = new FileFilter ();
			filter.AddPattern ("*.dll");
			d.Filter = filter;
			if ((ResponseType) d.Run () == ResponseType.Cancel) {
				d.Destroy ();
				return null;
			}
			d.Hide ();
			string base_path = d.Filename;
			statusbar.Pop (cid);
			statusbar.Push (cid, "Choosing Target Assembly");
			d.Title = "Choose Target Assembly";
			if ((ResponseType) d.Run () == ResponseType.Cancel) {
				d.Destroy ();
				return null;
			}
			string target_path = d.Filename;
			d.Hide ();
			d.Destroy ();
			return new ComparisonInfo (base_path, target_path);
		}

		string Status {
			set {
				uint cid = statusbar.GetContextId ("status");
				statusbar.Pop (cid);
				statusbar.Push (cid, value);
			}
		}

		void LoadUrl (string url)
		{
			browser.LoadUrl (url);
		}

		void GenerateHtmlTarget ()
		{
			Gtk.Application.Invoke (delegate { Status = "Loading Base Assembly Information"; });
			XmlDocument base_info = LoadAssemblyInfo (info.BaseAssemblyPath);

			Gtk.Application.Invoke (delegate { Status = "Loading Target Assembly Information"; });
			XmlDocument target_info = LoadAssemblyInfo (info.TargetAssemblyPath);

			Gtk.Application.Invoke (delegate { Status = "Comparing Assembly Information"; });
			XmlDocument diff_doc = CompareAssemblies (base_info, target_info);

			Gtk.Application.Invoke (delegate { Status = "Formatting Comparison for Display"; });
			XslTransform xsl = new XslTransform ();
			xsl.Load (new XmlTextReader (System.Reflection.Assembly.GetCallingAssembly ().GetManifestResourceStream ("mono-api.xsl")));

			using(System.IO.FileStream stream = new System.IO.FileStream(info.ResultPath, System.IO.FileMode.Create, System.IO.FileAccess.Write)) {
				System.IO.StreamWriter writer = new System.IO.StreamWriter (stream);
				WriteHtmlHeader (writer);
				xsl.Transform (diff_doc, null, stream);
				WriteHtmlFooter (writer);
				writer.Close ();
			}

			Gtk.Application.Invoke (delegate { LoadUrl (info.ResultPath); Status = info.BaseAssemblyPath + " -> " + info.TargetAssemblyPath; });
			worker = null;
		}

		void WriteHtmlFooter (System.IO.StreamWriter writer)
		{
			writer.WriteLine ("  </body>");
			writer.WriteLine ("</html>");
		}

		void WriteHtmlHeader (System.IO.StreamWriter writer)
		{
			writer.WriteLine ("<html> ");

  			writer.WriteLine ("  <head>");
    			writer.WriteLine ("    <title>Class Status Page</title>");
    			writer.WriteLine ("    <LINK rel=\"stylesheet\" type=\"text/css\" href=\"cm/cormissing.css\">");
    			writer.WriteLine ("    <script src=\"cm/cormissing.js\"></script>");
  			writer.WriteLine ("  </head>");
  			writer.WriteLine ("  <body>");
		}

		XmlDocument LoadAssemblyInfo (string path)
		{
			AssemblyCollection collection = new AssemblyCollection ();
			collection.Add (path);
			XmlDocument result = new XmlDocument ();
			collection.Document = result;
			collection.DoOutput ();
			return result;
		}

		XmlDocument CompareAssemblies (XmlDocument base_doc, XmlDocument updated_doc)
		{
			XMLAssembly base_assm = new XMLAssembly ();
			base_assm.LoadData (base_doc.SelectSingleNode ("/assemblies/assembly"));
			XMLAssembly updated_assm = new XMLAssembly ();
			updated_assm.LoadData (updated_doc.SelectSingleNode ("/assemblies/assembly"));
			return base_assm.CompareAndGetDocument (updated_assm);
		}

		void QuitActivated (object o, EventArgs args)
		{
			Gtk.Application.Quit ();
		}
		
		void AboutActivated (object o, EventArgs args)
		{
			Dialog about = new Dialog ("About ilContrast", this, DialogFlags.DestroyWithParent, Stock.Close, ResponseType.Accept);
			about.VBox.PackStart (new Gtk.Image (new Gdk.Pixbuf (null, "ilcontrast.png", 64, 64)), false, false, 6);
			Label label = new Label ("");
			label.Markup = "<b>ilContrast " + Global.VersionNumber + "</b>";
			about.VBox.PackStart (label, false, false, 6);
			about.VBox.PackStart (new Label ("Assembly Comparison Tool"), false, false, 6);
			about.VBox.PackStart (new Label ("Copyright (c) 2007 Novell, Inc."), false, false, 6);
			about.VBox.ShowAll ();
			about.Run ();
			about.Destroy ();
		}

		const string uiInfo =
			"<ui>" +
			"  <menubar name='MenuBar'>" +
			"    <menu action='FileMenu'>" +
			"      <menuitem action='Quit'/>" +
			"    </menu>" +
			"    <menu action='HelpMenu'>" +
			"      <menuitem action='About'/>" +
			"    </menu>" +
			"  </menubar>" +
			"  <toolbar  name='ToolBar'>" +
			"    <toolitem action='Quit'/>" +
			"  </toolbar>" +
			"</ui>";

		void AddActionUI ()
		{
			ActionEntry[] actions = new ActionEntry[]
			{
				new ActionEntry ("FileMenu", null, "_File", null, null, null),
				new ActionEntry ("Quit", Stock.Quit, "_Quit", "<control>Q", "Quit", new EventHandler (QuitActivated)),
				new ActionEntry ("HelpMenu", null, "_Help", null, null, null),
				new ActionEntry ("About", null, "_About IlContrast...", "<control>A", "About IlContrast", new EventHandler (AboutActivated)),
			};

			ActionGroup group = new ActionGroup ("group");
			group.Add (actions);

			UIManager uim = new UIManager ();
			uim.InsertActionGroup (group, (int) uim.NewMergeId ());
			uim.AddUiFromString (uiInfo);
			main_vbox.PackStart (uim.GetWidget ("/MenuBar"), false, true, 0);
			//main_vbox.PackStart (uim.GetWidget ("/ToolBar"), false, true, 0);
		}
	}
}
