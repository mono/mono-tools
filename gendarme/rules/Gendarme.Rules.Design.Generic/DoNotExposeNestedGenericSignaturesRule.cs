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
	/// This rule will fire if an externally visible method has a parameter or return type
	/// whose type is a generic type which contains a generic type. For example, 
	/// <c>List&lt;List&lt;int&gt;&gt;</c>. Such types are hard to construct and should
	/// be avoided because simpler alternatives generally exist.
	/// Since some language, like C#, have direct support for nullable types, i.e. 
	/// <c>System.Nullable&lt;T&gt;</c> this specific case is ignored by the rule.
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
	/// <remarks>This rule applies only to assemblies targeting .NET 2.0 and later.</remarks>
	[Problem ("This method exposes a nested generic type in its signature.")]
	[Solution ("Remove the nested generics to keep the visible API simple to use.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
	public class DoNotExposeNestedGenericSignaturesRule : GenericsBaseRule, IMethodRule {

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
						if (git.ElementType.IsNamed ("System", "Nullable`1"))
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

			MethodReturnType return_type = method.MethodReturnType;
			Severity? severity = Check (return_type.ReturnType);
			if (severity.HasValue)
				Runner.Report (return_type, severity.Value, Confidence.Total);

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
