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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	// TODO: This is a nice rule, but it's fairly often that people do want to ignore return
	// values (e.g. from List.Remove) and it's not at all clear to a beginner how best to
	// do so. You can't cast to void as you can in C and C++ and you can't assign to an
	// unused local because the compiler will complain. The best approach seems to be
	// to create a little type like so:
//	internal static class Unused
//	{
//		public static object Value
//		{
//			set {}
//		}
//	}

	// TODO: Why is the first issue that the summary brings up the expense of allocating
	// the return value? Maybe this is a problem if the return value is never used but surely
	// it is a second tier concern. It seems to me that the primary benefit of this rule is
	// that it points out places where you may be ignoring results without realizing it and
	// (if you use something like the Unused struct above) you can make it clear to readers
	// of the code that you are deliberately ignoring the return value.

	/// <summary>
	/// This rule fires if the result of a method call is not used.
	/// Since any returned object potentially requires memory allocations this impacts 
	/// performance. Furthermore this often indicates that the code might not be doing 
	/// what is expected. This is seen frequently on <c>string</c> where people forget
	/// that string is an immutable type. There are some special cases, e.g. <c>StringBuilder</c>, where 
	/// some methods returns the current instance (to chain calls). The rule will ignore 
	/// those well known cases. 
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void GetName ()
	/// {
	///	string name = Console.ReadLine ();
	///	// a new trimmed string is created but never assigned to anything
	///	// and name itself is unchanged
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
			switch (method.DeclaringType.FullName) {
			case "System.String":
				// Since strings are immutable, calling System.String methods that returns strings 
				// better be assigned to something
				return (method.ReturnType.ReturnType.FullName != "System.String");
			case "System.IO.DirectoryInfo":
				// GetDirectories overloads don't apply to the instance
				return (method.Name != "GetDirectories");
			case "System.Security.PermissionSet":
				// Intersection and Union returns a new PermissionSet (it does not change the instance)
				return (method.ReturnType.ReturnType.FullName != "System.Security.PermissionSet");
			default:
				// this is useless anytime, if unassigned, more in cases like a StringBuilder
				return (method.Name != "ToString");
			}
		}

		private static bool IsNewException (MemberReference method)
		{
			switch (method.ToString ()) {
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
					string s = String.Format ("Unused object of type '{0}' created.", member.ToString ());
					Runner.Report (method, instruction, Severity.High, Confidence.Normal, s);
				}
			}

			if (instruction.OpCode.Code == Code.Call || instruction.OpCode.Code == Code.Callvirt) {
				MethodReference callee = instruction.Operand as MethodReference;
				if (callee != null && !callee.ReturnType.ReturnType.IsValueType) {
					// check for some common exceptions (to reduce false positive)
					if (!IsCallException (callee)) {
						string s = String.Format ("Do not ignore method results from call to '{0}'.", callee.ToString ());
						Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, s);
					}
				}
			}
		}
	}
}
