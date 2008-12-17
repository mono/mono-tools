//
// Gendarme.Rules.Smells.Pattern class
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
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Smells {
	internal sealed class Pattern {
		Instruction[] instructions;
		int[] prefixes;
		bool? compilerGeneratedBlock;
		bool? extraibleToMethodBlock;

		internal Pattern (Instruction[] instructions) {
			if (instructions == null)
				throw new ArgumentNullException ("instructions");
			this.instructions = instructions;
		}

		bool IsDisposingBlock ()
		{
			return (Count == 8 && 
				((instructions[0].OpCode.Code == Code.Brtrue &&
				instructions[1].OpCode.Code == Code.Leave &&
				instructions[2].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[3].OpCode.Code == Code.Isinst &&
				instructions[4].OpCode.Code == Code.Dup &&
				instructions[5].OpCode.Code == Code.Brtrue_S &&
				instructions[6].OpCode.Code == Code.Endfinally &&
				//To Disposable
				instructions[7].OpCode.Code == Code.Callvirt)
				||
				(instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[1].OpCode.Code == Code.Isinst &&
				instructions[2].OpCode.Code == Code.Dup &&
				instructions[3].OpCode.Code == Code.Brtrue_S &&
				instructions[4].OpCode.Code == Code.Endfinally &&
				instructions[5].OpCode.Code == Code.Callvirt && //ToDisposable
				instructions[6].OpCode.Code == Code.Endfinally &&
				instructions[7].OpCode.Code == Code.Ret)))
				||
				(Count == 9 &&
				(instructions[0].OpCode.Code == Code.Brtrue &&
				instructions[1].OpCode.Code == Code.Leave &&
				instructions[2].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[3].OpCode.Code == Code.Isinst &&
				instructions[4].OpCode.Code == Code.Dup &&
				instructions[5].OpCode.Code == Code.Brtrue_S &&
				instructions[6].OpCode.Code == Code.Endfinally &&
				instructions[7].OpCode.Code == Code.Callvirt && //ToDisposable
				instructions[8].OpCode.Code == Code.Endfinally) ||
				(
				instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[1].OpCode.Code == Code.Isinst &&
				instructions[2].OpCode.Code == Code.Dup &&
				instructions[3].OpCode.Code == Code.Brtrue_S &&
				instructions[4].OpCode.Code == Code.Endfinally &&
				instructions[5].OpCode.Code == Code.Callvirt && //ToDisposable
				instructions[6].OpCode.Code == Code.Endfinally &&
				instructions[7].OpCode.StackBehaviourPush == StackBehaviour.Pushi && //return value
				instructions[8].OpCode.Code == Code.Ret))
			;
		}
		
		bool IsForeachEnumeratorBlock ()
		{
			return (Count == 5 &&
				instructions[0].OpCode.StackBehaviourPop == StackBehaviour.Pop1  &&
				instructions[1].OpCode.Code == Code.Br &&
				instructions[2].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[3].OpCode.Code == Code.Callvirt &&
				//To get_Current
				instructions[4].OpCode.Code == Code.Castclass)
				||
				(Count == 4 &&
				instructions[0].OpCode.Code == Code.Callvirt &&
				instructions[1].OpCode.Code == Code.Castclass &&
				instructions[2].OpCode.StackBehaviourPop == StackBehaviour.Pop1 &&
				instructions[3].OpCode.StackBehaviourPush == StackBehaviour.Push1)
			;
		}

		bool IsUsingCleanupBlock ()
		{
			return Count == 4 &&
				((instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[1].OpCode.Code == Code.Brfalse &&
				instructions[2].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[3].OpCode.Code == Code.Callvirt)
				//to disposable ...
				||
				(instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[1].OpCode.Code == Code.Callvirt &&
				//to disposable
				instructions[2].OpCode.Code == Code.Endfinally &&
				instructions[3].OpCode.Code == Code.Ret))
			;
		}

		bool IsLoadingOperandForSwitch () 
		{
			return (Count == 4 &&
				instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[1].OpCode.StackBehaviourPop == StackBehaviour.Pop1 &&
				instructions[2].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[3].OpCode.Code == Code.Brfalse)
				||
				(Count == 5 &&
				instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[1].OpCode.Code == Code.Ldfld &&
				instructions[2].OpCode.StackBehaviourPop == StackBehaviour.Pop1 &&
				instructions[3].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[4].OpCode.Code == Code.Brfalse)
			;
		}

		bool IsFillingDictionaryForSwitch () 
		{
			//As we ignore the ldstr operands, the filling of this
			//should be ignored (we will get the error later)
			return Count == 8 &&
				instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[1].OpCode.StackBehaviourPush == StackBehaviour.Pushref &&
				instructions[2].OpCode.StackBehaviourPush == StackBehaviour.Pushi &&
				instructions[3].OpCode.Code == Code.Callvirt && //To dictionary
				instructions[4].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[5].OpCode.StackBehaviourPush == StackBehaviour.Pushref &&
				instructions[6].OpCode.StackBehaviourPush == StackBehaviour.Pushi &&
				instructions[7].OpCode.Code == Code.Callvirt //To dictionary
			;
		}

		bool IsReturningLoopsWithValue () 
		{
			return Count == 4 &&
				instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Pushi &&
				instructions[1].OpCode.Code == Code.Ret &&
				instructions[2].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[3].OpCode.Code == Code.Ret
			;
		}

		bool IsDoubleReturn () 
		{
			return Count > 1 &&
				instructions[Count - 1].OpCode.Code == Code.Ret &&
				instructions[Count - 2].OpCode.Code == Code.Ret
			;
		}
		
		void ComputeCompilerGeneratedBlock ()
		{
			compilerGeneratedBlock = IsUsingCleanupBlock () ||
			IsForeachEnumeratorBlock () || IsDisposingBlock () ||
			IsLoadingOperandForSwitch () || IsFillingDictionaryForSwitch () ||
			IsReturningLoopsWithValue () || IsDoubleReturn ();
		}

		internal bool IsCompilerGeneratedBlock {
			get {
				if (compilerGeneratedBlock == null)
					ComputeCompilerGeneratedBlock ();
				return (bool) compilerGeneratedBlock;
			}
		}

		bool IsReturningCode () 
		{
			return ((Count == 4 &&
				instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
				instructions[1].OpCode.Code == Code.Brtrue &&
				instructions[2].OpCode.StackBehaviourPush == StackBehaviour.Pushi && 
				instructions[3].OpCode.Code == Code.Ret)
				||
				(Count > 1 &&
				instructions[Count - 1].OpCode.Code == Code.Ret &&
				instructions[Count - 2].OpCode.FlowControl == FlowControl.Cond_Branch))
			;
		}

		void ComputeExtraible ()
		{
			extraibleToMethodBlock = !IsReturningCode ();
		}

		internal bool IsExtraibleToMethodBlock {
			get {
				if (extraibleToMethodBlock == null) 
					ComputeExtraible ();
				return (bool) extraibleToMethodBlock;
			}
		}
			
		void ComputePrefixes ()
		{
			prefixes = new int[instructions.Length];
			int offset = 0;

			for (int index = 1; index < instructions.Length; index++) {
				while (offset > 0 &&
					!InstructionMatcher.AreEquivalent (instructions[offset], instructions[index]))
					offset = prefixes[offset - 1];

				if (InstructionMatcher.AreEquivalent (instructions[offset], instructions[index]))  
					offset++;

				prefixes[index] = offset;
			}
		}

		internal int Count {
			get {
				return instructions.Length;
			}
		}
		
		internal Instruction this[int index] {
			get {
				return instructions[index];
			}
		}

		internal int[] Prefixes {
			get {
				if (prefixes == null) 
					ComputePrefixes ();
				return prefixes;
			}
		}
	}
}
