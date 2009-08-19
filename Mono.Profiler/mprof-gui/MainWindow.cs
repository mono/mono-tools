using System;
using System.Diagnostics;
using System.Text;
using Gtk;
using Mono.Profiler;
using Mono.Profiler.Widgets;
using Mono.Unix;

namespace Mono.Profiler.Gui {

	public class MainWindow : Gtk.Window {	

		Gtk.ToggleAction logging_enabled_action;
		Gtk.Action save_action;
		Gtk.Action show_system_nodes_action;
		ProfilerProcess proc;
		ProfileView contents;
		string filename;
		
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
	
		void Refresh ()
		{
			Application.Invoke (delegate { 
				if (contents.LoadProfile (proc.LogFile)) {
					filename = proc.LogFile; 
					save_action.Sensitive = true;
					show_system_nodes_action.Sensitive = contents.SupportsFiltering;
				}
			});
		}

		void OnNewActivated (object sender, System.EventArgs e)
		{
			ProfileSetupDialog d = new ProfileSetupDialog (this);
			if (d.Run () == (int) ResponseType.Accept && !String.IsNullOrEmpty (d.AssemblyPath)) {
				string args = d.Args;
				proc = new ProfilerProcess (args, d.AssemblyPath);
				proc.Paused += delegate { Refresh (); };
				proc.Exited += delegate { Refresh (); };
				logging_enabled_action.Sensitive = true;
				logging_enabled_action.Active = d.StartEnabled;
				proc.Start ();
			}
			d.Destroy ();		
		}

		void OnOpenActivated (object sender, System.EventArgs e)
		{
			FileChooserDialog d = new FileChooserDialog ("Open Profile Log", this, FileChooserAction.Open, Stock.Cancel, ResponseType.Cancel, Stock.Open, ResponseType.Accept);
			if (d.Run () == (int) ResponseType.Accept && contents.LoadProfile (d.Filename)) {
				filename = d.Filename;
				logging_enabled_action.Active = false;
				logging_enabled_action.Sensitive = false;
				save_action.Sensitive = false;
				show_system_nodes_action.Sensitive = contents.SupportsFiltering;
			}
			d.Destroy ();
		}
	
		void OnQuitActivated (object sender, System.EventArgs e)
		{
			Application.Quit ();
		}
	
		void OnSaveAsActivated (object sender, System.EventArgs e)
		{
			FileChooserDialog d = new FileChooserDialog ("Save Profile Log", this, FileChooserAction.Save, Stock.Cancel, ResponseType.Cancel, Stock.Save, ResponseType.Accept);
			if (d.Run () == (int) ResponseType.Accept) {
				System.IO.File.Move (filename, d.Filename);
				filename = d.Filename;
				save_action.Sensitive = false;
				show_system_nodes_action.Sensitive = contents.SupportsFiltering;
			}
			d.Destroy ();
		}

		void OnLoggingActivated (object sender, System.EventArgs e)
		{
			ToggleAction ta = sender as ToggleAction;
			if (ta.Active)
				proc.Resume ();
			else
				proc.Pause ();
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
			"      <menuitem action='OpenAction'/>" +
			"      <menuitem action='SaveAsAction'/>" +
			"      <menuitem action='QuitAction'/>" +
			"    </menu>" +
			"    <menu action='RunMenu'>" +
			"      <menuitem action='LogEnabledAction'/>" +
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
				new ActionEntry ("OpenAction", Stock.Open, null, "<control>O", Catalog.GetString ("Open Existing Profile Log"), new EventHandler (OnOpenActivated)),
				new ActionEntry ("SaveAsAction", Stock.SaveAs, null, "<control>S", Catalog.GetString ("Save Profile Data"), new EventHandler (OnSaveAsActivated)),
				new ActionEntry ("QuitAction", Stock.Quit, null, "<control>Q", Catalog.GetString ("Quit Profiler"), new EventHandler (OnQuitActivated)),
				new ActionEntry ("RunMenu", null, Catalog.GetString ("_Run"), null, null, null),
				new ActionEntry ("ViewMenu", null, Catalog.GetString ("_View"), null, null, null),
			};

			ToggleActionEntry[] toggle_actions = new ToggleActionEntry[] {
				new ToggleActionEntry ("ShowSystemNodesAction", null, Catalog.GetString ("_Show system nodes"), null, Catalog.GetString ("Shows internal nodes of system library method invocations"), new EventHandler (OnShowSystemNodesActivated), false),
				new ToggleActionEntry ("LogEnabledAction", null, Catalog.GetString ("_Logging enabled"), null, Catalog.GetString ("Profile logging enabled"), new EventHandler (OnLoggingActivated), true),
			};
	    		ActionGroup group = new ActionGroup ("group");
			group.Add (actions);
			group.Add (toggle_actions);
	    		UIManager uim = new UIManager ();
 
	    		uim.InsertActionGroup (group, (int) uim.NewMergeId ());
	    		uim.AddUiFromString (ui_info);
			AddAccelGroup (uim.AccelGroup);
			logging_enabled_action = group.GetAction ("LogEnabledAction") as ToggleAction;
			save_action = group.GetAction ("SaveAsAction");
			save_action.Sensitive = false;
			show_system_nodes_action = group.GetAction ("ShowSystemNodesAction");
 			return uim.GetWidget ("/Menubar");
		}
	}
}
