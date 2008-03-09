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
		
		// The handle to the Recent Comparisons Menu
		Menu recentmenu;
		
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
			//"UIAutomationClient",
			//"UIAutomationClientsideProviders",
			//"UIAutomationProvider",
			//"UIAutomationTypes",
			//"WindowsFormsIntegration",	
		};
		
		string [] api_sl11 = {
			"mscorlib",
			"agclr.dll",
			"Microsoft.VisualBasic",
			"System",
			"System.Core",
			"System.Net",
			"System.SilverLight",
			"System.Xml.Core",
		};

		string [] api_sl2_beta = {
			"mscorlib",
			"agclr",
			"Microsoft.VisualBasic",
			"System",
			"System.Core",
			"System.Net",
			"System.Runtime.Serialization",
			"System.ServiceModel",
			"System.SilverLight",
			"System.Xml.Core",
		};
		
		string [] api_3_5 = {
			"mscorlib",
			"System",
			"System.AddIn",
			"System.AddIn.Contract",
			"System.Configuration",
			"System.Core", 
			// "System.Configuration.Install",
			"System.Data",
			"System.Data.Linq",
			"System.Data.OracleClient",
			"System.DirectoryServices",
			// "System.DirectoryServices.AccountManagement",
			// "System.DirectoryServices.Protocols",
			"System.Drawing",
			"System.Net",
			"System.Runtime.Remoting",
			"System.Security",
			"System.ServiceProcess",
			"System.Transactions",
			"System.Web",
			"System.Web.Extensions",
			// "System.Web.Extensions.Design",
			// "System.Web.Mobile",
			// "System.Web.RegularExpressions",
			//
			"System.Web.Services",
			"System.Windows.Forms",
			"System.Xml",
			"System.Xml.Linq",
			"",
			"System.Runtime.Serialization.Formatters.Soap",
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
			// "System.Management.Instrumentation",
			"System.Messaging",
		};
		
		string [] api_3_5_wxf = {			
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
		};
		
		public static void Init ()
		{
			
			string xdg_data_home = Environment.GetEnvironmentVariable ("XDG_DATA_HOME");
			if (xdg_data_home == null || xdg_data_home == String.Empty || xdg_data_home.IndexOf (':') != -1){
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

			string target = Path.Combine (infos, "masterinfos-" + prof + ".tar.gz");
			string masterinfo = Path.Combine (pdir, assemblyname) + ".xml";
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

			case "SL2":
				u = new Uri ("http://mono.ximian.com/masterinfos/masterinfos-sl2b1.tar.gz");
				break;
				
			case "3.5":
				u = new Uri ("http://mono.ximian.com/masterinfos/masterinfos-3.5.tar.gz");
				break;
				
			default:
				main.Status = "Profile is unknown";
				return;
			}

			//check to see if the online news file has been modified since it was last downloaded
			Thread async_thread = new Thread (delegate (object state) {
				HttpWebRequest request = (HttpWebRequest) WebRequest.Create (u);
				FileInfo targetInfo = new FileInfo (target);
				if (targetInfo.Exists)
					request.IfModifiedSince = targetInfo.CreationTime;

				try {
					HttpWebResponse response = (HttpWebResponse)request.GetResponse ();
					if (response.StatusCode == HttpStatusCode.OK) {
						Application.Invoke (delegate {
							main.Status = "Downloading masterinfo file...";
						});
						Stream responseStream = response.GetResponseStream ();
						using (FileStream fs = File.Create (target)) {
							int position = 0;
							int readBytes = -1;
							byte[] buffer = new byte[4096];
							while (position < response.ContentLength) {
								readBytes = responseStream.Read (buffer, 0, buffer.Length);
								if (readBytes > 0) {
									position += readBytes;
									fs.Write (buffer, 0, readBytes);
									if (response.ContentLength > 0) {
										Application.Invoke (delegate {
											main.Progress = ((double)position / response.ContentLength) * 100;
										});
									}
								}
							}
						}
						
						Application.Invoke (delegate {
							main.Status = "Unpacking masterinfo file...s";
						});
						
						ProcessStartInfo pi = new ProcessStartInfo();
						pi.WorkingDirectory = pdir;
						pi.UseShellExecute = true;
						pi.FileName = "tar xzf " + target + " --strip-components=1";
						Process p = Process.Start (pi);
						p.WaitForExit ();
							
						Application.Invoke (delegate {
							main.Progress = 0;
							main.Status = "Download complete";
							if (done != null)
								done (masterinfo);
						});
					}
				}
				catch (System.Net.WebException wex) {
					if (wex != null && wex.Response != null && ((HttpWebResponse)wex.Response).StatusCode == HttpStatusCode.NotModified) {
						Console.WriteLine ("remote file not modified since we downloaded it");
						Application.Invoke (delegate {
							main.Progress = 0;
							main.Status = "";
							if (done != null)
								done (masterinfo);
						});
					}
					else {
						Application.Invoke (delegate {
							main.Progress = 0;
							main.Status = "Download failed";
						});
					}
				}
				catch (Exception e) {
					Console.WriteLine (e);
					Application.Invoke (delegate {
						main.Progress = 0;
						main.Status = "Download failed";
					});
				}
			});
			
			Console.WriteLine ("Downloading {0} to {1}", u, target);
			async_thread.Start ();
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
				CompareDefinition cd = new CompareDefinition (true, masterinfo, false, assemblyfile);
				cd.Title = assemblyname;
				main.Config.AddRecent (cd);
				PopulateRecent ();
				main.Config.Save ();
				
				main.SetReference (delegate {
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
		
		void Populate (Menu container, string caption, string pdir, string collection, string [] elements)
		{
			string profiledir = System.IO.Path.Combine (monodir, pdir);
			
			MenuItem item = new MenuItem (caption);
			Menu sub = new Menu ();
			item.Submenu = sub;
			
			MenuItem child = null;
			foreach (string e in elements){
				
				if (e == String.Empty){
					// Avoid inserting separators twice
					if (child is SeparatorMenuItem)
						continue;
					child = new SeparatorMenuItem ();
				} else {
					string assemblyfile = System.IO.Path.Combine (profiledir, e + ".dll");
					string element = e;
					if (!System.IO.File.Exists (assemblyfile)){
						Console.WriteLine ("Skipping {0} as {1} does not have it", e, profiledir);
						continue;
					}
					child = new MenuItem (e);
					child.Activated += delegate {
						StartPresetCompare (assemblyfile, collection, element);
					};
				}
				sub.Add (child);
			}
			
			item.ShowAll ();
			container.Add (item);
		}
		
		/// <summary>
		///   Populates the "RecentComparison" sub menu from File
		/// </summary>
		public void PopulateRecent ()
		{
			foreach (MenuItem mi in recentmenu.AllChildren)
				recentmenu.Remove (mi);
			
			if (main.Config.Recent == null || main.Config.Recent.Length == 0){
				MenuItem empty = new MenuItem ("(Empty)");
				empty.Sensitive = false;
				recentmenu.Add (empty);				
			} else {
				foreach (CompareDefinition cdd in main.Config.Recent){
					CompareDefinition cd = cdd;
					if (cd == null)
						throw new Exception ("FGGG");
				
					MenuItem c = new MenuItem (cd.ToString ());
					c.Activated += delegate {
						main.SetCompareDefinition (cd);

						main.StartCompare (delegate { main.Title = cd.ToString ();});
						main.Config.MoveToTop (cd);
						PopulateRecent ();
						main.Config.Save ();
					};
					
					recentmenu.Add (c);
				}
			}
			recentmenu.ShowAll ();
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
			
			// Populate File/Recent Comparisons, this would be so much
			// easier if Stetic had any support for this.
			recentmenu = new Menu ();
			
			MenuItem filemenuitem = (main.MenuBar.Children [0]) as MenuItem;
			Menu filesub = (Menu) filemenuitem.Submenu;
			MenuItem recentmenuitem = new MenuItem ("Recent Comparisons");
			recentmenuitem.Submenu = recentmenu;
			recentmenuitem.ShowAll ();
			filesub.Insert (recentmenuitem, 0);
			MenuItem sep = new MenuItem ();
			sep.ShowAll ();
			filesub.Insert (sep, 1);

			PopulateRecent ();
			
			
			// Populate the list of profile comparisons
			Menu sub = null;
			
			foreach (MenuItem mi in main.MenuBar.AllChildren){
				AccelLabel a = mi.Child as AccelLabel;
	
				if (a == null || a.LabelProp != "_Compare")
					continue;
				
				if (a.LabelProp == "Recent Comparisons"){
				}
				sub = (Menu) mi.Submenu;
				break;
			}
			
			if (sub == null){
				Console.WriteLine ("Unable to found Compare submenu");
				return;
			}
			
			MenuItem separator = new SeparatorMenuItem ();
			separator.ShowAll ();
			sub.Add (separator);
			
			Populate (sub, "API 1.1", "1.0", "1.0", api_1_1);
			Populate (sub, "API 2.0", "2.0", "2.0", api_2_0);
			Populate (sub, "API 3.0 (WxF)", "3.0", "3.0", api_3_0);
			Populate (sub, "API 3.5 (2.0 SP1 + LINQ)", "2.0", "3.5", api_3_5);
			Populate (sub, "API 3.5 (WxF SP1)", "3.0", "3.5", api_3_5_wxf);
			
			Populate (sub, "Silverlight 1.1 (Deprecated)", "2.1", "2.1", api_sl11);
			Populate (sub, "Silverlight 2.0", "2.1", "SL2", api_sl2_beta);
		}
	}
}
