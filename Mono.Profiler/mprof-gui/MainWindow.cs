using System;
using System.Diagnostics;
using System.Text;
using Gtk;
using Mono.Profiler;
using Mono.Profiler.Widgets;
using Mono.Unix;

namespace Mono.Profiler.Gui {

	public class MainWindow : Gtk.Window {	

		ProfileView contents;

		public MainWindow () : base (Gtk.WindowType.Toplevel)
		{
			DefaultSize = new Gdk.Size (800, 600);
			Title = Catalog.GetString ("Mono Visual Profiler");
			Box box = new Gtk.VBox (false, 6);
			box.PackStart (BuildMenubar (), false, false, 0);
			contents = new ProfileView ();
			box.PackStart (contents, true, true, 0);
			box.ShowAll ();
			Add (box);
		}
	
		protected override bool OnDeleteEvent (Gdk.Event ev)
		{
			Application.Quit ();
			return true;
		}
	
		void OnNewActivated (object sender, System.EventArgs e)
		{
			ProfileSetupDialog d = new ProfileSetupDialog (this);
			if (d.Run () == (int) ResponseType.Accept && !String.IsNullOrEmpty (d.AssemblyPath)) {
				ProfileType type = d.ProfileType;
				string log_file = System.IO.Path.GetTempFileName ();
				Process proc = new Process ();
				proc.StartInfo.FileName = "mono";
				string flags = d.ProfileType == ProfileType.Calls ? "calls" : "alloc";
				proc.StartInfo.Arguments = "--profile=logging:" + flags + ",o=" + log_file + " " + d.AssemblyPath;
				proc.EnableRaisingEvents = true;
				proc.Exited += delegate {
					Application.Invoke (delegate { contents.LoadProfile (log_file, type); });
				};
				proc.Start ();
			}
			d.Destroy ();		
		}

		void OnQuitActivated (object sender, System.EventArgs e)
		{
			Application.Quit ();
		}
	
		void OnSaveAsActivated (object sender, System.EventArgs e)
		{
		}

		void OnShowSystemNodesActivated (object sender, System.EventArgs e)
		{
			ToggleAction ta = sender as ToggleAction;
			contents.Options.ShowSystemNodes = ta.Active;
		}
		
		const string ui_info = 
			"<ui>" +
			"  <menubar name='Menubar'>" +
			"    <menu action='ProfileMenu'>" +
			"      <menuitem action='NewAction'/>" +
			"      <menuitem action='SaveAsAction'/>" +
			"      <menuitem action='QuitAction'/>" +
			"    </menu>" +
			"    <menu action='ViewMenu'>" +
			"      <menuitem action='ShowSystemNodesAction'/>" +
			"    </menu>" +
			"  </menubar>" +
			"</ui>";

		Widget BuildMenubar ()
		{
			ActionEntry[] actions = new ActionEntry[] {
				new ActionEntry ("ProfileMenu", null, Catalog.GetString ("_Profile"), null, null, null),
				new ActionEntry ("NewAction", Stock.New, null, "<control>N", Catalog.GetString ("Create New Profile"), new EventHandler (OnNewActivated)),
				new ActionEntry ("SaveAsAction", Stock.SaveAs, null, "<control>S", Catalog.GetString ("Save Profile Data"), new EventHandler (OnSaveAsActivated)),
				new ActionEntry ("QuitAction", Stock.Quit, null, "<control>Q", Catalog.GetString ("Quit Profiler"), new EventHandler (OnQuitActivated)),
				new ActionEntry ("ViewMenu", null, Catalog.GetString ("_View"), null, null, null),
			};

			ToggleActionEntry[] toggle_actions = new ToggleActionEntry[] {
				new ToggleActionEntry ("ShowSystemNodesAction", null, Catalog.GetString ("_Show system nodes"), null, Catalog.GetString ("Shows internal nodes of system library method invocations"), new EventHandler (OnShowSystemNodesActivated), false)
			};
            ActionGroup group = new ActionGroup ("group");
			group.Add (actions);
			group.Add (toggle_actions);
            UIManager uim = new UIManager ();
 
            uim.InsertActionGroup (group, (int) uim.NewMergeId ());
            uim.AddUiFromString (ui_info);
			AddAccelGroup (uim.AccelGroup);
 			return uim.GetWidget ("/Menubar");
		}
	}
}
