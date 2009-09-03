// Copyright (c) 2009  Novell, Inc.  <http://www.novell.com>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


using System;
using Mono.Profiler;
using Mono.Profiler.Widgets;
using Mono.Unix;

namespace Mono.Profiler.Gui {

	public class MainWindow : Gtk.Window {	

		int max_history_count = 5;
		Gtk.Action save_action;
		Gtk.Action show_system_nodes_action;
		Gtk.ActionGroup group;
		Gtk.ToggleAction logging_enabled_action;
		Gtk.Box content_area;
		Gtk.Widget view;
		History history;
		LogInfo log_info;
		ProfilerProcess proc;

		public MainWindow () : base (Catalog.GetString ("Mono Visual Profiler"))
		{
			history = History.Load ();
			history.LogFiles.Changed += UpdateRecentLogs;
			history.Configs.Changed += UpdateRepeatSessions;
			DefaultSize = new Gdk.Size (800, 600);
			Gtk.Box box = new Gtk.VBox (false, 0);
			Gtk.UIManager uim = BuildUIManager ();
 			box.PackStart (uim.GetWidget ("/Menubar"), false, false, 0);
 			box.PackStart (uim.GetWidget ("/Toolbar"), false, false, 0);
			UpdateRecentLogs (null, null);
			UpdateRepeatSessions (null, null);
			content_area = new Gtk.VBox (false, 0);
			content_area.Show ();
			box.PackStart (content_area, true, true, 0);
			StartPage start_page = new StartPage (history);
			start_page.Activated += OnStartPageActivated;
			start_page.Show ();
			View = start_page;
			box.ShowAll ();
			Add (box);
		}

		Gtk.Widget View {
			get { return view; }
			set {
				if (view != null)
					content_area.Remove (view);
				view = value;
				content_area.PackStart (view, true, true, 0);
			}
		}

		protected override bool OnDeleteEvent (Gdk.Event ev)
		{
			Shutdown ();
			return true;
		}
	
		protected override void OnShown ()
		{
			base.OnShown ();
			View.GrabFocus ();
		}

		void UpdateRecentLogs (object o, EventArgs args)
		{
			for (int i = 0; i < max_history_count; i++) {
				Gtk.Action action = group.GetAction ("RecentLogs" + i);
				if (i < history.LogFiles.Count) {
					LogInfo info = history.LogFiles [i];
					action.Label = "_" + i + ": " + info.Caption;
					action.Tooltip = info.Filename;
					action.Visible = true;
				} else
					action.Visible = false;
			}
		}

		void UpdateRepeatSessions (object o, EventArgs args)
		{
			for (int i = 0; i < max_history_count; i++) {
				Gtk.Action action = group.GetAction ("RepeatSession" + i);
				if (i < history.Configs.Count) {
					ProfileConfiguration config = history.Configs [i];
					action.Label = "_" + i + ": " + config.ToString ();
					action.Visible = true;
				} else
					action.Visible = false;
			}
		}

		void Refresh (ProfileView contents)
		{
			Gtk.Application.Invoke (delegate { 
				if (contents.LoadProfile (proc.LogFile)) {
					if (log_info == null || log_info.Filename != proc.LogFile) {
						log_info = new LogInfo (proc.LogFile, log_info == null ? null : log_info.Detail);
						history.LogFiles.Add (log_info);
					}
					save_action.Sensitive = true;
					show_system_nodes_action.Sensitive = contents.SupportsFiltering;
				}
			});
		}

		void StartProfile (ProfileConfiguration config)
		{
			ProfileView view = new ProfileView ();
			view.Show ();
			View = view;
			logging_enabled_action.Visible = true;
			logging_enabled_action.Active = config.StartEnabled;
			proc = new ProfilerProcess (config);
			proc.Paused += delegate { Refresh (view); };
			proc.Exited += delegate { Refresh (view); logging_enabled_action.Visible = false; };
			proc.Start ();
			log_info = new LogInfo (proc.LogFile, config.ToString ());
			history.LogFiles.Prepend (log_info);
			history.Configs.Prepend (config);
		}

		void Shutdown ()
		{
			history.Save ();
			Gtk.Application.Quit ();
		}

		void OnNewActivated (object sender, System.EventArgs e)
		{
			ProfileSetupDialog d = new ProfileSetupDialog (this);
			if (d.Run () == (int) Gtk.ResponseType.Accept)
				StartProfile (d.Config);
			d.Destroy ();		
		}

		static object[] chooser_button_params = new object[] { Gtk.Stock.Cancel, Gtk.ResponseType.Cancel, Gtk.Stock.Open, Gtk.ResponseType.Accept };

		void OpenProfile (string filename)
		{
			ProfileView view = new ProfileView ();
			if (view.LoadProfile (filename)) {
				view.Show ();
				View = view;
				logging_enabled_action.Visible = false;
				save_action.Sensitive = true;
				show_system_nodes_action.Sensitive = view.SupportsFiltering;
				log_info = null;
				foreach (LogInfo info in history.LogFiles) {
					if (info.Filename == filename) {
						log_info = info;
						break;
					}
				}
				if (log_info == null)
					log_info = new LogInfo (filename, null);
				history.LogFiles.Prepend (log_info);
			}
		}

		void OnOpenActivated (object sender, System.EventArgs e)
		{
			Gtk.FileChooserDialog d = new Gtk.FileChooserDialog ("Open Profile Log", this, Gtk.FileChooserAction.Open, chooser_button_params);
			if (d.Run () == (int) Gtk.ResponseType.Accept)
				OpenProfile (d.Filename);
			d.Destroy ();
		}
	
		void OnQuitActivated (object sender, System.EventArgs e)
		{
			Shutdown ();
		}

		void OnRecentLogsActivated (object o, EventArgs args)
		{
			Gtk.Action action = o as Gtk.Action;
			OpenProfile (action.Tooltip);
		}

		void OnRepeatSessionActivated (object o, EventArgs args)
		{
			Gtk.Action action = o as Gtk.Action;
			int idx = Int32.Parse (action.Name.Substring (13));
			ProfileConfiguration config = history.Configs [idx];
			StartProfile (config);
		}

		void OnSaveAsActivated (object sender, System.EventArgs e)
		{
			Gtk.FileChooserDialog d = new Gtk.FileChooserDialog ("Save Profile Log", this, Gtk.FileChooserAction.Save, chooser_button_params);
			if (d.Run () == (int) Gtk.ResponseType.Accept) {
				System.IO.File.Copy (log_info.Filename, d.Filename);
				log_info.Filename = d.Filename;
				save_action.Sensitive = false;
			}
			d.Destroy ();
		}

		void OnStartPageActivated (object o, StartEventArgs args)
		{
			switch (args.Type) {
			case StartEventType.Create:
				OnNewActivated (null, null);
				break;
			case StartEventType.Open:
				if (args.Detail == null)
					OnOpenActivated (null, null);
				else
					OpenProfile (args.Detail);
				break;
			case StartEventType.Repeat:
				StartProfile (args.Config);
				break;
			default:
				throw new NotSupportedException ();
			}
		}

		void OnLoggingActivated (object sender, System.EventArgs e)
		{
			if (proc == null)
				return;
			Gtk.ToggleAction ta = sender as Gtk.ToggleAction;
			if (ta.Active)
				proc.Resume ();
			else
				proc.Pause ();
		}
		
		void OnShowSystemNodesActivated (object sender, System.EventArgs e)
		{
			Gtk.ToggleAction ta = sender as Gtk.ToggleAction;
			(View as ProfileView).Options.ShowSystemNodes = ta.Active;
		}
		
		const string ui_info = 
			"<ui>" +
			"  <menubar name='Menubar'>" +
			"    <menu action='ProfileMenu'>" +
			"      <menuitem action='NewAction'/>" +
			"      <menuitem action='OpenAction'/>" +
			"      <menuitem action='SaveAsAction'/>" +
			"      <separator/>" +
			"      <menu action='RecentLogsMenu'>" +
			"        <menuitem action='RecentLogs0'/>" +
			"        <menuitem action='RecentLogs1'/>" +
			"        <menuitem action='RecentLogs2'/>" +
			"        <menuitem action='RecentLogs3'/>" +
			"        <menuitem action='RecentLogs4'/>" +
			"      </menu>" +
			"      <menu action='RepeatSessionsMenu'>" +
			"        <menuitem action='RepeatSession0'/>" +
			"        <menuitem action='RepeatSession1'/>" +
			"        <menuitem action='RepeatSession2'/>" +
			"        <menuitem action='RepeatSession3'/>" +
			"        <menuitem action='RepeatSession4'/>" +
			"      </menu>" +
			"      <separator/>" +
			"      <menuitem action='QuitAction'/>" +
			"    </menu>" +
			"    <menu action='RunMenu'>" +
			"      <menuitem action='LogEnabledAction'/>" +
			"    </menu>" +
			"    <menu action='ViewMenu'>" +
			"      <menuitem action='ShowSystemNodesAction'/>" +
			"    </menu>" +
			"  </menubar>" +
			"  <toolbar name='Toolbar'>" +
			"    <toolitem action='NewAction'/>" +
			"    <toolitem action='OpenAction'/>" +
			"    <toolitem action='SaveAsAction'/>" +
			"  </toolbar>" +
			"</ui>";

		Gtk.UIManager BuildUIManager ()
		{
			Gtk.ActionEntry[] actions = new Gtk.ActionEntry[] {
				new Gtk.ActionEntry ("ProfileMenu", null, Catalog.GetString ("_Profile"), null, null, null),
				new Gtk.ActionEntry ("NewAction", Gtk.Stock.New, null, "<control>N", Catalog.GetString ("Create New Profile"), new EventHandler (OnNewActivated)),
				new Gtk.ActionEntry ("OpenAction", Gtk.Stock.Open, null, "<control>O", Catalog.GetString ("Open Existing Profile Log"), new EventHandler (OnOpenActivated)),
				new Gtk.ActionEntry ("SaveAsAction", Gtk.Stock.SaveAs, null, "<control>S", Catalog.GetString ("Save Profile Data"), new EventHandler (OnSaveAsActivated)),
				new Gtk.ActionEntry ("RecentLogsMenu", null, Catalog.GetString ("Recent _Logs"), null, null, null),
				new Gtk.ActionEntry ("RecentLogs0", null, "_0", null, null, new EventHandler (OnRecentLogsActivated)),
				new Gtk.ActionEntry ("RecentLogs1", null, "_1", null, null, new EventHandler (OnRecentLogsActivated)),
				new Gtk.ActionEntry ("RecentLogs2", null, "_2", null, null, new EventHandler (OnRecentLogsActivated)),
				new Gtk.ActionEntry ("RecentLogs3", null, "_3", null, null, new EventHandler (OnRecentLogsActivated)),
				new Gtk.ActionEntry ("RecentLogs4", null, "_4", null, null, new EventHandler (OnRecentLogsActivated)),
				new Gtk.ActionEntry ("RepeatSessionsMenu", null, Catalog.GetString ("Re_peat Sessions"), null, null, null),
				new Gtk.ActionEntry ("RepeatSession0", null, "_0", null, null, new EventHandler (OnRepeatSessionActivated)),
				new Gtk.ActionEntry ("RepeatSession1", null, "_1", null, null, new EventHandler (OnRepeatSessionActivated)),
				new Gtk.ActionEntry ("RepeatSession2", null, "_2", null, null, new EventHandler (OnRepeatSessionActivated)),
				new Gtk.ActionEntry ("RepeatSession3", null, "_3", null, null, new EventHandler (OnRepeatSessionActivated)),
				new Gtk.ActionEntry ("RepeatSession4", null, "_4", null, null, new EventHandler (OnRepeatSessionActivated)),
				new Gtk.ActionEntry ("QuitAction", Gtk.Stock.Quit, null, "<control>Q", Catalog.GetString ("Quit Profiler"), new EventHandler (OnQuitActivated)),
				new Gtk.ActionEntry ("RunMenu", null, Catalog.GetString ("_Run"), null, null, null),
				new Gtk.ActionEntry ("ViewMenu", null, Catalog.GetString ("_View"), null, null, null),
			};

			Gtk.ToggleActionEntry[] toggle_actions = new Gtk.ToggleActionEntry[] {
				new Gtk.ToggleActionEntry ("ShowSystemNodesAction", null, Catalog.GetString ("_Show system nodes"), null, Catalog.GetString ("Shows internal nodes of system library method invocations"), new EventHandler (OnShowSystemNodesActivated), false),
				new Gtk.ToggleActionEntry ("LogEnabledAction", null, Catalog.GetString ("_Logging enabled"), null, Catalog.GetString ("Profile logging enabled"), new EventHandler (OnLoggingActivated), true),
			};
	    		group = new Gtk.ActionGroup ("group");
			group.Add (actions);
			group.Add (toggle_actions);
	    		Gtk.UIManager uim = new Gtk.UIManager ();
 
	    		uim.InsertActionGroup (group, (int) uim.NewMergeId ());
	    		uim.AddUiFromString (ui_info);
			AddAccelGroup (uim.AccelGroup);
			logging_enabled_action = group.GetAction ("LogEnabledAction") as Gtk.ToggleAction;
			logging_enabled_action.Visible = false;
			save_action = group.GetAction ("SaveAsAction");
			save_action.Sensitive = false;
			show_system_nodes_action = group.GetAction ("ShowSystemNodesAction");
 			return uim;
		}
	}
}
