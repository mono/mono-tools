// InfoManager.cs
//
// Copyright (c) 2007 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.IO;
using System.Net;
using System.Diagnostics; 
using System.Threading;
using Gtk;

namespace GuiCompare
{	
	
	
	public class InfoManager
	{
		// base directory where we store all of our profile information,
		// this is usually ~/.local/share/mono-gui-compare
		static string infos;
		
		// The base directory where Mono stores its profile assemblies 
		string monodir;
		
		// A handle to our container window
		MainWindow main;
		
		string [] api_1_1 = {
			"mscorlib",
			"System",
			"System.Data",
			"System.Data.OracleClient",
			"System.DirectoryServices",
			"System.Drawing",
			"System.Runtime.Remoting",
			"System.Runtime.Serialization.Formatters.Soap",
			"System.Security",
			"System.ServiceProcess",
			"System.Web",
			"System.Web.Services",
			"System.Windows.Forms",
			"System.Xml",
			"cscompmgd",
			"Microsoft.VisualBasic",
			"",
			"System.Configuration.Install",
			"System.Design",
			"System.Drawing.Design",
			"System.EnterpriseServices",
			"System.Management",
			"System.Messaging"
		};
		
		string [] api_2_0 = {
			"mscorlib",
			"System",
			"System.Configuration",
			"System.Data",
			"System.Data.OracleClient",
			"System.DirectoryServices",
			"System.Drawing",
			"System.Runtime.Remoting",
			"System.Runtime.Serialization.Formatters.Soap",
			"System.Security",
			"System.ServiceProcess",
			"System.Transactions",
			"System.Web",
			"System.Web.Services",
			"System.Windows.Forms",
			"System.Xml",
			"cscompmgd",
			"Microsoft.VisualBasic",
			"",
			"Microsoft.Build.Engine",
			"Microsoft.Build.Framework",
			"Microsoft.Build.Tasks",
			"Microsoft.Build.Utilities",
			"",
			"System.Configuration.Install",
			"System.Design",
			"System.Drawing.Design",
			"System.EnterpriseServices",
			"System.Management",
			"System.Messaging",
		};
		
		string [] api_3_0 = {			
			"PresentationCore",
			"PresentationFramework",
			"System.Speech",
			"WindowsBase",
			"",
			"System.IdentityModel",
			"System.IdentityModel.Selectors",
			"System.IO.Log",
			"System.Runtime.Serialization",
			"System.ServiceModel",
			"",
			"System.Workflow.Activities",
			"System.Workflow.ComponentModel",
			"System.Workflow.Runtime",
			"",
			"PresentationBuildTasks",
			"",
			"PresentationFramework.Aero",
			"PresentationFramework.Classic",
			"PresentationFramework.Luna",
			"PresentationFramework.Royale",
			"ReachFramework",
			"System.Printing",
			"UIAutomationClient",
			"UIAutomationClientsideProviders",
			"UIAutomationProvider",
			"UIAutomationTypes",
			"WindowsFormsIntegration",	
		};
		
		string [] api_sl11 = {
			"mscorlib",
			"System",
			"System.Core",
			"System.Xml.Core",
		};
		
		public static void Init ()
		{
			
			string xdg_data_home = Environment.GetEnvironmentVariable ("XDG_DATA_HOME");
			if (xdg_data_home == null || xdg_data_home == String.Empty){
				xdg_data_home = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
			}
			
			infos = Path.Combine (xdg_data_home, "mono-gui-compare");
			Directory.CreateDirectory (infos);
		}
		
		public delegate void MasterInfoLoaded (string path);
		
		// 
		// Ensures that masterinfo file for assembly name @file in @prof profile
		// is downloaded;   On success, executes the given callback
		//
		public void Ensure (string prof, string assemblyname, MasterInfoLoaded done)
		{
			string pdir = Path.Combine (infos, prof);
			if (!Directory.Exists (pdir)){
				Directory.CreateDirectory (pdir);
			}
			Console.WriteLine (pdir);
			string masterinfo = Path.Combine (pdir, assemblyname) + ".xml";
			if (File.Exists (masterinfo)){
				done (masterinfo);
				return;
			}
			
			Console.WriteLine ("Starting download...");
			WebClient w = new WebClient ();
			Uri u = null;
			
			switch (prof){
			case "1.0":
				u = new Uri ("http://mono.ximian.com/masterinfos/masterinfos-1.1.tar.gz");
				break;
				
			case "2.0":
				u = new Uri ("http://mono.ximian.com/masterinfos/masterinfos-2.0.tar.gz");
				break;
				
			case "3.0":
				u = new Uri ("http://mono.ximian.com/masterinfos/masterinfos-3.0.tar.gz");
				break;
				
			case "2.1":
				u = new Uri ("http://mono.ximian.com/masterinfos/masterinfos-sl11a-refresh.tar.gz");
				break;
				
			default:
				main.Status = "Profile is unknown";
				break;
			}
			string target = Path.Combine (infos, "masterinfos-" + prof + ".tar.gz");
			
			w.DownloadFileCompleted += delegate {
				string msg;
				
				if (File.Exists (target)){
					ProcessStartInfo pi = new ProcessStartInfo();
					pi.WorkingDirectory = pdir;
					pi.UseShellExecute = true;
					pi.FileName = "tar xzf " + target + " --strip-components=1";
					Process p = Process.Start (pi);
					p.WaitForExit ();
					msg = "Download complete";
				} else {
					msg = "Download failed";
					done = null;
				}
				
				Application.Invoke (delegate {
					main.Progress = 0;
					main.Status = msg;
					if (done != null)
						done (masterinfo);
				});				
			};
			w.DownloadProgressChanged += delegate (object sender, DownloadProgressChangedEventArgs args) {
				Application.Invoke (delegate {
					main.Progress = args.ProgressPercentage;
				});
			};
			Console.WriteLine ("Downloading {0} to {1}", u, target);
			w.DownloadFileAsync (u, target);
		}

		/// <summary>
		///   Starts a preset compare.
		/// </summary>
		/// <param name="assemblyfile">
		/// The full path to a system installed assembly (/mono/lib/mono/1.0/mscorlib.dll)
		/// </param>
		/// <param name="profile">
		/// The profile, for example "1.0"
		/// </param>
		/// <param name="assemblyname">
		/// The name of the assembly to compare, in this case "mscorlib"
		/// </param>
		void StartPresetCompare (string assemblyfile, string profile, string assemblyname)
		{
			Ensure (profile, assemblyname, delegate (string masterinfo){
				main.SetReference (delegate {
					Console.WriteLine ("Doing it for the {0}-{1}-{2}", assemblyfile, profile, assemblyname);
						return new MasterAssembly (masterinfo);
					});
				main.SetTarget (delegate {
						return new CecilAssembly (assemblyfile);
					 });
					
				main.StartCompare (delegate {
					main.Title = assemblyfile;
				});
			});
		}
		
		void Populate (Menu container, string caption, string pdir, string [] elements)
		{
			string profiledir = System.IO.Path.Combine (monodir, pdir);
			
			MenuItem item = new MenuItem (caption);
			Menu sub = new Menu ();
			item.Submenu = sub;
			
			foreach (string e in elements){
				MenuItem child;
				
				if (e == String.Empty)
					child = new SeparatorMenuItem ();
				else {
					string assemblyfile = System.IO.Path.Combine (profiledir, e + ".dll");
					string element = e;
					if (!System.IO.File.Exists (assemblyfile)){
						Console.WriteLine ("Skipping {0} as {1} does not have it", e, profiledir);
						continue;
					}
					child = new MenuItem (e);
					child.Activated += delegate {
						StartPresetCompare (assemblyfile, pdir, element);
					};
				}
				sub.Add (child);
			}
			
			item.ShowAll ();
			container.Add (item);
		}
		
		// 
		// Constructor
		//
		public InfoManager (MainWindow main)
		{
			this.main = main;
			
			string corlibdir = System.IO.Path.GetDirectoryName (typeof (int).Assembly.Location);
			monodir = System.IO.Path.GetFullPath (System.IO.Path.Combine (corlibdir, "..")); 
	
			// Work around limitation in Stetic, there is no way
			// of getting a handle on the menu (!!!)
			
			Menu sub = null;
			
			foreach (MenuItem mi in main.MenuBar.AllChildren){
				AccelLabel a = mi.Child as AccelLabel;
				if (a != null)
					Console.WriteLine (a.LabelProp);
				if (a == null || a.LabelProp != "_Compare")
					continue;
				
				sub = (Menu) mi.Submenu;
				break;
			}
			
			if (sub == null){
				Console.WriteLine ("Unable to found Compare submenu");
				return;
			}
			
			Populate (sub, "API 1.1", "1.0", api_1_1);
			Populate (sub, "API 2.0", "2.0", api_2_0);
			Populate (sub, "API 3.0 (WxF)", "3.0", api_3_0);
			Populate (sub, "Silverlight 1.1", "2.1", api_sl11);
		}
	}
}
