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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	// rule idea credits to FindBug - http://findbugs.sourceforge.net/
	// IM: Check for oddness that won't work for negative numbers (IM_BAD_CHECK_FOR_ODD)

	/// <summary>
	/// This rule checks for problematic oddness checks. Often this is done by comparing
	/// a value modulo two (% 2) with one (1). However this will not work if the value is
	/// negative because negative one will be returned. A better (and faster) approach is 
	/// to check the least significant bit of the integer.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool IsOdd (int x)
	/// {
	/// 	// (x % 2) won't work for negative numbers (it returns -1)
	/// 	return ((x % 2) == 1);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public bool IsOdd (int x)
	/// {
	///	return ((x % 2) != 0);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (faster):
	/// <code>
	/// public bool IsOdd (int x)
	/// {
	///	return ((x &amp; 1) == 1);
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The method contains code which looks as if it is doing an oddness check, but the code will not work for negative integers.")]
	[Solution ("Verify the code logic and, if required, replace the defective code with code that works for negative values as well.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ReplaceIncompleteOddnessCheckRule : Rule, IMethodRule {

		// Conv_[Ovf_][I|U][1|2|4|8][_Un] - about all except Conv_R[4|8]
		private static OpCodeBitmask Conversion = new OpCodeBitmask (0x0, 0x800003C000000000, 0xE07F8000001FF, 0x0);

		// Rem[_Un]
		private static OpCodeBitmask Remainder = new OpCodeBitmask (0x0, 0x30000000, 0x0, 0x0);

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
				// recurse on integer convertion
				if (Conversion.Get (ins.OpCode.Code))
					return IsLoadConstant (ins.Previous, constant);
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
				if (Conversion.Get (cmp.OpCode.Code))
					cmp = cmp.Next;
				if (cmp.OpCode.Code != Code.Ceq)
					continue;

				Runner.Report (method, ins, severity, Confidence.Normal);
			}
			return Runner.CurrentRuleResult;
		}
#if false

		public void BuildRemainder ()
		{
			OpCodeBitmask remainder = new OpCodeBitmask ();
			remainder.Set (Code.Rem);
			remainder.Set (Code.Rem_Un);
			Console.WriteLine (remainder);
		}

		public void BuildConversion ()
		{
			OpCodeBitmask convert = new OpCodeBitmask ();
			convert.Set (Code.Conv_I);
			convert.Set (Code.Conv_I1);
			convert.Set (Code.Conv_I2);
			convert.Set (Code.Conv_I4);
			convert.Set (Code.Conv_I8);
			convert.Set (Code.Conv_Ovf_I);
			convert.Set (Code.Conv_Ovf_I_Un);
			convert.Set (Code.Conv_Ovf_I1);
			convert.Set (Code.Conv_Ovf_I1_Un);
			convert.Set (Code.Conv_Ovf_I2);
			convert.Set (Code.Conv_Ovf_I2_Un);
			convert.Set (Code.Conv_Ovf_I4);
			convert.Set (Code.Conv_Ovf_I4_Un);
			convert.Set (Code.Conv_Ovf_I8);
			convert.Set (Code.Conv_Ovf_I8_Un);
			convert.Set (Code.Conv_Ovf_U);
			convert.Set (Code.Conv_Ovf_U_Un);
			convert.Set (Code.Conv_Ovf_U1);
			convert.Set (Code.Conv_Ovf_U1_Un);
			convert.Set (Code.Conv_Ovf_U2);
			convert.Set (Code.Conv_Ovf_U2_Un);
			convert.Set (Code.Conv_Ovf_U4);
			convert.Set (Code.Conv_Ovf_U4_Un);
			convert.Set (Code.Conv_Ovf_U8);
			convert.Set (Code.Conv_Ovf_U8_Un);
			convert.Set (Code.Conv_Ovf_U);
			convert.Set (Code.Conv_Ovf_U1);
			convert.Set (Code.Conv_Ovf_U2);
			convert.Set (Code.Conv_Ovf_U4);
			convert.Set (Code.Conv_Ovf_U8);
			Console.WriteLine (convert);
		}
#endif
	}
}
