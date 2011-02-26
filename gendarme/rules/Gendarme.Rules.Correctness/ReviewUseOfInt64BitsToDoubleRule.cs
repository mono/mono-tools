//
// Gendarme.Rules.Correctness.ReviewUseOfInt64BitsToDoubleRule
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
	// DMI: Double.longBitsToDouble invoked on an int (DMI_LONG_BITS_TO_DOUBLE_INVOKED_ON_INT)

	/// <summary>
	/// This rule checks for invalid integer to double conversion using the, confusingly named,
	/// <c>BitConverter.Int64BitsToDouble</c> method. This method converts the actual bits,
	/// i.e. not the value, into a <c>Double</c>. The rule will warn when anything other than an 
	/// <c>Int64</c> is being used as a parameter to this method.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public double GetRadians (int degrees)
	/// {
	///	return BitConverter.Int64BitsToDouble (degrees) * Math.PI / 180.0d;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public double GetRadians (int degree)
	/// {
	///	return degrees * Math.PI / 180.0d;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method calls System.BitConverter.Int64BitsToDouble(Int64) in a way that suggests it is trying to convert an integer value, not the bits, into a double.")]
	[Solution ("Verify the code logic. This could be a bad, non-working, conversion from an integer type into a double.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ReviewUseOfInt64BitsToDoubleRule : Rule, IMethodRule {

		// Conv_I8, Conv_U8, Conv_Ovf_I8, Conv_Ovf_I8_Un, Conv_Ovf_U8, Conv_Ovf_U8_Un
		private static OpCodeBitmask Convert8 = new OpCodeBitmask (0x0, 0x220000000000, 0x60000000044, 0x0);
		
		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				// if the module does not reference System.BitConverter.Int64BitsToDouble then no
				// then there's no point in enabling the rule
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyMemberReference ((MemberReference mr) => {
						return IsInt64BitsToDouble (mr);
					})
				);
			};
		}

		static bool IsInt64BitsToDouble (MemberReference method)
		{
			return method.IsNamed ("System", "BitConverter", "Int64BitsToDouble");
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
		
			// exclude methods that don't have calls
			OpCodeBitmask mask = OpCodeEngine.GetBitmask (method);
			if (!OpCodeBitmask.Calls.Intersect (mask))
				return RuleResult.DoesNotApply;
			// *and* methods that don't convert into an [u]int64
			if (!Convert8.Intersect (mask))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.FlowControl != FlowControl.Call)
					continue;

				if (!IsInt64BitsToDouble (ins.Operand as MethodReference))
					continue;

				// if the previous call convert a value into a long (int64)
				// then it's likely that the API is being misused
				switch (ins.Previous.OpCode.Code) {
				case Code.Conv_I8:
				case Code.Conv_U8:
				case Code.Conv_Ovf_I8:
				case Code.Conv_Ovf_I8_Un:
				case Code.Conv_Ovf_U8:
				case Code.Conv_Ovf_U8_Un:
					Runner.Report (method, ins, Severity.High, Confidence.High);
					break;
				}
			}
			return Runner.CurrentRuleResult;
		}
#if false
		public void Convert ()
		{
			OpCodeBitmask Convert8 = new OpCodeBitmask (0x0, 0x0, 0x0, 0x0);
			Convert8.Set (Code.Conv_I8);
			Convert8.Set (Code.Conv_U8);
			Convert8.Set (Code.Conv_Ovf_I8);
			Convert8.Set (Code.Conv_Ovf_I8_Un);
			Convert8.Set (Code.Conv_Ovf_U8);
			Convert8.Set (Code.Conv_Ovf_U8_Un);
			Console.WriteLine (Convert8);
		}
#endif
	}
}
