//
// Gendarme.Rules.Performance.CompareWithStringEmptyEfficientlyRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Performance {

	[Problem ("This method compares a string with an empty string by using the Equals method or the equality (==) or inequality (!=) operators.")]
	[Solution ("Compare String.Length with 0 instead. The string length is known and it's faster to compare integers than to compare strings.")]
	public class CompareWithEmptyStringEfficientlyRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.FlowControl != FlowControl.Call)
					continue;

				MethodReference mref = (ins.Operand as MethodReference);
				if (mref.DeclaringType.FullName != "System.String")
					continue;

				// covers Equals(string) method and both == != operators
				switch (mref.Name) {
				case "Equals":
					if (mref.Parameters.Count > 1)
						continue;
					break;
				case "op_Equality":
				case "op_Inequality":
					break;
				default:
					continue;
				}

				Instruction prev = ins.Previous;
				switch (prev.OpCode.Code) {
				case Code.Ldstr:
					if ((prev.Operand as string).Length > 0)
						continue;
					break;
				case Code.Ldsfld:
					FieldReference field = (prev.Operand as FieldReference);
					if (field.DeclaringType.FullName != "System.String")
						continue;
					if (field.Name != "Empty")
						continue;
					break;
				}

				Runner.Report (method, ins, Severity.Medium, Confidence.High, String.Empty);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
