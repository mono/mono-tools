//
// Gendarme.Rules.Design.MarkAssemblyWithAssemblyVersionRule
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
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if an assembly does not contain a <c>[AssemblyVersion]</c>
	/// attribute. Early and correct versioning of assemblies is easy and crucial for consumers
	/// of your assemblies. Note that the <c>[AssemblyVersion]</c> should
	/// match the <c>[AssemblyFileVersion]</c> attribute (if it exists).
	/// </summary>
	/// <example>
	/// Good example:
	/// <code>
	/// [assembly: AssemblyVersion ("1.0.0.0")]
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This assembly is not decorated with the [AssemblyVersion] attribute.")]
	[Solution ("Add the missing [AssemblyVersion] attribute with a valid version number.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1016:MarkAssembliesWithAssemblyVersion")]
	public class MarkAssemblyWithAssemblyVersionRule : Rule, IAssemblyRule {

		// [AssemblyVersion] is not compiled into a "real" custom attribute so
		// we're not inheriting from MarkAssemblyWithAttributeRule for this rule

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			if (!assembly.Name.Version.IsEmpty ())
				return RuleResult.Success;

			Runner.Report (assembly, Severity.Critical, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
