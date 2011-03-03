//
// Gendarme.Rules.Smells.CodeDuplicatedLocator class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007-2008 Néstor Salceda
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {

	internal sealed class CodeDuplicatedLocator {

		static ReadOnlyCollection<Pattern> Empty = new ReadOnlyCollection<Pattern> (new List<Pattern> ());

		HashSet<string> methods = new HashSet<string> ();
		HashSet<string> types = new HashSet<string> ();
		Dictionary<MethodDefinition, IList<Pattern>> patternsCached = new Dictionary<MethodDefinition, IList<Pattern>> ();
		IRule parent_rule;

		internal CodeDuplicatedLocator (IRule rule) 
		{
			parent_rule = rule;
		}

		internal ICollection<string> CheckedMethods {
			get {
				return methods;
			}
		}

		internal ICollection<string> CheckedTypes {
			get {
				return types;
			}
		}

		internal void Clear ()
		{
			methods.Clear ();
			types.Clear ();
		}

		internal void CompareMethodAgainstTypeMethods (MethodDefinition current, TypeDefinition targetType)
		{
			if (CheckedTypes.Contains (targetType.Name)) 
				return;
			
			foreach (MethodDefinition target in targetType.Methods) {
				if (target.IsConstructor || target.IsGeneratedCode ())
					continue;

				Pattern duplicated = GetDuplicatedCode (current, target);
				if (duplicated != null && duplicated.Count > 0) {
					parent_rule.Runner.Report (current, duplicated[0], Severity.High, Confidence.Normal, 
						String.Format (CultureInfo.InvariantCulture, "Duplicated code with {0}", target.GetFullName ()));
				}
			}
		}

		bool CanCompareMethods (MethodDefinition current, MethodDefinition target)
		{
			return current.HasBody && target.HasBody &&
				!CheckedMethods.Contains (target.Name) &&
				current != target;
		}

		[Conditional ("DEBUG")]
		void WriteToOutput (MethodDefinition current, MethodDefinition target, Pattern found) 
		{
			Log.WriteLine (this, "Found pattern in {0} and {1}", current, target);
			Log.WriteLine (this, "\t Pattern");
			for (int index = 0; index < found.Count; index++) 
				Log.WriteLine (this, "\t\t{0} - {1}",
					found[index].OpCode.Code,
					found[index].Operand != null? found[index].Operand : "No operator");
		}

		Pattern GetDuplicatedCode (MethodDefinition current, MethodDefinition target)
		{
			if (!CanCompareMethods (current, target))
				return null;

			IList<Pattern> patterns = GetPatterns (current);
			if (patterns.Count == 0)
				return null;
			
			InstructionMatcher.Current = current;
			InstructionMatcher.Target = target;

			foreach (Pattern pattern in patterns) {
				if (pattern.IsCompilerGeneratedBlock || !pattern.IsExtractableToMethodBlock)
					continue;

				if (InstructionMatcher.Match (pattern, target.Body.Instructions)) {
					WriteToOutput (current, target, pattern);
					return pattern;
				}
			}

			return null;
		}
		

		IList<Pattern> GetPatterns (MethodDefinition method) 
		{
			IList<Pattern> patterns = Empty;
			if (!patternsCached.TryGetValue (method, out patterns)) {
				patterns = GeneratePatterns (method);
				patternsCached.Add (method, patterns);
			}
			return patterns;
		}

		//TODO: Still needs some testing in order to get the best size
		//for every case:
		//  The idea is get two overlapped statements in high level language
		static IList<Pattern> GeneratePatterns (MethodDefinition method) 
		{
			Stack<Stack<Instruction>> result = new Stack<Stack<Instruction>> ();
			Stack<Instruction> current = new Stack<Instruction> ();
			int stackCounter = 0;

			var instructions = method.Body.Instructions;
			for (int index = instructions.Count - 1; index >= 0; index--) {
				Instruction currentInstruction = instructions [index];
				stackCounter += currentInstruction.GetPushCount ();
				stackCounter -= currentInstruction.GetPopCount (method);	
				
				if (result.Count != 0)
					result.Peek ().Push (currentInstruction);

				current.Push (currentInstruction);

				if (stackCounter == 0 && current.Count > 1) {//&& currentInstruction.OpCode.FlowControl != FlowControl.Branch) {  
					result.Push (current);
					current = new Stack<Instruction> ();
				}
			}

			//We can remove the first ocurrence
			if (result.Count != 0)
				result.Pop ();

			if (result.Count == 0)
				return Empty;

			IList<Pattern> res = new List<Pattern> ();
			foreach (Stack<Instruction> stack in result) 
				res.Add (new Pattern (stack.ToArray ()));

			return res;
		}
	}
}
