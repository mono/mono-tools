//
// Gendarme.Rules.Design.Generic.DoNotExposeNestedGenericSignaturesRule
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design.Generic {

	/// <summary>
	/// This rule checks for visible method parameters or return values that are exposing
	/// nested generic types. Such types are harder to construct and should not be imposed
	/// on the consumer of the API since simpler alternative generally exists.
	/// Since some language, like C#, have direct support for nullable types, i.e. 
	/// <c>Nullable&lt;T&gt;</c> this specific case is ignored by the rule.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Generic&lt;T&gt; {
	///	public void Process (KeyValuePair&lt;T, ICollection&lt;int&gt;&gt; value)
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Generic&lt;T&gt; {
	///	public void Process (KeyValuePair&lt;T, int[]&gt; value)
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("This method expose a nested generic parameter or return value in its signature.")]
	[Solution ("Remove the nested generics to keep the visible API simple to use.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
	public class DoNotExposeNestedGenericSignaturesRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// we only want to run this on assemblies that use 2.0 or later
			// since generics were not available before
			Runner.AnalyzeAssembly += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Runtime >= TargetRuntime.NET_2_0);
			};
		}

		static Severity? Check (TypeReference type)
		{
			GenericInstanceType git = (type as GenericInstanceType);
			if ((git != null) && git.HasGenericArguments) {
				foreach (TypeReference tr in git.GenericArguments) {
					git = (tr as GenericInstanceType);
					if (git != null) {
						// nullable are an exception because there is syntaxic sugar (at 
						// least in some language like C#) to make them easier to use
						// note: FxCop does not ignore them
						if (git.ElementType.FullName == "System.Nullable`1")
							return null;
						// FIXME: we should look at ignoring LINQ queries too, because it
						// too pretty much hides the complexity of nested generics
						return Severity.High;
					}
				}
			}
			return null;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// note: using anonymous methods creates a lot of defects but they are all non-visible
			if (!method.IsVisible ())
				return RuleResult.DoesNotApply;

			Severity? severity = Check (method.ReturnType.ReturnType);
			if (severity.HasValue)
				Runner.Report (method.ReturnType, severity.Value, Confidence.Total);

			if (method.HasParameters) {
				foreach (ParameterDefinition parameter in method.Parameters) {
					severity = Check (parameter.ParameterType);
					if (severity.HasValue)
						Runner.Report (parameter, severity.Value, Confidence.Total);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
