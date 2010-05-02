//
// Gendarme.Rules.Smells.InstructionMatcher class
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
using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {
	internal static class InstructionMatcher {
		static MethodDefinition current;
		static MethodDefinition target;

		internal static MethodDefinition Current {
			get {
				return current;
			}
			set {
				current = value;
			}
		}

		internal static MethodDefinition Target {
			get {
				return target;
			}
		 	set {
				target = value;
			}
		}

		internal static bool AreEquivalent (Instruction source, Instruction target)
		{
			if (source.OpCode.Code != target.OpCode.Code)
				return false;

			if (source.OpCode.Code == Code.Ldstr)
				return true;

			//Check the types in ldarg stuff.  Sometimes this scheme
			//could lead us to false positives.  We need an analysis
			//which depends on the context.
			if (source.IsLoadArgument ()) {
				ParameterDefinition p = source.GetParameter (Current);
				if (p != null) {
					ParameterDefinitionCollection cp = Current.Parameters;
					ParameterDefinitionCollection tp = Target.Parameters;
					int pos = p.Sequence - 1;
					return cp.Count > pos && tp.Count > pos ?
						cp [pos].ParameterType.Equals (tp [pos].ParameterType) :
						false;
				}
			}

			// The same for ldloc / stloc
			if (source.IsLoadLocal ()) {
				VariableDefinition v = source.GetVariable (Current);
				if (v != null) {
					VariableDefinitionCollection cv = Current.Body.Variables;
					VariableDefinitionCollection tv = Target.Body.Variables;
					int pos = v.Index;
					return cv.Count > pos && tv.Count > pos ?
						cv [pos].VariableType.Equals (tv [pos].VariableType) :
						false;
				}
			}

			//WARNING: Dirty Evil Hack: this should be in the
			//Pattern class
			if (source.OpCode.Code == Code.Ret && source.Previous != null &&
				(source.Previous.OpCode.StackBehaviourPush == StackBehaviour.Pushi ||
				source.Previous.OpCode.Code == Code.Ldnull ||
				source.Previous.OpCode.StackBehaviourPush == StackBehaviour.Push1))
				return false;
			
			//if (source.Operand != target.Operand)
			if (source.Operand != null && target.Operand != null) {
				// we're sure that target.Operand is of the same type as source.Operand (same OpCode is used)
				Instruction si = (source.Operand as Instruction);
				if (si != null)
					return (si.Offset == ((Instruction) target.Operand).Offset);
				IMetadataTokenProvider sm = (source.Operand as IMetadataTokenProvider);
				if (sm != null)
					return sm.Equals (target.Operand as IMetadataTokenProvider);
				if (source.Operand == target.Operand)
					return true;
				// last chance: we do call ToString
				if (source.Operand.ToString () != target.Operand.ToString ())
					return false;
			}

			return true;
		}

		//This is a Knuth-Morris-Pratt string-matching algorithm, but applied to
		//Mono.Cecil.Cil.Instruction instead of characters, and a
		//InstructionCollection instead of string.  I think it's a nice
		//metaphor :)
		internal static bool Match (Pattern pattern, InstructionCollection target)
		{
			int instructionsMatched = 0;

			for (int index = 0; index < target.Count; index++) {
				while (instructionsMatched > 0 && !AreEquivalent (pattern[instructionsMatched], target[index]))
					instructionsMatched = pattern.Prefixes[instructionsMatched - 1];

				if (AreEquivalent (pattern[instructionsMatched], target[index]))
					instructionsMatched++;

				if (instructionsMatched == pattern.Count)
					return true;
			}

			return false;
		}
	}
}
