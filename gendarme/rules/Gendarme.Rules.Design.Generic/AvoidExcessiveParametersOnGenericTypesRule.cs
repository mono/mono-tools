// 
// Gendarme.Rules.Design.Generic.AvoidExcessiveParametersOnGenericTypesRule
//
// Authors:
//	Nicholas Rioux
//
// Copyright (C) 2010 Nicholas Rioux
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

namespace Gendarme.Rules.Design.Generic {

	/// <summary>
	/// A visible type should not have more than two generic parameters. This makes it
	/// hard for consumers to remember what each parameter is required for.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class BadClass&lt;A, B, C&gt; {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class GoodClass&lt;A, B&gt; {
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule applies only to assemblies targeting .NET 2.0 and later.</remarks>
	[Problem ("A visible type has more than two generic parameters.")]
	[Solution ("Redesign the type so it doesn't take more than two generic parameters.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public class AvoidExcessiveParametersOnGenericTypesRule : GenericsBaseRule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsClass || !type.HasGenericParameters || !type.IsVisible ())
				return RuleResult.DoesNotApply;

			if (type.GenericParameters.Count > 2)
				Runner.Report (type, Severity.Medium, Confidence.Total);

			return Runner.CurrentRuleResult;
		}
	}
}
