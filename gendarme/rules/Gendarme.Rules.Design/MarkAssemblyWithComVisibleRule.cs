//
// Gendarme.Rules.Design.MarkAssemblyWithComVisibleRule
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
	/// This rule fires if an assembly does not contain a <c>[ComVisible]</c> attribute. 
	/// Unless the assembly is designed with COM interoperability in mind it is better to declare
	/// it as non-COM visible, i.e. <c>[ComVisible (false)]</c>.
	/// </summary>
	/// <example>
	/// Good example:
	/// <code>
	/// // by default everything in this assembly is not visible to COM consumers
	/// [assembly: ComVisible (false)]
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("This assembly is not decorated with the [ComVisible] attribute.")]
	[Solution ("Add this attribute to ease the use (or non-use) of your assembly by COM consumers.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1017:MarkAssembliesWithComVisible")]
	public class MarkAssemblyWithComVisibleRule : MarkAssemblyWithAttributeRule {

		protected override string AttributeNamespace	{
			get { return "System.Runtime.InteropServices"; }
		}

		protected override string AttributeName	{
			get { return "ComVisibleAttribute"; }
		}

		protected override Severity Severity {
			get { return Severity.Medium; }
		}
	}
}
