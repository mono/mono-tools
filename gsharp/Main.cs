// Main.cs created with MonoDevelop
// User: miguel at 10:22 PM 9/27/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using Gtk;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using Mono.Options;
using System.Collections.Generic;
using System.IO;

namespace Mono.CSharp.Gui
{
	class MainClass
	{
		public static bool Attached;
		public static bool HostHasGtkRunning;
		public static bool Debug;
		static Evaluator evaluator;
		
		public static void ShowHelp (OptionSet p)
		{
			Console.WriteLine ("Usage it: gsharp [--agent] [file1 [fileN]]");
			
			p.WriteOptionDescriptions (Console.Out);
		}

		static void ResetOutput ()
		{
			var stdout = new StreamWriter (Console.OpenStandardOutput ()) { AutoFlush = true };
			var stderr = new StreamWriter (Console.OpenStandardError ()) { AutoFlush = true };
			Console.SetOut (stdout);
			Console.SetError (stderr);
		}
		
		public static void Main (string[] args)
		{
			bool agent = false;

			OptionSet p = null;

			p = new OptionSet () {
				{ "agent", "Start up as an agent", f => agent = f != null },
				{ "help", "Shows the help", f => { ShowHelp (p); Environment.Exit (0); } },
				{ "debug", "Runs in debug mode, does not redirect IO to the window", f => Debug = true }
			};

			List<string> extra = null;
			try {
				extra = p.Parse (args);
			} catch (OptionException) {
				ShowHelp (p);
			}
			
			if (agent)
				StartAgent ();
			else
				Start ("C# InteractiveBase Shell", extra);
		}
		
		static void AssemblyLoaded (object sender, AssemblyLoadEventArgs e)
		{
			evaluator.ReferenceAssembly (e.LoadedAssembly);
		}

		internal static object RenderBitmaps (object o)
		{
			System.Drawing.Bitmap bitmap = o as System.Drawing.Bitmap;
			if (bitmap == null)
				return null;
			return new BitmapWidget (bitmap);
		}

		public static void StartAgent ()
		{
			Attached = true;
			
			// First, try to detect if Gtk.Application.Run is running,
			// to determine whether we need to run a mainloop oursvels or not.
			//
			// This test is not bullet proof, its just a simple guess.
			//
			// Thanks to Alan McGovern for this brilliant hack. 
			//
			ManualResetEvent handle = new ManualResetEvent(false);
			
			Gtk.Application.Invoke (delegate { handle.Set (); });
			HostHasGtkRunning = handle.WaitOne (3000, true);
			
			InteractiveGraphicsBase.Attached = true;
			Gtk.Application.Invoke (delegate {
				try {
					evaluator = new Evaluator (new CompilerSettings (), new Report (new ConsoleReportPrinter ()));
				} catch {
					return;
				}
				
				try {
					// Add all assemblies loaded later
					AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoaded;
					
					// Add all currently loaded assemblies
					foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies ())
						evaluator.ReferenceAssembly (a);
					
					Start (String.Format ("Attached C# Interactive Shell at Process {0}", Process.GetCurrentProcess ().Id), null);
				} finally {
					AppDomain.CurrentDomain.AssemblyLoad -= AssemblyLoaded;
				}
				
				});
		}
		
		public static void Start (string title, List<string> files)
		{
			if (!HostHasGtkRunning)
				Application.Init ();

			InteractiveGraphicsBase.RegisterTransformHandler (RenderBitmaps);
			
			MainWindow m = new MainWindow ();
			InteractiveGraphicsBase.MainWindow = m;
			InteractiveGraphicsBase.PaneContainer = m.PaneContainer;
			
			m.Title = title;
			m.LoadStartupFiles ();
			if (files != null)
				m.LoadFiles (files, false);
			m.ShowAll ();

			if (!HostHasGtkRunning){
				try {
					GLib.ExceptionManager.UnhandledException += delegate (GLib.UnhandledExceptionArgs a) {
						ResetOutput ();
						Console.WriteLine ("Application terminating: " + a.ExceptionObject);
					};
					
					Application.Run ();
				} catch (Exception e) {
					ResetOutput ();
					throw;
				}
			}
		}
	}
}