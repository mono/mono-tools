//
// Gendarme.Rules.Correctness.UseNoInliningWithGetCallingAssemblyRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule warns when a method call <c>Assembly.GetCallingAssembly()</c> from a 
	/// method that is not decorated with <c>[MethodImpl(MethodImplOptions.NoInlining)]</c>.
	/// Without this attribute the method could be inlined by the JIT. In this case the
	/// calling assembly would be the assembly of the caller (of the inlined method), 
	/// which could be different than the assembly of the real, source-wise, caller to
	/// <c>Assembly.GetCallingAssembly</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public void ShowInfo ()
	/// {
	///	Console.WriteLine (Assembly.GetCallingAssembly ().Location);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [MethodImpl (MethodImplOptions.NoInlining)]
	/// public void ShowInfo ()
	/// {
	///	Console.WriteLine (Assembly.GetCallingAssembly ().Location);
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.8</remarks>
	[Problem ("Assembly.GetCallingAssembly() is called from a method that could be inlined by the JIT")]
	[Solution ("Decorate method with [MethodImpl(MethodImplOptions.NoInlining)] to ensure it will never be inlined.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class UseNoInliningWithGetCallingAssemblyRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				// if the module does not reference System.Reflection.Assembly.GetCallingAssembly
				// then there's no point in enabling the rule
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyMemberReference ((MemberReference mr) => {
						return IsGetCallingAssembly (mr);
					})
				);
			};
		}

		static bool IsGetCallingAssembly (MemberReference method)
		{
			return method.IsNamed ("System.Reflection", "Assembly", "GetCallingAssembly");
		}

		static bool IsCallToGetCallingAssembly (Instruction instruction)
		{
			var code = instruction.OpCode.Code;
			if (code != Code.Call && code != Code.Callvirt)
				return false;

			return IsGetCallingAssembly (instruction.GetMethod ());
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			// we do not need to check if the method can't be inlined
			if (method.NoInlining)
				return RuleResult.Success;

			foreach (Instruction current in method.Body.Instructions) {
				if (IsCallToGetCallingAssembly (current)) {
					Runner.Report (method, current, Severity.High, Confidence.Total);
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}

