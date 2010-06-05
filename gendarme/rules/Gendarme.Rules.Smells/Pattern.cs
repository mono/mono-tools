//
// Gendarme.Rules.Smells.Pattern class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2008 Néstor Salceda
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Smells {
	internal sealed class Pattern {
		Instruction[] instructions;
		int[] prefixes;
		bool? compilerGeneratedBlock;
		bool? extractableToMethodBlock;

		internal Pattern (Instruction[] instructions) {
			if (instructions == null)
				throw new ArgumentNullException ("instructions");
			this.instructions = instructions;
		}

		// look for: isinst System.IDisposable
		static bool IsInstanceOfIDisposable (Instruction ins)
		{
			if (ins.OpCode.Code != Code.Isinst)
				return false;
			return ((ins.Operand as TypeReference).FullName == "System.IDisposable");
		}

		// look for a virtual call to a specific method
		static bool IsCallVirt (Instruction ins, string typeName, string methodName)
		{
			if (ins.OpCode.Code != Code.Callvirt)
				return false;
			MethodReference mr = (ins.Operand as MethodReference);
			return ((mr != null) && (mr.Name == methodName) && (mr.DeclaringType.FullName == typeName));
		}

		// look for:
		//	callvirt System.Void System.IDisposable::Dispose()
		//	endfinally 
		static bool IsIDisposableDisposePattern (Instruction ins)
		{
			if (!IsCallVirt (ins, "System.IDisposable", "Dispose"))
				return false;
			Instruction next = ins.Next;
			return ((next != null) && (next.OpCode.Code == Code.Endfinally));
		}

		// IIRC older xMCS generated that quite often
		bool IsDoubleReturn {
			get {
				return ((Count > 1) && (instructions[Count - 1].OpCode.Code == Code.Ret) &&
					(instructions[Count - 2].OpCode.Code == Code.Ret));
			}
		}

		// small patterns that highly suggest they were compiler generated
		bool ComputeUnlikelyUserPatterns ()
		{
			for (int i = 0; i < Count; i++) {
				Instruction ins = instructions [i];
				// foreach
				if (IsCallVirt (ins, "System.Collections.IEnumerator", "get_Current"))
					return true;
				// foreach
				if (IsInstanceOfIDisposable (ins))
					return true;
				// foreach, using
				if (IsIDisposableDisposePattern (ins))
					return true;
			}
			return false;
		}
		
		internal bool IsCompilerGeneratedBlock {
			get {
				if (compilerGeneratedBlock == null)
					compilerGeneratedBlock = ComputeUnlikelyUserPatterns () || IsDoubleReturn;
				return (bool) compilerGeneratedBlock;
			}
		}

		bool IsReturningCode {
			get {
				return ((Count == 4 &&
					instructions[0].OpCode.StackBehaviourPush == StackBehaviour.Push1 &&
					instructions[1].OpCode.Code == Code.Brtrue &&
					instructions[2].OpCode.StackBehaviourPush == StackBehaviour.Pushi && 
					instructions[3].OpCode.Code == Code.Ret)
					||
					(Count > 1 &&
					instructions[Count - 1].OpCode.Code == Code.Ret &&
					instructions[Count - 2].OpCode.FlowControl == FlowControl.Cond_Branch));
			}
		}

		internal bool IsExtractableToMethodBlock {
			get {
				if (extractableToMethodBlock == null) 
					extractableToMethodBlock = !IsReturningCode;
				return (bool) extractableToMethodBlock;
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
