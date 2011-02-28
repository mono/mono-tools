//
// DoNotDestroyStackTraceRule
//
// Copyright (C) 2008, 2011 Novell, Inc (http://www.novell.com)
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

using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Exceptions.Impl;

namespace Gendarme.Rules.Exceptions {

	/// <summary>
	/// This rule will fire if a catch handler throws the exception it caught. What it should
	/// do instead is rethrow the original exception (e.g. use <c>throw</c> instead of
	/// <c>throw ex</c>). This is helpful because rethrow preserves the stacktrace of the
	/// original exception.
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

	[Problem ("A catch block throws the exception it caught which destroys the original stack trace.")]
	[Solution ("Use 'throw;' instead of 'throw ex;'")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails")]
	public class DoNotDestroyStackTraceRule : Rule, IMethodRule {

		// all branches instructions except leave[_s]
		static OpCodeBitmask branches = new OpCodeBitmask (0xFFFFFC0000000000, 0xF, 0x0, 0x0);

		private List<int> warned_offsets_in_method = new List<int> ();

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to methods with IL and exceptions handlers
			if (!method.HasBody || !method.Body.HasExceptionHandlers)
				return RuleResult.DoesNotApply;

			// and when the IL contains a Throw instruction (Rethrow is fine)
			OpCodeBitmask mask = OpCodeEngine.GetBitmask (method);
			if (!mask.Get (Code.Throw))
				return RuleResult.DoesNotApply;

			// we can use a faster code path when no branches are present in the method
			if (mask.Intersect (branches)) {
				Branches (method);
			} else {
				Branchless (method);
			}

			warned_offsets_in_method.Clear ();

			return Runner.CurrentRuleResult;
		}

		private void Branchless (MethodDefinition method)
		{
			// Current stack position: 0 = top of stack
			int exStackPos = 0;
			// Local variable position: -1 = not stored in local variable
			int localVarPos = -1;

			foreach (ExceptionHandler eh in method.Body.ExceptionHandlers) {
				if (eh.HandlerType != ExceptionHandlerType.Catch)
					continue;

				ProcessCatchBlock (eh.HandlerStart, eh.HandlerEnd, method, ref exStackPos, ref localVarPos);
			}
		}

		private void ProcessCatchBlock (Instruction first, Instruction last, MethodDefinition method, ref int exStackPos, ref int localVarPos)
		{
			Instruction cur = null;
			
			while (cur != last) {
				if (cur == null)
					cur = first;
				else
					cur = cur.Next;

				// Rethrown exception - no problem!
				if (cur.Is (Code.Rethrow))
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
				} else if (cur.Is (Code.Throw) && exStackPos == 0) {
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

		private void Branches (MethodDefinition method)
		{
			ExecutionPathFactory epf = new ExecutionPathFactory ();

			foreach (ExceptionHandler eh in method.Body.ExceptionHandlers) {
				if (eh.HandlerType != ExceptionHandlerType.Catch)
					continue;

				var list = epf.CreatePaths (eh.HandlerStart, eh.HandlerEnd);
				if (list.Count == 0) {
					Runner.Report (method, eh.HandlerStart, Severity.Medium, Confidence.Normal, "Handler too complex for analysis");
				} else {
					foreach (ExecutionPathCollection catchPath in list)
						ProcessCatchPath (catchPath, method);
				}
			}
		}

		private void ProcessCatchPath (IEnumerable<ExecutionBlock> catchPath, MethodDefinition method)
		{
			// Track original exception (top of stack at start) through to the final
			// return (be it throw, rethrow, leave, or leave.s)

			// Current stack position: 0 = top of stack
			int exStackPos = 0;
			// Local variable position: -1 = not stored in local variable
			int localVarPos = -1;

			foreach (ExecutionBlock block in catchPath) {
				ProcessCatchBlock (block.First, block.Last, method, ref exStackPos, ref localVarPos);
			}
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask branches = new OpCodeBitmask ();
			branches.Set (Code.Beq);
			branches.Set (Code.Beq_S);
			branches.Set (Code.Bge);
			branches.Set (Code.Bge_S);
			branches.Set (Code.Bge_Un);
			branches.Set (Code.Bge_Un_S);
			branches.Set (Code.Bgt);
			branches.Set (Code.Bgt_S);
			branches.Set (Code.Bgt_Un);
			branches.Set (Code.Bgt_Un_S);
			branches.Set (Code.Ble);
			branches.Set (Code.Ble_S);
			branches.Set (Code.Ble_Un);
			branches.Set (Code.Ble_Un_S);
			branches.Set (Code.Blt);
			branches.Set (Code.Blt_S);
			branches.Set (Code.Blt_Un);
			branches.Set (Code.Blt_Un_S);
			branches.Set (Code.Bne_Un);
			branches.Set (Code.Bne_Un_S);
			branches.Set (Code.Br);
			branches.Set (Code.Br_S);
			branches.Set (Code.Brfalse);
			branches.Set (Code.Brfalse_S);
			branches.Set (Code.Brtrue);
			branches.Set (Code.Brtrue_S);
			Console.WriteLine (branches);
		}
#endif
	}
}
