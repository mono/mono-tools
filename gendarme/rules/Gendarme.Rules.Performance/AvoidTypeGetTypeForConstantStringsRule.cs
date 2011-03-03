//
// Gendarme.Rules.Performance.AvoidTypeGetTypeForConstantStringsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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

	/// <summary>
	/// This rule warns when a method use <c>Type.GetType(string)</c> with a constant string.
	/// Such calls requires reflection in order to return a <c>Type</c> instance and, for 
	/// known types, can be replaced with a much faster <c>typeof(x)</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// booleanType = Type.GetType ("System.Boolean");
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// booleanType = typeof (bool);
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method calls Type.GetType(string) on a constant string. This call requires reflection in order return an instance of the type.")]
	[Solution ("For known values it is faster to use typeof(x), C# syntax, to obtain the same result without the reflection penality.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidTypeGetTypeForConstantStringsRule : Rule, IMethodRule {

		// the rule idea came from
		// http://lists.ximian.com/archives/public/mono-patches/2008-June/121564.html

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Call or Callvirt instructions in the method ?
			OpCodeBitmask bitmask = OpCodeEngine.GetBitmask (method);
			if (!OpCodeBitmask.Calls.Intersect (bitmask))
				return RuleResult.DoesNotApply;
			// and a call to Ldstr ?
			if (!bitmask.Get (Code.Ldstr))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.FlowControl != FlowControl.Call)
					continue;

				// look for calls to: static Type System.Type.GetType(string...)
				MethodReference mr = (ins.Operand as MethodReference);
				if ((mr == null) || !mr.HasParameters)
					continue;
				if (!mr.IsNamed ("System", "Type", "GetType"))
					continue;

				if (ins.Previous.OpCode.Code != Code.Ldstr)
					continue;

				string type_name = (ins.Previous.Operand as string);
				// one good reason to use this (besides non-visible types) is get a type from an unreferenced assembly
				if ((type_name == null) || type_name.Contains (", "))
					continue;
				// another good reason is the supported way to detect if running on mono runtime
				if (type_name == "Mono.Runtime")
					continue;

				string msg = String.Format (CultureInfo.InvariantCulture, 
					"Replace call to Type.GetType(\"{0}\") into typeof({0}).", type_name);
				Runner.Report (method, ins, Severity.Medium, Confidence.Normal, msg);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
