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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if an <c>abstract</c> type has a <c>public</c> constructor. This is
	/// a bit misleading because the constructor can only be called by the constructor of
	/// a derived type. To make the type's semantics clearer make the constructor
	/// <c>protected</c>.
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

	[Problem ("This abstract type defines public constructor(s).")]
	[Solution ("Change the constructor access to protected.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1012:AbstractTypesShouldNotHaveConstructors")]
	public class AbstractTypesShouldNotHavePublicConstructorsRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only on abstract types
			if (!type.IsAbstract)
				return RuleResult.DoesNotApply;

			// rule applies!

			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor && method.IsPublic) {
					Runner.Report (method, Severity.Low, Confidence.Total);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
