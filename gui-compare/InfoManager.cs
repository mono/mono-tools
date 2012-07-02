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

		// The base directory where the Moonlight plugin stores its profile assemblies 
		string moondir;
		
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

		string [] api_sl2 = {
			"mscorlib",
			"System",
			"System.Core",
			"System.Net",
			"System.Runtime.Serialization",
			"System.ServiceModel",
			"System.ServiceModel.Web",
			"System.Windows",
			"System.Windows.Browser",
			"System.Xml",
			"Microsoft.VisualBasic",
			"",
			// sdk assemblies:
			"System.Xml.Linq",
			"System.Windows.Controls",
			"System.Windows.Controls.Data",
		};
		
		string [] api_sl4 = {
			"mscorlib",
			"System",
			"System.Core",
			"System.Net",
			"System.Runtime.Serialization",
			"System.ServiceModel",
			"System.ServiceModel.Web",
			"System.Windows",
			"System.Windows.Browser",
			"System.Xml",
			"Microsoft.VisualBasic",
			"",
			// sdk assemblies:
			"Microsoft.CSharp",
			"System.ComponentModel.Composition",
			"System.ComponentModel.Composition.Initialization",
			"System.ComponentModel.DataAnnotations",
			"System.Data.Services.Client",
			"System.Json",
			"System.Numerics",
			"System.Runtime.Serialization.Json",
			"System.ServiceModel.Extensions",
			"System.ServiceModel.NetTcp",
			"System.ServiceModel.PollingDuplex",
			"System.ServiceModel.Syndication",
			"System.ServiceModel.Web.Extensions",
			"System.Windows.Controls.Data",
			"System.Windows.Controls.Data.Input",
			"System.Windows.Controls",
			"System.Windows.Controls.Input",
			"System.Windows.Controls.Navigation",
			"System.Windows.Data",
			"System.Xml.Linq",
			"System.Xml.Serialization",
			"System.Xml.Utils",
			"System.Xml.XPath"
		};

		string [] api_3_5 = {
			"mscorlib",
			"System",
			"System.AddIn",
			"System.AddIn.Contract",
			"System.Configuration",
			"System.ComponentModel.DataAnnotations",
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
			"Microsoft.Build.Conversion.v3.5",
			"Microsoft.Build.Utilities.v3.5",
			"",
			"System.Configuration.Install",
			"System.Design",
			"System.Drawing.Design",
			"System.EnterpriseServices",
			"System.Management",
			// "System.Management.Instrumentation",
			"System.Messaging",
		};

		string [] api_4_0 = {
			"mscorlib",

			"System.Activities",			
			"System.Activities.Core.Presentation",
			"System.Activities.DurableInstancing",
			"System.Activities.Design",
			"System.AddIn.Contract",
			"System.AddIn",
			"System.ComponentModel.Composition",
			"System.ComponentModel.DataAnnotations",
			"System.configuration",
		//	"System.Configuration.Install",
			"System.Core",
			"System.Data.DataSetExtensions",
			"System.Data",
			"System.Data.Entity.Design",
			"System.Data.Entity",
			"System.Data.Linq",
			"System.Data.OracleClient",
			"System.Data.Services.Client",
			"System.Data.Services.Design",
			"System.Data.Services",
			"System.Data.SqlXml",
			"System.Deployment",
			"System.Design",
			"System.Device",
		//	"System.DirectoryServices.AccountManagement",
			"System.DirectoryServices",
		//	"System.DirectoryServices.Protocols",
			"System",
			"System.Drawing.Design",
			"System.Drawing",
			"System.Dynamic",
			"System.EnterpriseServices",
			"System.EnterpriseServices.Thunk",
			"System.EnterpriseServices.Wrapper",
			"System.IdentityModel",
			"System.IdentityModel.Selectors",
			"System.IO.Log",
			"System.Management",
		//	"System.Management.Instrumentation",
			"System.Messaging",
			"System.Net",
			"System.Numerics",
			"System.Printing",
			"System.Runtime.Caching",
			"System.Runtime.Remoting",
			"System.Runtime.Serialization",
			"System.Runtime.Serialization.Formatters.Soap",
			"System.Security",
			"System.ServiceModel.Activation",
			"System.ServiceModel.Activities",
			"System.ServiceModel.Channels",
			"System.ServiceModel.Discovery",
			"System.ServiceModel",
			"System.ServiceModel.Routing",
			"System.ServiceModel.Web",
			"System.ServiceProcess",
			"System.Speech",
			"System.Transactions",
			"System.Web.Abstractions",
			"System.Web.ApplicationServices",
			"System.Web.DataVisualization.Design",
			"System.Web.DataVisualization",
			"System.Web",
			"System.Web.DynamicData.Design",
			"System.Web.DynamicData",
			"System.Web.Entity.Design",
			"System.Web.Entity",
		//	"System.Web.Extensions.Design",
			"System.Web.Extensions",
		//	"System.Web.Mobile",
		//	"System.Web.RegularExpressions",
			"System.Web.Routing",
			"System.Web.Services",
			"System.Windows.Forms.DataVisualization.Design",
			"System.Windows.Forms.DataVisualization",
			"System.Windows.Forms",
			"System.Windows.Presentation",
			"System.Workflow.Activities",
			"System.Workflow.ComponentModel",
			"System.Workflow.Runtime",
			"System.WorkflowServices",
			"System.Xaml",
			"System.Xaml.Hosting",
			"System.Xml",
			"System.Xml.Linq",

			"Microsoft.Build.Conversion.v4.0",
			"Microsoft.Build",
			"Microsoft.Build.Engine",
			"Microsoft.Build.Framework",
			"Microsoft.Build.Tasks.v4.0",
			"Microsoft.Build.Utilities.v4.0",
			"Microsoft.CSharp",
			"Microsoft.JScript",
			"Microsoft.VisualBasic.Compatibility.Data",
			"Microsoft.VisualBasic.Compatibility",
			"Microsoft.VisualBasic",

			"PresentationBuildTasks",
			"PresentationCore",
			"PresentationFramework.Aero",
			"PresentationFramework.Classic",
			"PresentationFramework",
			"PresentationFramework.Luna",
			"PresentationFramework.Royale",
			"PresentationUI",
			"ReachFramework",

			"WindowsBase",
		//	"XamlBuildTask"
		};

		string [] api_4_5 = {
			"mscorlib",

			"System.Activities",			
			"System.Activities.Core.Presentation",
			"System.Activities.DurableInstancing",
			"System.Activities.Design",
			"System.AddIn.Contract",
			"System.AddIn",
			"System.ComponentModel.Composition",
			"System.ComponentModel.DataAnnotations",
			"System.configuration",
		//	"System.Configuration.Install",
			"System.Core",
			"System.Data.DataSetExtensions",
			"System.Data",
			"System.Data.Entity.Design",
			"System.Data.Entity",
			"System.Data.Linq",
			"System.Data.OracleClient",
			"System.Data.Services.Client",
			"System.Data.Services.Design",
			"System.Data.Services",
			"System.Data.SqlXml",
			"System.Deployment",
			"System.Design",
			"System.Device",
		//	"System.DirectoryServices.AccountManagement",
			"System.DirectoryServices",
		//	"System.DirectoryServices.Protocols",
			"System",
			"System.Drawing.Design",
			"System.Drawing",
			"System.Dynamic",
			"System.EnterpriseServices",
			"System.EnterpriseServices.Thunk",
			"System.EnterpriseServices.Wrapper",
			"System.IdentityModel",
			"System.IdentityModel.Selectors",
			"System.IO.Log",
			"System.IO.Compression",
			"System.IO.Compression.FileSystem",
			"System.Management",
		//	"System.Management.Instrumentation",
			"System.Messaging",
			"System.Net",
			"System.Net.Http",
			"System.Net.Http.WebRequest",
			"System.Numerics",
			"System.Printing",
			"System.Runtime.Caching",
			"System.Runtime.Remoting",
			"System.Runtime.Serialization",
			"System.Runtime.Serialization.Formatters.Soap",
			"System.Security",
			"System.ServiceModel.Activation",
			"System.ServiceModel.Activities",
			"System.ServiceModel.Channels",
			"System.ServiceModel.Discovery",
			"System.ServiceModel",
			"System.ServiceModel.Routing",
			"System.ServiceModel.Web",
			"System.ServiceProcess",
			"System.Speech",
			"System.Threading.Tasks.Dataflow",
			"System.Transactions",
			"System.Web.Abstractions",
			"System.Web.ApplicationServices",
			"System.Web.DataVisualization.Design",
			"System.Web.DataVisualization",
			"System.Web",
			"System.Web.DynamicData.Design",
			"System.Web.DynamicData",
			"System.Web.Entity.Design",
			"System.Web.Entity",
		//	"System.Web.Extensions.Design",
			"System.Web.Extensions",
		//	"System.Web.Mobile",
		//	"System.Web.RegularExpressions",
			"System.Web.Routing",
			"System.Web.Services",
			"System.Windows.Forms.DataVisualization.Design",
			"System.Windows.Forms.DataVisualization",
			"System.Windows.Forms",
			"System.Windows.Presentation",
			"System.Workflow.Activities",
			"System.Workflow.ComponentModel",
			"System.Workflow.Runtime",
			"System.WorkflowServices",
			"System.Xaml",
			"System.Xaml.Hosting",
			"System.Xml",
			"System.Xml.Linq",

			"Microsoft.Build.Conversion.v4.0",
			"Microsoft.Build",
			"Microsoft.Build.Engine",
			"Microsoft.Build.Framework",
			"Microsoft.Build.Tasks.v4.0",
			"Microsoft.Build.Utilities.v4.0",
			"Microsoft.CSharp",
			"Microsoft.JScript",
			"Microsoft.VisualBasic.Compatibility.Data",
			"Microsoft.VisualBasic.Compatibility",
			"Microsoft.VisualBasic",

			"PresentationBuildTasks",
			"PresentationCore",
			"PresentationFramework.Aero",
			"PresentationFramework.Classic",
			"PresentationFramework",
			"PresentationFramework.Luna",
			"PresentationFramework.Royale",
			"PresentationUI",
			"ReachFramework",

			"WindowsBase",
		//	"XamlBuildTask"
		};

		const string masterinfos_version = "2.8";

		static Uri GetMasterInfoUri (string file)
		{
			return new Uri (string.Format ("http://go-mono.com/masterinfos/{0}/{1}", masterinfos_version, file));
		}
		
		public static void Init ()
		{
			
			string xdg_data_home = Environment.GetEnvironmentVariable ("XDG_DATA_HOME");
			if (xdg_data_home == null || xdg_data_home == String.Empty || xdg_data_home.IndexOf (':') != -1){
				xdg_data_home = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);
			}
			
			infos = Path.Combine (xdg_data_home, "mono-gui-compare");
			Directory.CreateDirectory (infos);
		}

		// 
		// Ensures that masterinfo file for assembly name @file in @prof profile
		// is downloaded;   On success, executes the given callback
		//
		public void Ensure (string prof, string assemblyname, Action<string> done)
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
				u = GetMasterInfoUri ("masterinfos-1.1.tar.gz");
				break;
				
			case "2.0":
				u = GetMasterInfoUri ("masterinfos-2.0.tar.gz");
				break;
				
			case "3.0":
				u = GetMasterInfoUri ("masterinfos-3.0.tar.gz");
				break;
				
			case "3.5":
				u = GetMasterInfoUri ("masterinfos-3.5.tar.gz");
				break;
				
			case "4.0":
				u = GetMasterInfoUri ("masterinfos-4.0.tar.gz");
				break;

			case "4.5":
				u = GetMasterInfoUri ("masterinfos-4.5.tar.gz");
				break;

			case "SL2":
				u = GetMasterInfoUri ("masterinfos-SL2.tar.gz");
				break;

			case "SL3":
				u = GetMasterInfoUri ("masterinfos-SL3.tar.gz");
				break;
			
			case "SL4":
				u = GetMasterInfoUri ("masterinfos-SL4.tar.gz");
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
						pi.FileName = "tar";
						pi.Arguments = "xzf " + target + " --strip-components=1";
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
							FileInfo masterinfoInfo = new FileInfo (masterinfo);
							if (masterinfoInfo.Exists) {
								main.Status = "Download failed, reusing cached (possibly out of date) masterinfo";
								if (done != null)
									done (masterinfo);
							}
							else
								main.Status = "Download failed";
						});
					}
				}
				catch (Exception e) {
					Console.WriteLine (e);
					Application.Invoke (delegate {
						main.Progress = 0;
						FileInfo masterinfoInfo = new FileInfo (masterinfo);
						if (masterinfoInfo.Exists) {
							main.Status = "Download failed, reusing cached (possibly out of date) masterinfo";
							if (done != null)
								done (masterinfo);
						}
						else
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
		void StartPresetCompare (string assemblyfile, string profile, string assemblyname, string groupName)
		{
			Ensure (profile, assemblyname, delegate (string masterinfo){
				CompareDefinition cd = new CompareDefinition (true, masterinfo, false, assemblyfile);
				cd.Title = assemblyname + " (" + groupName + ")";
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
					main.Title = String.Format ("{0} to {1}", assemblyfile, masterinfo);
				});
			});
			
			main.SetComparedProfile (profile);
		}

		void Populate (Menu container, string caption, string pdir, string collection, string [] elements)
		{
			string profiledir = System.IO.Path.Combine (monodir, pdir);
			string MONO_GAC_PREFIX = Environment.GetEnvironmentVariable ("MONO_GAC_PREFIX");
			string[] gac_prefixes = null;

			if (!string.IsNullOrEmpty (MONO_GAC_PREFIX))
				gac_prefixes = MONO_GAC_PREFIX.Split (':');

			MenuItem item = new MenuItem (caption);
			Menu sub = new Menu ();
			item.Submenu = sub;
			
			MenuItem child = null;
			foreach (string e in elements){
				
				if (e == String.Empty){
					// Avoid inserting separators twice
					if (child is SeparatorMenuItem || sub.Children.Length == 0)
						continue;
					child = new SeparatorMenuItem ();
				} else {
					string assemblyfile = null;
					bool found = false;

					if (gac_prefixes == null) {
						assemblyfile = System.IO.Path.Combine (profiledir, e + ".dll");
						found = System.IO.File.Exists (assemblyfile);
					}
					else {
						foreach (string prefix in gac_prefixes) {
							assemblyfile = System.IO.Path.Combine (
									       System.IO.Path.Combine (
										       System.IO.Path.Combine (
											       System.IO.Path.Combine (prefix, "lib"),
											       "mono"),
										       pdir),
									       e + ".dll");
							found = System.IO.File.Exists (assemblyfile);
							if (found)
								break;
						}
					}
					if (!found) {
						assemblyfile = System.IO.Path.Combine (moondir, e + ".dll");
						found = System.IO.File.Exists (assemblyfile);
					}

					if (!found) {
						Console.WriteLine ("Skipping {0} for profile {1}, could not locate it in profile dir or MONO_GAC_PREFIX", e, pdir);
						continue;
					}

					string element = e;
					child = new MenuItem (e);
					child.Activated += delegate {
						StartPresetCompare (assemblyfile, collection, element, caption);
					};
				}
				sub.Add (child);
			}

			if (sub.Children.Length > 0) {
				item.ShowAll ();
				container.Add (item);
			}
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
		public InfoManager (MainWindow main, string profilePath)
		{
			this.main = main;
			
			if (profilePath == null) {
				string corlibdir = System.IO.Path.GetDirectoryName (typeof (int).Assembly.Location);
				monodir = System.IO.Path.GetFullPath (System.IO.Path.Combine (corlibdir, "..")); 
			} else {
				monodir = profilePath;
			}
			
			moondir = System.IO.Path.Combine (monodir, @"../moonlight/plugin");
	
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
			
			Populate (sub, "API 1.1", GetVersionPath ("1.0", "net_1_1"), "1.0", api_1_1);
			Populate (sub, "API 2.0 sp2", GetVersionPath ("2.0", "net_2_0"), "2.0", api_2_0);
			Populate (sub, "API 3.0 sp1", GetVersionPath ("2.0", "net_2_0"), "3.0", api_3_0);
			Populate (sub, "API 3.5 sp1", GetVersionPath ("2.0", "net_2_0"), "3.5", api_3_5);
			Populate (sub, "API 4.0", GetVersionPath ("4.0", "net_4_0"), "4.0", api_4_0);
			Populate (sub, "API 4.5", GetVersionPath ("4.5", "net_4_5"), "4.5", api_4_5);
//			Populate (sub, "Silverlight 2.0", GetVersionPath ("2.1", "net_2_1"), "SL2", api_sl2);
//			Populate (sub, "Silverlight 3.0", GetVersionPath ("2.1", "net_2_1"), "SL3", api_sl2);
			Populate (sub, "Silverlight 4.0", GetVersionPath ("2.1", "net_2_1"), "SL4", api_sl4);
		}
		
		string GetVersionPath (string version, string profile)
		{
			if (!monodir.Contains (Path.Combine ("mcs", "class", "lib")))
				return version;

			// Developer's version pointing to /mcs/class/lib/<profile>/
			return profile;
		}
	}
}
