// 
// Gendarme.Rules.Design.MissingAttributeUsageOnCustomAttributeRule
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

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule verifies that every custom attribute (i.e. types that inherit from 
	/// <c>System.Attribute</c>) is decorated with an <c>[AttributeUsage]</c> 
	/// attribute to specify which kind of code instances of that custom attribute can be applied to.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// // this applies to everything - but the meaning is not clear
	/// public sealed class SomeAttribute : Attribute {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good examples:
	/// <code>
	/// // this clearly applies to everything
	/// [AttributeUsage (AttributeTargets.All)]
	/// public sealed class AttributeApplyingToAnything : Attribute {
	/// }
	/// 
	/// // while this applies only to fields
	/// [AttributeUsage (AttributeTargets.Field)]
	/// public sealed class AttributeApplyingToFields : Attribute {
	/// }
	/// </code>
	/// </example>

	[Problem ("This attribute does not specify the items it can be used upon.")]
	[Solution ("Specify [AttributeUsage] on this attribute type.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1018:MarkAttributesWithAttributeUsage")]
	public class MissingAttributeUsageOnCustomAttributeRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to attributes
			if (!type.IsAttribute ())
				return RuleResult.DoesNotApply;

			if (type.HasAttribute ("System", "AttributeUsageAttribute")) // it's ok
				return RuleResult.Success;

			Runner.Report (type, Severity.High, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
