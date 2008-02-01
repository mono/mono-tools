using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Rules.Exceptions.Impl;

namespace Gendarme.Rules.Exceptions {

	public class DontDestroyStackTrace : IMethodRule {

		private TypeReference void_reference;
		private ArrayList warned_offsets_in_method;

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			if (!method.HasBody)
				return runner.RuleSuccess;

			ModuleDefinition module = method.DeclaringType.Module;
			if (void_reference == null)
				void_reference = module.Import (typeof (void));

			ArrayList executionPaths = new ArrayList ();
			ExecutionPathFactory epf = new ExecutionPathFactory (method);
			ISEHGuardedBlock[] guardedBlocks = ExceptionBlockParser.GetExceptionBlocks (method);
			foreach (ISEHGuardedBlock guardedBlock in guardedBlocks) {
				foreach (ISEHHandlerBlock handlerBlock in
					 guardedBlock.SEHHandlerBlocks) {
					if (handlerBlock is SEHCatchBlock) {
					    ExecutionPath[] ret =
						epf.CreatePaths (handlerBlock.Start,
								 handlerBlock.End);
						executionPaths.AddRange (ret);
					}
				}
			}

			MessageCollection violations = new MessageCollection ();
			warned_offsets_in_method = new ArrayList ();

			// Look for paths that 'throw ex;' instead of 'throw'
			foreach (ExecutionPath catchPath in executionPaths)
				ProcessCatchPath (catchPath, method, violations);

			return (violations.Count == 0) ? null : violations;
		}

		private void ProcessCatchPath (ExecutionPath catchPath,
					       MethodDefinition method,
					       MessageCollection violations)
		{
			// Track original exception (top of stack at start) through to the final
			// return (be it throw, rethrow, leave, or leave.s)

			// Current stack position: 0 = top of stack
			int exStackPos = 0;
			// Local variable position: -1 = not stored in local variable
			int localVarPos = -1;

			foreach (ExecutionBlock block in catchPath) {
				Instruction cur = null;
				while (cur != block.Last) {
					if (cur == null)
						cur = block.First;
					else
						cur = cur.Next;

					if (cur.OpCode == OpCodes.Rethrow)
						// Rethrown exception - no problem!
						return;

					if (cur.OpCode == OpCodes.Stloc ||
					   cur.OpCode == OpCodes.Stloc_0 ||
					   cur.OpCode == OpCodes.Stloc_1 ||
					   cur.OpCode == OpCodes.Stloc_2 ||
					   cur.OpCode == OpCodes.Stloc_3 ||
					   cur.OpCode == OpCodes.Stloc_S) {

						int varIndex = GetVarIndex (cur);
						if (exStackPos == 0)
						{
							// Storing argument on top of stack in local variable reference
							localVarPos = varIndex;
							exStackPos = -1;
						} else if (localVarPos != -1 && varIndex == localVarPos)
							// Writing over orignal exception...
							localVarPos = -1;
					} else if (localVarPos != -1 &&
						   (cur.OpCode == OpCodes.Ldloc ||
						    cur.OpCode == OpCodes.Ldloc_0 ||
						    cur.OpCode == OpCodes.Ldloc_1 ||
						    cur.OpCode == OpCodes.Ldloc_2 ||
						    cur.OpCode == OpCodes.Ldloc_3 ||
						    cur.OpCode == OpCodes.Ldloc_S)) {

						int varIndex = GetVarIndex (cur);
						if (varIndex == localVarPos)
							// Loading exception from local var back onto stack
							exStackPos = 0;
					} else if (cur.OpCode == OpCodes.Throw && exStackPos == 0) {
						// If our original exception is on top of the stack,
						// we're rethrowing it.This is deemed naughty...
						if (!warned_offsets_in_method.Contains(cur.Offset)) {
							Location loc =
								new Location (method, cur.Offset);
							Message msg = new Message (
								"Throwing original exception - destroys stack trace!",
								loc,
								MessageType.Error);
							violations.Add (msg);
							warned_offsets_in_method.Add (cur.Offset);
						}
						return;
					} else if (exStackPos != -1) {
						// If we're still on the stack, track our position after
						// this instruction
						int numPops = GetNumPops (cur);
						if (exStackPos < numPops) {
							// Popped ex off of stack
							exStackPos = -1;
						} else {
							int numPushes = GetNumPushes (cur);
							exStackPos += numPushes - numPops;
						}
					}
				}
			}
			return;
		}

		private static int GetNumPops (Instruction instr)
		{
			switch (instr.OpCode.StackBehaviourPop) {
			case StackBehaviour.Pop0:
				return 0;
			case StackBehaviour.Pop1:
			case StackBehaviour.Popi:
			case StackBehaviour.Popref:
				return 1;
			case StackBehaviour.Pop1_pop1:
			case StackBehaviour.Popi_pop1:
			case StackBehaviour.Popi_popi:
			case StackBehaviour.Popi_popi8:
			case StackBehaviour.Popi_popr4:
			case StackBehaviour.Popi_popr8:
			case StackBehaviour.Popref_pop1:
			case StackBehaviour.Popref_popi:
				return 2;
			case StackBehaviour.Popi_popi_popi:
			case StackBehaviour.Popref_popi_popi:
			case StackBehaviour.Popref_popi_popi8:
			case StackBehaviour.Popref_popi_popr4:
			case StackBehaviour.Popref_popi_popr8:
			case StackBehaviour.Popref_popi_popref:
				return 3;
			case StackBehaviour.Varpop:
				if(instr.Operand is MethodReference) {
					// We have to determine from the call how many arguments will
					// be popped from the stack
					MethodReference callMethod = (MethodReference)instr.Operand;
					return callMethod.Parameters.Count;
				} else {
					throw new InvalidOperationException("Unexpected instruction: '" +
					instr.OpCode.ToString() + "' at offset 0x" +
					instr.Offset.ToString("X"));
				}
			}

			return 0;
		}

		private int GetNumPushes (Instruction instr)
		{
			switch (instr.OpCode.StackBehaviourPush) {
			case StackBehaviour.Push0:
				return 0;
			case StackBehaviour.Push1:
			case StackBehaviour.Pushi:
			case StackBehaviour.Pushi8:
			case StackBehaviour.Pushr4:
			case StackBehaviour.Pushr8:
			case StackBehaviour.Pushref:
				return 1;
			case StackBehaviour.Push1_push1:
				return 2;
			case StackBehaviour.Varpush:
				// We have to determine from the call how many arguments will
				// be pushed onto the stack
				MethodReference callMethod = (MethodReference)instr.Operand;
				return (callMethod.ReturnType.ReturnType == void_reference) ?
					0 : 1;
			}

			return 0;
		}

		private static int GetVarIndex (Instruction instr)
		{
			if (instr.OpCode == OpCodes.Stloc_0 || instr.OpCode == OpCodes.Ldloc_0)
				return 0;
			else if (instr.OpCode == OpCodes.Stloc_1 || instr.OpCode == OpCodes.Ldloc_1)
				return 1;
			else if (instr.OpCode == OpCodes.Stloc_2 || instr.OpCode == OpCodes.Ldloc_2)
				return 2;
			else if (instr.OpCode == OpCodes.Stloc_3 || instr.OpCode == OpCodes.Ldloc_3)
				return 3;
			else if (instr.OpCode == OpCodes.Stloc_S || instr.OpCode == OpCodes.Stloc ||
				instr.OpCode == OpCodes.Ldloc_S || instr.OpCode == OpCodes.Ldloc)
			{
				VariableDefinition varDef = (VariableDefinition)instr.Operand;
				return varDef.Index;
			}

			return -1;
		}
	}
}
