//
// Gendarme.Rules.Design.AvoidVisibleNestedTypesRule
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

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks for nested types which are externally visible. Such types are often
	/// confused with namespaces which makes them more difficult to document and find by
	/// developers. In most cases it is better to make these types private or to scope them
	/// within a namespace instead of a type.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Outer {
	///	public class Inner {
	///		// ...
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (visibility):
	/// <code>
	/// public class Outer {
	///	internal class Inner {
	///		// ...
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (unnested):
	/// <code>
	/// public class Outer {
	///	// ...
	/// }
	/// 
	/// public class Inner {
	///	// ...
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This type is both nested and visible outside the assembly. Nested types are often confused with namespaces.")]
	[Solution ("Change the nested type to be invisible outside the assembly or un-nest it.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
	public class AvoidVisibleNestedTypesRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// apply only to nested types
			if (!type.IsNested)
				return RuleResult.DoesNotApply;

			// it's ok if the type is not visible
			if (!type.IsVisible ())
				return RuleResult.Success;

			// otherwise we warn about the nested type
			Runner.Report (type, Severity.Medium, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
