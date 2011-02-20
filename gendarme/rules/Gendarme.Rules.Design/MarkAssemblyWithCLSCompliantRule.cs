//
// Gendarme.Rules.Design.MarkAssemblyWithCLSCompliantRule
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
	/// This rule fires if an assembly does not contain a <c>[CLSCompliant]</c> attribute. 
	/// CLS compliant assemblies can be reused by any CLS-compliant language. It is a good practice
	/// to declare your global CLS goal at the assembly level and, if needed, mark some types or
	/// members that behave differently inside your assembly.
	/// </summary>
	/// <example>
	/// Good example:
	/// <code>
	/// // by default everything in this assembly is CLS compliant
	/// [assembly: CLSCompliant (true)]
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This assembly is not decorated with the [CLSCompliant] attribute.")]
	[Solution ("Add this attribute to ease the use (or non-use) of your assembly by CLS consumers.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1014:MarkAssembliesWithClsCompliant")]
	public class MarkAssemblyWithCLSCompliantRule : MarkAssemblyWithAttributeRule {

		protected override string AttributeNamespace {
			get { return "System"; }
		}

		protected override string AttributeName {
			get { return "CLSCompliantAttribute"; }
		}

		protected override Severity Severity {
			get { return Severity.High; }
		}
	}
}
