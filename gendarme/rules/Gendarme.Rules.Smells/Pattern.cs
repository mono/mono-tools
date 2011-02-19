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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {
	internal sealed class Pattern {
		Instruction[] instructions;
		int[] prefixes;
		bool? compilerGeneratedBlock;
		bool? extractableToMethodBlock;

		internal Pattern (Instruction[] block)
		{
			if (block == null)
				throw new ArgumentNullException ("block");
			this.instructions = block;
		}

		// look for: isinst System.IDisposable
		static bool IsInstanceOfIDisposable (Instruction ins)
		{
			if (ins.OpCode.Code != Code.Isinst)
				return false;
			return (ins.Operand as TypeReference).IsNamed ("System", "IDisposable");
		}

		// look for:
		//	callvirt System.Void System.IDisposable::Dispose()
		//	endfinally 
		static bool IsIDisposableDisposePattern (Instruction ins)
		{
			if (ins.OpCode.Code != Code.Callvirt)
				return false;
			if (!(ins.Operand as MethodReference).IsNamed ("System", "IDisposable", "Dispose"))
				return false;
			return ins.Next.Is (Code.Endfinally);
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
			bool call = false;
			for (int i = 0; i < Count; i++) {
				Instruction ins = instructions [i];
				// foreach
				if (ins.OpCode.Code == Code.Callvirt) {
					MethodReference mr = (ins.Operand as MethodReference);
					if (mr.IsNamed ("System.Collections", "IEnumerator", "get_Current"))
						return true;
					if (mr.IsNamed ("System.Collections", "IEnumerator", "MoveNext"))
						return !call;
				}
				// if there's a unknown call then it's likely not (totally) compiler generated
				call |= (ins.OpCode.FlowControl == FlowControl.Call);
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
					(instructions [1].OpCode.Code == Code.Brtrue || instructions [1].OpCode.Code == Code.Brtrue_S) &&
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
			
		internal void ComputePrefixes (MethodDefinition method)
		{
			MethodDefinition target = InstructionMatcher.Target;
			InstructionMatcher.Target = method;
			try {
				int offset = 0;
				if ((prefixes == null) || (prefixes.Length < instructions.Length))
					prefixes = new int [instructions.Length];

				for (int index = 1; index < instructions.Length; index++) {
					while (offset > 0 &&
						!InstructionMatcher.AreEquivalent (instructions [offset], instructions [index]))
						offset = prefixes [offset - 1];

					if (InstructionMatcher.AreEquivalent (instructions [offset], instructions [index]))
						offset++;

					prefixes [index] = offset;
				}
			}
			finally {
				InstructionMatcher.Target = target;
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
				return prefixes;
			}
		}
	}
}
