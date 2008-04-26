//
// AvoidMessageChainsRule class
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

using Gendarme.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Smells {

	[Problem ("The code contains long sequences of method calls or temporary variables, this means your code is hardly coupled to the navigation structure.")]
	[Solution ("You can apply the Hide Delegate refactoring or Extract Method to push down the chain.")]
	public class AvoidMessageChainsRule : Rule, IMethodRule {
		private int maxChainLength = 5;

		public int MaxChainLength {
			get {
				return maxChainLength;
			}
			set {
				maxChainLength = value;
			}
		}

		private bool IsDelimiter (Instruction instruction)
		{
			return instruction.OpCode == OpCodes.Pop ||
				instruction.OpCode == OpCodes.Stloc_0 ||
				instruction.OpCode == OpCodes.Stloc_1 ||
				instruction.OpCode == OpCodes.Stloc_2 ||
				instruction.OpCode == OpCodes.Stloc_3 ||
				instruction.OpCode == OpCodes.Stloc_S ||
				instruction.OpCode == OpCodes.Stloc ||
				instruction.OpCode == OpCodes.Stfld ||
				instruction.OpCode.FlowControl == FlowControl.Branch ||
				instruction.OpCode.FlowControl == FlowControl.Cond_Branch||
				instruction.OpCode == OpCodes.Throw;
		}

		private void CheckConsecutiveCalls (MethodDefinition method)
		{
			int counter = 0;
			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode == OpCodes.Callvirt)
					counter++;
				if (IsDelimiter (instruction)) {
					if (counter >= MaxChainLength)
						Runner.Report (method, instruction, Severity.Medium, Confidence.Low, "This method contains a message chain, your code could be hardly coupled to with its navigation structure.");
					counter = 0;
				}
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody) 
				return RuleResult.DoesNotApply;
			
			CheckConsecutiveCalls (method);

			return Runner.CurrentRuleResult;
		}
	}
}
