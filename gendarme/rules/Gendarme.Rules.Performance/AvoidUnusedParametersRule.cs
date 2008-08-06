//
// Gendarme.Rules.Performance.AvoidUnusedParametersRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2007 Néstor Salceda
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	[Problem ("The method contains unused parameters.")]
	[Solution ("You should remove or use the unused parameters.")]
	public class AvoidUnusedParametersRule : Rule, IMethodRule {

		private static bool ContainsReferenceDelegateInstructionFor (MethodDefinition method, MethodDefinition delegateMethod)
		{
			if (!method.HasBody)
				return false;

			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.Code == Code.Ldftn &&
				    instruction.Operand.Equals (delegateMethod))
					return true;
			}
			return false;
		}

		private static bool IsReferencedByDelegate (MethodDefinition delegateMethod)
		{
			TypeDefinition declaringType = delegateMethod.DeclaringType as TypeDefinition;
			if (declaringType == null)
				return false;

			foreach (var method in declaringType.AllMethods ())
				if (ContainsReferenceDelegateInstructionFor (method, delegateMethod))
					return true;

			return false;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// catch abstract, pinvoke and icalls - where rule does not apply
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// skip methods without parameters
			int pcount = method.Parameters.Count;
			if (pcount == 0)
				return RuleResult.DoesNotApply;

			// rule doesn't apply to virtual, overrides or generated code

			// doesn't apply to code referenced by delegates (note: more complex check moved last)
			if (method.IsVirtual || method.Overrides.Count != 0 || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;
		
			// Also EventArgs parameters are often required in method signatures,
			// but often not required. Reduce "false positives"(*) for GUI apps
			// (*) it's more a "don't report things outside developer's control"
			if (method.IsEventCallback () || IsReferencedByDelegate (method))
				return RuleResult.DoesNotApply;

			// rule applies

			// we limit ourselves to the first 64 parameters
			if (pcount > 64)
				pcount = 64;
			ulong mask = 0;

			// scan IL to see which parameter is being used
			foreach (Instruction ins in method.Body.Instructions) {
				ParameterDefinition parameter = ins.GetParameter (method);
				if (parameter == null)
					continue;
				mask |= ((ulong)1 << (parameter.Sequence - 1));
			}

			// quick out based on value - i.e. every parameter is being used
			int shift = 64 - pcount;
			if ((mask << shift) == (UInt64.MaxValue << shift))
				return RuleResult.Success;

			for (int i = 0; i < pcount; i++) {
				if ((mask & ((ulong) 1 << i)) == 0) {
					ParameterDefinition parameter = method.Parameters [i];
					string text = String.Format ("Parameter '{0}' of type '{1}' is never used in the method.",
						parameter.Name, parameter.ParameterType);
					Runner.Report (parameter, Severity.Medium, Confidence.Normal, text);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
