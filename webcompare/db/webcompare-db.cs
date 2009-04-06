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
using System.Threading;
using MySql.Data.MySqlClient;
using GuiCompare;

class Populate {
	static void Help ()
	{
		Console.WriteLine ("    Compares masterinfos to Mono assemblies and stores the data in a DB.");
		Console.WriteLine ("    The masterinfos are expected to be in ../masterinfos and the assemblies");
		Console.WriteLine ("    in ../binary.");
		Console.WriteLine ();
		Console.WriteLine ("    When invoked with no arguments it is equivalent to:");
		Console.WriteLine ();
		Console.WriteLine ("       webcompare-db.exe '3.5 2.0' 'SL2 2.1' '2.0 2.0' '1.1 1.0'");
		Console.WriteLine ();
		Console.WriteLine ("    The first argument of each pair is a directory in ../masterinfos.");
		Console.WriteLine ("    The second argument of each pair is a directory in ../binary.");
		Console.WriteLine ();
		Console.WriteLine ("    --help: displays this help");
		Console.WriteLine ("    --delete-tables: delete ALL the data in ALL the tables and exits.");
		Console.WriteLine ();
	}

	static string [] default_compares = new string [] { "3.5 2.0", "SL2 2.1", "2.0 2.0", "1.1 1.0" };
	static string cnc_string;

	static int Main (string [] args)
	{
		NameValueCollection col = ConfigurationManager.AppSettings;
		cnc_string = col ["WebCompareDB"];
		if (String.IsNullOrEmpty (cnc_string)) {
			Console.Error.WriteLine ("Missing connection string from configuration file.");
			return 1;
		}
		List<string> compares = new List<string>();
		foreach (string arg in args) {
			if (arg == "--help") {
				Help ();
				return 0;
			}
			if (arg == "--delete-tables") {
				DeleteTables ();
				Console.WriteLine ("Tables deleted");
				return 0;
			}
			string [] compare = arg.Split ();
			if (compare.Length != 2) {
				Console.Error.WriteLine ("Invalid argument: {0}", arg);
				return 1;
			}
			compares.Add (arg);
		}

		string [] actual_compares = null;
		if (compares.Count == 0)
			actual_compares = default_compares;
		else
			actual_compares = compares.ToArray ();

		CreateWorkItems (actual_compares);
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
		int end = work_items.Count;
		for (int i = 0; i < end; i++) {
			State state = work_items [i];
			if (Interlocked.CompareExchange (ref state.LockInUse, 1, 0) == 1)
				continue;
			state.UpdateLock.WaitOne ();
			if (state.Root != null) {
				Console.WriteLine ("Inserting {0} {1} {2}", state.Reference, state.Profile, state.Assembly);
				InsertRoot (state);
			} else {
				Console.WriteLine ("No insertions for {0} {1} {2}", state.Reference, state.Profile, state.Assembly);
			}
			state.UpdateLock.Close ();
			state.UpdateLock = null;
			state.Root = null;
		}
	}

	static IDbConnection GetConnection ()
	{
		IDbConnection cnc = new MySqlConnection ();
		cnc.ConnectionString = cnc_string;
		cnc.Open ();
		return cnc;
	}

	static string [] tables = { "master", "nodes", "messages" };
	static void DeleteTables ()
	{
		using (IDbConnection cnc = GetConnection ()) {
			foreach (string tbl in tables) {
				IDbCommand cmd = cnc.CreateCommand ();
				cmd.CommandText = String.Format ("TRUNCATE TABLE {0}", tbl);
				cmd.ExecuteNonQuery ();
			}
		}
	}

	static List<State> work_items = new List<State> ();
	static void CreateWorkItems (string [] compares)
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
				string info_file = Path.Combine (mpath, assembly + ".xml");
				string dll_file = Path.Combine (bpath, assembly + ".dll");
				State state = new State (reference, profile, assembly, info_file, dll_file);
				work_items.Add (state);
			}
		}
	}

	static void InsertRoot (State state)
	{
		using (IDbConnection cnc = GetConnection ()) {
			IDbCommand cmd = cnc.CreateCommand ();
			cmd.CommandText = "insert_master";
			cmd.CommandType = CommandType.StoredProcedure;
			AddParameter (cmd, "reference", state.Reference);
			AddParameter (cmd, "profile", state.Profile);
			AddParameter (cmd, "assembly", state.Assembly);
			AddParameter (cmd, "last_updated", state.AssemblyLastWrite);
			IDataParameter p = AddOutputParameter (cmd, "id");
			cmd.ExecuteNonQuery ();
			state.MasterId = (int) p.Value;
			using (state.NodesWriter = new StreamWriter (state.NodesFileName)) {
				using (state.MessagesWriter = new StreamWriter (state.MessagesFileName)) {
					InsertTree (cnc, state, state.Root, null, 0);
				}
			}
			LoadFile (cnc, "nodes", state.NodesFileName);
			LoadFile (cnc, "messages", state.MessagesFileName);
			File.Delete (state.NodesFileName);
			File.Delete (state.MessagesFileName);
			EnableNewMaster (cnc, state.MasterId);
		}
		DeleteInactive (state);
	}

	static void LoadFile (IDbConnection cnc, string table, string file_name)
	{
		IDbCommand cmd = cnc.CreateCommand ();	
		cmd.CommandType = CommandType.Text;
		cmd.CommandText = String.Format ("LOAD DATA INFILE '{0}' INTO TABLE {1}", file_name, table);
		cmd.ExecuteNonQuery ();
	}

	static void EnableNewMaster (IDbConnection cnc, int new_master_id)
	{
		IDbCommand cmd = cnc.CreateCommand ();
		cmd.CommandText = "update_active_master";
		cmd.CommandType = CommandType.StoredProcedure;
		AddParameter (cmd, "master_id", new_master_id);
		cmd.ExecuteNonQuery ();
	}

	static void DeleteInactive (State state)
	{
		using (IDbConnection cnc = GetConnection ()) {
			IDbCommand cmd = cnc.CreateCommand ();
			cmd.CommandText = "SELECT id FROM master WHERE active = 0 AND reference = @reference AND profile = @profile AND assembly = @assembly";
			AddParameter (cmd, "reference", state.Reference);
			AddParameter (cmd, "profile", state.Profile);
			AddParameter (cmd, "assembly", state.Assembly);
			List<int> ids = new List<int> ();
			using (IDataReader reader = cmd.ExecuteReader ()) {
				while (reader.Read ()) {
					ids.Add (Convert.ToInt32 (reader [0]));
				}
			}
			CleanupTables (cnc, ids);
		}
	}

	static void CleanupTables (IDbConnection cnc, List<int> ids)
	{
		if (ids.Count == 0)
			return;
		StringBuilder sb = new StringBuilder ();
		sb.Append ('(');
		foreach (int i in ids)
			sb.AppendFormat ("{0},", i);
		sb.Length--;
		sb.Append (')');
		string str = sb.ToString ();
		IDbCommand cmd = cnc.CreateCommand ();
		cmd.CommandText = "DELETE FROM messages WHERE master_id IN " + str;
		cmd.ExecuteNonQuery ();
		cmd = cnc.CreateCommand ();
		cmd.CommandText = "DELETE FROM nodes WHERE master_id IN " + str;
		cmd.ExecuteNonQuery ();
		cmd = cnc.CreateCommand ();
		cmd.CommandText = "DELETE FROM master WHERE id IN " + str;
		cmd.ExecuteNonQuery ();
	}

	static IDataParameter AddParameter (IDbCommand cmd, string name, object val)
	{
		IDataParameter p = cmd.CreateParameter ();
		p.ParameterName = name;
		p.Value = val;
		cmd.Parameters.Add (p);
		return p;
	}

	static IDataParameter AddOutputParameter (IDbCommand cmd, string name)
	{
		IDataParameter p = cmd.CreateParameter ();
		p.ParameterName = name;
		p.Direction = ParameterDirection.Output;
		cmd.Parameters.Add (p);
		return p;
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

	static string GetNodeName (string base_name, int node_id)
	{
		if (base_name == null)
			return node_id.ToString ();
		return String.Format ("{0}-{1}", base_name, node_id);
	}

	static void AppendValue (StringBuilder sb, object o)
	{
		if (!((o is string) || (o is DBNull))) {
			if (o is bool)
				sb.AppendFormat ("{0}\t", ((bool) o) ? 1 : 0);
			else
				sb.AppendFormat ("{0}\t", o);
		} else {
			sb.AppendFormat ("{0}\t", FormatString (o as string));
		}
	}

	static void InsertTree (IDbConnection cnc, State state, ComparisonNode node, string base_name, int node_id)
	{
		StringBuilder sb = new StringBuilder ();
		string node_name = GetNodeName (base_name, node_id); 
		AppendValue (sb, node_name); // node_name
		AppendValue (sb, state.MasterId); // master_id
		AppendValue (sb, node_id); // child_id
		AppendValue (sb, base_name == null ? "-" : base_name); // parent_name
		AppendValue (sb, (int) node.Type); // comparison_type
		AppendValue (sb, (int) node.Status); // status
		AppendValue (sb, node.Extra); // extras
		AppendValue (sb, node.Missing); // missing
		AppendValue (sb, node.Present); // present
		AppendValue (sb, node.Warning); // warning
		AppendValue (sb, node.Todo); // todo
		AppendValue (sb, node.Niex); // niex
		AppendValue (sb, node.ThrowsNIE); // throwsnie
		AppendValue (sb, node.Children.Count > 0); // has_children
		AppendValue (sb, node.Messages.Count > 0 || node.Todos.Count > 0); // has_messages
		AppendValue (sb, node.Name); // name
		AppendValue (sb, node.TypeName); // typename
		sb.Length--; // remove trailing \t
		state.NodesWriter.WriteLine (sb);

		InsertMessages (state, node_name, node.Messages, false);
		InsertMessages (state, node_name, node.Todos, true);
		int counter = 0;
		foreach (ComparisonNode n in node.Children) {
			InsertTree (cnc, state, n, node_name, counter);
			counter++;
		}
	}

	static void InsertMessages (State state, string node_name, List<string> strs, bool is_todo)
	{
		if (strs == null || strs.Count == 0)
			return;

		StringBuilder sb = new StringBuilder ();
		foreach (string s in strs) {
			sb.AppendFormat ("{0}\t{1}\t{2}\t{3}", node_name, state.MasterId, (is_todo) ? 1 : 0, FormatString (s));
			state.MessagesWriter.WriteLine (sb);
			sb.Length = 0;
		}
	}

	static string FormatString (string str)
	{
		if (String.IsNullOrEmpty (str))
			return "\\N";
		return str.Replace ("\t", "\\\t").Replace ('\n', ' ').Replace ('\r', ' ');
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

