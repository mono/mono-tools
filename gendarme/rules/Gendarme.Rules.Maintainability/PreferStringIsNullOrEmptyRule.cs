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

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule checks methods for cases where <c>String.IsNullOrEmpty</c> could be
	/// used instead of doing separate null and length checks. This does not affect
	/// execution nor (much) performance but it improves source code readability.
	/// This rule only applies to assemblies compiled with the .NET framework version 
	/// 2.0 (or later).
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

	[Problem ("This method does string null and length check which can be harder on code readability/maintainability.")]
	[Solution ("Replace both checks with a single call to String.IsNullOrEmpty(string).")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class PreferStringIsNullOrEmptyRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// we only want to run this on assemblies that use 2.0 or later
			// since String.IsNullOrEmpty did not exist before that
			Runner.AnalyzeAssembly += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Runtime >= TargetRuntime.NET_2_0);
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
				return (ins.Operand == null) ? String.Empty : ins.Operand.ToString ();
			}
		}

		private static bool PreLengthCheck (MethodDefinition method, Instruction ins)
		{
			if (ins == null)
				return false;

			string name_length = GetName (method, ins);
			while (ins.Previous != null) {
				ins = ins.Previous;
				switch (ins.OpCode.Code) {
				case Code.Brfalse:
				case Code.Brfalse_S:
					string name_null = GetName (method, ins.Previous);
					return name_null == name_length;
				}
			}
			return false;
		}

		private static bool PostLengthCheck (Instruction ins)
		{
			if ((ins == null) || (ins.Next == null))
				return false;

			switch (ins.OpCode.Code) {
			// e.g. if 
			case Code.Brtrue:
			case Code.Brtrue_S:
				return true;
			// e.g. return
			case Code.Ldc_I4:
				if ((int) ins.Operand != 0)
					return false;
				break;
			case Code.Ldc_I4_0:
				break;
			default:
				return false;
			}
			return (ins.Next.OpCode.Code == Code.Ceq);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't not apply to methods without code (e.g. p/invokes)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Call or Callvirt instructions in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			// go!

			// we look for a call to String.Length property (since it's much easier 
			// than checking a string being compared to null)
			foreach (Instruction current in method.Body.Instructions) {
				if (!OpCodeBitmask.Calls.Get (current.OpCode.Code))
					continue;

				MethodReference mr = (current.Operand as MethodReference);
				if ((mr.Name == "get_Length") && (mr.DeclaringType.FullName == "System.String")) {
					// now that we found it we check that
					// 1 - we previously did a check null on the same value (that we already know is a string)
					// 2 - we compare the return value (length) with 0
					if (PreLengthCheck (method, current.Previous) && PostLengthCheck (current.Next)) {
						Runner.Report (method, current, Severity.Medium, Confidence.High);
					}
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
