//
// Gendarme.Rules.Security.Cas.AddMissingTypeInheritanceDemandRule
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
using System.Security;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Security.Cas {

	/// <summary>
	/// The rule checks for types that are not <c>sealed</c> but have a <c>LinkDemand</c>.
	/// In this case the type should also have an <c>InheritanceDemand</c> for the same 
	/// permissions. An alternative is to seal the type.
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
	/// <remarks>Before Gendarme 2.2 this rule was part of Gendarme.Rules.Security and named TypeLinkDemandRule.</remarks>

	[Problem ("The type isn't sealed and has a LinkDemand. It should also have an InheritanceDemand for the same permissions.")]
	[Solution ("Add an InheritanceDemand for the same permissions (as the LinkDemand) or seal the class.")]
	[FxCopCompatibility ("Microsoft.Security", "CA2126:TypeLinkDemandsRequireInheritanceDemands")]
	public class AddMissingTypeInheritanceDemandRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule apply only to types that
			// - are not sealed;
			// - have a LinkDemand (so some security declarations); and
			// - are visible outside the current assembly
			if (type.IsSealed || !type.HasSecurityDeclarations || !type.IsVisible ())
				return RuleResult.DoesNotApply;

			PermissionSet link = null;
			PermissionSet inherit = null;
			// rule apply to types with a LinkDemand
			foreach (SecurityDeclaration declsec in type.SecurityDeclarations) {
				switch (declsec.Action) {
				case Mono.Cecil.SecurityAction.LinkDemand:
				case Mono.Cecil.SecurityAction.NonCasLinkDemand:
					link = declsec.ToPermissionSet ();
					break;
				case Mono.Cecil.SecurityAction.InheritDemand:
				case Mono.Cecil.SecurityAction.NonCasInheritance:
					inherit = declsec.ToPermissionSet ();
					break;
				}
			}

			// no LinkDemand == no problem
			if (link == null)
				return RuleResult.DoesNotApply;

			// rule apply if there are virtual methods defined
			bool virt = false;
			foreach (MethodDefinition method in type.Methods) {
				// ensure that the method is declared in this type (i.e. not in a parent)
				if (method.IsVirtual && ((method.DeclaringType as TypeDefinition) == type))
					virt = true;
			}

			// no virtual method == no problem
			if (!virt)
				return RuleResult.DoesNotApply;

			// *** ok, the rule applies! ***

			// Ensure the LinkDemand is a subset of the InheritanceDemand

			if (inherit == null) {
				// LinkDemand without InheritanceDemand
				Runner.Report (type, Severity.High, Confidence.High, "LinkDemand is present but no InheritanceDemand is specified.");
			} else if (!link.IsSubsetOf (inherit)) {
				Runner.Report (type, Severity.High, Confidence.High, "LinkDemand is not a subset of InheritanceDemand.");
			}
			return Runner.CurrentRuleResult;
		}
	}
}
