//
// DoNotDestroyStackTraceRule
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Rules.Exceptions.Impl;

namespace Gendarme.Rules.Exceptions {

	[Problem ("A catch block in the method throws back the caught exception which destroys the stack trace.")]
	[Solution ("If you need to throw the exception caught by the catch block, use 'throw;' instead of 'throw ex;'")]
	public class DoNotDestroyStackTrace : Rule, IMethodRule {

		private TypeReference void_reference;
		private List<ExecutionPathCollection> executionPaths = new List<ExecutionPathCollection> ();
		private List<int> warned_offsets_in_method = new List<int> ();

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				if (void_reference == null)
					void_reference = e.CurrentModule.Import (typeof (void));
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to methods with IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			executionPaths.Clear ();
			ExecutionPathFactory epf = new ExecutionPathFactory (method);
			foreach (SEHGuardedBlock guardedBlock in ExceptionBlockParser.GetExceptionBlocks (method)) {
				foreach (SEHHandlerBlock handlerBlock in guardedBlock.SEHHandlerBlocks) {
					if (handlerBlock is SEHCatchBlock) {
						executionPaths.AddRange (epf.CreatePaths (handlerBlock.Start, handlerBlock.End));
					}
				}
			}

			warned_offsets_in_method.Clear ();

			// Look for paths that 'throw ex;' instead of 'throw'
			foreach (ExecutionPathCollection catchPath in executionPaths)
				ProcessCatchPath (catchPath, method);

			return Runner.CurrentRuleResult;
		}

		private static bool IsStoreLoc (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Stloc:
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
			case Code.Stloc_S:
				return true;
			default:
				return false;
			}
		}

		private static bool IsLoadLoc (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Ldloc:
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
			case Code.Ldloc_S:
				return true;
			default:
				return false;
			}
		}

		private void ProcessCatchPath (ExecutionPathCollection catchPath, MethodDefinition method)
		{
			// Track original exception (top of stack at start) through to the final
			// return (be it throw, rethrow, leave, or leave.s)

			// Current stack position: 0 = top of stack
			int exStackPos = 0;
			// Local variable position: -1 = not stored in local variable
			int localVarPos = -1;

			foreach (ExecutionBlock block in catchPath) {
				Instruction cur = block.First;
				while (cur != block.Last) {

					if (cur.OpCode == OpCodes.Rethrow)
						// Rethrown exception - no problem!
						return;

					if (IsStoreLoc (cur)) {
						int varIndex = GetVarIndex (cur);
						if (exStackPos == 0) {
							// Storing argument on top of stack in local variable reference
							localVarPos = varIndex;
							exStackPos = -1;
						} else if (localVarPos != -1 && varIndex == localVarPos)
							// Writing over orignal exception...
							localVarPos = -1;
					} else if (localVarPos != -1 && IsLoadLoc (cur)) {
						int varIndex = GetVarIndex (cur);
						if (varIndex == localVarPos)
							// Loading exception from local var back onto stack
							exStackPos = 0;
					} else if (cur.OpCode == OpCodes.Throw && exStackPos == 0) {
						// If our original exception is on top of the stack,
						// we're rethrowing it.This is deemed naughty...
						if (!warned_offsets_in_method.Contains (cur.Offset)) {
							Runner.Report (method, cur, Severity.Critical, Confidence.High, String.Empty);
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
					
					cur = cur.Next;
				}
			}
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
				if (instr.Operand is MethodReference) {
					// We have to determine from the call how many arguments will
					// be popped from the stack
					MethodReference callMethod = (MethodReference) instr.Operand;
					return callMethod.Parameters.Count;
				} else {
					throw new InvalidOperationException ("Unexpected instruction: '" +
					instr.OpCode.ToString () + "' at offset 0x" +
					instr.Offset.ToString ("X"));
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
				MethodReference callMethod = (MethodReference) instr.Operand;
				return (callMethod.ReturnType.ReturnType == void_reference) ?
					0 : 1;
			}

			return 0;
		}

		private static int GetVarIndex (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Ldloc_0:
			case Code.Stloc_0:
				return 0;
			case Code.Ldloc_1:
			case Code.Stloc_1:
				return 1;
			case Code.Ldloc_2:
			case Code.Stloc_2:
				return 2;
			case Code.Ldloc_3:
			case Code.Stloc_3:
				return 3;
			case Code.Ldloc_S:
			case Code.Ldloc:
			case Code.Stloc_S:
			case Code.Stloc:
				VariableDefinition varDef = (ins.Operand as VariableDefinition);
				return varDef.Index;
			default:
				return -1;
			}
		}
	}
}
