// 
// Gendarme.Rules.Design.AbstractTypesShouldNotHavePublicConstructorsRule
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
	/// This rule ensure that <c>abstract</c> types do not have <c>public</c> constructor 
	/// since they cannot be instantied by user code. Changing the constructor(s) visibility
	/// to <c>protected</c> keeps the type functionality identical and makes it clear that
	/// the type is there to be inherited from.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// abstract public class MyClass {
	///	public MyClass ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// abstract public class MyClass {
	///	protected MyClass ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This abstract type provides public constructor(s).")]
	[Solution ("Change constructor visibility to protected.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1012:AbstractTypesShouldNotHaveConstructors")]
	public class AbstractTypesShouldNotHavePublicConstructorsRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only on abstract types
			if (!type.IsAbstract)
				return RuleResult.DoesNotApply;

			// rule applies!

			foreach (MethodDefinition ctor in type.Constructors) {
				if (ctor.IsPublic) {
					Runner.Report (ctor, Severity.Low, Confidence.Total);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
