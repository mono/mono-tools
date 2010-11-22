//
// Gendarme.Rules.Security.Cas.DoNotReduceTypeSecurityOnMethodsRule
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
using System.Security;

using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Security.Cas {

	/// <summary>
	/// This rule checks for types that have declarative security permission which aren't a
	/// subset of the security permission of some of their methods.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [SecurityPermission (SecurityAction.Assert, ControlThread = true)]
	/// public class NotSubset {
	/// 	[EnvironmentPermission (SecurityAction.Assert, Unrestricted = true)]
	/// 	public void Method ()
	/// 	{
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [SecurityPermission (SecurityAction.Assert, ControlThread = true)]
	/// public class Subset {
	///	[SecurityPermission (SecurityAction.Assert, Unrestricted = true)]
	///	public void Method ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>Before Gendarme 2.2 this rule was part of Gendarme.Rules.Security and named TypeIsNotSubsetOfMethodSecurityRule.</remarks>

	[Problem ("This type has a declarative security permission that isn't a subset of the security on some of it's methods.")]
	[Solution ("Ensure that the type security is a subset of any method security. This rule doesn't apply for LinkDemand an Inheritance demands as both the type and methods security will be executed.")]
	[FxCopCompatibility ("Microsoft.Security", "CA2114:MethodSecurityShouldBeASupersetOfType")]
	public class DoNotReduceTypeSecurityOnMethodsRule : Rule, ITypeRule {

		private PermissionSet assert;
		private PermissionSet deny;
		private PermissionSet permitonly;
		private PermissionSet demand;

		private bool RuleDoesAppliesToType (ISecurityDeclarationProvider type)
		{
			assert = null;
			deny = null;
			permitonly = null;
			demand = null;

			// #1 - this rules apply if type has security permissions
			if (!type.HasSecurityDeclarations)
				return false;

			bool apply = false;
			// #2 - this rules doesn't apply to LinkDemand (both are executed)
			// and to InheritanceDemand (both are executed at different time).
			foreach (SecurityDeclaration declsec in type.SecurityDeclarations) {
				switch (declsec.Action) {
				case Mono.Cecil.SecurityAction.Assert:
					assert = declsec.ToPermissionSet ();
					apply = true;
					break;
				case Mono.Cecil.SecurityAction.Deny:
					deny = declsec.ToPermissionSet ();
					apply = true;
					break;
				case Mono.Cecil.SecurityAction.PermitOnly:
					permitonly = declsec.ToPermissionSet ();
					apply = true;
					break;
				case Mono.Cecil.SecurityAction.Demand:
					demand = declsec.ToPermissionSet ();
					apply = true;
					break;
				}
			}
			return apply;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only if type has security declarations
			if (!RuleDoesAppliesToType (type))
				return RuleResult.DoesNotApply;

			// *** ok, the rule applies! ***

			// ensure that method-level security doesn't replace type-level security
			// with a subset of the original check
			foreach (MethodDefinition method in type.Methods) {
				if (!method.HasSecurityDeclarations)
					continue;

				foreach (SecurityDeclaration declsec in method.SecurityDeclarations) {
					switch (declsec.Action) {
					case Mono.Cecil.SecurityAction.Assert:
						if (assert == null)
							continue;
						if (!assert.IsSubsetOf (declsec.ToPermissionSet ()))
							Runner.Report (method, Severity.High, Confidence.Total, "Assert");
						break;
					case Mono.Cecil.SecurityAction.Deny:
						if (deny == null)
							continue;
						if (!deny.IsSubsetOf (declsec.ToPermissionSet ()))
							Runner.Report (method, Severity.High, Confidence.Total, "Deny");
						break;
					case Mono.Cecil.SecurityAction.PermitOnly:
						if (permitonly == null)
							continue;
						if (!permitonly.IsSubsetOf (declsec.ToPermissionSet ()))
							Runner.Report (method, Severity.High, Confidence.Total, "PermitOnly");
						break;
					case Mono.Cecil.SecurityAction.Demand:
					case Mono.Cecil.SecurityAction.NonCasDemand:
						if (demand == null)
							continue;
						if (!demand.IsSubsetOf (declsec.ToPermissionSet ()))
							Runner.Report (method, Severity.High, Confidence.Total, "Demand");
						break;
					}
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}

