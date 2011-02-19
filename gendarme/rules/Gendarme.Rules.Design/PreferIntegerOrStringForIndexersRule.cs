//
// Gendarme.Rules.Design.PreferIntegerOrStringForIndexersRule
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

	// TODO: Is there a reason that the summary does not mention System.Object?

	/// <summary>
	/// This rule checks for indexer properties which use unusual types as indexes.
	/// Recommended types include <c>Int32</c>, <c>Int64</c> and <c>String</c>.
	/// Using other types can be OK if the indexer is providing an abstraction onto a
	/// logical data store, but this is often not the case.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool this [DateTime date] {
	///	get {
	///		return false;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public bool IsSomethingPlanned (DateTime date)
	/// {
	///	return false;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This indexer should be using integers or strings for its indexes.")]
	[Solution ("Convert this indexer into a method if an integer or a string cannot be used.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
	public class PreferIntegerOrStringForIndexersRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to indexers
			if (method.Name != "get_Item")
				return RuleResult.DoesNotApply;

			// ok if the method is not visible outside the assembly
			if (!method.IsVisible ())
				return RuleResult.Success;

			foreach (ParameterDefinition parameter in method.Parameters) {
				TypeReference ptype = parameter.ParameterType;
				bool ok = (ptype.Namespace == "System");
				if (ok) {
					switch (ptype.Name) {
					case "Int32":
					case "Int64":
					case "String":
					case "Object": // tolerable in some circumstances
						break;
					default:
						ok = false;
						break;
					}
				}
				if (!ok)
					Runner.Report (parameter, Severity.Medium, Confidence.Total);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
