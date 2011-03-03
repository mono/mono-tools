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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	// rule idea credits to FindBug - http://findbugs.sourceforge.net/
	// Dm: Method invokes toString() method on a String (DM_STRING_TOSTRING)
	// and was later extended to more cases, including one that covers
	// DMI: Invocation of substring(0), which returns the original value (DMI_USELESS_SUBSTRING)

	/// <summary>
	/// This rule detects when some methods, like <c>Clone()</c>, <c>Substring(0)</c>, 
	/// <c>ToString()</c> or <c>ToString(IFormatProvider)</c>, are being called on a 
	/// string instance. Since these calls all return the original string they don't do anything
	/// useful and should be carefully reviewed to see if they are working as intended and,
	/// if they are, the method call can be removed.
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

					if (text.Length > 0)
						Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, text);
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}

		private static string CheckClone (MethodReference call, Instruction ins, MethodDefinition method)
		{
			if (call.HasParameters)
				return String.Empty;

			if (!ins.Previous.GetOperandType (method).IsNamed ("System", "String"))
				return  String.Empty;

			return String.Format (CultureInfo.InvariantCulture, MessageString, call.Name, String.Empty);
		}

		private static string CheckSubstring (MethodReference call, Instruction ins)
		{
			if (!call.DeclaringType.IsNamed ("System", "String"))
				return String.Empty;

			// ensure it's System.String::Substring(System.Int32) and that it's given 0 as a parameter
			if (call.HasParameters && (call.Parameters.Count != 1))
				return String.Empty;
			if (!ins.Previous.IsOperandZero ())
				return String.Empty;

			return String.Format (CultureInfo.InvariantCulture, MessageString, call.Name, "0");
		}

		private static string CheckToString (MethodReference call, Instruction ins, MethodDefinition method)
		{
			if (call.DeclaringType.IsNamed ("System", "String")) {
				// most probably ToString(IFormatProvider), possibly ToString()
				return String.Format (CultureInfo.InvariantCulture, MessageString, call.Name, 
					(call.HasParameters && (call.Parameters.Count > 1)) ? "IFormatProvider" : String.Empty);
			} else {
				// signature for Clone is identical (well close enough) to share code
				return CheckClone (call, ins, method);
			}
		}
	}
}
