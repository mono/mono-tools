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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule detects when some methods, like <c>Clone()</c>, <c>Substring(0)</c>, 
	/// <c>ToString()</c> or <c>ToString(IFormatProvider)</c>, are being called on a 
	/// string instance. Since every case returns the exact same reference the extra 
	/// call(s) only slow downs performance.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void PrintName (string name)
	/// {
	///	Console.WriteLine ("Name: {0}", name.ToString ());
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void PrintName (string name)
	/// {
	///	Console.WriteLine ("Name: {0}", name);
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.0 this rule was more limited and named AvoidToStringOnStringsRule</remarks>

	[Problem ("This method needlessly calls some method(s) on a string instance. This may produce some performance penalities.")]
	[Solution ("Remove the unneeded call(s) on the string instance.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidUnneededCallsOnStringRule : Rule, IMethodRule {

		private const string MessageString = "There is no need to call {0}({1}) on a System.String instance.";

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Call or Callvirt instructions in the method ?
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction instruction in method.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					MethodReference mr = (instruction.Operand as MethodReference);
					if ((mr == null) || !mr.HasThis)
						continue;

					string text = null;
					switch (mr.Name) {
					case "Clone":
						text = CheckClone (mr, instruction, method);
						break;
					case "Substring":
						text = CheckSubstring (mr, instruction);
						break;
					case "ToString":
						text = CheckToString (mr, instruction, method);
						break;
					default:
						continue;
					}

					if (text != null)
						Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, text);
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}

		private static string CheckClone (MethodReference call, Instruction ins, MethodDefinition method)
		{
			if (call.Parameters.Count != 0)
				return null;
			if (!CheckStack (ins.Previous, method))
				return null;

			return String.Format (MessageString, call.Name, String.Empty);
		}

		private static string CheckSubstring (MethodReference call, Instruction ins)
		{
			if (!CheckTypeReference (call.DeclaringType))
				return null;

			// ensure it's System.String::Substring(System.Int32) and that it's given 0 as a parameter
			if (call.Parameters.Count != 1)
				return null;
			if (!CheckParam (ins.Previous))
				return null;

			return String.Format (MessageString, call.Name, "0");
		}

		private static string CheckToString (MethodReference call, Instruction ins, MethodDefinition method)
		{
			if (CheckTypeReference (call.DeclaringType)) {
				// most probably ToString(IFormatProvider), possibly ToString()
				return String.Format (MessageString, call.Name, 
					(call.Parameters.Count > 1) ? "IFormatProvider" : String.Empty);
			} else {
				// signature for Clone is identical (well close enough) to share code
				return CheckClone (call, ins, method);
			}
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
			case Code.Ldc_I4_S:
				return ((sbyte) instruction.Operand == 0);
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
			case Code.Ldloc:
			case Code.Ldloc_S:
			case Code.Ldloca:
			case Code.Ldloca_S:
				return CheckTypeReference (instruction.GetVariable (method).VariableType);
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
				ParameterReference parameter = instruction.GetParameter (method);
				if (parameter == null)
					return false;
				return CheckTypeReference (parameter.ParameterType);
			case Code.Call:
			case Code.Callvirt:
				MethodReference call = instruction.Operand as MethodReference;
				return CheckTypeReference (call.ReturnType.ReturnType);
			case Code.Ldfld:
			case Code.Ldflda:
			case Code.Ldsfld:
			case Code.Ldsflda:
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
