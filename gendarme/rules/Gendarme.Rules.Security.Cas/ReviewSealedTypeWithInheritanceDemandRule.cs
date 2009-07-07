//
// Gendarme.Rules.Security.SealedTypeWithInheritanceDemandRule
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

namespace Gendarme.Rules.Security.Cas {

	/// <summary>
	/// This rule checks for sealed types that have <c>InheritanceDemand</c> declarative
	/// security applied to them. Since those types cannot be inherited from the 
	/// <c>InheritanceDemand</c> will never be executed by the runtime. Check if the permission
	/// is required and, if so, change the <c>SecurityAction</c> to the correct one. Otherwise
	/// remove the permission.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [SecurityPermission (SecurityAction.InheritanceDemand, Unrestricted = true)]
	/// public sealed class Bad {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (non sealed):
	/// <code>
	/// [SecurityPermission (SecurityAction.InheritanceDemand, Unrestricted = true)]
	/// public class Good {
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (LinkDemand):
	/// <code>
	/// [SecurityPermission (SecurityAction.LinkDemand, Unrestricted = true)]
	/// public sealed class Good {
	/// }
	/// </code>
	/// </example>
	/// <remarks>Before Gendarme 2.2 this rule was part of Gendarme.Rules.Security and named SealedTypeWithInheritanceDemandRule.</remarks>

	[Problem ("This sealed type has an InheritanceDemand that the runtime will never execute.")]
	[Solution ("Review the InheritanceDemand on this type and either remove it or change its SecurityAction to, probably, a LinkDemand.")]
	public class ReviewSealedTypeWithInheritanceDemandRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// 1 - this applies only to sealed types
			if (!type.IsSealed)
				return RuleResult.DoesNotApply;

			// 2 - the type must have an InheritanceDemand
			if (!type.HasSecurityDeclarations)
				return RuleResult.DoesNotApply;

			foreach (SecurityDeclaration declsec in type.SecurityDeclarations) {
				if (declsec.Action == SecurityAction.InheritDemand) {
					Runner.Report (type, Severity.Low, Confidence.Total);
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
