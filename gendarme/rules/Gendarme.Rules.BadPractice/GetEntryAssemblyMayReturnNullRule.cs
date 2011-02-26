// 
// Gendarme.Rules.BadPractice.GetEntryAssemblyMayReturnNullRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
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
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule warns when an assembly without an entry point (i.e. a dll or library) calls 
	/// <c>Assembly.GetEntryAssembly ()</c>. This call is problematic since it will always 
	/// return <c>null</c> when called from outside the root (main) application domain. This may 
	/// become a problem inside libraries that can be used, for example, inside ASP.NET
	/// applications.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // this will throw a NullReferenceException from an ASP.NET page
	/// Response.WriteLine (Assembly.GetEntryAssembly ().CodeBase);
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class MainClass {
	///	static void Main ()
	///	{
	///		Console.WriteLine (Assembly.GetEntryAssembly ().CodeBase);
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This method calls Assembly.GetEntryAssembly which may return null if not called from the root application domain.")]
	[Solution ("Avoid depending on Assembly.GetEntryAssembly inside reusable code.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class GetEntryAssemblyMayReturnNullRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active &= 
					// GetEntryAssembly will work inside executables
					e.CurrentAssembly.EntryPoint == null &&
					
					// if the module does not reference System.Reflection.Assembly.GetEntryAssembly
					// then there's no point in enabling the rule
					(e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyMemberReference ((MemberReference mr) => {
						return IsGetEntryAssembly (mr);
					}));
			};
		}

		static bool IsGetEntryAssembly (MemberReference method)
		{
			return method.IsNamed ("System.Reflection", "Assembly", "GetEntryAssembly");
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't not apply to methods without code (e.g. p/invokes)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// not for executables
			if (method.DeclaringType.Module.Assembly.EntryPoint != null)
				return RuleResult.DoesNotApply;

			// avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			// go!

			foreach (Instruction current in method.Body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					if (IsGetEntryAssembly (current.Operand as MethodReference))
						Runner.Report (method, current, Severity.Medium, Confidence.Total);
					break;
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
