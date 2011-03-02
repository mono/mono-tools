//
// Gendarme.Rules.Design.Generic.AvoidDeclaringCustomDelegatesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design.Generic {

	/// <summary>
	/// This rule will fire if custom delegates are defined when either pre-defined <code>System.Action</code>, 
	/// <code>Action&lt;T[,...]&gt;</code> or <code>Func&lt;[Tx,...]TResult&gt;</code> could have been used.
	/// </summary>
	/// <example>
	/// Bad example (without return value):
	/// <code>
	/// delegate void MyCustomDelegate (int a);
	/// private MyCustomDelegate custom_delegate;
	/// </code>
	/// </example>
	/// <example>
	/// Good example (without return value):
	/// <code>
	/// private Action&lt;int&gt; action_delegate;
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (with return value):
	/// <code>
	/// delegate int MyCustomDelegate (int a, string s);
	/// private MyCustomDelegate custom_delegate;
	/// </code>
	/// </example>
	/// <example>
	/// Good example (with return value):
	/// <code>
	/// private Func&lt;int,string,int&gt; func_delegate;
	/// </code>
	/// </example>
	/// <remarks>This rule applies only to assemblies targeting .NET 2.0 and later.</remarks>
	[Problem ("This delegate could be replaced with an existing framework delegate.")]
	[Solution ("Prefer the use of Action, Action<T...> and Func<...,TResult> types.")]
	public class AvoidDeclaringCustomDelegatesRule : GenericsBaseRule, ITypeRule {

		static string[] ActionMessage = {
			"Replace with Action()",
			"Replace with Action<T>",
			"Replace with Action<T1,T2>",
			"Replace with Action<T1,T2,T3>",
			"Replace with Action<T1,T2,T3,T4>",
			// NET_4_0
			"Replace with Action<T1,T2,T3,T4,T5>",
			"Replace with Action<T1,T2,T3,T4,T5,T6>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7,T8>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7,T8,T9>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15>",
			"Replace with Action<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16>",
		};

		static string [] FuncMessage = {
			"Replace with Func<TResult>",
			"Replace with Func<T,TResult>",
			"Replace with Func<T1,T2,TResult>",
			"Replace with Func<T1,T2,T3,TResult>",
			"Replace with Func<T1,T2,T3,T4,TResult>",
			// NET_4_0
			"Replace with Func<T1,T2,T3,T4,T5,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,T8,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,TResult>",
			"Replace with Func<T1,T2,T3,T4,T5,T6,T7,T8,T9,T10,T11,T12,T13,T14,T15,T16,TResult>",
		};

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule only apply to (non generated) delegates
			if (!type.IsDelegate () || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			MethodDefinition invoke = type.GetMethod ("Invoke");
			// this happens for System.MulticastDelegate
			if (invoke == null)
				return RuleResult.DoesNotApply;

			int n = 0;
			Severity severity = Severity.Medium;
			bool use_structure = false;
			// check parameters for 'ref', 'out', 'params'
			if (invoke.HasParameters) {
				IList<ParameterDefinition> pdc = invoke.Parameters;
				n = pdc.Count;
				// too many parameters to directly use Action/Func
				// so we lower severity and suggest grouping them
				if (n > ((type.Module.Runtime >= TargetRuntime.Net_4_0) ? 16 : 4)) {
					severity = Severity.Low;
					n = 1;
					use_structure = true;
				}

				// special cases not directly usable with Action/Func
				foreach (ParameterDefinition pd in pdc) {
					if (pd.IsOut || pd.ParameterType.IsByReference || pd.IsParams ())
						return RuleResult.Success;
				}
			}

			string msg = invoke.ReturnType.IsNamed ("System", "Void") ? ActionMessage [n] : FuncMessage [n];
			if (use_structure)
				msg += " and use a structure to hold all your parameters into <T>.";
			Runner.Report (type, severity, Confidence.High, msg);
			return Runner.CurrentRuleResult;
		}
	}
}

