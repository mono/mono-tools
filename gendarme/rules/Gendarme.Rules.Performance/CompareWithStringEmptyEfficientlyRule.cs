//
// Gendarme.Rules.Performance.CompareWithStringEmptyEfficientlyRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule will fire if a string is compared to <c>""</c> or <c>String.Empty</c>.
	/// Instead use a <c>String.Length</c> test which should be a bit faster. Another
	/// possibility (with .NET 2.0) is to use the static <c>String.IsNullOrEmpty</c> method.
	/// <c>String.IsNullOrEmpty</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void SimpleMethod (string myString)
	/// {
	///	if (myString.Equals (String.Empty)) {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void SimpleMethod (string myString)
	/// {
	///	if (myString.Length == 0) {
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method compares a string with an empty string by using the Equals method or the equality (==) or inequality (!=) operators.")]
	[Solution ("Compare String.Length with 0 instead. The string length is known and it's faster to compare integers than to compare strings.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Performance", "CA1820:TestForEmptyStringsUsingStringLength")]
	public class CompareWithEmptyStringEfficientlyRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// is there any Call or Callvirt instructions in the method
			OpCodeBitmask bitmask = OpCodeEngine.GetBitmask (method);
			if (!OpCodeBitmask.Calls.Intersect (bitmask))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				Code code = ins.OpCode.Code;
				if ((code != Code.Call) && (code != Code.Callvirt))
					continue;

				MethodReference mref = (ins.Operand as MethodReference);

				// covers Equals(string) method and both == != operators
				switch (mref.Name) {
				case "Equals":
					if (mref.Parameters.Count > 1)
						continue;
					TypeReference type = mref.DeclaringType;
					if (type.Namespace != "System")
						continue;
					string name = type.Name;
					if ((name != "String") && (name != "Object"))
						continue;
					break;
				case "op_Equality":
				case "op_Inequality":
					if (!mref.DeclaringType.IsNamed ("System", "String"))
						continue;
					break;
				default:
					continue;
				}

				Instruction prev = ins.Previous;
				switch (prev.OpCode.Code) {
				case Code.Ldstr:
					if ((prev.Operand as string).Length > 0)
						continue;
					break;
				case Code.Ldsfld:
					FieldReference field = (prev.Operand as FieldReference);
					if (!field.DeclaringType.IsNamed ("System", "String"))
						continue;
					// unlikely to be anything else (at least with released fx)
					if (field.Name != "Empty")
						continue;
					break;
				default:
					continue;
				}

				Runner.Report (method, ins, Severity.Medium, Confidence.High);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
