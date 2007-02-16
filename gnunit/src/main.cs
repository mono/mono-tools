//
// main.cs: gnunit.exe - a frontend for running NUnit2 test cases.
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
//
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using Gdk;
using GLib;
using Gnome;
using Gtk;
using NUnit.Core;
using NUnit.Util;
using Dialog = Gtk.Dialog;

namespace Mono.NUnit.GUI
{
	class OverWriteDialog
	{
		[Glade.Widget] Dialog overwriteDialog;
		[Glade.Widget] Label header;
		[Glade.Widget] Label message;
		bool yes;
		
		public OverWriteDialog (string filename)
		{
			Glade.XML gXML = new Glade.XML (null, "nunit-gtk.glade", "overwriteDialog", null);
			gXML.Autoconnect (this);
			overwriteDialog.Response += new ResponseHandler (OnResponse);
			header.Markup = String.Format ( _("<b>File {0} exists.</b>"), filename);
			message.Text = _("Do you want to overwrite it?");
		}

		void OnResponse (object sender, ResponseArgs args)
		{
			yes = (args.ResponseId == ResponseType.Yes);
		}

		public void Run ()
		{
			overwriteDialog.Run ();
			overwriteDialog.Hide ();
			overwriteDialog.Dispose ();
		}

		public bool Yes {
			get { return yes; }
		}

		static string _ (string key)
		{
			return Catalog.GetString (key);
		}
	}

	class ErrorDialog
	{
		[Glade.Widget] Dialog errorDialog;
		[Glade.Widget] Label message;
		[Glade.Widget] HBox detailBox;
		[Glade.Widget] Button btnDetails;
		[Glade.Widget] Button btnExit;
		[Glade.Widget] Button btnOK;
		[Glade.Widget] TextView details;
		
		public ErrorDialog (string title, string text)
			: this (title, text, null)
		{
		}

		public ErrorDialog (string title, string text, string more)
		{
			Glade.XML gXML = new Glade.XML (null, "nunit-gtk.glade", "errorDialog", null);
			gXML.Autoconnect (this);
			errorDialog.Title = title;
			errorDialog.Response += new ResponseHandler (OnResponse);
			message.Markup = text;
			btnExit.Visible = false;
			detailBox.Visible = false;
			if (more == null || more == "") {
				btnDetails.Visible = false;
			} else {
				details.Buffer.InsertAtCursor (more);
			}
		}

		public void UseExitButton ()
		{
			btnOK.Visible = false;
			btnExit.Visible = true;
		}
		
		void OnDetailsClicked (object sender, EventArgs args)
		{
			if (detailBox.Visible) {
				detailBox.Visible = false;
				btnDetails.Label = _("Show details >>");
			} else {
				detailBox.Visible = true;
				btnDetails.Label = _("Hide details <<");
			}
		}

		void OnResponse (object sender, ResponseArgs args)
		{
		}

		public void Run ()
		{
			errorDialog.Run ();
			errorDialog.Hide ();
			errorDialog.Dispose ();
		}

		static string _ (string key)
		{
			return Catalog.GetString (key);
		}
	}

	class CategoryMaster : IFilter {
		static Hashtable known_categories;
		string [] categories;
		bool [] states;
		object [] menues;
		EventHandler cb;
		string [] exclude;
		bool uncategorized;
		bool all;
		IFilter inner_filter;

		static CategoryMaster ()
		{
			known_categories = new Hashtable (CaseInsensitiveHashCodeProvider.DefaultInvariant,
							 CaseInsensitiveComparer.DefaultInvariant);

			RetrieveKnownCategories (null, null);
			Settings.KnownCategoriesChanged += new GConf.NotifyEventHandler (RetrieveKnownCategories);
		}

		static void RetrieveKnownCategories (object sender, GConf.NotifyEventArgs args)
		{
			lock (known_categories) {
				string [] known;
				try {
					known = Settings.KnownCategories.Split (':');
				} catch {
					known_categories.Clear ();
					return;
				}

				foreach (string k in known) {
					int n = k.IndexOf ('=');
					if (n == -1)
						continue;

					known_categories [k.Substring (n)] = (k [n + 1] == '1');
				}
			}
		}

		public CategoryMaster ()
		{
			cb = new EventHandler (ToggleState);
		}

		public CategoryMaster (ICollection categories) : this ()
		{
			SetCategories (categories);
		}

		public void SetMenuItemForIndex (MenuItem item, int idx)
		{
			menues [idx] = item;
		}

		public void SetCategories (ICollection categories)
		{
			Cancel = false;
			int n = (categories != null) ? categories.Count + 1 : 1;
			this.categories = new string [n];
			states = new bool [n];
			menues = new object [n];
			int idx = 0;
			if (n > 1) {
				foreach (string s in categories) {
					this.categories [idx] = s;
					states [idx] = true; //TODO: Default from known_categories
					idx++;
				}
			}

			this.categories [idx] = _("Uncategorized");
			states [idx] = true;
			exclude = null;
			uncategorized = false;
			all = false;
		}

		public string [] Categories {
			get { return categories; }
		}

		public bool [] States {
			get { return states; }
		}

		public EventHandler Callback {
			get { return cb; }
		}

		void ToggleState (object sender, EventArgs args)
		{
			CheckMenuItem item = (CheckMenuItem) sender;
			int idx = Array.IndexOf (menues, sender);
			if (idx == -1) {
				return;
			}

			states [idx] = item.Active;
		}

		public void PrepareFilters ()
		{
			int length = states.Length;
			uncategorized = states [length - 1];
			int nexcludes = 0;
			for (int i = 0; i < length - 1; i++) {
				if ((uncategorized && !states [i]) || (!uncategorized && states [i]))
					nexcludes++;
			}

			if (uncategorized && nexcludes == 0) {
				all = true;
				exclude = null;
				return;
			}

			if (nexcludes > 0) {
				exclude = new string [nexcludes];
				int idx = 0;
				for (int i = 0; i < length - 1; i++) {
					if (uncategorized && !states [i]) {
						exclude [idx] = categories [i];
						idx++;
					} else if (!uncategorized && states [i]) {
						exclude [idx] = categories [i];
						idx++;
					}
				}
				inner_filter = new CategoryFilter (exclude, uncategorized);
			} else {
				inner_filter = new CategoryFilter (categories, uncategorized);
			}
		}

		public bool Cancel;
		bool IFilter.Pass (TestSuite suite)
		{
			if (Cancel)
				return false;

			if (all)
				return true;

			return inner_filter.Pass (suite);
		}
		
		bool IFilter.Pass (TestCase test)
		{
			if (Cancel)
				return false;

			if (all)
				return true;

			return inner_filter.Pass (test);
		}

		static string _ (string key)
		{
			return Catalog.GetString (key);
		}
	}

	class NUnitGUI : Program, EventListener
	{
		static string version;
		static string title;
		static string copyright;
		static string description;
		
		[Glade.Widget("nunitgui")] Gtk.Window window;
		[Glade.Widget] AppBar appbar;

		// Run frame
		[Glade.Widget] ProgressBar frameProgress;
		[Glade.Widget] Label frameLabel;
		[Glade.Widget] Label runStatus;
		[Glade.Widget] Label clock;

		// Menu
		[Glade.Widget] ImageMenuItem menuRecent;
		[Glade.Widget] MenuBar menubar;
		[Glade.Widget] MenuItem menuSaveAs;
		// gtk-sharp 2.0
		//[Glade.Widget] ToolButton btnOpen;
		//[Glade.Widget] ToolButton btnSaveAs;
		//[Glade.Widget] ToolButton btnRun;
		//[Glade.Widget] ToolButton btnExit;
		//[Glade.Widget] ToolButton btnStop;
		[Glade.Widget] Button btnOpen;
		[Glade.Widget] Button btnSaveAs;
		[Glade.Widget] Button btnRun;
		[Glade.Widget] Button btnExit;
		[Glade.Widget] Button btnStop;
		[Glade.Widget] MenuItem categories_menu;

		// Notebook
		[Glade.Widget] TreeView failures;
		[Glade.Widget] Label failuresLabel;

		[Glade.Widget] TreeView notRun;
		[Glade.Widget] Label notRunLabel;

		[Glade.Widget] TextView stdoutTV;
		[Glade.Widget] Label stdoutLabel;

		[Glade.Widget] TextView stderrTV;
		[Glade.Widget] Label stderrLabel;

		Label [] nbLabels;
		//

		[Glade.Widget] TreeView assemblyView;
		[Glade.Widget] Paned hpaned;

		string [] args;
		AssemblyStore store;
		TreeStore notRunStore;
		TreeStore failuresStore;
		int ntests;
		int finishedTests;
		int ignoredTests;
		int errorTests;
		int assertions;
		TextWriter origStdout = Console.Out;
		TextWriter origStderr = Console.Error;
		StringWriter stdout = new StringWriter ();
		StringWriter stderr = new StringWriter ();
		Hashtable errorIters;
		TreeViewColumn nameCol;
		CellRendererPixbuf pr;
		CellRendererText tr;
		Dialog about;
		bool quitting;
		long startTime;
		CategoryMaster catman = new CategoryMaster ();

		static string _ (string key)
		{
			return Catalog.GetString (key);
		}

		static NUnitGUI ()
		{
			Assembly assembly = Assembly.GetExecutingAssembly ();
			version = assembly.GetName ().Version.ToString ();
			object [] att = assembly.GetCustomAttributes (typeof (AssemblyTitleAttribute), false);
			title = ((AssemblyTitleAttribute) att [0]).Title;
			att = assembly.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
			copyright = ((AssemblyCopyrightAttribute) att [0]).Copyright;
			att = assembly.GetCustomAttributes (typeof (AssemblyDescriptionAttribute), false);
			description = ((AssemblyDescriptionAttribute) att [0]).Description;
		}
		
		public NUnitGUI (string [] args, params object [] props)
			: base ("gnunit", version, Modules.UI, args, props)
		{
			Glade.XML gXML = new Glade.XML (null, "nunit-gtk.glade", "nunitgui", null);
			gXML.Autoconnect (this);
			window.Title = title;
			btnStop.Sensitive = false;
			nbLabels = new Label [] {failuresLabel, notRunLabel, stderrLabel, stdoutLabel};
			this.args = args;

			pr = new CellRendererPixbuf ();
			tr = new CellRendererText ();
			nameCol = new TreeViewColumn ();
			nameCol.PackStart (pr, false);
			nameCol.SetCellDataFunc (pr, CircleRenderer.CellDataFunc);
			nameCol.PackStart (tr, false);
			nameCol.AddAttribute (tr, "text", 1);
			assemblyView.AppendColumn (nameCol);

			if (args.Length == 1) {
				LoadAssembly (args [0]);
			} else {
				btnRun.Sensitive = false;
				window.Title = title;
				appbar.SetStatus (_("No assembly loaded."));
			}
			menuSaveAs.Sensitive = false;
			btnSaveAs.Sensitive = false;
			SetupRecentAssembliesMenu (null, null);
			Settings.RecentassembliesChanged += new GConf.NotifyEventHandler (SetupRecentAssembliesMenu);

			int width, height, position;
			try {
				width = Settings.Width;
				height = Settings.Height;
				position = Settings.Hpaned;
			} catch {
				Settings.Width = width = 450;
				Settings.Width = height = 300;
				Settings.Hpaned = position = 150;
			}
			
			window.Resize (width, height);
			hpaned.Position = position;
			window.ShowAll ();
		}

		void LoadAssembly (string name)
		{
			window.Title = String.Format ("{0} - [{1}]", title, name);
			appbar.SetStatus (String.Format (_("Loading {0}..."), name));
			frameProgress.Fraction = 0.0;
			frameProgress.Text = "";
			frameLabel.Text = "";
			runStatus.Text = "";
			menubar.Sensitive = false;
			btnOpen.Sensitive = false;
			btnRun.Sensitive = false;
			btnSaveAs.Sensitive = false;
			clock.Text = "";

			errorIters = null;
			if (notRunStore != null)
				notRunStore.Clear ();

			if (failuresStore != null)
				failuresStore.Clear ();

			if (store != null) {
				store.Clear ();
				store.Dispose ();
			}

			if (catman != null)
				catman.SetCategories (null);

			stdoutTV.Buffer.Clear ();
			stderrTV.Buffer.Clear ();
			foreach (Label l in nbLabels)
				SetColorLabel (l, false);

			store = new AssemblyStore (name);
			store.Filter = catman;
			ntests = 0;
			store.FixtureLoadError += new FixtureLoadErrorHandler (OnFixtureLoadError);
			store.FixtureAdded += new FixtureAddedEventHandler (OnFixtureAdded);
			store.FinishedRunning += new EventHandler (OnFinishedRunning);
			store.FinishedLoad += new EventHandler (OnFinishedLoad);
			store.IdleCallback += new EventHandler (ClockUpdater);
			assemblyView.Model = store;
			assemblyView.SearchColumn = store.SearchColumn;
			store.Load ();
			string path = store.Location;
			AddRecent (path);
			Directory.SetCurrentDirectory (Path.GetDirectoryName (path));
		}

		void AddRecent (string name)
		{
			if (name == null || name == "")
				return;

			string [] recent;
			try {
				recent = Settings.Recentassemblies.Split (':');
			} catch {
				recent = new string [0];
			}
			ArrayList list = new ArrayList (recent);
			list.Remove ("");

			int i;
			if ((i = list.IndexOf (name)) != -1) {
				if (list.Count == 1)
					return;

				list.RemoveAt (i);
				list.Insert (0, name);
			} else {
				list.Add (name);
			}

			while (list.Count > 10)
				list.RemoveAt (0);

			recent = (string []) list.ToArray (typeof (string));
			Settings.Recentassemblies = String.Join (":", recent);
		}

		void RemoveRecent (string name)
		{
			string [] recent;
			try {
				recent = Settings.Recentassemblies.Split (':');
			} catch {
				recent = new string [0];
			}
			ArrayList list = new ArrayList (recent);
			list.Remove ("");
			list.Remove (name);

			recent = (string []) list.ToArray (typeof (string));
			Settings.Recentassemblies = String.Join (":", recent);
		}

		// assemblyView events
		void OnTestActivated (object sender, RowActivatedArgs args)
		{
			if (store == null)
				return;

			TreeView tv = (TreeView) sender;
			tv.ExpandRow (args.Path, true);
			if (!store.Running) {
				PrepareRun ();
				store.RunTestAtPath (args.Path, this, ref ntests);
			}
		}

		// AssemblyStore events
		void OnFixtureAdded (object sender, FixtureAddedEventArgs args)
		{
			string msg = String.Format (_("Loading test {0} of {1}"), args.Current, args.Total);
			ntests = args.Current;
			appbar.Progress.Fraction = ntests / (double) args.Total;
			appbar.SetStatus (msg);
		}

		void OnFixtureLoadError (object sender, FixtureLoadErrorEventArgs args)
		{
			store.Clear ();
			store = null;
			RemoveRecent (args.FileName);
			appbar.SetStatus (_("Error loading assembly"));
			Error (String.Format (_("Error loading '{0}'"), args.FileName), args.Message, null, false);
			appbar.SetStatus ("");
			btnRun.Sensitive = false;
			menubar.Sensitive = true;
			btnOpen.Sensitive = true;
		}

		// Window event handlers
		void OnWindowDelete (object sender, DeleteEventArgs args)
		{
			OnQuitActivate (sender, args);
		}

		// Menu and toolbar event handlers
		void OnQuitActivate (object sender, EventArgs args)
		{
			if (store != null)
				store.CancelRequest ();

			quitting = true;
			Settings.Width = window.Allocation.Width;
			Settings.Height = window.Allocation.Height;
			Settings.Hpaned = hpaned.Position;
			Quit ();
			Environment.Exit (0);
		}

		void OnExitActivate (object sender, EventArgs args)
		{
			OnQuitActivate (sender, args);
		}

		void OnCopyActivate (object sender, EventArgs args)
		{
			Console.WriteLine ("OnCopy");
		}

		void OnOpenActivate (object sender, EventArgs args)
		{
			FileDialog fd = new FileDialog ();
			fd.Run ();
			if (fd.Ok)
				LoadAssembly (fd.Filename);
		}

		void OnAboutActivate (object sender, EventArgs args)
		{
			Pixbuf logo = Pixbuf.LoadFromResource ("nunit-gui.png");
			string [] authors = new string[] { "Gonzalo Paniagua Javier (gonzalo@ximian.com)" };
			string [] docs = new string[] { };
			string translator = Catalog.GetString ("translator_credits");

			about = new About (title, version, copyright, description, authors, docs, translator, logo);
			about.Show ();
			System.GC.SuppressFinalize (about);
		}

		void OnPreferencesActivate (object sender, EventArgs args)
		{
			Console.WriteLine ("OnPreferencesActivate");
		}

		void OnRunActivate (object sender, EventArgs args)
		{
			if (assemblyView.Model == null)
				return;

			TreeSelection selection = assemblyView.Selection;
			TreeModel model;
			TreeIter iter;

			if (!selection.GetSelected (out model, out iter)) {
				appbar.SetStatus (_("You have to select a test to run."));
				return;
			}
			
			PrepareRun ();
			store.RunTestAtIter (iter, this, ref ntests);
		}

		void OnLoadRecent (object sender, EventArgs args)
		{
			string assembly = (string) recent_items [sender];
			LoadAssembly (assembly);
		}

		void OnClearRecent (object sender, EventArgs args)
		{
			Settings.Recentassemblies = "";
		}

		void OnStopActivate (object sender, EventArgs args)
		{
			if (store != null) {
				store.CancelRequest ();
				catman.Cancel = true;
			}
		}

		void OnSaveAs (object sender, EventArgs args)
		{
			if (store == null || store.LastResult == null) {
				Error (_("No test results available"),
				       _("You must run some tests in order to get results to save."),
				       null,
				       false);
				return;
			}

			FileDialog fd = new FileDialog (_("Save results to..."), "*.xml");
			fd.Run ();
			if (!fd.Ok)
				return;

			if (File.Exists (fd.Filename)) {
				OverWriteDialog dlg = new OverWriteDialog (fd.Filename);
				dlg.Run ();
				if (!dlg.Yes) {
					appbar.SetStatus (_("Results not saved."));
					return;
				}
			}
			
			TestResult result = store.LastResult;
			XmlResultVisitor visitor = null;
			try {
				visitor = new XmlResultVisitor (fd.Filename, result);
			} catch (Exception e) {
				Error (_("Error"),
					_("There has been an error saving the results.\n") +
					_("Do you have correct permissions to write to that file?"),
					e.ToString (),
					false);
				return;
			}
			
			result.Accept (visitor);
			visitor.Write ();
			appbar.SetStatus (String.Format (_("Results saved to {0}"), fd.Filename));
		}

		// Notebook
		void OnSwitchPage (object sender, SwitchPageArgs args)
		{
			Notebook nb = (Notebook) sender;
			if (nb.Page != args.PageNum) {
				SetColorLabel (nbLabels [nb.Page], false);
			}

			SetColorLabel (nbLabels [args.PageNum], false);
		}

		//	Used for the 2 treeviews in the notebook
		void OnRowActivated (object sender, RowActivatedArgs args)
		{
			TreeView tv = (TreeView) sender;
			TreePath path = args.Path;
			if (tv.GetRowExpanded (path))
				tv.CollapseRow (path);
			else
				tv.ExpandRow (path, true);
		}

		// Interface NUnit.Core.EventListener
		void EventListener.RunStarted (Test [] tests)
		{
		}

		void EventListener.RunFinished (TestResult [] results)
		{
		}

		void EventListener.UnhandledException (Exception exception)
		{
		}

		void EventListener.RunFinished (Exception exc)
		{
		}

		void EventListener.TestStarted (TestCase testCase)
		{
			frameLabel.Text = "Test: " + testCase.FullName;
		}
			
		void EventListener.TestFinished (TestCaseResult result)
		{
			frameProgress.Fraction = ++finishedTests / (double) ntests;
			frameProgress.Text = String.Format ("{0}/{1}", finishedTests, ntests);

			assertions += result.AssertCount;
			if (result.Executed == false) {
				AddIgnored (result.Test.FullName, result.Test.IgnoreReason);
			} else if (result.IsFailure) {
				AddError (result);
			}

			CheckWriters ();
			UpdateRunStatus ();
			ClockUpdater (this, EventArgs.Empty);
		}

		void EventListener.SuiteStarted (TestSuite suite)
		{
			frameLabel.Text = "Suite: " + suite.FullName;
		}

		void EventListener.SuiteFinished (TestSuiteResult result)
		{
			ClockUpdater (this, EventArgs.Empty);
		}

		// Misc.

		void UpdateRunStatus ()
		{
			runStatus.Markup = String.Format (_("Tests (assertions): {0} ({3}) Ignored: {1} Failures: {2}"),
							  finishedTests, ignoredTests, errorTests, assertions);
		}
		
		void AddIgnored (string name, string reason)
		{
			ignoredTests++;
			if (notRunStore == null) {
				notRunStore = new TreeStore (typeof (string));
				CellRendererText tr = new CellRendererText ();
				TreeViewColumn col = new TreeViewColumn ();
				col.PackStart (tr, false);
				col.AddAttribute (tr, "text", 0);
				notRun.AppendColumn (col);
				notRun.Model = notRunStore;
				notRun.ShowAll ();
			}

			TreeIter iter;
			iter = notRunStore.AppendValues (name);
			iter = notRunStore.AppendValues (iter, reason);
			
			SetColorLabel (notRunLabel, true);
		}

		void AddError (TestCaseResult result)
		{
			errorTests++;
			if (failuresStore == null) {
				failuresStore = new TreeStore (typeof (string));
				CellRendererText tr = new CellRendererText ();
				TreeViewColumn col = new TreeViewColumn ();
				col.PackStart (tr, false);
				col.AddAttribute (tr, "text", 0);
				failures.AppendColumn (col);
				failures.Model = failuresStore;
				failures.ShowAll ();
			}

			if (errorIters == null)
				errorIters = new Hashtable ();

			int dot;
			TreeIter main = TreeIter.Zero;
			TreeIter iter;
			string fullname = result.Test.FullName;
			if ((dot = fullname.LastIndexOf ('.')) != -1) {
				string key = fullname.Substring (0, dot);
				if (!errorIters.ContainsKey (key)) {
					main = failuresStore.AppendValues (key);
					errorIters [key] = main;
				} else {
					main = (TreeIter) errorIters [key];
					failuresStore.SetValue (main, 0, key);
				}
			} else {
				main = failuresStore.AppendValues (fullname);
				errorIters [fullname] = main;
			}

			iter = failuresStore.AppendValues (main, result.Test.Name);
			iter = failuresStore.AppendValues (iter, result.Message);
			iter = failuresStore.AppendValues (iter, result.StackTrace);
			
			SetColorLabel (failuresLabel, true);
		}

		void SetOriginalWriters ()
		{
			Console.SetOut (origStdout);
			Console.SetError (origStderr);
		}

		void SetStringWriters ()
		{
			Console.SetOut (stdout);
			Console.SetError (stderr);
		}

		void CheckWriters ()
		{
			StringBuilder sb = stdout.GetStringBuilder ();
			if (sb.Length != 0) {
				InsertOutText (stdoutTV, sb.ToString ());
				sb.Length = 0;
				SetColorLabel (stdoutLabel, true);
			}

			sb = stderr.GetStringBuilder ();
			if (sb.Length != 0) {
				stderrTV.Buffer.InsertAtCursor (sb.ToString ());
				sb.Length = 0;
				SetColorLabel (stderrLabel, true);
			}
		}

		void InsertOutText (TextView tv, string str)
		{
			TextBuffer buf = tv.Buffer;
			buf.InsertAtCursor (str);
		}

		void SetColorLabel (Label label, bool color)
		{
			string text = label.Text;
			if (color)
				label.Markup = String.Format ("<span foreground=\"blue\">{0}</span>", text);
			else
				label.Markup = text;
		}
		
		void Error (string title, string text, string details, bool isExit)
		{
			ErrorDialog ed = new ErrorDialog (title, text, details);
			if (isExit)
				ed.UseExitButton ();

			ed.Run ();
		}

		Hashtable recent_items = new Hashtable ();
		void SetupRecentAssembliesMenu (object sender, GConf.NotifyEventArgs args)
		{
			string [] recent;
			try {
				recent = Settings.Recentassemblies.Split (':');
			} catch {
				recent = new string [0];
			}
			
			if (recent.Length == 0) {
				menuRecent.Submenu = null;
				return;
			}

			EventHandler cb = new EventHandler (OnLoadRecent);
			recent_items.Clear ();
			Menu menu = new Menu ();
			int index = 1;
			foreach (string s in recent) {
				if (s == "")
					continue;

				MenuItem item = new MenuItem (String.Format ("_{0}. {1}",
									     index++,
									     s.Replace ("_", "__")));
				recent_items [item] = s;
				item.Activated += cb;
				menu.Append (item);
			}
			menu.ShowAll ();
			menuRecent.Submenu = menu;
		}

		void PrepareRun ()
		{
			if (errorIters != null)
				errorIters.Clear ();

			if (notRunStore != null)
				notRunStore.Clear ();

			if (failuresStore != null)
				failuresStore.Clear ();

			stdoutTV.Buffer.Clear ();
			stderrTV.Buffer.Clear ();
			foreach (Label l in nbLabels)
				SetColorLabel (l, false);

			errorTests = 0;
			ignoredTests = 0;
			assertions = 0;

			ntests = -1;
			finishedTests = 0;
			frameProgress.Fraction = 0.0;

			appbar.SetStatus (_("Running tests..."));
			SetStringWriters ();
			catman.PrepareFilters ();
			ToggleMenues (false);
			startTime = DateTime.Now.Ticks;
			ClockUpdater (this, EventArgs.Empty);
		}

		void ToggleMenues (bool saveAs)
		{
			if (btnStop.Sensitive) {
				btnOpen.Sensitive = true;
				btnSaveAs.Sensitive = saveAs;
				menuSaveAs.Sensitive = saveAs;
				btnRun.Sensitive = true;
				btnExit.Sensitive = true;
				btnStop.Sensitive = false;
				menubar.Sensitive = true;
			} else {
				btnOpen.Sensitive = false;
				btnSaveAs.Sensitive = false;
				menuSaveAs.Sensitive = false;
				btnRun.Sensitive = false;
				btnExit.Sensitive = false;
				btnStop.Sensitive = true;
				menubar.Sensitive = false;
			}
		}

		void OnFinishedRunning (object sender, EventArgs args)
		{
			if (quitting)
				return;

			catman.Cancel = false;
			ClockUpdater (this, EventArgs.Empty);
			ToggleMenues (store.LastResult != null);
			SetOriginalWriters ();
			UpdateRunStatus ();
			frameProgress.Fraction = 1.0;
			frameProgress.Text = String.Format ("{0}/{0}", finishedTests);
			finishedTests = ntests;
			if (store.Cancelled) {
				appbar.SetStatus (_("Cancelled on user request."));
				frameLabel.Markup = String.Format ("<span foreground=\"red\">Cancelled: {0}</span>",
								   frameLabel.Text);
			} else {
				appbar.SetStatus ("");
			}
			stdout.GetStringBuilder ().Length = 0;
			stderr.GetStringBuilder ().Length = 0;
		}

		void BuildCategoriesMenu ()
		{
			string [] categories = catman.Categories;
			bool [] states = catman.States;
			Menu menu = new Menu ();
			CheckMenuItem item;
			int idx = 0;
			foreach (string s in categories) {
				item = new CheckMenuItem (s);
				catman.SetMenuItemForIndex (item, idx);
				item.Toggled += catman.Callback;
				item.Active = states [idx];
				menu.Append (item);
				idx++;
			}

			menu.ShowAll ();
			categories_menu.Submenu = menu;
		}

		void OnFinishedLoad (object sender, EventArgs args)
		{
			if (store.Cancelled) // Application finished while loading
				return;

			catman.SetCategories (((CategoriesEventArgs) args).Categories);
			BuildCategoriesMenu ();
			appbar.Progress.Fraction = 0.0;
			appbar.SetStatus (String.Format (_("{0} tests loaded."), ntests));
			btnOpen.Sensitive = true;
			btnRun.Sensitive = true;
			menubar.Sensitive = true;
			assemblyView.Selection.SelectPath (TreePath.NewFirst ());
		}

		long lastTick = -1;
		void ClockUpdater (object o, EventArgs args)
		{
			long now = DateTime.Now.Ticks;	
			if (!store.Running || now - lastTick >= 10000 * 100) { // 100ms
				lastTick = now;
				string fmt = new TimeSpan (now - startTime).ToString ();
				int i = fmt.IndexOf ('.');
				if (i > 0 && fmt.Length - i > 2)
					fmt = fmt.Substring (0, i + 2);

				clock.Text = String.Format (_("Elapsed time: {0}"), fmt);
			}
		}

		public void UnhandledException (object sender, UnhandledExceptionEventArgs args)
		{
			if (store != null)
				store.CancelRequest ();

			try {
				Error (_("Unhandled Exception"), _("There has been an unhandled exception.\n") +
				       _("The program will terminate now."), args.ExceptionObject.ToString (), true);
			} catch (Exception e) {
				Console.WriteLine (e);
			}
			Quit ();
			Environment.Exit (0);
		}
		
		// Main
		static void Main (string [] args)
		{
			Catalog.InitCatalog ();
			NUnitGUI gui = new NUnitGUI (args);
			AppDomain current = AppDomain.CurrentDomain;
			current.UnhandledException += new UnhandledExceptionEventHandler (gui.UnhandledException);
			gui.Run ();
		}
	}
}

