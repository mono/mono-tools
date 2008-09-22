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
	/// Comparing floating points values isn't easy, because simple values, such as 0.2, 
	/// cannot be precisely represented. This rule ensures the code doesn't contains 
	/// floating point [in]equality comparison for <c>Single</c> and <c>Double</c> values.
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
	/// void AMethod ()
	/// {
	///	float f1 = 0.1;
	///	float f2 = 0.001 * 100;
	///	if (f1 == f2) {
	///		// ^^^ this equality can be false !
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (delta):
	/// <code>
	/// const float delta = 0.000001;
	/// 
	/// void AMethod ()
	/// { 
	///	float f1 = 0.1;
	///	float f2 = 0.001 * 100;
	///	if (Math.Abs (f1 - f2) &lt; delta) {
	///		// this will work with known value but in real-life
	///		// you may hit [Positive|Negative]Infinity and NaN
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (decimal):
	/// <code>
	/// void BMethod ()
	/// { 
	///	decimal d1 = 0.1m;
	///	decimal d2 = 0.001m * 100;
	///	// decimals are slower but keep their precision
	///	if (d1 == d2) {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.2 this rule was named FloatComparisonRule.</remarks>

	[Problem ("This method contais some code that performs equality operation between floating points.")]
	[Solution ("Try comparing the absolute difference between the two floating point values and a small constant value.")]
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
				return call.ReturnType.ReturnType.IsFloatingPoint ();
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
					if ((member != null) && member.Name.Equals ("Equals") && member.DeclaringType.IsFloatingPoint ()) {
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
