//
// Gendarme.Rules.Concurrency.DoubleCheckLockingRule.cs: 
//	looks for instances of double-check locking.
//
// Authors:
//	Aaron Tomb <atomb@soe.ucsc.edu>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) 2005 Aaron Tomb
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
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Concurrency {

	// note: the rule only report a single double-lock per method

	[Problem ("This method uses the unreliable double-check locking technique.")]
	[Solution ("Remove the lock check that occurs outside of the protected region.")]
	public class DoubleCheckLockingRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't apply if the method has no IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			Hashtable comparisons = new Hashtable ();
			InstructionCollection insns = method.Body.Instructions;

			ArrayList monitorOffsetList = new ArrayList(10);
			for(int i = 0; i < insns.Count; i++) {
				int mcount = monitorOffsetList.Count;
				Instruction[] twoBefore = TwoBeforeBranch(insns[i]);
				if(twoBefore != null) {
					if(monitorOffsetList.Count > 0) {
						/* If there's a comparison in the list matching this
						* one, we have double-check locking. */
						foreach(Instruction insn in comparisons.Keys) {
							Instruction[] twoBeforeI =
								(Instruction[])comparisons[insn];
							if(!EffectivelyEqual(insn, insns[i]))
								continue;
							if(!EffectivelyEqual(twoBeforeI[0], twoBefore[0]))
								continue;
							if(!EffectivelyEqual(twoBeforeI[1], twoBefore[1]))
								continue;
							if(mcount <= 0)
								continue;
							if(insn.Offset >= (int)monitorOffsetList[mcount - 1])
								continue;

							Runner.Report (method, insn, Severity.Medium, Confidence.High, String.Empty);
							return RuleResult.Failure;
						}
					}
					comparisons[insns[i]] = twoBefore;
				}
				if(IsMonitorMethod(insns[i], "Enter"))
					monitorOffsetList.Add(insns[i].Offset);
				if(IsMonitorMethod(insns[i], "Exit"))
					if(mcount > 0)
						monitorOffsetList.RemoveAt(monitorOffsetList.Count - 1);
			}
			return RuleResult.Success;
		}
		
		private static bool IsMonitorMethod(Instruction insn, string methodName)
		{
			if(!insn.OpCode.Name.Equals("call"))
				return false;
			MethodReference method = (MethodReference)insn.Operand;
			if(!method.Name.Equals(methodName))
				return false;
			if(!method.DeclaringType.FullName.Equals("System.Threading.Monitor"))
				return false;
			return true;
		}

		private static Instruction[] TwoBeforeBranch(Instruction insn)
		{
			if(insn.OpCode.FlowControl != FlowControl.Cond_Branch)
				return null;
			if(insn.Previous == null || insn.Previous.Previous == null)
				return null;
			Instruction[] twoInsns = new Instruction[2];
			twoInsns[0] = insn.Previous;
			twoInsns[1] = insn.Previous.Previous;
			return twoInsns;
		}

		private static bool EffectivelyEqual(Instruction insn1, Instruction insn2)
		{
			if(!insn1.OpCode.Equals(insn2.OpCode))
				return false;
			/* If both are branch instructions, we don't care about their
			* targets, only their opcodes. */
			if(insn1.OpCode.FlowControl == FlowControl.Cond_Branch)
				return true;
			/* For other instructions, their operands must also be equal. */
			if(insn1.Operand == null && insn2.Operand == null)
				return true;
			if(insn1.Operand != null && insn2.Operand != null)
				if(insn1.Operand.Equals(insn2.Operand))
					return true;
				return false;
		}
	}
}
