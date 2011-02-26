//
// Gendarme.Rules.Portability.FeatureRequiresRootPrivilegeOnUnixRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2007 Andreas Noever
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
using System.Diagnostics;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Portability {

	/// <summary>
	/// This rule fires if a feature is used which is, by default, restricted under Unix.
	/// <list type="bullet">
	/// <item>
	/// <description><c>System.Net.NetworkInformation.Ping</c>: This type can only be used
	/// by root on Unix systems. As an alternative you can execute the ping command and 
	/// parse its result.</description>
	/// </item>
	/// <item>
	/// <description><c>System.Diagnostics.Process</c>: The PriorityClass property can only
	/// be set to <c>Normal</c> by non-root users. To avoid this problem you can do a 
	/// platform check before assigning a priority.</description>
	/// </item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// process.PriorityClass = ProcessPriorityClass.AboveNormal;
	/// process.Start ();
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// if (Environment.OSVersion.Platform != PlatformID.Unix) {
	///	process.PriorityClass = ProcessPriorityClass.AboveNormal;
	/// }
	/// process.Start ();
	/// </code>
	/// </example>

	[Problem ("The method uses some features which require 'root' privilege under Unix.")]
	[Solution ("Make sure your code can work without requiring users to have 'root' privilege.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class FeatureRequiresRootPrivilegeOnUnixRule : Rule, IMethodRule {

		// includes Call, Callvirt and Newobj
		private static OpCodeBitmask CallsNew = new OpCodeBitmask (0x8000000000, 0x4400000000000, 0x0, 0x0);

		// localizable
		private const string ProcessMessage = "Setting Process.PriorityClass to something else than ProcessPriorityClass.Normal requires root privileges.";
		private const string PingMessage = "Usage of System.Net.NetworkInformation.Ping requires root privileges.";

		private bool ping_present = true;
		private bool process_present = true;

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// if the module does not reference either Ping or Process
			// then it's not being used inside it
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
					ping_present = tr.IsNamed ("System.Net.NetworkInformation", "Ping");
					process_present = tr.IsNamed ("System.Diagnostics", "Process");
					// return true to stop looping when both Ping and Process are found
					return (ping_present && process_present);
				});
				// activate the rule if any (or both) is/are present(s)
				Active = (ping_present || process_present);
			};
			// note: this ignores on purpose System.dll since there's
			// no point in reporting the use of both class inside it
		}

		//Check for usage of System.Diagnostics.Process.set_PriorityClass
		private static bool CheckProcessSetPriorityClass (Instruction ins)
		{
			MethodReference method = (ins.Operand as MethodReference);
			if ((method == null) || (method.Name != "set_PriorityClass"))
				return false;
			if (!method.DeclaringType.IsNamed ("System.Diagnostics", "Process"))
				return false;

			Instruction prev = ins.Previous; //check stack
			if (prev == null)
				return false;

			switch (prev.OpCode.Code) {
			case Code.Ldc_I4_S:
				return ((ProcessPriorityClass) (sbyte) prev.Operand != ProcessPriorityClass.Normal);
			case Code.Ldc_I4:
				return ((ProcessPriorityClass) prev.Operand != ProcessPriorityClass.Normal);
			default:
				return false;
			}
		}

		private static bool CheckPing (Instruction ins)
		{
			MethodReference method = (ins.Operand as MethodReference);
			return ((method != null) && (method.DeclaringType.IsNamed ("System.Net.NetworkInformation", "Ping")));
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply to methods without IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!CallsNew.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {

				// check for calls (or newobj)
				if (ins.OpCode.FlowControl != FlowControl.Call)
					continue;

				// Check for usage of Process or Ping based on their presence
				if (process_present && CheckProcessSetPriorityClass (ins)) {
					// code won't work with default (non-root) users == High
					Runner.Report (method, ins, Severity.High, Confidence.High, ProcessMessage);
				} else 	if (ping_present && CheckPing (ins)) {
					// code won't work with default (non-root) users == High
					Runner.Report (method, ins, Severity.High, Confidence.High, PingMessage);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
