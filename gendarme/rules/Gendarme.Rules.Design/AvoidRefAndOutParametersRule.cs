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

	[Problem ("This method use ref and/or out parameters in a visible API, which should be as simple as possible.")]
	[Solution ("If multiple return values are needed then refactor the method to return an object that contains them.")]
	public class AvoidRefAndOutParametersRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to visible API
			if (!method.IsVisible ())
				return RuleResult.DoesNotApply;

			foreach (ParameterDefinition parameter in method.Parameters) {
				string how = null;
				if (parameter.IsOut) {
					// out is permitted for the "bool Try* (...)" pattern
					if ((method.ReturnType.ReturnType.FullName == "System.Boolean") && method.Name.StartsWith ("Try"))
						continue;

					how = "out";
				} else if (parameter.ParameterType.Name.EndsWith ("&")) {
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
