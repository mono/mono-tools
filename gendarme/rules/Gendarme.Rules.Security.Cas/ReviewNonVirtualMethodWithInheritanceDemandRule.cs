//
// Gendarme.Rules.Security.Cas.ReviewNonVirtualMethodWithInheritanceDemandRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005,2008 Novell, Inc (http://www.novell.com)
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
using System.Collections;

using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Security.Cas {

	// TODO: Shouldn't this rule have a summary?

	[Problem ("This non-virtual method has an InheritanceDemand that the runtime will never execute.")]
	[Solution ("Review the InheritanceDemand on this method and either remove it or change its SecurityAction to, probably, a LinkDemand.")]
	public class ReviewNonVirtualMethodWithInheritanceDemandRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// this rule apply only to methods with an inheritance demand
			if (!method.HasSecurityDeclarations)
				return RuleResult.DoesNotApply;

			bool inherit = false;
			foreach (SecurityDeclaration declsec in method.SecurityDeclarations) {
				switch (declsec.Action) {
				case SecurityAction.InheritDemand:
				case SecurityAction.NonCasInheritance:
					inherit = true;
					break;
				}
			}
			if (!inherit)
				return RuleResult.DoesNotApply;

			// *** ok, the rule applies! ***

			// InheritanceDemand doesn't make sense on methods that cannot be overriden
			if (method.IsVirtual)
				return RuleResult.Success;

			// Severity.Low -> code works, it just won't get called. Problematic if the wrong action was choosen
			Runner.Report (method, Severity.Low, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
