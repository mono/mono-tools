//
// Gendarme.Rules.BadPractice.ReplaceIncompleteOddnessCheckRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	// rule idea credits to FindBug - http://findbugs.sourceforge.net/
	// IM: Check for oddness that won't work for negative numbers (IM_BAD_CHECK_FOR_ODD)

	[Problem ("This method looks like it check if an integer is odd or even but the implementation wont work on negative integers.")]
	[Solution ("Verify the code logic and, if required, replace the defective logic with one that works with negative values too.")]
	public class ReplaceIncompleteOddnessCheckRule : Rule, IMethodRule {

		// if/when needed this could be refactored (e.g. missing Ldc_I4__#) 
		// and turned into an InstructionRock
		static bool IsLoadConstant (Instruction ins, int constant)
		{
			if (ins == null)
				return false;

			switch (ins.OpCode.Code) {
			case Code.Ldc_I4_1:
				return (constant == 1);
			case Code.Ldc_I4_2:
				return (constant == 2);
			case Code.Ldc_I4:
				return ((int) ins.Operand == constant);
			case Code.Ldc_I4_S:
				return ((int)(sbyte) ins.Operand == constant);
			case Code.Ldc_I8:
				return ((long) ins.Operand == constant);
			default:
				if (IsConvertion (ins))
					return IsLoadConstant (ins.Previous, constant);
				return false;
			}
		}

		static bool IsConvertion (Instruction ins)
		{
			if (ins == null)
				return false;

			switch (ins.OpCode.Code) {
			case Code.Conv_I4:
			case Code.Conv_I8:
				return true;
			default:
				return false;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				Severity severity;
				// look for a remainder operation
				switch (ins.OpCode.Code) {
				case Code.Rem:
					// this won't work when negative numbers are used
					severity = Severity.High;
					break;
				case Code.Rem_Un:
					// this will work since it can't be a negative number
					// but it's a coding bad (practice) example
					severity = Severity.Low;
					break;
				default:
					continue;
				}

				// x % 2
				if (!IsLoadConstant (ins.Previous, 2))
					continue;
				// compared to 1
				if (!IsLoadConstant (ins.Next, 1))
					continue;
				// using equality
				Instruction cmp = ins.Next.Next;
				if (IsConvertion (cmp))
					cmp = cmp.Next;
				if (cmp.OpCode.Code != Code.Ceq)
					continue;

				Runner.Report (method, ins, severity, Confidence.Normal);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
