// 
// Gendarme.Rules.Design.TypesShouldBeInsideNamespacesRule
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
	/// This rule will fire if a type which is visible outside the assembly is not declared
	/// within a namespace. Using namespaces greatly reduces the probability of name
	/// collisions, allows tools such as auto-complete to operate better, and can make
	/// the assemblies API clearer.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// using System;
	/// 
	/// public class Configuration {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// using System;
	/// 
	/// namespace My.Stuff {
	///	public class Configuration {
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type is visible outside the assembly so it should be defined inside a namespace to avoid conflicts.")]
	[Solution ("Move this type inside a namespace or reduce it's visibility (e.g. internal or private).")]
	[FxCopCompatibility ("Microsoft.Design", "CA1050:DeclareTypesInNamespaces")]
	public class TypesShouldBeInsideNamespacesRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule doesn't apply to nested types, since the declaring type will already be reported
			if (type.IsNested)
				return RuleResult.DoesNotApply;
			
			// rule applies to only to types visible outside the assembly
			if (!type.IsVisible ())
				return RuleResult.DoesNotApply;

			// rule applies!

			// check if the type resides inside a namespace
			if (!String.IsNullOrEmpty (type.Namespace))
				return RuleResult.Success;

			Runner.Report (type, Severity.Low, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
