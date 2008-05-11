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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Maintainability {

	[Problem ("This type inheritance tree is more than four level deep.")]
	[Solution ("Refactor your class hierarchy to reduce it's depth. Consider using extension methods to extend existing types.")]
	public class AvoidDeepInheritanceTreeRule : Rule, ITypeRule {

		private const int MaximumDepth = 4;

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsInterface || type.IsValueType)
				return RuleResult.DoesNotApply;

			int depth = 0;
			while (type.BaseType != null) {
				type = type.BaseType.Resolve ();
				depth++;
			}

			if (depth <= MaximumDepth)
				return RuleResult.Success;

			Runner.Report (type, Severity.Medium, Confidence.Total, String.Format ("Inheritance tree depth {0}.", depth));
			return RuleResult.Failure;
		}
	}
}
