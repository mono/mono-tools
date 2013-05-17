//
// Unit tests for SqlCommandShouldUseNamedParameters
//
// Authors:
//	Andres G. Aragoneses <andres@7digital.com>
//
// Copyright (C) 2013 7digital Media, Ltd (http://www.7digital.com)
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

using System.Data.SqlClient;
using Gendarme.Rules.Security;
using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

//FAKE API so as not to reference System.Data
namespace System.Data.SqlClient {

	public class SqlCommand {
		public SqlCommand (string commandText)
		{
		}

		public SqlCommand ()
		{
		}

		public SqlCommand (string commandText, SqlConnection conn)
		{
		}

		public SqlCommand (string commandText, SqlConnection conn, SqlTransaction trans)
		{
		}

		public string CommandText {
			get { return null; }
			set { }
		}

		public void ExecuteNonQuery ()
		{
		}
	}

	public class SqlConnection
	{
	}

	public class SqlTransaction
	{
	}
}

namespace Test.Rules.Security {

	[TestFixture]
	public class SqlCommandShouldUseNamedParametersTest : MethodRuleTestFixture<SqlCommandShouldUseNamedParameters> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL for p/invokes
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no calls[virt]
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);

			AssertRuleDoesNotApply<SqlExecutor> ("CallWithoutSqlCommandUsage");
		}

		public class SqlExecutor {
			public static void CallWithoutSqlCommandUsage ()
			{
				var c = new SqlConnection ();
			}

			public static void CallsSqlCommandCtorWithVariableString (string param)
			{
				new SqlCommand ("some SQL query " + param).ExecuteNonQuery ();
			}

			private static SqlConnection conn;
			public static void CallsSqlCommandCtorWithVariableStringAndAConn (string param)
			{
				new SqlCommand ("some SQL query " + param, conn).ExecuteNonQuery ();
			}

			private const string aField = "some SQL query from a field";
			private static SqlCommand commandField;
			public static void CallsSqlCommandPropertyWithVariableString (string param)
			{
				commandField.CommandText = aField + param;
				commandField.ExecuteNonQuery ();
			}

			public static void CallsSqlCommandEmptyCtor ()
			{
				new SqlCommand ().ExecuteNonQuery ();
			}

			private const string someField = "some SQL query from a field";
			public static void CallsSqlCommandCtorWithConstantString ()
			{
				new SqlCommand (someField).ExecuteNonQuery ();
			}

			private const string someOtherField = "some SQL query from a field";
			public static void CallsSqlCommandPropertyWithConstantString ()
			{
				var c = new SqlCommand ();
				c.CommandText = someOtherField;
				c.ExecuteNonQuery ();
			}
		}

		[TestCase ("CallsSqlCommandCtorWithVariableString")]
		[TestCase ("CallsSqlCommandCtorWithVariableStringAndAConn")]
		[TestCase ("CallsSqlCommandPropertyWithVariableString")]
		public void CheckFailures (string method)
		{
			AssertRuleFailure<SqlExecutor> (method);
		}

		[TestCase ("CallsSqlCommandCtorWithConstantString")]
		[TestCase ("CallsSqlCommandEmptyCtor")]
		[TestCase ("CallsSqlCommandPropertyWithConstantString")]
		public void CheckSuccesses (string method)
		{
			AssertRuleSuccess<SqlExecutor> (method);
		}
	}
}
