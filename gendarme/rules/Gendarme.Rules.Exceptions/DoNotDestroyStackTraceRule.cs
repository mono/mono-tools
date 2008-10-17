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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Exceptions.Impl;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// This rule check method's catch block to see if they are throwing back the caught 
	/// exception. Doing so would destroy the stack trace of the original exception. If you
	/// need to (re-)throw the exception caught by the catch block, you should use <c>throw;</c>
	/// instead of <c>throw ex;</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// try {
	///	Int32.Parse ("Broken!");
	/// }
	/// catch (Exception ex) {
	///	Assert.IsNotNull (ex);
	///	throw ex;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// try {
	///	Int32.Parse ("Broken!");
	/// }
	/// catch (Exception ex) {
	///	Assert.IsNotNull (ex);
	///	throw;
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.0 this rule was named  DontDestroyStackTraceRule.</remarks>

	[Problem ("A catch block in the method throws back the caught exception which destroys the stack trace.")]
	[Solution ("If you need to throw the exception caught by the catch block, use 'throw;' instead of 'throw ex;'")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails")]
	public class DoNotDestroyStackTraceRule : Rule, IMethodRule {

		private List<ExecutionPathCollection> executionPaths = new List<ExecutionPathCollection> ();
		private List<int> warned_offsets_in_method = new List<int> ();

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to methods with IL
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// and when the IL contains a Throw instruction (Rethrow is fine)
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Throw))
				return RuleResult.DoesNotApply;

			executionPaths.Clear ();
			ExecutionPathFactory epf = new ExecutionPathFactory ();
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

		private void ProcessCatchPath (ExecutionPathCollection catchPath, MethodDefinition method)
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

					if (cur.IsStoreLocal ()) {
						int varIndex = cur.GetVariable (method).Index;
						if (exStackPos == 0) {
							// Storing argument on top of stack in local variable reference
							localVarPos = varIndex;
							exStackPos = -1;
						} else if (localVarPos != -1 && varIndex == localVarPos)
							// Writing over orignal exception...
							localVarPos = -1;
					} else if (localVarPos != -1 && cur.IsLoadLocal ()) {
						int varIndex = cur.GetVariable (method).Index;
						if (varIndex == localVarPos)
							// Loading exception from local var back onto stack
							exStackPos = 0;
					} else if (cur.OpCode == OpCodes.Throw && exStackPos == 0) {
						// If our original exception is on top of the stack,
						// we're rethrowing it.This is deemed naughty...
						if (!warned_offsets_in_method.Contains (cur.Offset)) {
							Runner.Report (method, cur, Severity.Critical, Confidence.High);
							warned_offsets_in_method.Add (cur.Offset);
						}
						return;
					} else if (exStackPos != -1) {
						// If we're still on the stack, track our position after
						// this instruction
						int numPops = cur.GetPopCount (method);
						if (exStackPos < numPops) {
							// Popped ex off of stack
							exStackPos = -1;
						} else {
							int numPushes = cur.GetPushCount ();
							exStackPos += numPushes - numPops;
						}
					}
				}
			}
		}
	}
}
