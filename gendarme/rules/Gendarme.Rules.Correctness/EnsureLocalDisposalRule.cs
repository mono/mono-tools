//
// Gendarme.Rules.Correctness.EnsureLocalDisposalRule class
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;


namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule checks that disposable locals are always disposed of before the
	/// method returns.
	/// Use a 'using' statement (or a try/finally block) to guarantee local disposal
	/// even in the event an unhandled exception occurs.
	/// </summary>
	/// <example>
	/// Bad example (non-guaranteed disposal):
	/// <code>
	/// void DecodeFile (string file)
	/// {
	/// 	var stream = new StreamReader (file);
	/// 	DecodeHeader (stream);
	/// 	if (!DecodedHeader.HasContent) {
	/// 		return;
	/// 	}
	/// 	DecodeContent (stream);
	/// 	stream.Dispose ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (non-guaranteed disposal):
	/// <code>
	/// void DecodeFile (string file)
	/// {
	/// 	using (var stream = new StreamReader (file)) {
	/// 		DecodeHeader (stream);
	/// 		if (!DecodedHeader.HasContent) {
	/// 			return;
	/// 		}
	/// 		DecodeContent (stream);
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (not disposed of / not locally disposed of):
	/// <code>
	/// void DecodeFile (string file)
	/// {
	/// 	var stream = new StreamReader (file);
	/// 	Decode (stream);
	/// }
	/// 
	/// void Decode (Stream stream)
	/// {
	/// 	/*code to decode the stream*/
	/// 	stream.Dispose ();
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (not disposed of / not locally disposed of):
	/// <code>
	/// void DecodeFile (string file)
	/// {
	/// 	using (var stream = new StreamReader (file)) {
	/// 		Decode (stream);
	/// 	}
	/// }
	/// 
	/// void Decode (Stream stream)
	/// {
	/// 	/*code to decode the stream*/
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("This disposable local is not guaranteed to be disposed of before the method returns.")]
	[Solution ("Use a 'using' statement or surround the local's usage with a try/finally block.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class EnsureLocalDisposalRule : Rule, IMethodRule {

		OpCodeBitmask callsAndNewobjBitmask = BuildCallsAndNewobjOpCodeBitmask ();
		HashSet<Instruction> suspectLocals = new HashSet<Instruction> ();

		static bool IsDispose (MethodReference call)
		{
			if (!call.HasThis)
				return false;
			return MethodSignatures.Dispose.Matches (call) || MethodSignatures.DisposeExplicit.Matches (call);
		}

		static bool DoesReturnDisposable (MethodReference call)
		{
			//ignore properties (likely not the place where the IDisposable is *created*)
			MethodDefinition method = call.Resolve ();
			if ((method == null) || call.IsProperty ())
				return false;

			if (method.IsConstructor) {
				if (method.DeclaringType.IsGeneratedCode ())
					return false; //eg. generators
				return method.DeclaringType.Implements ("System.IDisposable");
			}

			return method.ReturnType.ReturnType.Implements ("System.IDisposable");
		}

		static bool AreBothInstructionsInSameTryFinallyBlock (MethodBody body, Instruction a, Instruction b)
		{
			foreach (ExceptionHandler eh in body.ExceptionHandlers) {
				if (eh.Type != ExceptionHandlerType.Finally)
					continue;
				if (eh.TryStart.Offset <= a.Next.Offset && eh.TryEnd.Offset >= a.Offset
					&& eh.HandlerStart.Offset <= b.Offset && eh.HandlerEnd.Offset >= b.Offset)
					return true;
			}
			return false;
		}

		static Instruction LocalTraceBack (MethodDefinition method, Instruction ins)
		{
			ins = ins.TraceBack (method);
			while (ins != null) {
				if (ins.IsLoadLocal () || ins.IsStoreLocal ())
					return ins;
				ins = ins.TraceBack (method);
			}
			return null;
		}

		Instruction FindRelatedSuspectLocal (MethodDefinition method, Instruction ins)
		{
			ins = LocalTraceBack (method, ins);
			if (null == ins)
				return null;

			int index = ins.GetVariable (method).Index;
			foreach (var local in suspectLocals) {
				if (local.GetVariable (method).Index == index)
					return local;
			}
			return null;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			//we ignore methods/constructors that returns IDisposable themselves
			//where local(s) are most likely used for disposable object construction
			if (DoesReturnDisposable (method))
				return RuleResult.DoesNotApply;

			//is there any potential IDisposable-getting opcode in the method?
			OpCodeBitmask methodBitmask = OpCodeEngine.GetBitmask (method);
			if (!callsAndNewobjBitmask.Intersect (methodBitmask))
				return RuleResult.DoesNotApply;

			//do we potentially store that IDisposable in a local?
			if (!OpCodeBitmask.StoreLocal.Intersect (methodBitmask))
				return RuleResult.DoesNotApply;

			suspectLocals.Clear ();

			foreach (Instruction ins in method.Body.Instructions) {
				if (!callsAndNewobjBitmask.Get (ins.OpCode.Code))
					continue;

				MethodReference call = (MethodReference) ins.Operand;

				if (IsDispose (call)) {
					Instruction local = FindRelatedSuspectLocal (method, ins);
					if (local != null) {
						if (!AreBothInstructionsInSameTryFinallyBlock (method.Body, local, ins)) {
							string msg = string.Format ("Local {0}is not guaranteed to be disposed of.", GetFriendlyNameOrEmpty (local.GetVariable (method)));
							Runner.Report (method, local, Severity.Medium, Confidence.Normal, msg);
						}
						suspectLocals.Remove (local);
					}
					continue;
				}

				if (ins.Next == null || !ins.Next.IsStoreLocal ())
					continue; //even if an IDisposable, it isn't stored in a local

				if (!DoesReturnDisposable (call))
					continue;

				suspectLocals.Add (ins.Next);
			}

			foreach (var local in suspectLocals) {
				string msg = string.Format ("Local {0}is not disposed of (at least not locally).", GetFriendlyNameOrEmpty (local.GetVariable (method)));
				Runner.Report (method, local, Severity.High, Confidence.Normal, msg);
			}

			return Runner.CurrentRuleResult;
		}

		static string GetFriendlyNameOrEmpty (VariableReference variable)
		{
			if (null == variable.Name || variable.Name.StartsWith ("V_"))
				return string.Format ("of type '{0}' ", variable.VariableType.Name);
			return string.Format ("'{0}' of type '{1}' ", variable.Name, variable.VariableType.Name);
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
