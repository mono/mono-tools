/*
* DoubleCheckLockingRule.cs: looks for instances of double-check locking.
*
* Authors:
*   Aaron Tomb <atomb@soe.ucsc.edu>
*
* Copyright (c) 2005 Aaron Tomb and the contributors listed
* in the ChangeLog.
*
* This is free software, distributed under the MIT/X11 license.
* See the included MIT.X11 file for details.
**********************************************************************/

using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Concurrency {

	public class DoubleCheckLockingRule : IMethodRule {

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			if (!method.HasBody)
				return runner.RuleSuccess;

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

							Location location = new Location (method, insn.Offset);
							Message message = new Message ("A double check locking detected", 
								location, MessageType.Warning); 
							return new MessageCollection (message);
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
			return runner.RuleSuccess;
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
