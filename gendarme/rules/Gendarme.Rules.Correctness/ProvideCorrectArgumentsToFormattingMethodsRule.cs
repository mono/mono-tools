//
// Gendarme.Rules.Correctness.ProvideCorrectArgumentsToFormattingMethodsRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using System.Linq;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Correctness {
	[Problem ("You are calling to a Format method without the correct arguments.  This could throw an unexpected FormatException.")]
	[Solution ("Pass the correct arguments to the formatting method.")]
	public class ProvideCorrectArgumentsToFormattingMethodsRule : Rule, IMethodRule {
		static MethodSignature formatSignature = new MethodSignature ("Format", "System.String");
		static HashSet<char> results = new HashSet<char> ();

		private static IEnumerable<Instruction> GetCallsToStringFormat (MethodDefinition method)
		{
			return from Instruction instruction in method.Body.Instructions
			where (instruction.OpCode.FlowControl == FlowControl.Call) && 
				formatSignature.Matches ((MethodReference) instruction.Operand) &&
				String.Compare ("System.String", ((MethodReference) instruction.Operand).DeclaringType.ToString ()) == 0
			select instruction;
		}

		private static Instruction GetLoadStringInstruction (Instruction call)
		{
			Instruction current = call;
			Instruction farest = null;
			while (current != null) {
				if (current.OpCode == OpCodes.Ldstr) {
					//skip strings until get a "valid" one
					if (GetExpectedParameters ((string)current.Operand) != 0)
						return current;
					else 
						farest = current;
				}
				current = current.Previous;	
			}
			return farest;
		}

		//TODO: It only works with 0 - 9 digits, no more than 10.
		private static int GetExpectedParameters (string loaded)
		{
			results.Clear ();
			for (int index = 0; index < loaded.Length; index++) {
				if (loaded[index] == '{' && Char.IsDigit (loaded[index + 1]))
					results.Add (loaded[index]);
			}

			return results.Count;
		}

		private static int CountElementsInTheStack (MethodDefinition method, Instruction start, Instruction end)
		{
			Instruction current = start;
			int counter = 0;
			bool newarrDetected = false;
			while (end != current) {
				if (newarrDetected) {
					//Count only the stelem instructions if
					//there are a newarr instruction.
					if (current.OpCode == OpCodes.Stelem_Ref)
						counter++;
				}
				else {
					//Count with the stack
					counter += current.GetPushCount ();
					counter -= current.GetPopCount (method);
				}
				//If there are a newarr we need an special
				//behaviour
				if (current.OpCode == OpCodes.Newarr) {
					newarrDetected = true;
					counter = 0;
				}
				current = current.Next;
			}
			return counter;
		}

		private void CheckCallToFormatter (Instruction call, MethodDefinition method)
		{
			Instruction loadString = GetLoadStringInstruction (call);
			if (loadString == null) 
				return;

			int expectedParameters = GetExpectedParameters ((string) loadString.Operand);
			int elementsPushed = CountElementsInTheStack (method, loadString.Next, call);
			
			//There aren't parameters, and isn't a string with {
			//characters
			if (elementsPushed == 0 && expectedParameters == 0) {
				Runner.Report (method, call, Severity.Low, Confidence.Normal, "You are calling String.Format without arguments, you can remove the call to String.Format");
				return;
			}
			
			//It's likely you are calling a method for getting the
			//formatting string.
			if (expectedParameters == 0)
				return;
			
			if (elementsPushed < expectedParameters)
				Runner.Report (method, call, Severity.Critical, Confidence.Normal, String.Format ("The String.Format method is expecting {0} parameters, but only {1} are found.", expectedParameters, elementsPushed));
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			IEnumerable<Instruction> callsToStringFormat = GetCallsToStringFormat (method);	
			if (callsToStringFormat.Count () == 0)
				return RuleResult.DoesNotApply;

			foreach (Instruction call in callsToStringFormat) 
				CheckCallToFormatter (call, method);

			return Runner.CurrentRuleResult;
		}
	}
}
