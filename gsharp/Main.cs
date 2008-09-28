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
		
		public static void Main (string[] args)
		{
			if (args.Length > 0 && args [0] == "--agent"){
				Attached = true;
				
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
			Application.Run ();
		}

		static void AssemblyLoaded (object sender, AssemblyLoadEventArgs e)
		{
			Evaluator.ReferenceAssembly (e.LoadedAssembly);
		}
				
		public static void Start (string title)
		{
			Application.Init ();
			MainWindow m = new MainWindow ();
			m.Title = title;
			m.ShowAll ();
		}
	}
}