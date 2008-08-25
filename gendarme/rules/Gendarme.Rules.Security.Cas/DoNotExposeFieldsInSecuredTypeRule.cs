//
// Gendarme.Rules.Security.Cas.DoNotExposeFieldsInSecuredTypeRule
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Security.Cas {

	[Problem ("This type is secured by [Link]Demand but expose some visible fields.")]
	[Solution ("Remove the (unsecured) visible fields, turn them into (secured) properties or reduce their visibility.")]
	[FxCopCompatibility ("Microsoft.Security", "CA2112:SecuredTypesShouldNotExposeFields")]
	public class DoNotExposeFieldsInSecuredTypeRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only to visible types
			if (!type.IsVisible ())
				return RuleResult.DoesNotApply;

			// rule apply only to types protected by either a Demand or a LinkDemand
			if (type.SecurityDeclarations.Count == 0)
				return RuleResult.DoesNotApply;

			bool demand = false;
			foreach (SecurityDeclaration declsec in type.SecurityDeclarations) {
				switch (declsec.Action) {
				case Mono.Cecil.SecurityAction.Demand:
				case Mono.Cecil.SecurityAction.LinkDemand:
					demand = true;
					break;
				}
			}

			if (!demand)
				return RuleResult.DoesNotApply;

			// *** ok, the rule applies! ***

			// type shouldn't have any public fields
			foreach (FieldDefinition field in type.Fields) {
				if (field.IsVisible ()) {
					Runner.Report (field, Severity.Critical, Confidence.Total);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
