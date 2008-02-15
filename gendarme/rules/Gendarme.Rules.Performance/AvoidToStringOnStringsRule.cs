//
// Gendarme.Rules.Performance.AvoidToStringOnStringsRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
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

namespace Gendarme.Rules.Performance {

	[Problem ("You are calling ToString () in a string in the member '{0}', this is redundant and may produce some performance penalities.")]
	[Solution ("You should remove the ToString () call.")]
	public class AvoidToStringOnStringsRule : Rule, IMethodRule {

		private const string MessageString = "No need to call ToString on a System.String instance";

		public RuleResult CheckMethod (MethodDefinition method)
		{
			bool containsAvoidToStringOnStrings = false;
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					if (IsToString (instruction.Operand as MethodReference)) {
						if (CheckStack (instruction.Previous, method)) {
							Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, MessageString);
							containsAvoidToStringOnStrings = true;
						}
					}
					break;
				}
			}

			return containsAvoidToStringOnStrings? RuleResult.Failure : RuleResult.Success;
		}

		private static bool IsToString (MethodReference method)
		{
			if (method == null)
				return false;
			return (method.HasThis && (method.Name == "ToString") && (method.Parameters.Count == 0));
		}

		private static bool CheckStack (Instruction instruction, MethodDefinition method)
		{
			switch (instruction.OpCode.Code) {
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
				int loc_index = (int) (instruction.OpCode.Code - Code.Ldloc_0);
				return CheckTypeReference (method.Body.Variables [loc_index].VariableType);
			case Code.Ldloc_S:
				VariableReference local = instruction.Operand as VariableReference;
				return CheckTypeReference (local.VariableType);
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				int arg_index = (int) (instruction.OpCode.Code - Code.Ldarg_1);
				return CheckTypeReference (method.Parameters [arg_index].ParameterType);
			case Code.Ldarg:
				ParameterReference parameter = instruction.Operand as ParameterReference;
				return CheckTypeReference (parameter.ParameterType);
			case Code.Call:
			case Code.Calli:
			case Code.Callvirt:
				MethodReference call = instruction.Operand as MethodReference;
				return CheckTypeReference (call.ReturnType.ReturnType);
			case Code.Ldfld:
			case Code.Ldsfld:
				FieldReference field = instruction.Operand as FieldReference;
				return CheckTypeReference (field.FieldType);
			}
			return false;
		}

		private static bool CheckTypeReference (TypeReference type)
		{
			return (type.FullName == "System.String");
		}
	}
}
