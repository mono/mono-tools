//
// Gendarme.Rules.Correctness.ProvideCorrectRegexPatternRule class
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Cedric Vivier
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

using System.Text.RegularExpressions;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule verifies that valid regular expression strings are used as arguments.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// //Invalid end of pattern
	/// Regex re = new Regex ("^\\");
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// Regex re = new Regex (@"^\\");
	/// </code>
	/// </example>
	/// <example>
	/// Bad example:
	/// <code>
	/// //Unterminated [] set
	/// Regex re = new Regex ("([a-z)*");
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// Regex re = new Regex ("([a-z])*");
	/// </code>
	/// </example>
	/// <example>
	/// Bad example:
	/// <code>
	/// //Reference to undefined group number 2
	/// return Regex.IsMatch (code, @"(\w)-\2");
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// return Regex.IsMatch (code, @"(\w)-\1");
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("An invalid regular expression string is provided to a method/constructor.")]
	[Solution ("Fix the invalid regular expression.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ProvideCorrectRegexPatternRule : Rule, IMethodRule {

		static OpCodeBitmask callsAndNewobjBitmask = BuildCallsAndNewobjOpCodeBitmask ();

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				string assembly_name = e.CurrentAssembly.Name.Name;
				bool usingRegexClass = (assembly_name == "System");
				bool usingValidatorClass = (e.CurrentModule.Runtime >= TargetRuntime.Net_2_0) && (assembly_name == "System.Configuration");
				// if we're not analyzing System.dll or System.Configuration.dll then check if we're using them
				if (!usingRegexClass && !usingValidatorClass) {
					Active = e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return tr.IsNamed ("System.Text.RegularExpressions", "Regex") ||
							tr.IsNamed ("System.Configuration", "RegexStringValidator");
					});
				} else {
					Active = true;
				}
			};
		}

		void CheckArguments (MethodDefinition method, Instruction ins, Instruction ld)
		{
			// handle things like: boolean_condition ? "string-1" : "string-2"
			// where the first string (compiler dependent) is ok, while the second is bad
			if (CheckLoadInstruction (method, ins, ld, Confidence.High)) {
				Instruction previous = ld.Previous;
				if ((previous != null) && (previous.Operand == ins)) {
					CheckLoadInstruction (method, ins, previous.Previous, Confidence.Normal);
				}
			}
		}

		bool CheckLoadInstruction (MethodDefinition method, Instruction ins, Instruction ld, Confidence confidence)
		{
			switch (ld.OpCode.Code) {
			case Code.Ldstr:
				return CheckPattern (method, ins, (string) ld.Operand, confidence);
			case Code.Ldsfld:
				FieldReference f = (FieldReference) ld.Operand;
				if (f.Name != "Empty" || !f.DeclaringType.IsNamed ("System", "String"))
					return false;
				return CheckPattern (method, ins, null, confidence);
			case Code.Ldnull:
				return CheckPattern (method, ins, null, confidence);
			}
			return true;
		}

		bool CheckPattern (MethodDefinition method, Instruction ins, string pattern, Confidence confidence)
		{
			if (string.IsNullOrEmpty (pattern)) {
				Runner.Report (method, ins, Severity.High, Confidence.High, "Pattern is null or empty.");
				return false;
			}

			try {
				new Regex (pattern);
				return true;
			} catch (Exception e) {
				/* potential set of exceptions is not well documented and potentially changes with regarts to
				   different runtime and/or runtime version. */
				string msg = String.Format (CultureInfo.InvariantCulture, 
					"Pattern '{0}' is invalid. Reason: {1}", pattern, e.Message);
				Runner.Report (method, ins, Severity.High, confidence, msg);
				return false;
			}
		}

		void CheckCall (MethodDefinition method, Instruction ins, MethodReference call)
		{
			if (null == call) //resolution did not work
				return;
			if (!call.HasParameters)
				return;

			TypeReference type = call.DeclaringType;
			if (!type.IsNamed ("System.Text.RegularExpressions", "Regex") && !type.IsNamed ("System.Configuration", "RegexStringValidator"))
				return;

			MethodDefinition mdef = call.Resolve ();
			if (null == mdef)
				return;
			//check only constructors and static non-property methods
			if (!mdef.IsConstructor && (mdef.HasThis || mdef.IsProperty ()))
				return;

			foreach (ParameterDefinition p in mdef.Parameters) {
				string pname = p.Name;
				if ((pname == "pattern" || pname == "regex") && p.ParameterType.IsNamed ("System", "String")) {
					Instruction ld = ins.TraceBack (method, -(call.HasThis ? 0 : p.Index));
					if (ld != null)
						CheckArguments (method, ins, ld);
					return;
				}
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			//is there any interesting opcode in the method?
			if (!callsAndNewobjBitmask.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (!callsAndNewobjBitmask.Get (ins.OpCode.Code))
					continue;

				CheckCall (method, ins, (MethodReference) ins.Operand);
			}

			return Runner.CurrentRuleResult;
		}

		static OpCodeBitmask BuildCallsAndNewobjOpCodeBitmask ()
		{
			#if true
				return new OpCodeBitmask (0x8000000000, 0x4400000000000, 0x0, 0x0);
			#else
				OpCodeBitmask mask = new OpCodeBitmask ();
				mask.UnionWith (OpCodeBitmask.Calls);
				mask.Set (Code.Newobj);
				return mask;
			#endif
		}
	}
}
