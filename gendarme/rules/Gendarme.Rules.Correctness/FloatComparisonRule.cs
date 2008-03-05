//
// Gendarme.Rules.Correctness.FloatComparisonRule
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

	[Problem ("This method contais some code that performs equality operation between floating points.")]
	[Solution ("Try comparing the absolute difference between the two floating point values and a small constant value.")]
	public class FloatComparisonRule : Rule, IMethodRule {

		private const string EqualityMessage = "Floating point values should not be directly compared for equality (e.g. == or !=).";
		private const string EqualsMessage = "Floating point values should not be directly compared for equality using [Single|Double].Equals.";

		private static string[] FloatingPointTypes = { "System.Single", "System.Double" };

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// we want to avoid checking all methods if the module doesn't refer to either
			// System.Single or System.Double (big performance difference)
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = e.CurrentAssembly.MainModule.TypeReferences.ContainsAnyType (FloatingPointTypes);
			};
		}

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
				int loc_index = (int) (instruction.OpCode.Code - Code.Ldloc_0);
				return method.Body.Variables [loc_index].VariableType.IsFloatingPoint ();
			case Code.Ldloc_S:
				VariableReference local = instruction.Operand as VariableReference;
				return local.VariableType.IsFloatingPoint ();
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				int arg_index = (int) (instruction.OpCode.Code - Code.Ldarg_0);
				if (!method.IsStatic)
					arg_index--;
				// handle 'this'
				if (arg_index < 0)
					return method.DeclaringType.IsFloatingPoint ();
				return method.Parameters [arg_index].ParameterType.IsFloatingPoint ();
			case Code.Ldarg:
				ParameterReference parameter = instruction.Operand as ParameterReference;
				return parameter.ParameterType.IsFloatingPoint ();
			case Code.Call:
			case Code.Calli:
			case Code.Callvirt:
				MethodReference call = instruction.Operand as MethodReference;
				return call.ReturnType.ReturnType.IsFloatingPoint ();
			case Code.Ldfld:
			case Code.Ldsfld:
				FieldReference field = instruction.Operand as FieldReference;
				return field.FieldType.IsFloatingPoint ();
			}
			return problem;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// we only check methods with a body
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// we don't check System.Single and System.Double
			// special case for handling mscorlib.dll
			if (method.DeclaringType.IsFloatingPoint ())
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Ceq:
					if (CheckCeqInstruction (SkipArithmeticOperations (instruction), method)) {
						Runner.Report (method, instruction, Severity.High, Confidence.Total, EqualityMessage);
					}
					break;
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					MemberReference member = instruction.Operand as MemberReference;
					if (member.Name.Equals ("Equals") && member.DeclaringType.IsFloatingPoint ()) {
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
