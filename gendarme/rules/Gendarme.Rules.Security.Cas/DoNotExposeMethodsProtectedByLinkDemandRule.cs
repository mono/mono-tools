//
// Gendarme.Rules.Security.Cas.DoNotExposeMethodsProtectedByLinkDemandRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005, 2007-2008, 2010 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Security.Cas {

	/// <summary>
	/// This rule checks for visible methods that are less protected (i.e. lower security 
	/// requirements) than the method they call. If the called methods are protected by a 
	/// <c>LinkDemand</c> then the caller can be used to bypass security checks.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class BaseClass {
	///	[SecurityPermission (SecurityAction.LinkDemand, Unrestricted = true)]
	/// 	public virtual void VirtualMethod ()
	/// 	{
	/// 	}
	/// }
	/// 
	/// public class Class : BaseClass  {
	///	// bad since a caller with only ControlAppDomain will be able to call the base method
	/// 	[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
	///	public override void VirtualMethod ()
	///	{
	///		base.VirtualMethod ();
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (InheritanceDemand):
	/// <code>
	/// public class BaseClass {
	/// 	[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
	/// 	public virtual void VirtualMethod ()
	/// 	{
	/// 	}
	/// }
	/// 
	/// public class Class : BaseClass  {
	///	// ok since this permission cover the base class permission
	///	[SecurityPermission (SecurityAction.LinkDemand, Unrestricted = true)]
	///	public override void VirtualMethod ()
	///	{
	///		base.VirtualMethod ();
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>Before Gendarme 2.2 this rule was part of Gendarme.Rules.Security and named MethodCallWithSubsetLinkDemandRule.</remarks>

	[Problem ("This method is less protected than some of the methods it calls.")]
	[Solution ("Ensure that the LinkDemand on this method is a superset of any LinkDemand present on called methods.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
	public class DoNotExposeMethodsProtectedByLinkDemandRule : Rule, IMethodRule {

		static PermissionSet Empty = new PermissionSet (PermissionState.None);

		private static PermissionSet GetLinkDemand (ISecurityDeclarationProvider method)
		{
			foreach (SecurityDeclaration declsec in method.SecurityDeclarations) {
				switch (declsec.Action) {
				case Mono.Cecil.SecurityAction.LinkDemand:
				case Mono.Cecil.SecurityAction.NonCasLinkDemand:
					return declsec.ToPermissionSet ();
				}
			}
			return Empty;
		}

		private static bool Check (ISecurityDeclarationProvider caller, ISecurityDeclarationProvider callee)
		{
			// 1 - look if the callee has a LinkDemand
			PermissionSet calleeLinkDemand = GetLinkDemand (callee);
			if (calleeLinkDemand.Count == 0)
				return true;

			// 2 - Ensure the caller requires a superset (or the same) permissions
			return calleeLinkDemand.IsSubsetOf (GetLinkDemand (caller));
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// #1 - rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			//	otherwise we don't know what it's calling
			if (!method.HasBody)
				return RuleResult.DoesNotApply;
			
			// #2 - rule apply to methods are publicly accessible
			//	note that the type doesn't have to be public (indirect access)
			if (!method.IsVisible ())
				return RuleResult.DoesNotApply;

			// #3 - avoid looping if we're sure there's no call in the method
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			// *** ok, the rule applies! ***

			// #4 - look for every method we call
			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					MethodDefinition callee = (ins.Operand as MethodDefinition);
					if (callee == null)
						continue;

					// 4 - and if it has security, ensure we don't reduce it's strength
					if (callee.HasSecurityDeclarations && !Check (method, callee)) {
						Runner.Report (method, ins, Severity.High, Confidence.High);
					}
					break;
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
