//
// Gendarme.Rules.Correctness.ReviewCastOnIntegerDivisionRule
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
	// ICAST: int division result cast to double or float (ICAST_IDIV_CAST_TO_DOUBLE)

	/// <summary>
	/// This rule checks for integral divisions where the result is cast to a floating point
	/// type. It's usually best to instead cast an operand to the floating point type so
	/// that the result is not truncated.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public double Bad (int a, int b)
	/// {
	///	// integers are divided, then the result is casted into a double
	///	// i.e. Bad (5, 2) == 2.0d
	///	return a / b;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public double Good (int a, int b)
	/// {
	///	// a double is divided by an integer, which result in a double result
	///	// i.e. Good (5, 2) == 2.5d
	///	return (double) a / b;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("The result of an integral division is cast to a Single or Double. This is questionable unless you really want the truncated result.")]
	[Solution ("Cast an operand to Single or Double, not the result.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ReviewCastOnIntegerDivisionRule : Rule, IMethodRule {

		// DIV and DIV[_UN]
		static OpCodeBitmask Div = new OpCodeBitmask (0x0, 0xC000000, 0x0, 0x0);

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

			// exclude methods that don't have division instructions
			if (!OpCodeEngine.GetBitmask (method).Intersect (Div))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Div:
				case Code.Div_Un:
					// if the next step is a conversion to a FP value it means
					// that the result, on the stack, is not in it's final form
					switch (ins.Next.OpCode.Code) {
					case Code.Conv_R_Un:
						// no doubt since the result is unsigned it's not a FP
						Runner.Report (method, ins, Severity.High, Confidence.High);
						break;
					case Code.Conv_R4:
					case Code.Conv_R8:
						// it could be a R4 converted into a R8 (or vice versa)
						// Note: we don't have to check both divided and divisor since they will
						// be converted to the same type on the stack before the call to DIV
						if (!IsFloatingPointArguments (ins.TraceBack (method), method))
							Runner.Report (method, ins, Severity.High, Confidence.High);
						break;
					}
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask mask = new OpCodeBitmask ();
			mask.Set (Code.Div);
			mask.Set (Code.Div_Un);
			Console.WriteLine (mask);
		}
#endif
	}
}
