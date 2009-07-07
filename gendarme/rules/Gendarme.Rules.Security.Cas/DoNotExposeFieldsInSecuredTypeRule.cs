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

	/// <summary>
	/// The rule checks for types that are secured by <c>Demand</c> or <c>LinkDemand</c>
	/// but also expose visible fields. Access to these fields is not covered by the
	/// declarative demands, opening potential security holes.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
	/// public class Bad {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (InheritanceDemand):
	/// <code>
	/// [SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
	/// [SecurityPermission (SecurityAction.InheritanceDemand, ControlThread = true)]
	/// public class Correct {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (sealed):
	/// <code>
	/// [SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
	/// public sealed class Correct {
	/// }
	/// </code>
	/// </example>
	/// <remarks>Before Gendarme 2.2 this rule was part of Gendarme.Rules.Security and named TypeExposeFieldsRule.</remarks>

	[Problem ("This type is secured by [Link]Demand but exposes some visible fields.")]
	[Solution ("Remove the (unsecured) visible fields, turn them into (secured) properties or reduce their access.")]
	[FxCopCompatibility ("Microsoft.Security", "CA2112:SecuredTypesShouldNotExposeFields")]
	public class DoNotExposeFieldsInSecuredTypeRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only to types protected by either a Demand or a LinkDemand (i.e. SecurityDeclaration)
			// that have fields and are visible outside the assembly
			if (!type.HasSecurityDeclarations || !type.HasFields || !type.IsVisible ())
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

			// type shouldn't have any visible fields
			foreach (FieldDefinition field in type.Fields) {
				if (field.IsVisible ()) {
					Runner.Report (field, Severity.Critical, Confidence.Total);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
