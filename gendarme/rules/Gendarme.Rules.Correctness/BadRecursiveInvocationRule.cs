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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule checks for a few common scenarios where a method may be infinitely
	/// recursive. For example, getter properties which call themselves or methods
	/// with no conditional code which call themselves (instead of the base method).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// string CurrentDirectory {
	///	get {
	///		return CurrentDirectory;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// string CurrentDirectory {
	///	get {
	///		return base.CurrentDirectory;
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method, or property, invokes itself recursively in a suspicious way.")]
	[Solution ("Ensure that an exit condition exists to terminate recursion.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class BadRecursiveInvocationRule : Rule, IMethodRule {

		private static bool CompareMethods (MethodReference method1, MethodReference method2, bool virtual_call)
		{
			if (method1 == null)
				return (method2 == null);
			if (method2 == null)
				return false;

			// shortcut, if it's not virtual then only compare metadata tokens
			if (!virtual_call)
				return (method1.MetadataToken == method2.MetadataToken);

			// we could be implementing an interface (skip position 0 because of .ctor and .cctor)
			string m1name = method1.Name;
			bool explicit_interface = (m1name.IndexOf ('.') > 0);
			if (!explicit_interface && (m1name != method2.Name))
				return false;

			// compare parameters
			if (!method1.CompareSignature (method2))
				return false;

			TypeReference t2 = method2.DeclaringType;
			TypeDefinition t2r = t2.Resolve ();
			if (!explicit_interface && (t2r != null) && !t2r.IsInterface)
				return true;

			// we're calling into an interface and this could be us!
			foreach (MethodReference mr in method1.Resolve ().Overrides) {
				if (mr.DeclaringType.IsNamed (t2.Namespace, t2.Name))
					return true;
			}
			return false;
		}

		private static bool CheckForEndlessRecursion (MethodDefinition method, int index)
		{
			int pcount = method.Parameters.Count;
			if (!method.HasThis)
				pcount--;
			if (index <= pcount)
				return false;

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
				case Code.Ldarg_S:
					ParameterDefinition param = (ParameterDefinition) insn.Operand;
					if (method.IsStatic)
						paramNum++;
					return (param.Index == paramNum - 1);
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

		OpCodeBitmask CallsNew = new OpCodeBitmask (0x8000000000, 0x4400000000000, 0x0, 0x0);

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule applies only if the method has a body
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no Call[virt] or NewObj in the method
			if (!CallsNew.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			IList<Instruction> instructions = method.Body.Instructions;
			for (int i = 0; i < instructions.Count; i++) {
				Instruction ins = instructions [i];

				switch (ins.OpCode.FlowControl) {
				case FlowControl.Call:
					MethodReference callee = (ins.Operand as MethodReference);
					// check type name only if the call isn't virtual
					bool virtual_call = (ins.OpCode.Code == Code.Callvirt);
					// continue scanning unless we're calling ourself
					if (!CompareMethods (method, callee, virtual_call))
						break;

					// recursion detected! check if there a way out of it
					if (CheckForEndlessRecursion (method, i)) {
						Runner.Report (method, ins, Severity.Critical, Confidence.High);
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

			// should never be reached (since there's always a RET instruction)
			return RuleResult.Success;
		}
	}
}
