//
// Gendarme.Rules.Design.AvoidMultidimensionalIndexerRule
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
	/// This rule checks for externally visible indexer properties which have more
	/// than one index argument. These can be confusing to some developers and
	/// IDEs with auto-complete don't always handle them as well as methods so
	/// it can be hard to know which argument is which. 
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public int this [int x, int y] {
	///	get {
	///		return 0;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public int Get (int x, int y)
	/// {
	///	return 0;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This indexer use multiple indexes which can impair its usability.")]
	[Solution ("Consider converting the indexer into a method.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
	public class AvoidMultidimensionalIndexerRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to indexers
			if (method.Name != "get_Item")
				return RuleResult.DoesNotApply;

			// if there is a single argument or if the method is not visible outside the assembly
			if ((method.HasParameters && (method.Parameters.Count == 1)) || !method.IsVisible ())
				return RuleResult.Success;

			Runner.Report (method, Severity.Medium, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
