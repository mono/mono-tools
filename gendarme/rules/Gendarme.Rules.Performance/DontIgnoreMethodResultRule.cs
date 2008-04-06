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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	[Problem ("The member ignores the result from the call.")]
	[Solution ("You shouldn't ignore this result.")]
	public class DoNotIgnoreMethodResultRule : Rule,IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies if the method has a body
			// rule doesn't not apply to generated code (out of developer's control)
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.Code == Code.Pop) {
					Defect defect = CheckForViolation (method, instruction.Previous);
					if (defect != null) 
						Runner.Report (defect);
				}
			}
			return Runner.CurrentRuleResult;
		}

		// some method return stuff that isn't required in most cases
		// the rule ignores them
		private static bool IsCallException (MethodReference method)
		{
			// GetOriginalType makes generic type easier to compare
			switch (method.DeclaringType.GetOriginalType ().FullName) {
			case "System.IO.Directory":
			case "System.IO.DirectoryInfo":
				// the returned DirectoryInfo returned by Create* methods are optional
				return (method.ReturnType.ReturnType.FullName == "System.IO.DirectoryInfo");
			case "System.Text.StringBuilder":
				// StringBuilder Append*, Insert ... methods return the current 
				// StringBuilder so the calls can be chained
				return (method.ReturnType.ReturnType.FullName == "System.Text.StringBuilder");
			case "System.Security.PermissionSet":
				// PermissionSet return the permission (modified or unchanged) when
				// IPermission are added or removed
				return (method.Name == "AddPermission" || method.Name == "RemovePermission");
			case "System.Reflection.MethodBase":
				return (method.Name == "Invoke");
			case "System.Collections.Stack":
			case "System.Collections.Generic.Stack`1":
				return (method.Name == "Pop");
			case "Mono.Security.ASN1":
				// return the instance so we can chain operations
				return (method.Name == "Add");
			}

			// many types provide a BeginInvoke, which return value (deriving from IAsyncResult)
			// isn't needed in many cases
			if (method.Name == "BeginInvoke") {
				if (method.Parameters.Count > 0) {
					return (method.Parameters [0].ParameterType.FullName == "System.Delegate");
				}
			}

			return false;
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

		private Defect CheckForViolation (MethodDefinition method, Instruction instruction)
		{
			if ((instruction.OpCode.Code == Code.Newobj || instruction.OpCode.Code == Code.Newarr)) {
				MemberReference member = (instruction.Operand as MemberReference);
				if ((member != null) && !IsNewException (member)) {
					string s = String.Format ("Unused object of type {0} created", member.ToString ());
					return new Defect (this, method.DeclaringType, method, instruction, Severity.Medium, Confidence.Normal, s);
				}
			}

			if (instruction.OpCode.Code == Code.Call || instruction.OpCode.Code == Code.Callvirt) {
				MethodReference callee = instruction.Operand as MethodReference;
				if (callee != null && !callee.ReturnType.ReturnType.IsValueType) {
					// check for some common exceptions (to reduce false positive)
					if (!IsCallException (callee)) {
						string s = String.Format ("Do not ignore method results from call to '{0}'.", callee.ToString ());
						return new Defect (this, method.DeclaringType, method, instruction, Severity.Medium, Confidence.Normal, s);
					}
				}
			}
			return null;
		}
	}
}
