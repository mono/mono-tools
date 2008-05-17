//
// Gendarme.Rules.Performance.RemoveUnusedLocalVariablesRule
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
using System.Collections;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	[Problem ("This methods contains unused local variables.")]
	[Solution ("Remove unused variables to reduce size of methods.")]
	public class RemoveUnusedLocalVariablesRule : Rule, IMethodRule {

		// it does not make sense to allocate less than 16 bytes, 16 * 8
		private const int DefaultLength = (16 << 3);

		private BitArray used;

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// this rule cannot execute if debugging information is not available
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active &= e.CurrentModule.HasDebuggingInformation ();
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			int count = method.Body.Variables.Count;
			if (count == 0)
				return RuleResult.Success;

			if (used == null) {
				used = new BitArray (Math.Max (DefaultLength, count));
			} else if (count > used.Length) {
				used = new BitArray (count);
			}
			used.SetAll (false);

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Ldloc_0:
				case Code.Ldloc_1:
				case Code.Ldloc_2:
				case Code.Ldloc_3:
					used [(int) (ins.OpCode.Code - Code.Ldloc_0)] = true;
					break;
				case Code.Ldloc_S:
				case Code.Ldloc:
				case Code.Ldloca:
				case Code.Ldloca_S:
					VariableDefinition vd = (ins.Operand as VariableDefinition);
					used [vd.Index] = true;
					break;
				}
			}

			for (int i = 0; i < count; i++) {
				if (!used [i]) {
					// sometimes the compilers generates some locals without really
					// using them (e.g. assign only a constant). In this case we need
					// to determine if the variable is "genuine" or a compiler
					// (*) seen in a while (true) loop over a switch
					VariableDefinition variable = method.Body.Variables [i];
					string var_name = variable.Name;
					if (var_name.StartsWith ("V_") || var_name.Contains ("$"))
						continue;

					string s = String.Format ("Variable '{0}' of type '{1}'", 
						var_name, variable.VariableType.FullName);
					Runner.Report (method, Severity.Low, Confidence.Normal, s);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
