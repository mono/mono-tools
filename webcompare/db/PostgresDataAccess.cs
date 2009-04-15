//
// PostgresDataAccess.cs
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
using System.Data;
using System.Text;
using Npgsql;
using GuiCompare;

namespace Mono.WebCompareDB {
	class PostgresDataAccess : DataAccess {
		protected override IDbConnection GetConnection ()
		{
			IDbConnection cnc = new NpgsqlConnection ();
			//cnc.ConnectionString = ConnectionString;
			cnc.ConnectionString = "Server=127.0.0.1;Port=5432;User Id=gonzalo;Password=gonz;Database=webcompare;";
			cnc.Open ();
			return cnc;
		}

		protected override string GetParameterNameForQuery (string pname)
		{
			return ":" + pname;
		}

		protected override int InsertMaster (IDbConnection cnc, State state)
		{
			IDbTransaction trans = cnc.BeginTransaction ();
			try {
				IDbCommand cmd = cnc.CreateCommand ();
				cmd.Transaction = trans;
				cmd.CommandText = "INSERT INTO master (reference, profile, assembly, detail_level, last_updated, active) VALUES (:reference, :profile, :assembly, :detail_level, CURRENT_TIMESTAMP, FALSE);";

				AddParameter (cmd, "reference", state.Reference);
				AddParameter (cmd, "profile", state.Profile);
				AddParameter (cmd, "assembly", state.Assembly);
				AddParameter (cmd, "detail_level", state.DetailLevel);
				if (cmd.ExecuteNonQuery () != 1)
					throw new Exception ("Error inserting into 'master'");

				cmd = cnc.CreateCommand ();
				cmd.Transaction = trans;
				cmd.CommandText = "SELECT CURRVAL('seq_master_id')";
				int result = Convert.ToInt32 (cmd.ExecuteScalar ());
				trans.Commit ();
				trans = null;
				return result;
			} finally {
				if (trans != null) {
					trans.Rollback ();
					trans = null;
				}
			}
		}

		protected override void LoadFile (IDbConnection cnc, string table, string file_name)
		{
			IDbCommand cmd = cnc.CreateCommand ();	
			cmd.CommandType = CommandType.Text;
			string col_names = null;
			if (table == "messages")
				col_names = "(node_name, master_id, is_todo, message)";
			cmd.CommandText = String.Format ("COPY {1} {2} FROM '{0}'", file_name, table, col_names);
			cmd.ExecuteNonQuery ();
		}

		protected override void EnableNewMaster (IDbConnection cnc, int new_master_id)
		{
			IDbTransaction trans = cnc.BeginTransaction ();
			try {
				string reference, profile, assembly, detail_level;

				IDbCommand cmd = cnc.CreateCommand ();
				cmd.Transaction = trans;
				cmd.CommandText = "SELECT reference, profile, assembly, detail_level FROM master WHERE id = :master_id";
				AddParameter (cmd, "master_id", new_master_id);
				using (IDataReader reader = cmd.ExecuteReader ()) {
					if (!reader.Read ())
						throw new Exception ("MasterID not found");
					reference = reader [0] as string;
					profile = reader [1] as string;
					assembly = reader [2] as string;
					detail_level = reader [3] as string;
				}
				cmd = cnc.CreateCommand ();
				cmd.Transaction = trans;
				cmd.CommandText = "UPDATE master SET active = TRUE WHERE id = :master_id";
				AddParameter (cmd, "master_id", new_master_id);
				if (cmd.ExecuteNonQuery () != 1)
					throw new Exception ("Error activating masterID");

				cmd = cnc.CreateCommand ();
				cmd.Transaction = trans;
				cmd.CommandText = "UPDATE master SET active = FALSE " +
						"WHERE id <> :master_id AND reference = :reference AND " +
						"profile = :profile AND assembly = :assembly AND detail_level = :detail_level";
				AddParameter (cmd, "master_id", new_master_id);
				AddParameter (cmd, "reference", reference);
				AddParameter (cmd, "profile", profile);
				AddParameter (cmd, "assembly", assembly);
				AddParameter (cmd, "detail_level", detail_level);
				cmd.ExecuteNonQuery ();
				trans.Commit ();
				trans = null;
			} finally {
				if (trans != null) {
					trans.Rollback ();
					trans = null;
				}
			}
		}
	}
}

