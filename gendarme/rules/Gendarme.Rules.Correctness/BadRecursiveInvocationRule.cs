//
// BadRecursiveInvocationRule: looks for instances of problematic recursive
//	invocations.
//
// Authors:
//	Aaron Tomb <atomb@soe.ucsc.edu>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Aaron Tomb
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Correctness {

	[Problem ("This method, or property, invokes itself recursively in a suspcious way.")]
	[Solution ("Ensure that an exit condition exists to terminate recursion.")]
	public class BadRecursiveInvocationRule : Rule, IMethodRule {

		// note: parameter names do not have to match because we can be calling a base class virtual method
		private static bool CheckParameters (MethodReference caller, MethodReference callee)
		{
			for (int j = 0; j < callee.Parameters.Count; j++) {
				ParameterDefinition p1 = (ParameterDefinition) callee.Parameters [j];
				ParameterDefinition p2 = (ParameterDefinition) caller.Parameters [j];

				if (p1.ParameterType.FullName != p2.ParameterType.FullName)
					return false;
			}
			// complete match (of types)
			return true;
		}

		private static bool CompareMethods (MethodReference method1, MethodReference method2, bool checkType)
		{
			if (method1 == null)
				return (method2 == null);
			if (method2 == null)
				return false;

			// static or instance mismatch
			if (method1.HasThis != method2.HasThis)
				return false;

			if (method1.Name != method2.Name)
				return false;

			if (checkType && (method1.DeclaringType.FullName != method2.DeclaringType.FullName))
				return false;

			if (method1.Parameters.Count != method2.Parameters.Count)
				return false;

			if (!CheckParameters (method1, method2))
				return false;

			return true;
		}

		private static bool CheckForEndlessRecursion (MethodDefinition method, int index)
		{
			int pcount = method.Parameters.Count;
			if (index <= pcount)
				return false;

			if (!method.HasThis)
				pcount--;

			for (int i = pcount; i >= 0; i--) {
				if (!CheckParams (method, ref index, i))
					return false;
				index--;
			}
			return true;
		}

		private static bool CheckParams (MethodDefinition method, ref int index, int paramNum)
		{
			Instruction insn = method.Body.Instructions [index - 1];
			while (insn != null) {
				switch (insn.OpCode.Code) {
				case Code.Ldarg:
					return ((int) insn.Operand == paramNum);
				case Code.Ldarg_S:
					ParameterDefinition param = (ParameterDefinition) insn.Operand;
					return ((param.Sequence - 1) == paramNum);
				case Code.Ldarg_0:
				case Code.Ldarg_1:
				case Code.Ldarg_2:
				case Code.Ldarg_3:
					return (paramNum == (int) (insn.OpCode.Code - Code.Ldarg_0));
				case Code.Ldloc:
				case Code.Ldloc_0:
				case Code.Ldloc_1:
				case Code.Ldloc_2:
				case Code.Ldloc_3:
				case Code.Ldloc_S:
				case Code.Ldloca:
				case Code.Ldloca_S:
				case Code.Ldfld:
				case Code.Ldflda:
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
				case Code.Newarr:
				case Code.Newobj:
					return false;
				}
				index--;
				insn = insn.Previous;
			}
			return false;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule applies only if the method has a body
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.FlowControl) {
				case FlowControl.Call:
					MethodReference callee = (ins.Operand as MethodReference);
					// check type name only if the call isn't virtual
					bool check_type = (ins.OpCode.Code != Code.Callvirt);
					// continue scanning unless we're calling ourself
					if (!CompareMethods (method, callee, check_type))
						break;

					// recursion detected! check if there a way out of it
					if (CheckForEndlessRecursion (method, method.Body.Instructions.IndexOf (ins))) {
						Runner.Report (method, ins, Severity.Critical, Confidence.High, String.Empty);
						return RuleResult.Failure;
					}
					break;
				case FlowControl.Cond_Branch:
				case FlowControl.Return:
				case FlowControl.Throw:
					// if there's a way to break free before a recursive call then we let it go
					return RuleResult.Success;
				}
			}

			return RuleResult.Success;
		}
	}
}
