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
	/// This rule checks every interface to ensure they are not empty. Empty interface don't 
	/// add anything useful and in many case are only used as a marker on a type. For the 
	/// last case it is more elegant to define and use an attribute.
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

	[Problem ("This interface does not define any members. This is generally a sign that the interface is used as a marker.")]
	[Solution ("Review the interface usage. If used as a marker then see if it could be replaced by using attributes.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public class AvoidEmptyInterfaceRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule only applies to interfaces
			if (!type.IsInterface)
				return RuleResult.DoesNotApply;

			// rule applies!

			// first check if the interface defines it's own members
			if (type.Methods.Count > 0)
				return RuleResult.Success;

			// otherwise it may implement more than one interface itself
			if (type.Interfaces.Count > 1)
				return RuleResult.Success;

			Runner.Report (type, Severity.Low, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
