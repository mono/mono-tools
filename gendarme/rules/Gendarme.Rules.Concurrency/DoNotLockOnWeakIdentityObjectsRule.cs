//
// Gendarme.Rules.Concurrency.DoNotLockOnWeakIdentityObjectsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule ensures there are no locks on objects with weak identity.
	/// An object with weak identity is one that can be directly accessed across
	/// different application domains. Because these objects can be accessed
	/// by different application domains it is very difficult to ensure that the
	/// locking is done correctly so problems such as deadlocks are much more likely.
	/// The following types have a weak identities:
	/// <list type="bullet"> 
	/// <item> 
	/// <description><c>System.MarshalByRefObject</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.OutOfMemoryException</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.Reflection.MemberInfo</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.Reflection.ParameterInfo</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.ExecutionEngineException</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.StackOverflowException</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.String</c></description>
	/// </item>
	/// <item> 
	/// <description><c>System.Threading.Thread</c></description>
	/// </item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void WeakIdLocked () 
	/// {
	/// 	lock ("CustomString") {
	///		// ...
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void WeakIdNotLocked ()
	/// {
	/// 	Phone phone = new Phone ();
	///     lock (phone) {
	///		// ...
	///     }
	/// }
	/// </code>
	/// </example>

	[Problem ("This method uses a lock on an object with weak identity, i.e. one that is directly accessible across application domains.")]
	[Solution ("To be safe it is important to lock on an object that is private to your code.")]
	[FxCopCompatibility ("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity")]
	public class DoNotLockOnWeakIdentityObjectsRule : LockAnalyzerRule {

		static bool IsWeakSealedType (TypeReference type)
		{
			switch (type.Namespace) {
			case "System":
				string name = type.Name;
				return ((name == "String") || (name == "ExecutionEngineException") || (name == "StackOverflowException"));
			case "System.Threading":
				return (type.Name == "Thread");
			default:
				return false;
			}
		}

		static string InheritFromWeakType (TypeReference type, string nameSpace, string name)
		{
			if (!type.Inherits (nameSpace, name))
				return String.Empty;
			return String.Format (CultureInfo.InvariantCulture, "'{0}' inherits from '{1}.{2}'.", 
				type.GetFullName (), nameSpace, name);
		}

		public override void Analyze (MethodDefinition method, MethodReference enter, Instruction ins)
		{
			TypeReference type = null;

			// keep original instruction since this is where we will report the defect
			Instruction call = ins;

			// Monitor.Enter(object)
			// Monitor.Enter(object, ref bool) <-- new in FX4 and used by CSC10
			Instruction first = call.TraceBack (method);
			if (first.OpCode.Code == Code.Dup)
				first = first.Previous;
			type = first.GetOperandType (method);
			if (type.IsNamed ("System", "Object")) {
				// newer GMCS use a temporary local that hides the real type
				Instruction prev = first.Previous;
				if (first.IsLoadLocal () && prev.IsStoreLocal ()) {
					if (first.GetVariable (method) == prev.GetVariable (method))
						type = prev.Previous.GetOperandType (method);
				}
			}

			if (type == null)
				return;

			if (IsWeakSealedType (type)) {
				Runner.Report (method, call, Severity.High, Confidence.Normal, type.GetFullName ());
			} else {
				string msg = InheritFromWeakType (type, "System", "MarshalByRefObject");
				if (msg.Length > 0) {
					Runner.Report (method, call, Severity.High, Confidence.Normal, msg);
					return;
				}
				msg = InheritFromWeakType (type, "System", "OutOfMemoryException");
				if (msg.Length > 0) {
					Runner.Report (method, call, Severity.High, Confidence.Normal, msg);
					return;
				}
				msg = InheritFromWeakType (type, "System.Reflection", "MemberInfo");
				if (msg.Length > 0) {
					Runner.Report (method, call, Severity.High, Confidence.Normal, msg);
					return;
				}
				msg = InheritFromWeakType (type, "System.Reflection", "ParameterInfo");
				if (msg.Length > 0) {
					Runner.Report (method, call, Severity.High, Confidence.Normal, msg);
				}
			}
		}
	}
}
