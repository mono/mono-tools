//
// Gendarme.Rules.Concurrency.SqlCommandShouldUseNamedParameters
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

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Security {

	/// <summary>
	/// This rule will fire if a method creates or uses an instance of
	/// <c>System.Data.SqlClient.SqlCommand</c> with a command text that comes from 
	/// a string that has been concatenated with another one. This is a very bad
	/// idea because it is the most frequent cause of security holes regarding SQL
	/// injection.
	///
	/// The preferred way to execute SQL queries that contain variable input
	/// is using Named SQL Parameters.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class BadExample {
	/// 	public void EnableUser (string userName)
	/// 	{
	/// 		var sqlString = "UPDATE Users SET enabled = 1 WHERE UserName = " + userName;
	/// 		using (var conn = new SqlConnection (connString)) {
	/// 			var command = new SqlCommand (sqlString, conn);
	/// 			command.Connection.Open ();
	/// 			command.ExecuteNonQuery ();
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class GoodExample {
	///
	/// 	static readonly string sqlString = "UPDATE Users SET enabled = 1 WHERE UserName = @theUserName";
	///
	/// 	public void EnableUser (string userName)
	/// 	{
	/// 		using (var conn = new SqlConnection (connString)) {
	/// 			var command = new SqlCommand (sqlString, conn);
	/// 			command.Parameters.Add ("@theUserName", SqlDbType.VarChar).Value = userName;
	/// 			command.Connection.Open ();
	/// 			command.ExecuteNonQuery ();
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method uses a SqlCommand with a not constant command text, which is risky for SQL injection attacks")]
	[Solution ("Use a const/readonly string for the command text; and use Named SQL parameters for the variable parts of it")]
	[EngineDependency (typeof(OpCodeEngine))]
	public class SqlCommandShouldUseNamedParameters : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't apply if the method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.FlowControl != FlowControl.Call)
					continue;

				var call = (ins.Operand as MethodReference);
				if (call == null)
					continue;

				if (call.IsNamed ("System.Data.SqlClient", "SqlCommand", ".ctor") ||
					call.IsNamed ("System.Data.SqlClient", "SqlCommand", "set_CommandText")) {
					if (call.Parameters.Count > 0) {
						var offset = call.IsProperty () ? 1 : 0;
						var theString = ins.TraceBack (call, 0 - offset);
						if (theString.OpCode.FlowControl == FlowControl.Call) {
							return RuleResult.Failure;
						}
					}
				}
			}


			return RuleResult.Success;
		}
	}
}
