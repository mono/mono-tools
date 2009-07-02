//
// Gendarme.Rules.Correctness.ReviewCastOnIntegerMultiplicationRule
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
	// ICAST: Result of integer multiplication cast to long (ICAST_INTEGER_MULTIPLY_CAST_TO_LONG)

	/// <summary>
	/// This rule checks for integral multiply operations where the result is cast to
	/// a larger integral type. It's safer instead to cast an operand to the larger type
	/// to minimize the chance of overflow.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public long Bad (int a, int b)
	/// {
	///	// e.g. Bad (Int32.MaxInt, Int32.MaxInt) == 1
	///	return a * b;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public long Good (int a, int b)
	/// {
	///	// e.g. Good (Int32.MaxInt, Int32.MaxInt) == 4611686014132420609
	/// 	return (long) a * b;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This method casts the result of an integer multiplication into a larger integer. This may result in an overflow before the cast can be done.")]
	[Solution ("Cast the operands instead of the result.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ReviewCastOnIntegerMultiplicationRule : Rule, IMethodRule {

		static OpCodeBitmask Mul = new OpCodeBitmask (0x0, 0x2000000, 0xC0000000000000, 0x0);

		static bool IsFloatingPointArguments (Instruction ins, MethodDefinition method)
		{
			TypeReference tr = ins.GetOperandType (method);
			if (tr == null)
				return false;
			return tr.IsFloatingPoint ();
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// exclude methods that don't have MUL* instructions
			if (!OpCodeEngine.GetBitmask (method).Intersect (Mul))
				return RuleResult.DoesNotApply;

			Severity s;
			foreach (Instruction ins in method.Body.Instructions) {

				switch (ins.OpCode.Code) {
				case Code.Mul:
					// potential for bad values
					s = Severity.High;
					break;
				case Code.Mul_Ovf:
				case Code.Mul_Ovf_Un:
					// the code use 'checked' (or is compiled with it)
					// so a runtime exception will be thrown
					s = Severity.Medium;
					break;
				default:
					continue;
				}

				if (IsFloatingPointArguments (ins.TraceBack (method), method))
					continue;

				switch (ins.Next.OpCode.Code) {
				case Code.Conv_I8:
				case Code.Conv_Ovf_I8:
				case Code.Conv_Ovf_I8_Un:
				case Code.Conv_U8:
				case Code.Conv_Ovf_U8:
				case Code.Conv_Ovf_U8_Un:
					Runner.Report (method, ins, s, Confidence.Normal);
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask mask = new OpCodeBitmask ();
			mask.Set (Code.Mul);
			mask.Set (Code.Mul_Ovf);
			mask.Set (Code.Mul_Ovf_Un);
			Console.WriteLine (mask);
		}
#endif
	}
}
