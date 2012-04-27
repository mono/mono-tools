//
// MySqlDataAccess.cs
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
using System.Data;
using MySql.Data.MySqlClient;
using GuiCompare;

namespace Mono.WebCompareDB {
	class MySqlDataAccess : DataAccess {
		protected override IDbConnection GetConnection ()
		{
			IDbConnection cnc = new MySqlConnection ();
			cnc.ConnectionString = ConnectionString;
			cnc.Open ();
			return cnc;
		}

		protected override int InsertMaster (IDbConnection cnc, State state)
		{
			IDbCommand cmd = cnc.CreateCommand ();
			cmd.CommandText = "insert_master";
			cmd.CommandType = CommandType.StoredProcedure;
			AddParameter (cmd, "reference", state.Reference);
			AddParameter (cmd, "profile", state.Profile);
			AddParameter (cmd, "assembly", state.Assembly);
			AddParameter (cmd, "detail_level", state.DetailLevel);
			AddParameter (cmd, "last_updated", state.AssemblyLastWrite);
			IDataParameter p = AddOutputParameter (cmd, "id");
			cmd.ExecuteNonQuery ();
			return Convert.ToInt32 (p.Value);
		}

		protected override void LoadFile (IDbConnection cnc, string table, string file_name)
		{
			IDbCommand cmd = cnc.CreateCommand ();	
			cmd.CommandType = CommandType.Text;
			cmd.CommandText = String.Format ("LOAD DATA LOCAL INFILE '{0}' INTO TABLE {1}", file_name, table);
			cmd.ExecuteNonQuery ();
		}

		protected override void EnableNewMaster (IDbConnection cnc, int new_master_id)
		{
			IDbCommand cmd = cnc.CreateCommand ();
			cmd.CommandText = "update_active_master";
			cmd.CommandType = CommandType.StoredProcedure;
			AddParameter (cmd, "master_id", new_master_id);
			cmd.ExecuteNonQuery ();
		}
	}
}

