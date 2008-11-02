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

namespace Mono.CSharp.Gui
{
	class MainClass
	{
		public static bool Attached;
		public static bool HostHasGtkRunning;
		
		public static void Main (string[] args)
		{
			if (args.Length > 0 && args [0] == "--agent"){
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
						Evaluator.Init (new string [0]);
					} catch {
						return;
					}
	
					try {
						// Add all assemblies loaded later
						AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoaded;
		
						// Add all currently loaded assemblies
						foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies ())
							Evaluator.ReferenceAssembly (a);
			
							Start (String.Format ("Attached C# Interactive Shell at Process {0}", Process.GetCurrentProcess ().Id));
					} finally {
						AppDomain.CurrentDomain.AssemblyLoad -= AssemblyLoaded;
					}
					
				});
				return;
			}
			Start ("C# InteractiveBase Shell");
		}

		static void AssemblyLoaded (object sender, AssemblyLoadEventArgs e)
		{
			Evaluator.ReferenceAssembly (e.LoadedAssembly);
		}

		internal static Gtk.Widget RenderBitmaps (object o)
		{
			System.Drawing.Bitmap bitmap = o as System.Drawing.Bitmap;
			if (bitmap == null)
				return null;

			return new BitmapWidget (bitmap);
		}
		
		public static void Start (string title)
		{
			if (!HostHasGtkRunning)
				Application.Init ();

			InteractiveGraphicsBase.RegisterRenderHandler (RenderBitmaps);
			
			MainWindow m = new MainWindow ();
			InteractiveGraphicsBase.MainWindow = m;
			m.Title = title;
			m.LoadStartupFiles ();
			m.ShowAll ();
			if (!HostHasGtkRunning)
				Application.Run ();
		}
	}
}