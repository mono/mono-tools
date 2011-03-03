/*
 * Extracted from CFG.cs
 *
 * Authors:
 *   Aaron Tomb <atomb@soe.ucsc.edu>
 *
 * Copyright (c) 2005 Aaron Tomb and the contributors listed
 * in the ChangeLog.
 *
 * This is free software, distributed under the MIT/X11 license.
 * See the included LICENSE.MIT file for details.
 **********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Framework.Helpers {

	public sealed class MethodPrinter { 

		private IList<Instruction> instructions;
		private MethodDefinition method;
		private IDictionary branchTable;

		public MethodPrinter (MethodDefinition m)
		{
			if (m == null)
				throw new ArgumentNullException ("m");
			
			method = m;
			if (method.HasBody)
				instructions = method.Body.Instructions;
			
			if (instructions != null)
				InitBranchTable ();
		}

		public override string ToString () 
		{
			Instruction prevInstr = null;
			
			StringBuilder buffer = new StringBuilder ();

			if (method != null)
				buffer.AppendLine (method.ToString ());
				
			if (instructions != null) {
				foreach (Instruction instr in instructions) {
					if (StartsTryRegion (instr) != null)
						buffer.AppendLine ("Try {");
					if (StartsHandlerRegion (instr) != null)
						buffer.AppendLine ("Handle {");
		
					if (IsLeader (instr, prevInstr))
						buffer.Append ("* ");
					else
						buffer.Append ("  ");

					buffer.Append ("  ");
					buffer.Append (instr.Offset.ToString ("X4", CultureInfo.InvariantCulture));
					buffer.Append (": ");
					buffer.Append (instr.OpCode.Name);

					int[] targets = BranchTargets (instr);
					if (targets != null) {
						foreach (int target in targets) {
							buffer.Append (' ');
							buffer.Append (target.ToString ("X4", CultureInfo.InvariantCulture));
						}
					} else if (instr.Operand is string) {
						buffer.Append (" \"");
						buffer.Append (instr.Operand);
						buffer.Append ('"');
					} else if (instr.Operand != null) {
						buffer.Append (" ");
						buffer.Append (instr.Operand);
					}
					buffer.AppendLine ();

					prevInstr = instr;
					if (EndsTryRegion (instr) != null)
						buffer.AppendLine ("} (Try)");
					if (EndsHandlerRegion (instr) != null)
						buffer.AppendLine ("} (Handle)");
				}
			}
			return buffer.ToString ().Trim ();
		}

		#region Helpers (used by CFG)
		public static int[] BranchTargets (Instruction instruction)
		{
			if (instruction == null)
				throw new ArgumentNullException ("instruction");

			int[] result = null;
			switch (instruction.OpCode.OperandType) {
			case OperandType.InlineSwitch:
				Instruction[] targets = (Instruction[])instruction.Operand;
				result = new int[targets.Length];
				int i = 0;
				foreach (Instruction target in targets) {
					result [i] = target.Offset;
					i++;
				}
				break;
			case OperandType.InlineBrTarget:
				result = new int[1];
				result [0] = ((Instruction)instruction.Operand).Offset;
				break;
			case OperandType.ShortInlineBrTarget:
				result = new int[1];
				result [0] = ((Instruction)instruction.Operand).Offset;
				break;
			}
			return result;
		}

		public bool IsLeader (Instruction instruction, Instruction previous)
		{
			if (instruction == null)
				throw new ArgumentNullException ("instruction");

			/* First instruction in the method */
			if (previous == null)
				return true;

			/* Target of a branch */
			if (branchTable.Contains (instruction.Offset))
				return true;

			/* Follows a control flow instruction */
			if (IsBranch (instruction.Previous))
				return true;

			/* Is the beginning of a try region */
			if (StartsTryRegion (instruction) != null)
				return true;

			/* Is the beginning of a handler region */
			if (StartsHandlerRegion (instruction) != null)
				return true;

			return false;
		}

		public ExceptionHandler StartsHandlerRegion (Instruction instruction)
		{
			foreach (ExceptionHandler handler in method.Body.ExceptionHandlers) {
				if (OffsetsEqual (instruction, handler.HandlerStart))
					return handler;
			}
			return null;
		}

		public ExceptionHandler EndsTryRegion (Instruction instruction)
		{
			foreach (ExceptionHandler handler in method.Body.ExceptionHandlers) {
				if (instruction != null)
					if (OffsetsEqual (instruction.Next, handler.TryEnd))
						return handler;
			}
			return null;
		}

		public ExceptionHandler EndsHandlerRegion (Instruction instruction)
		{
			foreach (ExceptionHandler handler in method.Body.ExceptionHandlers) {
				if (instruction != null)
					if (OffsetsEqual (instruction.Next, handler.HandlerEnd))
						return handler;
			}
			return null;
		}
		#endregion

		#region Private Methods
		private static bool IsBranch (Instruction instruction)
		{
			if (instruction == null)
				return false;

			switch (instruction.OpCode.FlowControl) {
			case FlowControl.Branch:
			case FlowControl.Cond_Branch:
			case FlowControl.Return:
			/* Throw creates a new basic block, but it has no target,
			 * because the object to be thrown is taken from the stack.
			 * Thus, its type is not known before runtime, and we can't
			 * know which catch block will recieve it. */
			case FlowControl.Throw:
				return true;
			}
			return false;
		}

		private static bool OffsetsEqual (Instruction insn1, Instruction insn2)
		{
			if (insn1 == insn2)
				return true;
			if (insn1 == null)
				return false;
			if (insn2 == null)
				return false;
			return (insn1.Offset == insn2.Offset);
		}

		private ExceptionHandler StartsTryRegion (Instruction instruction)
		{
			foreach (ExceptionHandler handler in method.Body.ExceptionHandlers) {
				if (OffsetsEqual (instruction, handler.TryStart))
					return handler;
			}
			return null;
		}

		private void InitBranchTable ()
		{
			branchTable = new Hashtable ();
			foreach (Instruction instr in instructions) {
				int[] targets = BranchTargets (instr);
				if (targets != null) {
					foreach (int target in targets) {
						if (!branchTable.Contains (target)) {
							IList sources = new ArrayList ();
							sources.Add (target);
							branchTable.Add (target, sources);
						} else {
							IList sources =  (IList)branchTable [target];
							sources.Add (target);
						}
					}
				}
			}
		}
		#endregion
	}
}

