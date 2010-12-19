//
// Gendarme.Rules.Design.Generic.DoNotDeclareStaticMembersOnGenericTypesRule
//
// Authors:
//	Nicholas Rioux
// 
// Copyright (C) 2010 Nicholas Rioux
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
using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design.Generic {

	/// <summary>
	/// This rule checks for generic types that contain static members.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class BadClass&lt;T&gt; {
	///	public static string member () {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class GoodClass&lt;T&gt; {
	///	public string member () {
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("An externally visible generic type has a static member.")]
	[Solution ("Remove the static member or change it to an instance member.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
	public class DoNotDeclareStaticMembersOnGenericTypesRule : Rule, ITypeRule {
		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsClass || !type.HasGenericParameters || !type.IsVisible ())
				return RuleResult.DoesNotApply;

			if (type.HasFields) {
				foreach (var field in type.Fields) {
					if (field.IsStatic && field.IsVisible ())
						Runner.Report (field, Severity.Medium, Confidence.Total);
				}
			}
			if (type.HasMethods) {
				foreach (var method in type.Methods) {
					if (method.IsStatic && method.IsVisible ())
						Runner.Report (method, Severity.Medium, Confidence.Total);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
