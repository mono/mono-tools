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
		private int maxChainLength = 4;

		public int MaxChainLength {
			get {
				return maxChainLength;
			}
			set {
				maxChainLength = value;
			}
		}

		private void CheckConsecutiveCalls (MethodDefinition method)
		{
			int counter = 0;
			foreach (Instruction instruction in method.Body.Instructions) {
				if (IsCallInstruction (instruction))
					counter++;
				else
					counter = 0;
				if (counter == MaxChainLength)
					Runner.Report (method, instruction, Severity.High, Confidence.Normal, "You are making a message chain, your code is hardly coupled to the navigation structure.  Any change in the relationships will cause a client change.");
			}
		}

		private bool IsCallInstruction (Instruction instruction)
		{
			return instruction.OpCode == OpCodes.Call ||
				instruction.OpCode == OpCodes.Callvirt;
		}

		private bool IsStoreInstruction (Instruction instruction)
		{
			return instruction.OpCode.FlowControl == FlowControl.Next &&
				instruction.OpCode.OpCodeType == OpCodeType.Macro &&
				instruction.OpCode.OperandType == OperandType.InlineNone &&
				instruction.OpCode.StackBehaviourPop == StackBehaviour.Pop1 &&
				instruction.OpCode.StackBehaviourPush == StackBehaviour.Push0;
		}

		private bool IsLoadInstruction (Instruction instruction)
		{
			return instruction.OpCode.FlowControl == FlowControl.Next &&
				instruction.OpCode.OpCodeType == OpCodeType.Macro &&
				instruction.OpCode.OperandType == OperandType.InlineNone &&
				instruction.OpCode.StackBehaviourPop == StackBehaviour.Pop0 &&
				instruction.OpCode.StackBehaviourPush == StackBehaviour.Push1;
		}

		private int GetVariableIdentifierFrom (OpCode opCode) 
		{
			return Int32.Parse (opCode.Name.Substring (opCode.Name.Length - 1, 1));
		}

		private Dictionary<int, int> GetPossibleChains (MethodDefinition method)
		{
			//Is not the better choice, multiples values,
			//reassignations, but by the moment ...
			Dictionary<int, int> possibleChains = new Dictionary<int, int> ();
			int currentVariable = 0;
			Instruction current = method.Body.Instructions[method.Body.Instructions.Count - 1];
			while (current != null) {
				//if a load instruction is interfered with a
				//store we will have a dependency.
				if (IsStoreInstruction (current)) 
					currentVariable = GetVariableIdentifierFrom (current.OpCode);	
				if (IsLoadInstruction (current)) 
					if (currentVariable != GetVariableIdentifierFrom (current.OpCode))  
						possibleChains.Add (currentVariable, GetVariableIdentifierFrom (current.OpCode));
				current = current.Previous;
			}
			return possibleChains;
		}

		private Dictionary<int, int> GetCostTable (MethodDefinition method)
		{
			Dictionary<int, int> costTable = new Dictionary<int, int> ();
			int counter = 0;
			foreach (Instruction current in method.Body.Instructions) {
				if (IsStoreInstruction (current)) {
					costTable.Add (GetVariableIdentifierFrom (current.OpCode), counter);
					counter = 0;
				}
				else 
					if (IsCallInstruction (current))
						counter++;
					else
						counter = 0;
			}
			return costTable;
		}

		private void CheckTemporaryLocals (MethodDefinition method)
		{
			//At least one local var to continue
			if (method.Body.Variables.Count == 0)
				return;
			
			Dictionary<int, int> possibleChains = GetPossibleChains (method);
			Dictionary<int, int> costTable = GetCostTable (method);
			
			foreach (int key in possibleChains.Keys) {
				int depends = possibleChains[key];
				int totalCost = costTable[key] + costTable[depends];
				if (totalCost == MaxChainLength)
					Runner.Report (method, Severity.High, Confidence.Normal, "You are making a message chain, your code is hardly coupled to the navigation structure.  Any change in the relationships will cause a client change.");
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody) 
				return RuleResult.DoesNotApply;
			
			CheckConsecutiveCalls (method);
			CheckTemporaryLocals (method);

			return Runner.CurrentRuleResult;
		}
	}
}
