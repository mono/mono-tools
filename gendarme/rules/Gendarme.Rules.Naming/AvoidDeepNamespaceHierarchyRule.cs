//
// Gendarme.Rules.Naming.AvoidDeepNamespaceHierarchyRule class
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
using System.ComponentModel;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule checks for deeply nested namespaces within an assembly. It will
	/// warn if the depth is greater than four (default value) unless the fifth (or the
	/// next) part is one of the specialized name that the framework recommends or a
	/// name like an internal namespace (something not meant to be seen outside the assembly).
	/// <list>
	/// <item><term>Design</term><description>Namespace that provides design-time
	/// support for its base namespace.</description></item>
	/// <item><term>Interop</term><description>Namespace that provides all interoperability
	/// code (e.g. p./invokes) for its base namespace.</description></item>
	/// <item><term>Permissions</term><description>Namespace that provides all custom 
	/// permissions for its base namespace.</description></item>
	/// <item><term>Internal</term><description>Namespace that provides non visible (outside
	/// the assembly) helper code for its base namespace. <c>Impl</c> while allowed by the 
	/// rule is not encouraged.
	/// </description></item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// namespace One.Two.Three.Four.Five {
	///	internal class Helper {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// namespace One.Two.Three.Four {
	///	internal class FiveHelper {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (exception for some namespace specialization):
	/// <code>
	/// namespace One.Two.Three.Four.Internal {
	///	internal class Helper {
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("The depth of the namespace hierarchy is getting out of control.")]
	[Solution ("Try to keep the depth below 4, with an additional one for specialization (e.g. Design, Interop, Permissions).")]
	[EngineDependency (typeof (NamespaceEngine))]
	public class AvoidDeepNamespaceHierarchyRule : Rule, IAssemblyRule {

		private const int DefaultMaxDepth = 4;
		private int max_depth = DefaultMaxDepth;

		/// <summary>The depth at which namespaces may be nested without triggering a defect.</summary>
		/// <remarks>Defaults to 4.</remarks>
		[DefaultValue (DefaultMaxDepth)]
		[Description ("The depth at which namespaces may be nested without triggering a defect.")]
		public int MaxDepth {
			get { return max_depth; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ("MaxDepth", "Minimum: 1");
				max_depth = value;
			}
		}

		private static int CountLevels (string ns)
		{
			if (ns.Length == 0)
				return 0;

			int levels = 0;
			int index = 0;
			while (index != -1) {
				index = ns.IndexOf ('.', index + 1);
				levels++;
			}
			return levels;
		}

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			// check every namespaces inside the assembly using the NamespaceEngine
			foreach (string ns in NamespaceEngine.NamespacesInside (assembly)) {
				// shortest invalid namespace would be "a.b.c.d.e"
				// so we can skip anything less than 2 * MaxDepth
				// note: overflow does not matter
				if (ns.Length < unchecked (MaxDepth + MaxDepth))
					continue;

				// count the levels (i.e. number of dots + 1)
				int levels = CountLevels (ns);
				if (levels <= MaxDepth)
					continue;

				// we have some exceptions for namespace specialization
				// i.e. stuff we often prefer to have in a sub-namespace
				// and for internal (non-visible) namespaces
				if (levels == MaxDepth + 1) {
					if (NamespaceDefinition.IsSpecialized (ns)) {
						continue;
					} else if (ns.EndsWith (".Internal", StringComparison.Ordinal) || ns.EndsWith (".Impl", StringComparison.Ordinal)) {
						continue;
					}
				}

				Severity severity = (levels < MaxDepth * 2) ? Severity.Medium : Severity.High;
				Runner.Report (NamespaceDefinition.GetDefinition (ns), severity, Confidence.High);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
