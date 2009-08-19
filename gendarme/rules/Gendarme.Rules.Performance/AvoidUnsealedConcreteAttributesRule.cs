// 
// Gendarme.Rules.Performance.AvoidUnsealedConcreteAttributesRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2007 Daniel Abramov
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

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule fires if an attribute is defined which is both concrete (i.e. not abstract)
	/// and unsealed. This is a performance problem because it means that 
	/// <c>System.Attribute.GetCustomAttribute</c> has to search the attribute type
	/// hierarchy for derived types. To fix this either seal the type or make it abstract. 
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [AttributeUsage (AttributeTargets.All)]
	/// public class BadAttribute : Attribute {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (sealed):
	/// <code>
	/// [AttributeUsage (AttributeTargets.All)]
	/// public sealed class SealedAttribute : Attribute {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (abstract and sealed):
	/// <code>
	/// [AttributeUsage (AttributeTargets.All)]
	/// public abstract class AbstractAttribute : Attribute {
	/// }
	/// 
	/// [AttributeUsage (AttributeTargets.All)]
	/// public sealed class ConcreteAttribute : AbstractAttribute {
	/// }
	/// </code>
	/// </example>
	/// <remarks>Before Gendarme 2.0 this rule was named AvoidUnsealedAttributesRule.</remarks>

	[Problem ("Because of performance issues, concrete attributes should be sealed.")]
	[Solution ("Unless you plan to inherit from this attribute you should consider sealing it.")]
	[FxCopCompatibility ("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
	public class AvoidUnsealedConcreteAttributesRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to attributes
			if (!type.IsAttribute ())
				return RuleResult.DoesNotApply;

			if (type.IsAbstract) // it's ok
				return RuleResult.Success;

			if (type.IsSealed) // it's ok
				return RuleResult.Success;

			Runner.Report (type, Severity.Medium, Confidence.High);
			return RuleResult.Failure;
		}
	}
}
