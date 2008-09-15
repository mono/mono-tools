//
// Gendarme.Rules.Design.Generic.UseGenericEventHandlerRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design.Generic {

	/// <summary>
	/// This rule checks for delegate definitions that are not required when using the
	/// 2.0 (or later) .NET framework as it can be replaced by using  the generic-based
	/// <c>System.EventHandler&lt;TEventArgs&gt;</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public delegate void AuthenticityHandler (object sender, AuthenticityEventArgs e);
	/// 
	/// public event AuthenticityHandler CheckingAuthenticity;
	/// public event AuthenticityHandler CheckedAuthenticity;
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public event EventHandler&lt;AuthenticityEventArgs&gt; CheckingAuthenticity;
	/// public event EventHandler&lt;AuthenticityEventArgs&gt; CheckedAuthenticity;
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This delegate definition is not needed with FX 2.0 and later runtimes.")]
	[Solution ("Replace this delegate with a generic based EventHandler<TEventArgs>.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1003:UseGenericEventHandlerInstances")]
	public class UseGenericEventHandlerRule : Rule, ITypeRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// we only want to run this on assemblies that use 2.0 or later
			// since generics were not available before
			Runner.AnalyzeAssembly += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Runtime >= TargetRuntime.NET_2_0);
			};
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule only apply to (non generated) delegates
			if (!type.IsDelegate () || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			MethodDefinition invoke = type.GetMethod ("Invoke");
			// this happens for System.MulticastDelegate
			if (invoke == null)
				return RuleResult.DoesNotApply;

			if (invoke.ReturnType.ReturnType.FullName != "System.Void")
				return RuleResult.Success;
			if (invoke.Parameters.Count != 2)
				return RuleResult.Success;
			if (invoke.Parameters [0].ParameterType.FullName != "System.Object")
				return RuleResult.Success;
			if (!invoke.Parameters [1].ParameterType.Inherits ("System.EventArgs"))
				return RuleResult.Success;

			Runner.Report (type, Severity.Medium, Confidence.High);
			return RuleResult.Failure;
		}
	}
}
