//
// Gendarme.Rules.Design.ConsiderUsingStopwatch
//
// Authors:
//  Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2008 Cedric Vivier
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

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Maintainability {

	[Problem ("This method uses difference between two DateTime.Now calls to retrieve processing time. Developer's intent may not be very clear.")]
	[Solution ("Use System.Diagnostics.Stopwatch.")]
	public class ConsiderUsingStopwatchRule : Rule, IMethodRule {

		private const string DateTime = "System.DateTime";
		private const string GetNow = "get_Now";
		private const string Subtraction = "op_Subtraction";

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// if the module does not reference (sealed) System.DateTime
			// then no code inside the module will use it.
			// also we do not want to run this on <2.0 assemblies since Stopwatch
			// did not exist back then.
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Runtime >= TargetRuntime.NET_2_0
					&& (e.CurrentAssembly.Name.Name == Constants.Corlib
					|| e.CurrentModule.TypeReferences.ContainsType (DateTime)));
			};
		}

		private static bool AreMirrorInstructions(Instruction ld, Instruction st)
		{
			int ldIndex = -1;
			switch (ld.OpCode.Code) {
				case Code.Ldloc_0 :
					ldIndex = 0;
					break;
				case Code.Ldloc_1 :
					ldIndex = 1;
					break;
				case Code.Ldloc_2 :
					ldIndex = 2;
					break;
				case Code.Ldloc_3 :
					ldIndex = 3;
					break;
				case Code.Ldloc_S :
				case Code.Ldloc :
					ldIndex = ((VariableDefinition) ld.Operand).Index;
					break;
			}

			int stIndex = -1;
			switch (st.OpCode.Code) {
				case Code.Stloc_0 :
					stIndex = 0;
					break;
				case Code.Stloc_1 :
					stIndex = 1;
					break;
				case Code.Stloc_2 :
					stIndex = 2;
					break;
				case Code.Stloc_3 :
					stIndex = 3;
					break;
				case Code.Stloc_S :
				case Code.Stloc :
					stIndex = ((VariableDefinition) st.Operand).Index;
					break;
			}

			return ldIndex == stIndex;
		}

		private static bool IsGetNow (MethodDefinition method, Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Call :
				MethodReference calledMethod = (MethodReference) ins.Operand;
				return calledMethod.DeclaringType.FullName == DateTime && calledMethod.Name == GetNow;

			case Code.Ldloc_0 :
			case Code.Ldloc_1 :
			case Code.Ldloc_2 :
			case Code.Ldloc_3 :
			case Code.Ldloc_S :
			case Code.Ldloc :
				Instruction prev = ins.Previous;
				while (null != prev)
				{
					if (AreMirrorInstructions(ins, prev))
						return IsGetNow (method, prev.Previous);
					prev = prev.Previous;
				}
				break;
			}

			return false;
		}

		private static bool CheckUsage (MethodDefinition method, Instruction ins)
		{
			return IsGetNow (method, ins.Previous.Previous) && IsGetNow (method, ins.Previous);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.Code == Code.Call) {
					MethodReference calledMethod = (MethodReference) ins.Operand;
					if (calledMethod.DeclaringType.FullName == DateTime && calledMethod.Name == Subtraction)
						if (CheckUsage (method, ins))
							Runner.Report (method, ins, Severity.Low, Confidence.High);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}

}

