// 
// Gendarme.Rules.Maintainability.PreferStringIsNullOrEmptyRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule checks methods for cases where <c>String.IsNullOrEmpty</c> could be
	/// used instead of doing separate null and length checks. This does not affect
	/// execution nor performance (much) but it does improve source code readability.
	/// This rule only applies to assemblies compiled with .NET 2.0 (or later).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool SendMessage (string message)
	/// {
	///	if ((message == null) || (message.Length == 0)) {
	///		return false;
	///	}
	///	return SendMessage (Encode (message));
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public bool SendMessage (string message)
	/// {
	///	if (String.IsNullOrEmpty (message)) {
	///		return false;
	///	}
	///	return SendMessage (Encode (message));
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method does both string null and length checks which can be harder on code readability/maintainability.")]
	[Solution ("Replace both checks with a single call to String.IsNullOrEmpty(string).")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class PreferStringIsNullOrEmptyRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// we only want to run this on assemblies that use 2.0 or later
			// since String.IsNullOrEmpty did not exist before that
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentModule.Runtime >= TargetRuntime.Net_2_0);
			};
		}

		private static string GetName (MethodDefinition method, Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				int index = ins.OpCode.Code - Code.Ldarg_0;
				if (!method.IsStatic) {
					index--;
					if (index < 0)
						return "this";
				}
				return method.Parameters [index].Name;
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
				return (ins.Operand as ParameterDefinition).Name;
			case Code.Ldfld:
			case Code.Ldsfld:
				return (ins.Operand as FieldReference).Name;
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
				int vindex = ins.OpCode.Code - Code.Ldloc_0;
				return method.Body.Variables [vindex].Name;
			case Code.Ldloc:
			case Code.Ldloc_S:
				return (ins.Operand as VariableDefinition).Name;
			default:
				object o = ins.Operand;
				MemberReference mr = (o as MemberReference);
				if (mr != null)
					return mr.GetFullName ();
				else if (o != null)
					return o.ToString ();
				else
					return String.Empty;
			}
		}

		// if a valid check is found then return the instruction where it branch
		private static Instruction PreLengthCheck (MethodDefinition method, Instruction ins)
		{
			string name_length = GetName (method, ins);
			while (ins.Previous != null) {
				ins = ins.Previous;
				switch (ins.OpCode.Code) {
				case Code.Brfalse:
				case Code.Brfalse_S:
					if (name_length == GetName (method, ins.Previous))
						return (ins.Operand as Instruction);
					return null;
				}
			}
			return null;
		}

		private static bool PostLengthCheck (Instruction ins, Instruction branch)
		{
			switch (ins.OpCode.Code) {
			// e.g. if 
			case Code.Brtrue:
			case Code.Brtrue_S:
				return ((ins.Operand as Instruction).OpCode.Code != branch.OpCode.Code);
			// e.g. return
			default:
				if (!IsOperandZero (ins))
					return false;
				break;
			}

			ins = ins.Next;
			switch (ins.OpCode.Code) {
			case Code.Ceq:
				return true;
			case Code.Cgt:
				return IsOperandZero (ins.Next);
			default:
				return false;
			}
		}

		private static bool IsOperandZero (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Ldc_I4:
				return ((int) ins.Operand == 0);
			case Code.Ldc_I4_S:
				return ((int) (sbyte) ins.Operand == 0);
			case Code.Ldc_I4_0:
				return true;
			default:
				return false;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't not apply to methods without code (e.g. p/invokes)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Call or Callvirt instructions in the method
			OpCodeBitmask calls = OpCodeBitmask.Calls;
			if (!calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			// go!

			// we look for a call to String.Length property (since it's much easier 
			// than checking a string being compared to null)
			foreach (Instruction current in method.Body.Instructions) {
				if (!calls.Get (current.OpCode.Code))
					continue;

				MethodReference mr = (current.Operand as MethodReference);
				if (mr.IsNamed ("System", "String", "get_Length")) {
					// now that we found it we check that
					// 1 - we previously did a check null on the same value (that we already know is a string)
					Instruction branch = PreLengthCheck (method, current.Previous);
					if (branch == null)
						continue;
					// 2 - we compare the return value (length) with 0
					if (PostLengthCheck (current.Next, branch)) {
						Runner.Report (method, current, Severity.Medium, Confidence.High);
					}
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
