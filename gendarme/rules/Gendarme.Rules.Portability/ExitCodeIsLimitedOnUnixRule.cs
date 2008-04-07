// 
// Gendarme.Rules.Portability.ExitCodeIsLimitedOnUnixRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2007 Daniel Abramov
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Portability {

	[Problem ("The rule detected a value outside the 0-255 range or couldn't be sure of the returned value.")]
	[Solution ("Review that your return values are all between 0 and 255, this will ensure them to works under both Unix and Windows OS.")]
	public class ExitCodeIsLimitedOnUnixRule : Rule, IAssemblyRule, IMethodRule {

		private const string Message = "In Unix, unlike in Windows, Main () method must return values between 0 and 255 inclusively. Change the exit code or change method return type from 'int' to 'void'.";

		private enum InspectionResult {
			Good,
			Bad,
			Unsure
		}

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// we always want to call CheckAssembly (single call on each assembly)
			Runner.AnalyzeAssembly += delegate (object o, RunnerEventArgs e) {
				Active = true;
			};

			// but we want to avoid checking all methods if the Environment type
			// isn't referenced in a module (big performance difference)
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = e.CurrentAssembly.MainModule.TypeReferences.ContainsType ("System.Environment");
			};
		}

		private void Report (MethodDefinition method, Instruction ins, InspectionResult result)
		{
			switch (result) {
			case InspectionResult.Good:
				// should never occur
				break;
			case InspectionResult.Bad:
				Runner.Report (method, ins, Severity.Medium, Confidence.High, 
					"Return value is outside the range of valid values (0-255).");
				break;
			case InspectionResult.Unsure:
				Runner.Report (method, ins, Severity.Medium, Confidence.Low, 
					"Make sure not to return values that are out of range (0-255).");
				break;
			}
		}

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			MethodDefinition entry_point = assembly.EntryPoint;

			// the rule does not apply if the assembly has no entry point
			// or if it's entry point has no IL
			if ((entry_point == null) || !entry_point.HasBody)
				return RuleResult.DoesNotApply;

			// the rule does not apply of the entry point returns void
			// FIXME: entryPoint.ReturnType.ReturnType should not be null with void Main ()
			// either bad unit tests or bug in cecil
			if (entry_point.ReturnType.ReturnType == null || entry_point.ReturnType.ReturnType.FullName != "System.Int32")
				return RuleResult.DoesNotApply;

			Instruction previous = null;
			foreach (Instruction current in entry_point.Body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Nop:
					break;
				case Code.Ret:
					InspectionResult result = CheckInstruction (previous);
					if (result == InspectionResult.Good)
						break;

					Report (entry_point, current, result);
					break;
				default:
					previous = current;
					break;
				}
			}
			return Runner.CurrentRuleResult;
		}

		private static InspectionResult CheckInstruction (Instruction instruction)
		{
			// checks if an instruction loads an inapproriate value onto the stack			
			switch (instruction.OpCode.Code) {
			case Code.Ldc_I4_M1: // -1 is pushed onto stack
				return InspectionResult.Bad;
			case Code.Ldc_I4_0: // small numbers are pushed onto stack -- all OK
			case Code.Ldc_I4_1:
			case Code.Ldc_I4_2:
			case Code.Ldc_I4_3:
			case Code.Ldc_I4_4:
			case Code.Ldc_I4_5:
			case Code.Ldc_I4_6:
			case Code.Ldc_I4_7:
			case Code.Ldc_I4_8:
				return InspectionResult.Good;
			case Code.Ldc_I4_S: // sbyte ([-128, 127]) - should check >= 0
				sbyte b = (sbyte) instruction.Operand;
				return (b >= 0) ? InspectionResult.Good : InspectionResult.Bad;
			case Code.Ldc_I4: // normal int - should check whether is within [0, 255]
				int a = (int) instruction.Operand;
				return (a >= 0 && a <= 255) ? InspectionResult.Good : InspectionResult.Bad;
			case Code.Call:
			case Code.Callvirt:
				if ((instruction.Operand as MethodReference).ReturnType.ReturnType.FullName == "System.Byte")
					return InspectionResult.Good;
				else
					return InspectionResult.Unsure; // could be within 0-255 or not
			default:
				return InspectionResult.Unsure;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply if method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// go!
			Instruction previous = null;
			foreach (Instruction current in method.Body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Nop:
					break;
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					MethodReference calledMethod = (MethodReference) current.Operand;
					if (calledMethod.Name != "set_ExitCode" && calledMethod.Name != "Exit")
						break;
					if (calledMethod.DeclaringType.FullName != "System.Environment")
						break;

					InspectionResult result = CheckInstruction (previous);
					if (result == InspectionResult.Good)
						break;

					Report (method, current, result);
					break;
				}
				previous = current;
			}
			return Runner.CurrentRuleResult;
		}
	}
}
