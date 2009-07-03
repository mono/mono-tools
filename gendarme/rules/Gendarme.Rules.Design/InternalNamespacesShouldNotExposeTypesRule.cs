//
// Gendarme.Rules.Design.InternalNamespacesShouldNotExposeTypesRule
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks for externally visible types that reside inside internal namespaces, i.e.
	/// namespaces ending with <c>Internal</c> or <c>Impl</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// namespace MyStuff.Internal {
	///	public class Helper {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (internal type):
	/// <code>
	/// namespace MyStuff.Internal {
	///	internal class Helper {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (non-internal namespace):
	/// <code>
	/// namespace MyStuff {
	///	public class Helper {
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This internal namespace should not expose visible types outside the assembly.")]
	[Solution ("If the type needs to be public move it in another namespace, otherwise make the type internal.")]
	[EngineDependency (typeof (NamespaceEngine))]
	public class InternalNamespacesShouldNotExposeTypesRule : Rule, IAssemblyRule {

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			// check every namespaces inside the assembly using the NamespaceEngine
			foreach (string ns in NamespaceEngine.NamespacesInside (assembly)) {
				// rule only apply to "internal" namespaces
				if (!ns.EndsWith (".Internal", StringComparison.Ordinal) && 
					!ns.EndsWith (".Impl", StringComparison.Ordinal)) {
					continue;
				}

				foreach (TypeDefinition type in NamespaceEngine.TypesInside (ns)) {
					// only report for this assembly
					if (type.Module.Assembly != assembly)
						continue;

					// report all types inside that namespace
					if (type.IsVisible ())
						Runner.Report (type, Severity.Medium, Confidence.Total);
				}

			}
			return Runner.CurrentRuleResult;
		}
	}
}
