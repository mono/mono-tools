//
// Gendarme.Rules.Maintainability.AvoidDeepInheritanceTreeRule
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
using System.Globalization;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// This rule will fire if a type has (by default) more than four base classes defined
	/// within the assembly set being analyzed. Optionally it will also count base 
	/// classes defined outside the assembly set being analyzed.
	/// </summary>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This type has more than four base classes.")]
	[Solution ("Refactor your class hierarchy to reduce its depth. Consider using extension methods to extend existing types.")]
	[FxCopCompatibility ("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	public class AvoidDeepInheritanceTreeRule : Rule, ITypeRule {

		/// <summary>Classes with more base classes than this will result in a defect.</summary>
		/// <remarks>Defaults to 4.</remarks>
		[DefaultValue (DefaultMaximumDepth)]
		[Description ("Classes with more base classes than this will result in a defect.")]
		public int MaximumDepth {
			get { return maximumDepth; }
			set { maximumDepth = value; }
		}
		private const int DefaultMaximumDepth = 4;
		private int maximumDepth = DefaultMaximumDepth;

		/// <summary>If true the rule will count base classes defined outside the assemblies being analyzed.</summary>
		/// <remarks>Defaults to false.</remarks>
		[DefaultValue (DefaultCountExternalDepth)]
		[Description ("If true the rule will count base classes defined outside the assemblies being analyzed.")]
		public bool CountExternalDepth {
			get { return countExternalDepth; }
			set { countExternalDepth = value; }
		}
		private const bool DefaultCountExternalDepth = false;
		private bool countExternalDepth = DefaultCountExternalDepth;


		private Severity GetSeverity (int depth)
		{
			if (depth < 2 * MaximumDepth)
				return Severity.Medium;
			return (depth < 4 * MaximumDepth) ? Severity.High : Severity.Critical;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsInterface || type.IsValueType || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			int depth = 0;
			while (type.BaseType != null) {
				type = type.BaseType.Resolve ();
				if (type == null)
					break;
				if (countExternalDepth || Runner.Assemblies.Contains (type.Module.Assembly))
					depth++;
			}

			if (depth <= MaximumDepth)
				return RuleResult.Success;

			// Confidence is total unless we count outside the current assembly,
			// where it's possible we can't resolve up to System.Object
			Confidence confidence = countExternalDepth ? Confidence.High : Confidence.Total;
			// Severity is based on the depth
			string msg = String.Format (CultureInfo.CurrentCulture, "Inheritance tree depth : {0}.", depth);
			Runner.Report (type, GetSeverity (depth), confidence, msg);
			return RuleResult.Failure;
		}
	}
}
