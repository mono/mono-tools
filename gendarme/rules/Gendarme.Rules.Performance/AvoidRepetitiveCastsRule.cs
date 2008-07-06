//
// Gendarme.Rules.Performance.AvoidRepetitiveCastsRule
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

namespace Gendarme.Rules.Performance {

	[Problem ("The method seems to repeat the same cast operation multiple times.")]
	[Solution ("Change the logic to make sure the (somewhat expensive) cast is done a single time.")]
	public class AvoidRepetitiveCastsRule : Rule, IMethodRule {

		Dictionary<object, Dictionary<TypeReference, int>> isinst = new Dictionary<object, Dictionary<TypeReference, int>> ();

		static object GetOrigin (Instruction ins, MethodDefinition method)
		{
			Instruction previous = ins.Previous;
			if ((previous.OpCode.FlowControl == FlowControl.Call) || previous.IsLoadElement () || previous.IsLoadIndirect ())
				return previous.TraceBack (method).GetOperand (method);

			return previous.GetOperand (method);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			isinst.Clear ();
			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Isinst:		// if (t is T) ...
				case Code.Castclass:		// t = (T) t;
					break;
				default:
					continue;
				}

				TypeReference type = (ins.Operand as TypeReference);
				object origin = GetOrigin (ins, method);
				// rare, but it's possible to cast a null value (ldnull)
				// and it's rare enough that we don't track them
				if (origin == null)
					continue;

				Dictionary<TypeReference, int> tc;
				if (isinst.TryGetValue (origin, out tc)) {
					int count;
					if (tc.TryGetValue (type, out count)) {
						tc [type] = ++count;
					} else {
						tc.Add (type, 1);
					}
				} else {
					tc = new Dictionary<TypeReference, int> ();
					tc.Add (type, 1);
					isinst.Add (origin, tc);
				}
			}

			foreach (KeyValuePair<object, Dictionary<TypeReference, int>> kpv in isinst) {
				foreach (KeyValuePair<TypeReference, int> kpv2 in kpv.Value) {
					if (kpv2.Value == 1)
						continue;

					string msg = String.Format ("'{0}' is casted {1} times for type '{2}'.", kpv.Key.ToString (), kpv2.Value, kpv2.Key.FullName);
					Runner.Report (method, Severity.Medium, Confidence.Normal, msg);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
