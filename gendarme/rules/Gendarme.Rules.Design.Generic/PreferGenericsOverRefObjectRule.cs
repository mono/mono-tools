//
// Gendarme.Rules.Design.Generic.PreferGenericsOverRefObjectRule
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
	/// This rule fires if a method has a reference argument (<c>ref</c> or
	/// <c>out</c> in C#) to System.Object. These methods can generally be
	/// rewritten in .NET 2.0 using generics which provides type safety, eliminates
	/// casts, and makes the API easier to consume.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // common before 2.0 but we can do better now
	/// public bool TryGetValue (string key, ref object value)
	/// {
	///	// ...
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public bool TryGetValue&lt;T&gt; (string key, ref T value)
	/// {
	///	// ...
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This method contains a reference parameter to System.Object which is often an indication that the code is not type safe.")]
	[Solution ("Change the parameter to use a generic type where the caller will provide the type.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate")]
	public class PreferGenericsOverRefObjectRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// we only want to run this on assemblies that use 2.0 or later
			// since generics were not available before
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentModule.Runtime >= TargetRuntime.Net_2_0);
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply to properties, events, without parameters or for generated code
			if (method.IsSpecialName || !method.HasParameters || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// exclude the "bool Try* (ref)" pattern from the rule
			if (method.Name.StartsWith ("Try", StringComparison.Ordinal) && (method.ReturnType.FullName == "System.Boolean"))
				return RuleResult.DoesNotApply;

			foreach (ParameterDefinition parameter in method.Parameters) {
				if (parameter.ParameterType.FullName != "System.Object&")
					continue;

				// suggest using generics
				Runner.Report (parameter, Severity.Medium, Confidence.High);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
