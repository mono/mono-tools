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

namespace Gendarme.Rules.Portability {

	[Problem ("The method use some features that requires 'root' priviledge under Unix.")]
	[Solution ("Make sure your code can work without requiring users to have 'root' priviledge.")]
	public class FeatureRequiresRootPrivilegeOnUnixRule : Rule, IMethodRule {

		// localizable
		private const string ProcessMessage = "Setting Process.PriorityClass to something else than ProcessPriorityClass.Normal requires root privileges.";
		private const string PingMessage = "Usage of System.Net.NetworkInformation.Ping requires root privileges.";

		// non-localizable
		private const string Ping = "System.Net.NetworkInformation.Ping";

		//Check for usage of System.Diagnostics.Process.set_PriorityClass
		private static bool CheckProcessSetPriorityClass (Instruction ins)
		{
			if (ins.OpCode.FlowControl != FlowControl.Call)
				return false;

			MethodReference method = (ins.Operand as MethodReference);
			if ((method == null) || (method.Name != "set_PriorityClass"))
				return false;
			if (method.DeclaringType.FullName != "System.Diagnostics.Process")
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
			if (ins.OpCode.FlowControl != FlowControl.Call)
				return false;

			MethodReference method = (ins.Operand as MethodReference);
			return ((method != null) && (method.DeclaringType.FullName == Ping));
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply to methods without IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// Ping only exists in fx 2.0 and later
			bool fx20 = (method.DeclaringType.Module.Assembly.Runtime >= TargetRuntime.NET_2_0);

			foreach (Instruction ins in method.Body.Instructions) {

				// Check for usage of System.Diagnostics.Process.set_PriorityClass
				if (CheckProcessSetPriorityClass (ins)) {
					// code won't work with default (non-root) users == High
					Runner.Report (method, ins, Severity.High, Confidence.High, ProcessMessage);
				}

				// short-circuit
				if (fx20 && CheckPing (ins)) {
					// code won't work with default (non-root) users == High
					Runner.Report (method, ins, Severity.High, Confidence.High, PingMessage);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
