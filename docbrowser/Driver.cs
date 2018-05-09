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
}
