//
// Gendarme.Rules.Performance.AvoidUnusedParameters class
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	[Problem ("The method contains unused parameters.")]
	[Solution ("You should remove or use the unused parameters.")]
	public class AvoidUnusedParametersRule : Rule, IMethodRule {

		private static bool UseParameter (MethodDefinition method, ParameterDefinition parameter)
		{
			if (!method.HasBody)
				return false;

			foreach (Instruction instruction in method.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Ldarg_0:
					if (method.Parameters.IndexOf (parameter) == 0)
						return true;
					break;
				case Code.Ldarg_1:
					if (method.Parameters.IndexOf (parameter) == (method.IsStatic ? 1 : 0))
						return true;
					break;
				case Code.Ldarg_2:
					if (method.Parameters.IndexOf (parameter) == (method.IsStatic ? 2 : 1))
						return true;
					break;
				case Code.Ldarg_3:
					if (method.Parameters.IndexOf (parameter) == (method.IsStatic ? 3 : 2))
						return true;
					break;
				case Code.Ldarg_S:
				case Code.Ldarga:
				case Code.Ldarga_S:
					if (instruction.Operand.Equals (parameter))
						return true;
					break;
				default:
					break;
				}
			}
			return false;
		}

		private static bool ContainsReferenceDelegateInstructionFor (MethodDefinition method, MethodDefinition delegateMethod)
		{
			if (!method.HasBody)
				return false;
			foreach (Instruction instruction in method.Body.Instructions) {
				if (instruction.OpCode.Code == Code.Ldftn)
					return instruction.Operand.Equals (delegateMethod);
			}
			return false;
		}

		private static bool IsReferencedByDelegate (MethodDefinition delegateMethod)
		{
			TypeDefinition declaringType = delegateMethod.DeclaringType as TypeDefinition;
			if (declaringType != null) {
				foreach (MethodDefinition method in declaringType.Methods) {
					if (ContainsReferenceDelegateInstructionFor (method, delegateMethod))
						return true;
				}

				foreach (MethodDefinition method in declaringType.Constructors) {
					if (ContainsReferenceDelegateInstructionFor (method, delegateMethod))
						return true;
				}
			}
			return false;
		}

		private static List<ParameterDefinition> GetUnusedParameters (MethodDefinition method)
		{
			List<ParameterDefinition> unusedParameters = new List<ParameterDefinition> ();
			foreach (ParameterDefinition parameter in method.Parameters) {
				// EventArgs parameters are often required in method signatures,
				// but often not required. Reduce "false positives"(*) for GUI apps
				// (*) it's more a "don't report things outside developer's control"
				if (parameter.ParameterType.Inherits ("System.EventArgs")) {
					// even the other parameters are often ignored since
					// the signature is made to cover most cases
					unusedParameters.Clear ();
					return unusedParameters;
				}

				if (!UseParameter (method, parameter))
					unusedParameters.Add (parameter);
			}
			return unusedParameters;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// catch abstract, pinvoke and icalls - where rule does not apply
			// rule doesn't apply to virtual, overrides or generated code
			// doesn't apply to code referenced by delegates (note: more complex check moved last)
			if (!method.HasBody || method.IsVirtual || method.Overrides.Count != 0 || 
			     method.IsGeneratedCode () || IsReferencedByDelegate (method))
				return RuleResult.DoesNotApply;

			// rule applies
			foreach (ParameterDefinition parameter in GetUnusedParameters (method)) {
				string text = String.Format ("Parameter '{0}' of type '{1}' is never used in the method.", 
					parameter.Name, parameter.ParameterType);
				Runner.Report (parameter, Severity.Medium, Confidence.Normal, text);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
