// Main.cs created with MonoDevelop
// User: lupus at 12:08 PM 9/18/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using Gtk;

namespace mperfmon
{
	class MainClass
	{

		static void Report (Config cfg, string name, string[] args, int start, int count)
		{
			List<string> instances = new List<string> ();
			for (; start < args.Length; ++start)
				instances.Add (args [start]);
			if (instances.Count == 0) {
				Console.WriteLine ("Need to provide instance names.");
				return;
			}
			CounterSet cset = cfg [name];
			if (cset == null) {
				Console.WriteLine ("Unknown counter set: {0}.", name);
				return;
			}
			List<PerformanceCounter> counters = ReportList (cset, instances);
			for (int i = 0; i < count; ++i) {
				Report (counters);
				System.Threading.Thread.Sleep ((int)cfg.Timeout);
			}
		}

		static void Report (List<PerformanceCounter> counters)
		{
			string last_instance = null;
			foreach (PerformanceCounter c in counters) {
				string instance = c.InstanceName;
				if (last_instance != instance) {
					Console.WriteLine ("Report for: {0}", instance);
					last_instance = instance;
				}
				Console.Write ("\t{0}/{1}: ", c.CategoryName, c.CounterName);
				try {
					Console.WriteLine (c.NextValue ());
				} catch (Exception e) {
					Console.WriteLine (e.Message);
				}	
			}
		}
		
		static List<PerformanceCounter> ReportList (CounterSet  cset, List<string> instances)
		{
			List<string> counters = cset.Counters;
			List<PerformanceCounter> pcounters = new List<PerformanceCounter> ();
			foreach (string instance in instances) {
				for (int i = 0; i < counters.Count; i += 2) {
					try {
						PerformanceCounter counter = new PerformanceCounter (counters [i], counters [i + 1], instance);
						pcounters.Add (counter);
					} catch (Exception e) {
						//Console.WriteLine (e.Message);
					}
				}
			}
			return pcounters;
		}

		public static void Main (string[] args)
		{
			string report_name = null;
			int report_args = args.Length;
			int count = 2;
			Config cfg;
			string cfg_file = Path.Combine (Path.GetDirectoryName (Assembly.GetExecutingAssembly ().Location), "config");

			for (int i = 0; i < args.Length; ++i) {
				if (args [i].StartsWith ("--report=")) {
					report_name = args [i].Substring (9);
					continue;
				} else if (args [i].StartsWith ("--count=")) {
					count = int.Parse (args [i].Substring (8));
					continue;
				} else if (args [i].StartsWith ("--config=")) {
					cfg_file = args [i].Substring (9);
					continue;
				}
				report_args = i;
				break;
			}
	
			cfg = new Config (cfg_file, true);
			if (report_name != null) {
				Report (cfg, report_name, args, report_args, count);
				return;
			}
			Application.Init ();
			MainWindow win = new MainWindow (cfg);
			win.Show ();
			Application.Run ();
		}
	}
}