//
// Gendarme.Rules.Performance.AvoidUnneededUnboxingRule
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

namespace Gendarme.Rules.Performance {

	[Problem ("This method unbox (convert from object to a value type) several times the same variable.")]
	[Solution ("Cast the variable, one time, into a temporary variable and use it afterward.")]
	public class AvoidUnneededUnboxingRule : Rule, IMethodRule {

		private static string Previous (MethodDefinition method, Instruction ins)
		{
			string kind, name;
			string type = (ins.Operand as TypeReference).FullName;

			ins = ins.Previous;
			Code previous_op_code = ins.OpCode.Code;

			switch (previous_op_code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				kind = "Parameter";
				int index = previous_op_code - Code.Ldarg_0;
				if (!method.IsStatic)
					index--;
				name = method.Parameters [index].Name;
				break;
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
				kind = "Parameter";
				name = (ins.Operand as ParameterDefinition).Name;
				break;
			case Code.Ldfld:
			case Code.Ldsfld:
				kind = "Field";
				name = (ins.Operand as FieldReference).Name;
				break;
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
				kind = "Variable";
				int vindex = previous_op_code - Code.Ldloc_0;
				name = method.Body.Variables [vindex].Name;
				break;
			case Code.Ldloc:
			case Code.Ldloc_S:
				kind = "Variable";
				name = (ins.Operand as VariableDefinition).Name;
				break;
			default:
				return null;
			}
			return String.Format ("{0} '{1}' unboxed to type '{2}' {{0}} times.", kind, name, type);
		}

		// unboxing is never critical - but a high amount can be a sign of other problems too
		private static Severity GetSeverityFromCount (int count)
		{
			if (count < 4)
				return Severity.Low;
			if (count < 8)
				return Severity.Medium;
			// >= 8
			return Severity.High;
		}

		private Dictionary<string, int> unboxed;

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			if (unboxed == null)
				unboxed = new Dictionary<string, int> ();
			else
				unboxed.Clear ();

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Unbox:
				case Code.Unbox_Any:
					string previous = Previous (method, ins);
					if (previous == null)
						continue;

					int num;
					if (!unboxed.TryGetValue (previous, out num)) {
						unboxed.Add (previous, 1);
					} else {
						unboxed [previous] = ++num;
					}
					break;
				}
			}

			// report findings (one defect per variable/parameter/field)
			foreach (KeyValuePair<string,int> kvp in unboxed) {
				// we can't (always) avoid unboxing one time
				if (kvp.Value < 2)
					continue;
				string s = String.Format (kvp.Key, kvp.Value);
				Runner.Report (method, GetSeverityFromCount (kvp.Value), Confidence.Normal, s);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
