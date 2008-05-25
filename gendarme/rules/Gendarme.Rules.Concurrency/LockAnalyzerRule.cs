//
// Gendarme.Rules.Concurrency.LockAnalyzerRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	abstract public class LockAnalyzerRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// is this module using Monitor.Enter ? (lock in c#)
			// if not then this rule does not need to be executed for the module
			// note: mscorlib.dll is an exception since it defines, not refer, System.Threading.Monitor
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == Constants.Corlib) ||
					e.CurrentModule.TypeReferences.ContainsType ("System.Threading.Monitor");
			};
		}


		abstract public void Analyze (MethodDefinition method, Instruction ins);

		public virtual RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.FlowControl != FlowControl.Call)
					continue;

				MethodReference mr = (ins.Operand as MethodReference);
				if (mr.DeclaringType.FullName != "System.Threading.Monitor")
					continue;
				if (mr.Name != "Enter")
					continue;

				Analyze (method, ins);
			}
			return Runner.CurrentRuleResult;
		}

		internal static bool IsLoadLoc (Instruction ins)
		{
			if (ins == null)
				return false;

			switch (ins.OpCode.Code) {
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
			case Code.Ldloc:
			case Code.Ldloc_S:
				return true;
			default:
				return false;
			}
		}

		internal static bool IsStoreLoc (Instruction ins)
		{
			if (ins == null)
				return false;

			switch (ins.OpCode.Code) {
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
			case Code.Stloc:
			case Code.Stloc_S:
				return true;
			default:
				return false;
			}
		}

		internal static VariableDefinition GetVariable (MethodDefinition method, Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
				int vindex = ins.OpCode.Code - Code.Ldloc_0;
				return method.Body.Variables [vindex];
			case Code.Ldloc:
			case Code.Ldloc_S:
				return (ins.Operand as VariableDefinition);
			default:
				return null;
			}
		}
	}
}
