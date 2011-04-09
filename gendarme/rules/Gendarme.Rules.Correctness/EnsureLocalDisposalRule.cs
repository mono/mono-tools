//
// Gendarme.Rules.Correctness.EnsureLocalDisposalRule class
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Cedric Vivier
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

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
	[FxCopCompatibility("Microsoft.Reliability", "CA2000:DisposeObjectsBeforeLosingScope")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class EnsureLocalDisposalRule : Rule, IMethodRule {

		OpCodeBitmask callsAndNewobjBitmask = BuildCallsAndNewobjOpCodeBitmask ();
		Bitmask<ulong> locals = new Bitmask<ulong> ();

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
				return method.DeclaringType.Implements ("System", "IDisposable");
			}

			return method.ReturnType.Implements ("System", "IDisposable");
		}

		static bool IsSetter (MethodReference m)
		{
			if (m == null)
				return false;
			MethodDefinition md = m.Resolve ();
			if (md == null)
				return m.Name.StartsWith ("set_", StringComparison.Ordinal);
			return md.IsSetter;
		}

		static bool IsInsideFinallyBlock (MethodDefinition method, Instruction ins)
		{
			MethodBody body = method.Body;
			if (!body.HasExceptionHandlers)
				return false;

			foreach (ExceptionHandler eh in body.ExceptionHandlers) {
				if (eh.HandlerType != ExceptionHandlerType.Finally)
					continue;
				if (ins.Offset >= eh.HandlerStart.Offset || ins.Offset < eh.HandlerEnd.Offset)
					return true;
			}
			return false;
		}

		void Clear (MethodDefinition method, Instruction ins)
		{
			VariableDefinition v = ins.GetVariable (method);
			if (v != null)
				locals.Clear ((ulong) v.Index);
		}

		void CheckForReturn (MethodDefinition method, Instruction ins)
		{
			if (ins.IsLoadLocal ())
				Clear (method, ins);
		}

		void CheckForOutParameters (MethodDefinition method, Instruction ins)
		{
			Instruction iref = ins.TraceBack (method);
			if (iref == null)
				return;
			ParameterDefinition p = iref.GetParameter (method);
			if ((p != null) && p.IsOut) {
				ins = ins.Previous;
				if (ins.IsLoadLocal ())
					Clear (method, ins);
			}
		}

		void CheckDisposeCalls (MethodDefinition method, Instruction ins)
		{
			Instruction instance = ins.TraceBack (method);
			if (instance == null)
				return;

			VariableDefinition v = instance.GetVariable (method);
			ulong index = v == null ? UInt64.MaxValue : (ulong) v.Index;
			if (v != null && locals.Get (index)) {
				if (!IsInsideFinallyBlock (method, ins)) {
					string msg = String.Format (CultureInfo.InvariantCulture,
						"Local {0}is not guaranteed to be disposed of.",
						GetFriendlyNameOrEmpty (v));
					Runner.Report (method, Severity.Medium, Confidence.Normal, msg);
				}
				locals.Clear (index);
			}
		}

		bool CheckCallsToOtherInstances (MethodDefinition method, Instruction ins, MethodReference call)
		{
			Instruction p = ins.TraceBack (method, 0);
			if (p.Is (Code.Ldarg_0))
				return false;

			if (call.HasParameters) {
				for (int i = 1; i <= call.Parameters.Count; i++) {
					p = ins.TraceBack (method, -i);
					Clear (method, p);
				}
			}
			return true;
		}

		void CheckReassignment (MethodDefinition method, Instruction ins)
		{
			VariableDefinition v = ins.GetVariable (method);
			ulong index = (ulong) v.Index;
			if (locals.Get (index)) {
				string msg = String.Format (CultureInfo.InvariantCulture,
					"Local {0}is not disposed before being re-assigned.",
					GetFriendlyNameOrEmpty (v));
				Runner.Report (method, ins, Severity.High, Confidence.Normal, msg);
			} else {
				locals.Set (index);
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			//is there any potential IDisposable-getting opcode in the method?
			if (!callsAndNewobjBitmask.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			// we will not report IDiposable locals that are returned from a method
			bool return_idisposable = DoesReturnDisposable (method);

			locals.ClearAll ();

			foreach (Instruction ins in method.Body.Instructions) {
				Code code = ins.OpCode.Code;
				switch (code) {
				case Code.Ret:
					if (return_idisposable)
						CheckForReturn (method, ins.Previous);
					continue;
				case Code.Stind_Ref:
					CheckForOutParameters (method, ins);
					continue;
				default:
					if (!callsAndNewobjBitmask.Get (code))
						continue;
					break;
				}

				MethodReference call = (MethodReference) ins.Operand;

				if (IsDispose (call)) {
					CheckDisposeCalls (method, ins);
					continue;
				}

				if (call.HasThis && (code != Code.Newobj)) {
					if (!CheckCallsToOtherInstances (method, ins, call))
						continue;
				}

				if (!DoesReturnDisposable (call))
					continue;

				Instruction nextInstruction = ins.Next;
				if (nextInstruction == null)
					continue;

				Code nextCode = nextInstruction.OpCode.Code;
				if (nextCode == Code.Pop || OpCodeBitmask.Calls.Get (nextCode)) {
					// We ignore setter because it is an obvious share of the IDisposable
					if (!IsSetter (nextInstruction.Operand as MethodReference))
						ReportCall (method, ins, call);
				} else if (nextInstruction.IsStoreLocal ()) {
					// make sure we're not re-assigning over a non-disposed IDisposable
					CheckReassignment (method, nextInstruction);
				}
			}

			ReportNonDisposedLocals (method);

			return Runner.CurrentRuleResult;
		}

		void ReportNonDisposedLocals (MethodDefinition method)
		{
			for (ulong i = 0; i < 64; i++) {
				if (!locals.Get (i))
					continue;
				string msg = String.Format (CultureInfo.InvariantCulture,
					"Local {0}is not disposed of (at least not locally).",
					GetFriendlyNameOrEmpty (method.Body.Variables [(int) i]));
				Runner.Report (method, Severity.High, Confidence.Normal, msg);
			}
		}

		static bool IsFluentLike (MethodReference method)
		{
			TypeReference rtype = method.ReturnType;
			string nspace = rtype.Namespace;
			string name = rtype.Name;
			// StringBuilder StringBuilder.Append (...)
			if (method.DeclaringType.IsNamed (nspace, name))
				return true;
			return (method.HasParameters && method.Parameters [0].ParameterType.IsNamed (nspace, name));
		}

		void ReportCall (MethodDefinition method, Instruction ins, MethodReference call)
		{
			TypeReference type = ins.Is (Code.Newobj) ? call.DeclaringType : call.ReturnType;
			bool fluent = IsFluentLike (call);
			string msg = String.Format (CultureInfo.InvariantCulture, "Local of type '{0}' is not disposed of ({1}).",
				type.Name, fluent ? "is this a fluent-like API ?" : "at least not locally");
			Runner.Report (method, ins, Severity.High, fluent ? Confidence.Normal : Confidence.High, msg);
		}

		static string GetFriendlyNameOrEmpty (VariableReference variable)
		{
			string tname = variable.VariableType.Name;
			if (variable.IsGeneratedName ())
				return String.Format (CultureInfo.InvariantCulture, "of type '{0}' ", tname);
			return String.Format (CultureInfo.InvariantCulture, "'{0}' of type '{1}' ", variable.Name, tname);
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
