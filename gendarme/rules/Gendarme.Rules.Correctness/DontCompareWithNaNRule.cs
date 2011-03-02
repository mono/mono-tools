//
// Gendarme.Rules.Correctness.DoNotCompareWithNaNRule
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// As defined in IEEE 754 it's impossible to compare any floating-point value, even 
	/// another <c>NaN</c>, with <c>NaN</c>. Such comparison will always return <c>false</c>
	/// (more information on [http://en.wikipedia.org/wiki/NaN wikipedia]). The framework 
	/// provides methods, <c>Single.IsNaN</c> and <c>Double.IsNaN</c>, to check for 
	/// <c>NaN</c> values.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// double d = ComplexCalculation ();
	/// if (d == Double.NaN) {
	///	// this will never be reached, even if d is NaN
	///	Console.WriteLine ("No solution exists!");
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// double d = ComplexCalculation ();
	/// if (Double.IsNaN (d)) {
	///	Console.WriteLine ("No solution exists!");
	/// }
	/// </code>
	/// </example>

	[Problem ("This method compares a floating point value with NaN (Not a Number) which always return false, even for (NaN == NaN).")]
	[Solution ("Replace the code with a call to the appropriate Single.IsNaN(value) or Double.IsNaN(value).")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2242:TestForNaNCorrectly")]
	public class DoNotCompareWithNaNRule : FloatingComparisonRule, IMethodRule {

		private const string EqualityMessage = "A floating point value is compared (== or !=) with [Single|Double].NaN.";
		private const string EqualsMessage = "[Single|Double].Equals is called using NaN.";

		private static bool CheckPrevious (IList<Instruction> il, int index)
		{
			for (int i = index; i >= 0; i--) {
				Instruction ins = il [i];
				switch (ins.OpCode.Code) {
				case Code.Ldc_R4:
					// return false, invalid, is NaN is detected
					return !Single.IsNaN ((float) ins.Operand);
				case Code.Ldc_R8:
					// return false, invalid, is NaN is detected
					return !Double.IsNaN ((double) ins.Operand);
				case Code.Nop:
				case Code.Ldarg:
				case Code.Ldarg_1:
				case Code.Ldloca:
				case Code.Ldloca_S:
				case Code.Stloc:
				case Code.Stloc_0:
				case Code.Stloc_1:
					// continue
					break;
				default:
					return true;
				}
			}
			return true;
		}

		// contains LDC_R4 and LDC_R8
		static OpCodeBitmask Ldc_R = new OpCodeBitmask (0xC00000000, 0x0, 0x0, 0x0);

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!IsApplicable (method))
				return RuleResult.DoesNotApply;

			// extra check - rule applies only if the method contains Ldc_R4 or Ldc_R8
			if (!Ldc_R.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			IList<Instruction> il = method.Body.Instructions;
			for (int i = 0; i < il.Count; i++) {
				Instruction ins = il [i];
				switch (ins.OpCode.Code) {
				// handle == and !=
				case Code.Ceq:
					if (!CheckPrevious (il, i - 1)) {
						Runner.Report (method, ins, Severity.Critical, Confidence.Total, EqualityMessage);
					}
					break;
				// handle calls to [Single|Double].Equals
				case Code.Call:
				case Code.Callvirt:
					MemberReference callee = ins.Operand as MemberReference;
					if ((callee != null) && (callee.Name == "Equals") && callee.DeclaringType.IsFloatingPoint ()) {
						if (!CheckPrevious (il, i - 1)) {
							Runner.Report (method, ins, Severity.Critical, Confidence.Total, EqualsMessage);
						}
					}
					break;
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
