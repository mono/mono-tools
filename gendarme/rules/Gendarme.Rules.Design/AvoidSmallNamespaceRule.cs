//
// Gendarme.Rules.Design.AvoidSmallNamespaceRule
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule fires if a namespace contains less than five (by default) visible types. Note
	/// that this rule enumerates the types in all the assemblies being analyzed instead of
	/// simply considering each assembly in turn.
	/// The rule exempts:
	/// <list>
	/// <item><term>specialized namespaces</term><description>e.g. <c>*.Design</c>,
	/// <c>*.Interop</c> and <c>*.Permissions</c></description></item>
	/// <item><term>internal namespaces</term><description>namespaces without any visible 
	/// (outside the assemble) types</description></item>
	/// <item><term>small assemblies</term><description>that contains a single namespace but less than
	/// the minimal number of types (e.g. addins)</description></item>
	/// <item><term>assembly entry point</term><description>the namespace of the type being
	/// used in an assemble (EXE) entry-point</description></item>
	/// </list>
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// namespace MyStuff.Special {
	///	// single type inside a namespace
	///	public class Helper {
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// namespace MyStuff {
	///	public class Helper {
	///	}
	///	// ... many other types ...
	/// }
	/// </code>
	/// </example>

	[Problem ("This namespaces contains very few visible types.")]
	[Solution ("Consider merging this namespace with its parent or a sibling.")]
	[EngineDependency (typeof (NamespaceEngine))]
	[FxCopCompatibility ("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes")]
	public class AvoidSmallNamespaceRule : Rule, IAssemblyRule {

		private const int Default = 5;
		private int minimum = Default;

		/// <summary>
		/// The minimum number of types which must exist within a namespace.
		/// </summary>
		[DefaultValue (Default)]
		[Description ("The minimum of types that should exists inside a namespace.")]
		public int Minimum {
			get { return minimum; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("Minimum", "Must be positive");
				minimum = value;
			}
		}

		HashSet<string> ignore = new HashSet<string> ();

		public RuleResult CheckAssembly (AssemblyDefinition assembly)
		{
			MethodDefinition entry_point = assembly.EntryPoint;
			if (entry_point != null) {
				// for EXE assemblies ignore the namespace of the entry point
				ignore.AddIfNew (entry_point.DeclaringType.Namespace);
			}

			// ignore assemblies with a single namespace
			int count = 0;
			string single = null;
			foreach (string ns in NamespaceEngine.NamespacesInside (assembly)) {
				// no type inside the assembly 
				if (ns == null)
					break;

				// only count if there are some visible types inside the namespace
				bool visible = false;
				foreach (TypeDefinition type in NamespaceEngine.TypesInside (ns)) {
					if (type.IsVisible ()) {
						visible = true;
						break;
					}
				}

				if (visible) {
					single = ns;
					count++;
				}

				// no need to go more than 2
				if (count > 1)
					break;
			}
			if (count == 1)
				ignore.AddIfNew (single);

			return RuleResult.Success;
		}

		public override void TearDown ()
		{
			// check every namespaces inside the assembly set being analyzed
			foreach (string ns in NamespaceEngine.AllNamespaces ()) {

				// skip specialized namespaces
				if (NamespaceDefinition.IsSpecialized (ns))
					continue;

				if (ignore.Contains (ns))
					continue;

				int count = 0;
				foreach (TypeDefinition type in NamespaceEngine.TypesInside (ns)) {
					if (type.IsVisible ())
						count++;
					// no early termination here so we can report the correct count
				}

				// don't report namespaces that are not visible outside the assembly
				// e.g. VS.NET adds a .Properties namespace to SWF apps
				if ((count > 0) && (count < Minimum)) {
					NamespaceDefinition n = NamespaceDefinition.GetDefinition (ns);
					string msg = String.Format (CultureInfo.CurrentCulture, 
						"Only {0} visible types are defined inside this namespace.", count);
					// overloads of Report cannot be used here since the 'target' has been lost in the runner
					Runner.Report (new Defect (this, n, n, Severity.Low, Confidence.Total, msg));
				}
			}
			ignore.Clear ();
			base.TearDown ();
		}
	}
}
