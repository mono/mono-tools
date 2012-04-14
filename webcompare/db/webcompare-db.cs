//
// webcompare-db.cs
//
// Authors:
//      Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) Copyright 2009 Novell, Inc
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
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GuiCompare;

namespace Mono.WebCompareDB {
	class Populate {
		static DataAccess GetDataAccess ()
		{
			return new MySqlDataAccess ();
			//return new PostgresDataAccess ();
		}

		static void Help ()
		{
			Console.WriteLine ("    Compares masterinfos to Mono assemblies and stores the data in a DB.");
			Console.WriteLine ("    The masterinfos are expected to be in ../masterinfos and the assemblies");
			Console.WriteLine ("    in ../binary.");
			Console.WriteLine ();
			Console.WriteLine ("    When invoked with no arguments it is equivalent to:");
			Console.WriteLine ();
			Console.WriteLine ("       webcompare-db.exe '4.5 4.5' '4.0 4.0'");
			Console.WriteLine ();
			Console.WriteLine ("    The first argument of each pair is a directory in ../masterinfos.");
			Console.WriteLine ("    The second argument of each pair is a directory in ../binary.");
			Console.WriteLine ();
			Console.WriteLine ("    --help: displays this help");
			Console.WriteLine ("    --delete-tables: delete ALL the data in ALL the tables and exits.");
			Console.WriteLine ("    --assemblies A1[,A2,...]: comma-separated list of assemblies to compare.");
			Console.WriteLine ("                              All other assemblies are ignored.");
			Console.WriteLine ();
		}

		static string [] default_compares = new string [] { "4.5 4.5", "4.0 4.0" };

		static int Main (string [] args)
		{
			List<string> compares = new List<string>();
			List<string> include_list = new List<string> ();
			bool got_assemblies = false;
			foreach (string arg in args) {
				if (got_assemblies) {
					string [] strs = arg.Split (',');
					foreach (string s in strs)
						include_list.Add (s);
					got_assemblies = false;
					continue;
				}
				if (arg == "--help") {
					Help ();
					return 0;
				}
				if (arg == "--delete-tables") {
					DeleteTables ();
					Console.WriteLine ("Tables deleted");
					return 0;
				}
				if (arg == "--assemblies") {
					got_assemblies = true;
					continue;
				}

				string [] compare = arg.Split ();
				if (compare.Length != 2) {
					Console.Error.WriteLine ("Invalid argument: {0}", arg);
					return 1;
				}
				compares.Add (arg);
			}

			if (got_assemblies) {
				Console.Error.WriteLine ("Assembly list not provided for --assemblies");
				return 1;
			}

			string [] actual_compares = null;
			if (compares.Count == 0)
				actual_compares = default_compares;
			else
				actual_compares = compares.ToArray ();

			CreateWorkItems (actual_compares, include_list);
			Thread [] comparers = new Thread [1];
			for (int i = comparers.Length - 1; i >= 0; i--) {
				comparers [i]= new Thread (PerformComparison);
				comparers [i].Start ();
			}
			Thread [] dbupdaters = new Thread [2];
			for (int i = dbupdaters.Length - 1; i >= 0; i--) {
				dbupdaters [i]= new Thread (UpdateDB);
				dbupdaters [i].Start ();
			}
			for (int i = comparers.Length - 1; i >= 0; i--)
				comparers [i].Join ();
			for (int i = dbupdaters.Length - 1; i >= 0; i--)
				dbupdaters [i].Join ();

			return 0;
		}

		static void PerformComparison ()
		{
			int end = work_items.Count;
			for (int i = 0; i < end; i++) {
				State state = work_items [i];
				if (Interlocked.CompareExchange (ref state.AlreadyComparing, 1, 0) == 1)
					continue;
				Console.WriteLine ("Comparing {0} {1} {2}", state.Reference, state.Profile, state.Assembly);
				state.Root = MakeComparisonNode (state.InfoFile, state.DllFile);
				state.UpdateLock.Set ();
			}
		}

		static void UpdateDB ()
		{
			DataAccess da = GetDataAccess ();
			int end = work_items.Count;
			for (int i = 0; i < end; i++) {
				State state = work_items [i];
				if (Interlocked.CompareExchange (ref state.LockInUse, 1, 0) == 1)
					continue;
				state.UpdateLock.WaitOne ();
				if (state.Root != null) {
					Console.WriteLine ("Inserting {0} {1} {2}", state.Reference, state.Profile, state.Assembly);
					state.DetailLevel = "detailed";
					da.InsertRoot (state);
					state.Root.ResetCounts ();
					FilterRoot (state.Root);
					state.Root.PropagateCounts ();
					state.DetailLevel = "normal";
					da.InsertRoot (state);
				} else {
					Console.WriteLine ("No insertions for {0} {1} {2}", state.Reference, state.Profile, state.Assembly);
				}
				state.UpdateLock.Close ();
				state.UpdateLock = null;
				state.Root = null;
			}
		}

		static void DeleteTables ()
		{
			DataAccess da = GetDataAccess ();
			da.DeleteTables ();
		}

		static List<State> work_items = new List<State> ();
		static void CreateWorkItems (string [] compares, List<string> include_list)
		{
			foreach (string str in compares) {
				string [] s = str.Split ();
				string reference = s [0];
				string profile = s [1];
				string mpath = "../masterinfos/" + reference;
				string bpath = "../binary/" + profile;
				if (!Directory.Exists (mpath))
					continue;

				if (!Directory.Exists (bpath))
					continue;
			
				var infos = from p in Directory.GetFiles (mpath)
					select Path.GetFileNameWithoutExtension (p);

				var dlls  = from p in Directory.GetFiles (bpath)
					select Path.GetFileNameWithoutExtension (p);

				foreach (var assembly in (from p in infos.Intersect (dlls) orderby p select p)) {
					if (include_list.Count > 0 && include_list.IndexOf (assembly) == -1)
						continue;
					string info_file = Path.Combine (mpath, assembly + ".xml");
					string dll_file = Path.Combine (bpath, assembly + ".dll");
					State state = new State (reference, profile, assembly, info_file, dll_file);
					work_items.Add (state);
				}
			}
		}

		static ComparisonNode MakeComparisonNode (string info_file, string dll_file)
		{
			if (!File.Exists (info_file)) {
				Console.Error.WriteLine ("{0} does not exist", info_file);
				return null;
			}
			if (!File.Exists (dll_file)) {
				Console.Error.WriteLine ("{0} does not exist", dll_file);
				return null;
			}
		
			CompareContext cc = new CompareContext (
				() => new MasterAssembly (info_file),
				() => new CecilAssembly (dll_file));

			cc.ProgressChanged += delegate (object sender, CompareProgressChangedEventArgs a){
				//Console.Error.WriteLine (a.Message);
			};
			bool have_error = false;
			cc.Error += delegate (object sender, CompareErrorEventArgs args) {
				have_error = true;
				Console.Error.WriteLine ("Error loading {0}: {1}", info_file, args.Message.Split (Environment.NewLine.ToCharArray ())[0]);
			};
			ManualResetEvent r = new ManualResetEvent (false);
			cc.Finished += delegate { r.Set (); };
			cc.Compare ();
			r.WaitOne ();
			if (have_error)
				return null;
			cc.Comparison.PropagateCounts ();
			return cc.Comparison;
		}

		static void FilterRoot (ComparisonNode node)
		{
			Filters filters = GetDataAccess ().GetFilters ();
			FilterNode (filters, node);
		}

		static bool FilterNode (Filters filters, ComparisonNode node)
		{
			if (filters.Filter (node.Name, node.TypeName)) {
				//Console.WriteLine ("OUT: '{0}' '{1}'", node.Name, node.TypeName);
				return true;
			}

			List<ComparisonNode> removed = null;
			foreach (ComparisonNode child in node.Children) {
				if (FilterNode (filters, child)) {
					if (removed == null)
						removed = new List<ComparisonNode> ();
					removed.Add (child);
				}
			}
			if (removed == null)
				return false;

			foreach (ComparisonNode child in removed)
				node.Children.Remove (child);

			return false;
		}
	}

	class State {
		public readonly string Reference;
		public readonly string Profile;
		public readonly string Assembly;
		public readonly string InfoFile;
		public readonly string DllFile;
		public readonly DateTime AssemblyLastWrite;
		public readonly string NodesFileName;
		public readonly string MessagesFileName;

		public ComparisonNode Root;
		public int AlreadyComparing;
		public int LockInUse;
		public ManualResetEvent UpdateLock;

		public StreamWriter NodesWriter;
		public StreamWriter MessagesWriter;
		public int MasterId;

		public string DetailLevel;

		public State (string reference, string profile, string assembly, string info_file, string dll_file)
		{
			Reference = reference;
			Profile = profile;
			Assembly = assembly;
			InfoFile = info_file;
			DllFile = dll_file;
			UpdateLock = new ManualResetEvent (false);
			long ticks = DateTime.UtcNow.Ticks;
			NodesFileName = Path.Combine (Path.GetTempPath (), String.Format ("tmpnodes{0}{1}{2}{3}", reference, profile, assembly, ticks));
			MessagesFileName = Path.Combine (Path.GetTempPath (), String.Format ("tmpmessages{0}{1}{2}{3}", reference, profile, assembly, ticks + 1));
			File.Delete (NodesFileName);
			File.Delete (MessagesFileName);
			AssemblyLastWrite = new FileInfo (dll_file).LastWriteTimeUtc;
		}
	}

	class Filters {
		const RegexOptions rx_options = RegexOptions.Compiled;
		// -null matches null
		// -"*" matches any string, including null
		List<string []> static_filters;

		// -null matches null
		List<Regex []> rx_filters;

		public Filters (IDataReader reader)
		{
			static_filters = new List<string[]> ();	
			rx_filters = new List<Regex []> ();
			while (reader.Read ()) {
				string name_filter = reader ["name_filter"] as string;
				string typename_filter = reader ["typename_filter"] as string;
				if (false == Convert.ToBoolean (reader ["is_rx"]))
					static_filters.Add (new string [] { name_filter, typename_filter });
				else {
					Regex [] rxs = new Regex [2];
					if (name_filter != null)
						rxs [0] = new Regex (name_filter, rx_options);
					if (typename_filter != null)
						rxs [1] = new Regex (typename_filter, rx_options);
					rx_filters.Add (rxs);
				}
			}
		}

		public bool Filter (string name, string typename)
		{
			bool match = false;
			foreach (string [] strs in static_filters) {
				string s1 = strs [0];
				string s2 = strs [1];
				if (s2 == "*")
					match = (s1 == strs [0]);
				else if (s1 == "*")
					match = (s2 == strs [1]);
				else
					match = (name == s1 && typename == s2);

				if (match)
					return true;
			}

			foreach (Regex [] rx in rx_filters) {
				Regex rx1 = rx [0];
				Regex rx2 = rx [1];
				if (rx1 != null && rx2 != null && rx1.IsMatch (name) && rx2.IsMatch (typename))
					match = true;
				else if (rx1 != null && typename == null && rx1.IsMatch (name))
					match = true;
				else if (rx2 != null && name == null && rx2.IsMatch (typename))
					match = true;

				if (match)
					return true;
			}

			return false;
		}
	}
}
