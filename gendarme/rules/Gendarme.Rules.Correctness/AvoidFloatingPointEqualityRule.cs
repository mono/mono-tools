//
// Gendarme.Rules.Correctness.AvoidFloatingPointEqualityRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
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

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// In general floating point numbers cannot be usefully compared using the equality and
	/// inequality operators. This is because floating point numbers are inexact and most floating
	/// point operations introduce errors which can accumulate if multiple operations are performed.
	/// This rule will fire if [in]equality comparisons are used with <c>Single</c> or <c>Double</c> 
	/// types. In general such comparisons should be done with some sort of epsilon test instead
	/// of a simple compare (see the code below).
	///
	/// For more information:
	/// <list>
	/// <item>
	/// <description>[http://www.cygnus-software.com/papers/comparingfloats/comparingfloats.htm Floating Point Comparison (General Problem)]</description>
	/// </item>
	/// <item>
	/// <description>[http://www.yoda.arachsys.com/csharp/floatingpoint.html Another article about floating point comparison (more .NET adapted)]</description>
	/// </item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // This may or may not work as expected. In particular, if the values are from
	/// // high precision real world measurements or different algorithmic sources then
	/// // it's likely that they will have small errors and an exact inequality test will not 
	/// // work as expected.
	/// public static bool NearlyEqual (double [] lhs, double [] rhs)
	/// {
	/// 	if (ReferenceEquals (lhs, rhs)) {
	/// 		return true;
	/// 	}
	/// 	if (lhs.Length != rhs.Length) {
	/// 		return false;
	/// 	}
	/// 	for (int i = 0; i &lt; lhs.Length; ++i) {
	/// 		if (lhs [i] != rhs [i]) {
	/// 			return false;
	/// 		}
	/// 	}
	/// 	return true;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// // This will normally work fine. However it will not work with infinity (because
	/// // infinity - infinity is a NAN). It&apos;s also difficult to use if the values may 
	/// // have very large or very small magnitudes (because the epsilon value must 
	/// // be scaled accordingly).
	/// public static bool NearlyEqual (double [] lhs, double [] rhs, double epsilon)
	/// {
	/// 	if (ReferenceEquals (lhs, rhs)) {
	/// 		return true;
	/// 	}
	/// 	if (lhs.Length != rhs.Length) {
	/// 		return false;
	/// 	}
	/// 	for (int i = 0; i &lt; lhs.Length; ++i) {
	/// 		if (Math.Abs (lhs [i] - rhs [i]) &gt; epsilon) {
	/// 			return false;
	/// 			}
	/// 	}
	/// 	return true;
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.2 this rule was named FloatComparisonRule.</remarks>

	[Problem ("This method contains code that performs equality operations between floating point numbers.")]
	[Solution ("Instead use the absolute difference between the two floating point values and a small constant value.")]
	public class AvoidFloatingPointEqualityRule : FloatingComparisonRule, IMethodRule {

		private const string EqualityMessage = "Floating point values should not be directly compared for equality (e.g. == or !=).";
		private const string EqualsMessage = "Floating point values should not be directly compared for equality using [Single|Double].Equals.";

		private static bool CheckCeqInstruction (Instruction instruction, MethodDefinition method)
		{
			bool problem = false;
			switch (instruction.OpCode.Code) {
			case Code.Conv_R_Un:
			case Code.Conv_R4:
			case Code.Conv_R8:
				return true;
			case Code.Ldc_R4:
				return !CheckFloatConstants ((float) instruction.Operand);
			case Code.Ldc_R8:
				return !CheckDoubleConstants ((double) instruction.Operand);
			case Code.Ldelem_R4:
			case Code.Ldelem_R8:
				return true;
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
			case Code.Ldloc_S:
			case Code.Ldloc:
			case Code.Ldloca:
			case Code.Ldloca_S:
				return instruction.GetVariable (method).VariableType.IsFloatingPoint ();
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
				ParameterDefinition parameter = instruction.GetParameter (method);
				// handle 'this'
				if (parameter == null)
					return method.DeclaringType.IsFloatingPoint ();
				return parameter.ParameterType.IsFloatingPoint ();
			case Code.Call:
			case Code.Callvirt:
				MethodReference call = instruction.Operand as MethodReference;
				return call.ReturnType.IsFloatingPoint ();
			case Code.Ldfld:
			case Code.Ldflda:
			case Code.Ldsfld:
			case Code.Ldsflda:
				FieldReference field = instruction.Operand as FieldReference;
				return field.FieldType.IsFloatingPoint ();
			}
			return problem;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!IsApplicable (method))
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Ceq:
					if (CheckCeqInstruction (SkipArithmeticOperations (instruction), method)) {
						Runner.Report (method, instruction, Severity.High, Confidence.Total, EqualityMessage);
					}
					break;
				case Code.Call:
				case Code.Callvirt:
					MemberReference member = instruction.Operand as MemberReference;
					if ((member != null) && (member.Name == "Equals") && member.DeclaringType.IsFloatingPoint ()) {
						Runner.Report (method, instruction, Severity.High, Confidence.Total, EqualsMessage);
					}
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}

		static OpCode [] arithOpCodes = new OpCode [] {
			OpCodes.Mul,
			OpCodes.Add,
			OpCodes.Sub,
			OpCodes.Div
		};

		private static Instruction SkipArithmeticOperations (Instruction instruction)
		{
			Instruction prevInstr = instruction.Previous;

			while (Array.Exists (arithOpCodes,
				delegate (OpCode code) {
					return code == prevInstr.OpCode;
				})) {
				prevInstr = prevInstr.Previous;
			}

			return prevInstr;
		}

		private static bool CheckFloatConstants (float value)
		{
			// IsInfinity covers both positive and negative infinity
			return (Single.IsInfinity (value) ||
				(Single.MinValue.CompareTo (value) == 0) ||
				(Single.MaxValue.CompareTo (value) == 0));
		}

		private static bool CheckDoubleConstants (double value)
		{
			// IsInfinity covers both positive and negative infinity
			return (Double.IsInfinity (value) ||
				(Double.MinValue.CompareTo (value) == 0) ||
				(Double.MaxValue.CompareTo (value) == 0));
		}
	}
}
