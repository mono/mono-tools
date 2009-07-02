//
// Gendarme.Rules.Correctness.ReviewUseOfModuloOneOnIntegersRule
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	// rule idea credits to FindBug - http://findbugs.sourceforge.net/
	// INT: Integer remainder modulo 1 (INT_BAD_REM_BY_1)

	/// <summary>
	/// This rule checks for a modulo one (1) operation on an integral type. This is most
	/// likely a typo since the result is always 0. This usually happen when someone confuses
	/// a bitwise operation with a remainder.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool IsOdd (int i)
	/// {
	///	return ((i % 1) == 1);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public bool IsOdd (int i)
	/// {
	///	return ((i % 2) != 0); // or ((x &amp; 1) == 1)
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method computes the modulo (%) 1 of a integral value. This always evaluates to zero.")]
	[Solution ("Verify the code logic. The logic should probably be (i % 2) to separate even/odd or (i & 1) to check the least significant bit.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ReviewUseOfModuloOneOnIntegersRule : Rule, IMethodRule {

		// Rem[_Un]
		private static OpCodeBitmask Remainder = new OpCodeBitmask (0x0, 0x30000000, 0x0, 0x0);

		static bool CheckModuloOne (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Ldc_I4_1:
				return true;
			case Code.Ldc_I4_S:
				return ((int)(sbyte) ins.Operand == 1);
			case Code.Ldc_I4:
				return ((int)ins.Operand == 1);
			case Code.Ldc_I8:
				return ((long)ins.Operand == 1);
			case Code.Conv_I4:
			case Code.Conv_I8:
				return CheckModuloOne (ins.Previous);
			default:
				return false;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			if (!Remainder.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Rem:
				case Code.Rem_Un:
					if (CheckModuloOne (ins.Previous))
						Runner.Report (method, ins, Severity.High, Confidence.Total);
					break;
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
