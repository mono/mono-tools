//
// Gendarme.Rules.Design.AvoidRefAndOutParametersRule
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

	// TODO: MS is thinking of adding support for tuples to C#. If this is happens we
	// should mention them in the solution.

	/// <summary>
	/// This rule fires if a method uses <c>ref</c> or <c>out</c> parameters. 
	/// These are advanced features that can easily be misunderstood (by the consumer)
	/// and misused (by the consumer) and can result in an API that is difficult to use. 
	/// Avoid them whenever 
	/// possible or, if needed, provide simpler alternatives for most use cases.
	/// An exception is made, i.e. no defect are reported, for the <c>bool Try*(X out)</c> 
	/// pattern.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool NextJob (ref int id, out string display)
	/// {
	///	if (id &lt; 0)
	///		return false;
	///	display = String.Format ("Job #{0}", id++);
	///	return true;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// private int id = 0;
	/// 
	/// private int GetNextId ()
	/// {
	///	int id = this.id++;
	///	return id;
	/// }
	/// 
	/// public string NextJob ()
	/// {
	///	return String.Format ("Job #{0}", Id);
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method use ref and/or out parameters in a visible API which can confuse many developers.")]
	[Solution ("The most common reason to do this is to return multiple values from a method which can be rewritten so that it returns a custom type instead.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1021:AvoidOutParameters")]
	[FxCopCompatibility ("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
	public class AvoidRefAndOutParametersRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to visible methods with parameters
			// we also exclude all p/invokes since we have a rule for them not to be visible
			if (method.IsPInvokeImpl || !method.HasParameters || !method.IsVisible ())
				return RuleResult.DoesNotApply;

			foreach (ParameterDefinition parameter in method.Parameters) {
				string how = null;
				if (parameter.IsOut) {
					// out is permitted for the "bool Try* (...)" pattern
					if ((method.ReturnType.FullName == "System.Boolean") && 
						method.Name.StartsWith ("Try", StringComparison.Ordinal)) {
						continue;
					}

					how = "out";
				} else if (parameter.ParameterType.IsByReference) {
					how = "ref";
				}

				if (how != null) {
					// goal is to keep the API as simple as possible so this is more severe for public than protected methods
					Severity severity = method.IsPublic ? Severity.Medium : Severity.Low;
					string msg = String.Format ("Parameter '{0}' passed by reference ({1}).", parameter.Name, how);
					Runner.Report (parameter, severity, Confidence.Total, msg);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
