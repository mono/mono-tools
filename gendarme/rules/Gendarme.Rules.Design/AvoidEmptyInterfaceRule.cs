// 
// Gendarme.Rules.Design.AvoidEmptyInterfaceRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if an interface declares no members. Empty interfaces are generally
	/// not useful except as markers to categorize types and attributes are the preferred 
	/// way to handle that.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public interface IMarker {
	/// }
	/// 
	/// public class MyClass : IMarker {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [MarkedByAnAttribute]
	/// public class MyClass {
	/// }
	/// </code>
	/// </example>

	[Problem ("This interface does not define any members. This is often a sign that the interface is used simply to mark up types.")]
	[Solution ("Review the interface usage. If it is used as a marker then consider replacing it with an attribute.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public class AvoidEmptyInterfaceRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule only applies to interfaces
			if (!type.IsInterface)
				return RuleResult.DoesNotApply;

			// rule applies!

			// first check if the interface defines it's own members
			if (type.HasMethods)
				return RuleResult.Success;

			// otherwise it may implement more than one interface itself
			if (type.HasInterfaces && (type.Interfaces.Count > 1))
				return RuleResult.Success;

			Runner.Report (type, Severity.Low, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
