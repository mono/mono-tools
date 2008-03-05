//
// Gendarme.Rules.Performance.AvoidUnneededCallsOnStringRule
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

	// The String type returns "this" in the following cases
	// * Clone()
	// * ToString()
	// * ToString(IFormatProvider)
	// * Substring(0)
	// other cases are possible but harder to detect (and not worth the risk of reporting false positives)

	[Problem ("This method needlessly calls some method(s) on a string instance. This may produce some performance penalities.")]
	[Solution ("Remove the unneeded call(s) on the string instance.")]
	public class AvoidUnneededCallsOnStringRule : Rule, IMethodRule {

		private const string MessageString = "There is no need to call {0}({1}) on a System.String instance.";

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					MethodReference mr = (instruction.Operand as MethodReference);
					if ((mr == null) || !mr.HasThis)
						continue;

					// simplest cases where the type name is System.String
					// this occurs for Substring and ToString(IFormatProvider)
					if (CheckTypeReference (mr.DeclaringType)) {
						// in this case we don't have to check the stack to ensure
						// the type is System.String (which is sealed ;-)
						switch (mr.Name) {
						case "Substring":
							// ensure it's System.String::Substring(System.Int32) and that it's given 0 as a parameter
							if ((mr.Parameters.Count == 1) && CheckParam (instruction.Previous)) {
								string text = String.Format (MessageString, mr.Name, "0");
								Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, text);
							}
							continue;
						case "ToString": // most probably ToString(IFormatProvider), possibly ToString()
							string text2 = String.Format (MessageString, mr.Name,
								(mr.Parameters.Count > 1) ? "IFormatProvider" : String.Empty);
							Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, text2);
							continue;
						}
					}

					// complex cases where the type name could be different from String 
					// e.g. virtual Object.ToString ()
					switch (mr.Name) {
					case "ToString":
					case "Clone":
						// check for ToString() or Clone()
						// and ensure it's called on System.String
						if ((mr.Parameters.Count == 0) && CheckStack (instruction.Previous, method)) {
							string text = String.Format (MessageString, mr.Name, String.Empty);
							Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, text);
						}
						break;
					}
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}

		private static bool CheckParam (Instruction instruction)
		{
			if (instruction == null)
				return false;

			switch (instruction.OpCode.Code) {
			case Code.Ldc_I4_0:
				return true;
			case Code.Ldc_I4:
				return ((int) instruction.Operand == 0);
			default:
				return false;
			}
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
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				int arg_index = (int) (instruction.OpCode.Code - (method.HasThis ? Code.Ldarg_1 : Code.Ldarg_0));
				if (arg_index < 0)
					return false; // happens for stuff like this.ToString() or this.Clone()
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
