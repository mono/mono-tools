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
			if (source.OpCode != target.OpCode)
				return false;

			if (source.OpCode.Code == Code.Ldstr)
				return true;

			//Check the types in ldarg stuff.  Sometimes this scheme
			//could lead us to false positives.  We need an analysis
			//which depends on the context.
			if (source.OpCode.Code == Code.Ldarg_1)
				return Current.Parameters.Count > 0 && Target.Parameters.Count > 0 ? 
					Current.Parameters[0].ParameterType.Equals (Target.Parameters[0].ParameterType) : 
					false;

			if (source.OpCode.Code == Code.Ldarg_2)
				return Current.Parameters.Count > 1 && Target.Parameters.Count > 1 ?
					Current.Parameters[1].ParameterType.Equals (Target.Parameters[1].ParameterType) :
					false;

			if (source.OpCode.Code == Code.Ldarg_3)
				return Current.Parameters.Count > 2 && Target.Parameters.Count > 2 ?
					Current.Parameters[2].ParameterType.Equals (Target.Parameters[2].ParameterType) : 
					false;

			//TODO: The same for ldloc / stloc
			if (source.OpCode.Code == Code.Ldloc_0) 
				return Current.Body.Variables.Count > 0 && Target.Body.Variables.Count > 0 ? 
					Current.Body.Variables[0].VariableType.Equals (Target.Body.Variables[0].VariableType):
					false;
			
			if (source.OpCode.Code == Code.Ldloc_1) 
				return Current.Body.Variables.Count > 1 && Target.Body.Variables.Count > 1 ?
					Current.Body.Variables[1].VariableType.Equals (Target.Body.Variables[1].VariableType):
					false;

			if (source.OpCode.Code == Code.Ldloc_2) 
				return Current.Body.Variables.Count > 2 && Target.Body.Variables.Count > 2 ?
					Current.Body.Variables[2].VariableType.Equals (Target.Body.Variables[2].VariableType):
					false;

			if (source.OpCode.Code == Code.Ldloc_3) 
				return Current.Body.Variables.Count > 3 && Target.Body.Variables.Count > 3 ?
					Current.Body.Variables[3].VariableType.Equals (Target.Body.Variables[3].VariableType):
					false;

			//WARNING: Dirty Evil Hack: this should be in the
			//Pattern class
			if (source.OpCode.Code == Code.Ret && source.Previous != null &&
				(source.Previous.OpCode.StackBehaviourPush == StackBehaviour.Pushi ||
				source.Previous.OpCode.Code == Code.Ldnull ||
				source.Previous.OpCode.StackBehaviourPush == StackBehaviour.Push1))
				return false;
			
			//if (source.Operand != target.Operand)
			if (source.Operand != null && target.Operand != null) {
				if (source.Operand is Instruction)
					return ((Instruction) source.Operand).Offset == ((Instruction) target.Operand).Offset;
				//See the tests for more reference.
				//Alloc in comparisons isn't a nice idea :(
				//REFS: Perhaps for each method that cecil
				//reads, it creates a new instance for each
				//operator as FieldDefinition?
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
