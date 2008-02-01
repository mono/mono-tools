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

namespace Gendarme.Rules.Portability {

	public class ExitCodeIsLimitedOnUnixRule : IAssemblyRule, IMethodRule {

		private enum InspectionResult {
			Good,
			Bad,
			Unsure
		}

		public MessageCollection CheckAssembly (AssemblyDefinition assemblyDefinition, Runner runner)
		{
			MethodDefinition entryPoint = assemblyDefinition.EntryPoint;
			// if no entry point, good bye
			if (entryPoint == null)
				return runner.RuleSuccess;

			// if it returns void, we don't introspect it
			// FIXME: entryPoint.ReturnType.ReturnType should not be null with void Main ()
			// either bad unit tests or bug in cecil
			if (entryPoint.ReturnType.ReturnType == null || entryPoint.ReturnType.ReturnType.FullName != "System.Int32")
				return runner.RuleSuccess;

			return CheckIntMainBody (entryPoint.Body, runner);
		}


		private static MessageCollection CheckIntMainBody (MethodBody body, Runner runner)
		{
			MessageCollection messages = runner.RuleSuccess;
			Instruction previous = null;
			foreach (Instruction current in body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Nop:
					break;
				case Code.Ret:
					InspectionResult results = CheckInstruction (previous);
					if (results == InspectionResult.Good)
						break;

					Location loc = new Location (body.Method, current.Offset);
					Message message = null;
					if (results == InspectionResult.Bad)
						message = new Message ("In Unix, unlike in Windows, Main () method must return values between 0 and 255 inclusively. Change the exit code or change method return type from 'int' to 'void'.", loc, MessageType.Error);
					else if (results == InspectionResult.Unsure)
						message = new Message ("In Unix, unlike in Windows, Main () method must return values between 0 and 255 inclusively. Be sure not to return values that are out of range.", loc, MessageType.Warning);
					if (messages == null)
						messages = new MessageCollection ();
					messages.Add (message);
					break;
				default:
					previous = current;
					break;
				}
			}
			return messages;
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
			default:
				return InspectionResult.Unsure;
			}

		}

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			// here we check for usage of Environment.ExitCode property
			MethodDefinition entryPoint = method.DeclaringType.Module.Assembly.EntryPoint;

			// assembly must have an entry point
			if (entryPoint == null)
				return runner.RuleSuccess;

			// rule applies only to void Main-ish assemblies
			// int Main () anyways returns something so we shouldn't care about ExitCode usage
			if (entryPoint.ReturnType.ReturnType != null && entryPoint.ReturnType.ReturnType.FullName == "System.Int32")
				return runner.RuleSuccess;

			// go!
			MessageCollection messages = runner.RuleSuccess;
			Instruction previous = null;
			foreach (Instruction current in method.Body.Instructions) {
				// rather useful for debugging
				// Console.WriteLine ("{0} {1}", current.OpCode, current.Operand);
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

					InspectionResult results = CheckInstruction (previous);
					if (results == InspectionResult.Good)
						break;

					if (messages == runner.RuleSuccess)
						messages = new MessageCollection ();

					Message message = null;
					Location loc = new Location (method, current.Offset);
					if (results == InspectionResult.Unsure)
						message = new Message ("In Unix, unlike in Windows, process exit code can be a value between 0 and 255 inclusively. Be sure not to set it to values that are out of range.", loc, MessageType.Warning);
					else // bad
						message = new Message ("In Unix, unlike in Windows, process exit code can be a value between 0 and 255 inclusively. Do not set it to values that are out of range.", loc, MessageType.Error);
					messages.Add (message);
					break;
				}
				previous = current;
			}
			return messages;
		}
	}
}
