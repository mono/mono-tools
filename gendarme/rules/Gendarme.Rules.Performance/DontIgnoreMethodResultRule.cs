//
// Gendarme.Rules.Performance.DontIgnoreMethodResultRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule fires if a method is called that returns a new instance but that instance
	/// is not used. This is a performance problem because it is wasteful to create and
	/// collect objects which are never actually used. It may also indicate a logic problem.
	/// Note that this rule currently only checks methods within a small number of System
	/// types.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void GetName ()
	/// {
	///	string name = Console.ReadLine ();
	///	// This is a bug: strings are (mostly) immutable so Trim leaves
	/// 	// name untouched and returns a new string.
	///	name.Trim ();
	///	Console.WriteLine ("Name: {0}", name);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void GetName ()
	/// {
	///	string name = Console.ReadLine ();
	///	name = name.Trim ();
	///	Console.WriteLine ("Name: {0}", name);
	/// }
	/// </code>
	/// </example>

	[Problem ("The method ignores the result value from a method call.")]
	[Solution ("Don't ignore the result value.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
	public class DoNotIgnoreMethodResultRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies if the method has a body
			// rule doesn't not apply to generated code (out of developer's control)
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// check if the method contains a Pop instruction
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Pop))
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.Code == Code.Pop) {
					CheckForViolation (method, instruction.Previous);
				}
			}
			return Runner.CurrentRuleResult;
		}

		private static bool IsCallException (MethodReference method)
		{
			switch (method.DeclaringType.GetFullName ()) {
			case "System.String":
				// Since strings are immutable, calling System.String methods that returns strings 
				// better be assigned to something
				return !method.ReturnType.IsNamed ("System", "String");
			case "System.IO.DirectoryInfo":
				// GetDirectories overloads don't apply to the instance
				return (method.Name != "GetDirectories");
			case "System.Security.PermissionSet":
				// Intersection and Union returns a new PermissionSet (it does not change the instance)
				return !method.ReturnType.IsNamed ("System.Security", "PermissionSet");
			default:
				// this is useless anytime, if unassigned, more in cases like a StringBuilder
				return (method.Name != "ToString");
			}
		}

		private static bool IsNewException (MemberReference method)
		{
			switch (method.GetFullName ()) {
			// supplying a callback is enough to make the Timer creation worthwhile
			case "System.Void System.Threading.Timer::.ctor(System.Threading.TimerCallback,System.Object,System.Int32,System.Int32)":
				return true;
			default:
				return false;
			}
		}

		private void CheckForViolation (MethodDefinition method, Instruction instruction)
		{
			if ((instruction.OpCode.Code == Code.Newobj || instruction.OpCode.Code == Code.Newarr)) {
				MemberReference member = (instruction.Operand as MemberReference);
				if ((member != null) && !IsNewException (member)) {
					string s = String.Format (CultureInfo.InvariantCulture,
						"Unused object of type '{0}' created.", member.GetFullName ());
					Runner.Report (method, instruction, Severity.High, Confidence.Normal, s);
				}
			}

			if (instruction.OpCode.Code == Code.Call || instruction.OpCode.Code == Code.Callvirt) {
				MethodReference callee = instruction.Operand as MethodReference;
				if (callee != null && !callee.ReturnType.IsValueType) {
					// check for some common exceptions (to reduce false positive)
					if (!IsCallException (callee)) {
						string s = String.Format (CultureInfo.InvariantCulture,
							"Do not ignore method results from call to '{0}'.", callee.GetFullName ());
						Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, s);
					}
				}
			}
		}
	}
}
