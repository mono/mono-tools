//
// Gendarme.Rules.Performance.AvoidConcatenatingCharsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
	/// This rule will warn you if boxing is used to concatenate a string
	/// since this will slow down your code and can be easily avoided. This
	/// often happen when concatenating <c>System.String</c> and <c>System.Char</c>
	/// values together. However the rule is not limited to check boxing on 
	/// <c>System.Char</c> since compilers often transform a character into its 
	/// integer value (e.g. 'a' == 61) and the same boxing issue exists on integers.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public string PrintIndex (string a, char b)
	/// {
	///	// This is compiled into String.Concat(params object[])
	///	// and requires boxing the 4 characters
	///	Console.WriteLine ('[' + a + ',' + b + ']');
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public string PrintIndex (string a, char b)
	/// {
	///	// This is compiled into String.Concat(params string[])
	///	// and no boxing is required
	///	Console.WriteLine ("[" + a + "," + b.ToString () + "]");
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.8</remarks>
	[Problem ("Unneeded boxing was found for concatening a string.")]
	[Solution ("Change your code to avoid the boxing when creating your string.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidConcatenatingCharsRule : Rule, IMethodRule {

		static bool HasReferenceToStringConcatObject (ModuleDefinition module)
		{
			foreach (MemberReference mr in module.GetMemberReferences ()) {
				if (mr.IsNamed ("System", "String", "Concat")) {
					MethodReference method = (mr as MethodReference);
					// catch both System.Object and System.Object[]
					if (!method.HasParameters)
						continue;

					TypeReference ptype = method.Parameters [0].ParameterType;
					switch (ptype.Name) {
					case "Object":
					case "Object[]":
						return (ptype.Namespace == "System");
					}
				}
			}
			return false;
		}

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// turn off the rule if String.Concat(object...) is not used in the module
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = e.CurrentAssembly.Name.Name == "mscorlib" ||
					HasReferenceToStringConcatObject (e.CurrentModule);
			};
		}

		private void ReportBoxing (MethodDefinition method, Instruction ins, Confidence confidence)
		{
			string msg = String.Format (CultureInfo.InvariantCulture,
				"Type '{0}' is being boxed.", (ins.Operand as TypeReference).GetFullName ());
			Runner.Report (method, ins, Severity.High, confidence, msg);
		}

		// look for boxing in the compiler-constructed array
		private void ScanArray (MethodDefinition method, Instruction ins)
		{
			while (ins != null) {
				switch (ins.OpCode.Code) {
				case Code.Newarr:
					return;
				case Code.Box:
					ReportBoxing (method, ins, Confidence.Normal);
					break;
				}
				ins = ins.Previous;
			}
		}

		private void CheckParameters (IMethodSignature concat, MethodDefinition caller, Instruction ins)
		{
			// check for boxed (likely char, but could be other types too) on any parameter
			for (int i = 0; i < concat.Parameters.Count; i++) {
				Instruction source = ins.TraceBack (caller, -i);
				if ((source == null) || (source.OpCode.Code != Code.Box))
					continue;
				ReportBoxing (caller, source, Confidence.High);
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Call or Callvirt instructions in the method ?
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if ((ins.OpCode.Code != Code.Call) && (ins.OpCode.Code != Code.Callvirt))
					continue;

				// look for String.Concat overloads using System.Object
				MethodReference mr = (ins.Operand as MethodReference);
				if (!mr.HasParameters || !mr.IsNamed ("System", "String", "Concat"))
					continue;

				TypeReference ptype = mr.Parameters [0].ParameterType;
				if (ptype.Namespace != "System")
					continue; // very unlikely

				switch (ptype.Name) {
				case "Object":
					CheckParameters (mr, method, ins);
					break;
				case "Object[]":
					if ((ins.Previous.OpCode.Code == Code.Stelem_Ref) || ins.Previous.IsLoadLocal ())
						ScanArray (method, ins.Previous);
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}

