//
// DataAccess.cs
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
using System.Text;
using GuiCompare;

namespace Mono.WebCompareDB {
	abstract class DataAccess {
		static string cnc_string;
		protected static string [] default_delete_tables = { "master", "nodes", "messages" };

		static DataAccess ()
		{
			NameValueCollection col = ConfigurationManager.AppSettings;
			cnc_string = col ["WebCompareDB"];
			if (String.IsNullOrEmpty (cnc_string))
				throw new ApplicationException ("Missing connection string from configuration file.");
		}

		public DataAccess ()
		{
		}

		protected string ConnectionString {
			get { return cnc_string; }
		}

		protected abstract IDbConnection GetConnection ();
		protected abstract void LoadFile (IDbConnection cnc, string table, string file_name);
		protected abstract void EnableNewMaster (IDbConnection cnc, int new_master_id);
		protected abstract int InsertMaster (IDbConnection cnc, State state);

		public void InsertRoot (State state)
		{
			using (IDbConnection cnc = GetConnection ()) {
				state.MasterId = InsertMaster (cnc, state);
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


		protected virtual string GetParameterNameForQuery (string pname)
		{
			return "@" + pname;
		}

		protected virtual void InsertTree (IDbConnection cnc, State state, ComparisonNode node, string base_name, int node_id)
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

		protected virtual void InsertMessages (State state, string node_name, List<string> strs, bool is_todo)
		{
			if (strs == null || strs.Count == 0)
				return;

			StringBuilder sb = new StringBuilder ();
			foreach (string s in strs) {
				sb.AppendFormat ("\\N\t{0}\t{1}\t{2}\t{3}", node_name, state.MasterId, (is_todo) ? (char) 1 : (char) 0, FormatString (s));
				state.MessagesWriter.WriteLine (sb);
				sb.Length = 0;
			}
		}

		public void DeleteTables ()
		{
			DeleteTables (default_delete_tables);
		}

		protected virtual void DeleteTables (string [] tables)
		{
			using (IDbConnection cnc = GetConnection ()) {
				foreach (string tbl in tables) {
					IDbCommand cmd = cnc.CreateCommand ();
					cmd.CommandText = String.Format ("DELETE FROM {0}", tbl);
					cmd.ExecuteNonQuery ();
				}
			}
		}

		public virtual Filters GetFilters ()
		{
			Filters filters = null;
			using (IDbConnection cnc = GetConnection ()) {
				IDbCommand cmd = cnc.CreateCommand ();
				cmd.CommandText = "SELECT is_rx, name_filter, typename_filter FROM filters";
				using (IDataReader reader = cmd.ExecuteReader ()) {
					filters = new Filters (reader);
				}
			}
			return filters;
		}

		protected virtual void DeleteInactive (State state)
		{
			using (IDbConnection cnc = GetConnection ()) {
				IDbCommand cmd = cnc.CreateCommand ();
				cmd.CommandText = String.Format (
					"SELECT id FROM master WHERE active = FALSE AND reference = {0} AND profile = {1} AND assembly = {2}",
						GetParameterNameForQuery ("reference"),
						GetParameterNameForQuery ("profile"),
						GetParameterNameForQuery ("assembly"));
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

		protected virtual void CleanupTables (IDbConnection cnc, List<int> ids)
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

		protected static IDataParameter AddParameter (IDbCommand cmd, string name, object val)
		{
			IDataParameter p = cmd.CreateParameter ();
			p.ParameterName = name;
			p.Value = val;
			cmd.Parameters.Add (p);
			return p;
		}

		protected static IDataParameter AddOutputParameter (IDbCommand cmd, string name)
		{
			IDataParameter p = cmd.CreateParameter ();
			p.ParameterName = name;
			p.Direction = ParameterDirection.Output;
			cmd.Parameters.Add (p);
			return p;
		}

		protected static string GetNodeName (string base_name, int node_id)
		{
			if (base_name == null)
				return node_id.ToString ();
			return String.Format ("{0}-{1}", base_name, node_id);
		}

		protected virtual string FormatString (string str)
		{
			if (String.IsNullOrEmpty (str))
				return "\\N";

			return str.Replace ("\t", "\\\t").Replace ('\n', ' ').Replace ('\r', ' ').Replace ("\0", "");
		}

		protected virtual void AppendValue (StringBuilder sb, object o)
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
	}
}

