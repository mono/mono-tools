//
// Gendarme.Rules.Correctness.ProvideCorrectRegexPatternRule class
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2008 Cedric Vivier
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
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

using System.Text.RegularExpressions;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule checks if methods/constructors requiring a regular expression
	/// string are provided a valid pattern.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// Regex re = new Regex ("^\\"); //Invalid end of pattern
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
	/// Regex re = new Regex ("([a-z)*"); //Unterminated [] set
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
	/// return Regex.IsMatch (code, @"(\w)-\2"); //Reference to undefined group number 2
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

		MethodDefinition method;

		const string RegexClass = "System.Text.RegularExpressions.Regex";
		const string ValidatorClass = "System.Configuration.RegexStringValidator";

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				bool usingRegexClass = e.CurrentAssembly.Name.Name == "System"
				                       || e.CurrentModule.TypeReferences.ContainsType (RegexClass);
				bool usingValidatorClass = e.CurrentAssembly.Runtime >= TargetRuntime.NET_2_0
				                           && (e.CurrentAssembly.Name.Name == "System.Configuration"
				                              || e.CurrentModule.TypeReferences.ContainsType (ValidatorClass));
				Active = usingRegexClass | usingValidatorClass;
			};
		}

		void CheckPattern (Instruction ins, int argumentOffset)
		{
			Instruction ld = ins.TraceBack (method, argumentOffset);
			if (null == ld)
				return;

			switch (ld.OpCode.Code) {
			case Code.Ldstr:
				CheckPattern (ins, (string) ld.Operand);
				break;
			case Code.Ldsfld:
				FieldReference f = (FieldReference) ld.Operand;
				if (f.Name != "Empty" || f.DeclaringType.FullName != "System.String")
					return;
				CheckPattern (ins, null);
				break;
			case Code.Ldnull:
				CheckPattern (ins, null);
				break;
			}
		}

		void CheckPattern (Instruction ins, string pattern)
		{
			if (string.IsNullOrEmpty (pattern)) {
				Runner.Report (method, ins, Severity.High, Confidence.High, "Pattern is null or empty.");
				return;
			}

			try {
				new Regex (pattern);
			} catch (Exception e) {
				/* potential set of exceptions is not well documented and potentially changes with regarts to
				   different runtime and/or runtime version. */
				string msg = string.Format ("Pattern '{0}' is invalid. Reason: {1}", pattern, e.Message);
				Runner.Report (method, ins, Severity.High, Confidence.High, msg);
			}
		}

		void CheckCall (Instruction ins, MethodReference call)
		{
			if (null == call) //resolution did not work
				return;
			if (!call.HasParameters)
				return;
			if (call.DeclaringType.FullName != RegexClass && call.DeclaringType.FullName != ValidatorClass)
				return;

			MethodDefinition mdef = call.Resolve ();
			if (null == mdef)
				return;
			//check only constructors and static non-property methods
			if (!mdef.IsConstructor && (mdef.HasThis || mdef.IsProperty ()))
				return;

			foreach (ParameterDefinition p in mdef.Parameters) {
				if ((p.Name == "pattern" || p.Name == "regex") && p.ParameterType.FullName == "System.String") {
					CheckPattern (ins, -(call.HasThis ? 0 : -1 + p.Sequence));
					return;
				}
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			this.method = method;

			//is there any interesting opcode in the method?
			if (!callsAndNewobjBitmask.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (!callsAndNewobjBitmask.Get (ins.OpCode.Code))
					continue;

				CheckCall (ins, (MethodReference) ins.Operand);
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
