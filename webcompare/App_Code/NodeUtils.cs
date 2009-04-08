//
// NodeUtils.cs
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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web;
using System.Threading;
using GuiCompare;
using MySql.Data.MySqlClient;

public class NodeUtils {
	static string cnc_string;

	static IDbConnection GetConnection ()
	{
		if (String.IsNullOrEmpty (cnc_string))
			throw new ApplicationException ("DB connection string missing");
		IDbConnection cnc = new MySqlConnection ();
		cnc.ConnectionString = cnc_string;
		cnc.Open ();
		return cnc;
	}

	string reference;
	string profile;
	string assembly;
	int master_id;
	string detail_level;

	static NodeUtils ()
	{
		NameValueCollection col = ConfigurationManager.AppSettings;
		cnc_string = col ["WebCompareDB"];
	}

	public NodeUtils (string reference, string profile, string assembly, string detail_level)
	{
		if (String.IsNullOrEmpty (reference))
			throw new ArgumentNullException ("reference");
		if (String.IsNullOrEmpty (profile))
			throw new ArgumentNullException ("profile");
		if (String.IsNullOrEmpty (assembly))
			throw new ArgumentNullException ("assembly");
		if (String.IsNullOrEmpty (detail_level))
			throw new ArgumentNullException ("detail_level");

		if (detail_level != "normal" && detail_level != "detailed")
			throw new ArgumentException ("detail_level", "Invalid value");

		this.reference = reference;
		this.profile = profile;
		this.assembly = assembly;
		this.detail_level = detail_level;
		master_id = -1;
	}

	int GetMasterID (IDbConnection cnc)
	{
		if (master_id >= 0)
			return master_id;

		IDbCommand cmd = GetCommandForProcedure (cnc, "get_master_id");
		AddParameter (cmd, "reference", reference);
		AddParameter (cmd, "profile", profile);
		AddParameter (cmd, "assembly", assembly);
		AddParameter (cmd, "detail_level", detail_level);
		master_id = Convert.ToInt32 (cmd.ExecuteScalar ());
		return master_id;
	}

	public ComparisonNode GetRootNode ()
	{
		return GetNodeByName ("0");
	}

	static void SetValuesFromReader (IDataReader reader, ComparisonNode node)
	{
		node.InternalID = reader ["node_name"];
		node.Status = (ComparisonStatus) reader ["status"];
		node.ThrowsNIE = (bool) reader ["throwsnie"];
		node.Extra = (int) reader ["extras"];
		node.Missing = (int) reader ["missing"];
		node.Present = (int) reader ["present"];
		node.Warning = (int) reader ["warning"];
		node.Todo = (int) reader ["todo"];
		node.Niex = (int) reader ["niex"];
		node.HasChildren = (bool) reader ["has_children"];
		node.HasMessages = (bool) reader ["has_messages"];
	}

	void GetMessagesForNodeRecursive (IDbConnection cnc, ComparisonNode node)
	{
		if (node.HasMessages) {
			IDbCommand cmd = GetCommandForProcedure (cnc, "get_messages");
			AddParameter (cmd, "master_id", GetMasterID (cnc));
			AddParameter (cmd, "nodename", node.InternalID);
			//Console.WriteLine ("call get_messages('{0}')", node.InternalID);
			using (IDataReader reader = cmd.ExecuteReader ()) {
				while (reader.Read ()) {
					bool is_todo = Convert.ToBoolean (reader ["is_todo"]);
					object t = reader ["message"];
					if (t != null && t != DBNull.Value) {
						List<string> list = (is_todo) ? node.Todos : node.Messages;
						list.Add ((string) t);
					}
				}
			}
		}

		// Callers will not have the entire tree populated, just 1 node and its direct children
		foreach (ComparisonNode child in node.Children)
			GetMessagesForNodeRecursive (cnc, child);
	}

	public ComparisonNode GetNodeByName (string node_name)
	{
		if (String.IsNullOrEmpty (node_name))
			node_name = "0";

		ComparisonNode node = null;
		using (IDbConnection cnc = GetConnection ()) {
			IDbCommand cmd = GetCommandForProcedure (cnc, "get_node_by_name");
			AddParameter (cmd, "master_id", GetMasterID (cnc));
			AddParameter (cmd, "nodename", node_name);
			//Console.WriteLine ("call get_node_by_name ('{0}')", node_name);
			using (IDataReader reader = cmd.ExecuteReader ()) {
				if (reader.Read ()) {
					CompType comp_type = (CompType) reader ["comparison_type"];
					string display_name = (string) reader ["name"];
					string type_name = null;
					object t = reader ["typename"];
					if (t != null && t != DBNull.Value)
						type_name = (string) t;
					node = new ComparisonNode (comp_type, display_name, type_name);
					SetValuesFromReader (reader, node);
				}
			}
			if (node != null) {
				// Get only this node's messages before calling GetChildren
				GetMessagesForNodeRecursive (cnc, node);
				GetChildren (node);
			}
		}
		return node;
	}

	public List<ComparisonNode> GetChildren (ComparisonNode node)
	{
		if (node == null)
			throw new ArgumentNullException ("node");

		if (node.HasChildren == false || node.Children.Count > 0)
			return node.Children;

		using (IDbConnection cnc = GetConnection ()) {
			IDbCommand cmd = GetCommandForProcedure (cnc, "get_children");
			AddParameter (cmd, "master_id", GetMasterID (cnc));
			AddParameter (cmd, "parent_name", node.InternalID);
			//Console.WriteLine ("call get_children ({0}, '{1}')", GetMasterID (cnc), node.InternalID);
			using (IDataReader reader = cmd.ExecuteReader ()) {
				while (reader.Read ()) {
					CompType comp_type = (CompType) reader ["comparison_type"];
					string display_name = (string) reader ["name"];
					string type_name = null;
					object t = reader ["typename"];
					if (t != null && t != DBNull.Value)
						type_name = (string) t;
					ComparisonNode child = new ComparisonNode (comp_type, display_name, type_name);
					child.Parent = node;
					SetValuesFromReader (reader, child);
					node.Children.Add (child);
				}
			}
			GetMessagesForNodeRecursive (cnc, node);
		}
		return node.Children;
	}

	static IDataParameter AddParameter (IDbCommand cmd, string name, object val)
	{
		IDataParameter p = cmd.CreateParameter ();
		p.ParameterName = name;
		p.Value = val;
		cmd.Parameters.Add (p);
		return p;
	}

	static IDbCommand GetCommandForProcedure (IDbConnection cnc, string proc)
	{
		IDbCommand cmd = cnc.CreateCommand ();
		cmd.CommandText = proc;
		cmd.CommandType = CommandType.StoredProcedure;
		return cmd;
	}
}

