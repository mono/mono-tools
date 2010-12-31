//
// Gendarme.Rules.Interoperability.CentralizePInvokesIntoNativeMethodsTypeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Interoperability {

	/// <summary>
	/// This rule will warn you if p/invoke declarations are found outside some
	/// specially named types. The convention makes it easier to know which type
	/// of security checks are done (at runtime) and how critical is a security 
	/// audit for them. In all cases the type should not be visible (i.e. <c>internal</c> 
	/// in C#) outside the assembly.
	/// 
	/// Note that the type naming itself has no influence on security (either with 
	/// Code Access Security or with CoreCLR for Silverlight). The naming convention
	/// includes the presence or absence of the <c>[SuppressUnmanagedCodeSecurity]</c> 
	/// security attribute based on the type name.
	/// <list>
	/// <item><description><c>NativeMethods</c> should not be decorated with a 
	/// <c>[SuppressUnmanagedCodeSecurity]</c>. This will let CAS do a stackwalk to 
	/// ensure the code can be...</description></item>
	/// <item><description><c>SafeNativeMethods</c> should be decorated with a 
	/// <c>[SuppressUnmanagedCodeSecurity] attribute</c>. The attribute means that no 
	/// stackwalk will occurs.</description></item>
	/// <item><description><c>UnsafeNativeMethods</c> should be decorated with a 
	/// <c>[SuppressUnmanagedCodeSecurity] attribute</c>. The attribute means that no 
	/// stackwalk will occurs. However since the p/invoke methods are named unsafe then
	/// the rule will warn an audit-level defect to review the code.</description></item>
	/// </list>
	/// </summary>
	/// <remarks>This rule is available since Gendarme 2.8</remarks>
	[Problem ("A p/invoke declaration was found outside a *NativeMethods type.")]
	[Solution ("Move all p/invokes declarations into the right *NativeMethods type.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
	public class CentralizePInvokesIntoNativeMethodsTypeRule : Rule, ITypeRule, IMethodRule {

		// Note: you either like the rule or not (disable it). If you 
		// follow it then it is important to follow it 100% since other
		// people will expect specific behaviors from specific names

		const string Audit = "Unsafe p/invoke decorated with [SuppressUnmanagedCodeSecurity] needs a security review.";

		static bool CanInstantiateType (TypeDefinition type)
		{
			// type is static (>= 2.0)
			if (type.IsStatic ())
				return false;

			if (type.IsSealed && type.HasMethods) {
				foreach (MethodDefinition ctor in type.Methods) {
					if (ctor.IsConstructor && ctor.IsVisible ())
						return true;
				}
				return false;
			}
			return true;
		}

		private void CheckTypeVisibility (TypeDefinition type)
		{
			// *NativeMethods types should never be visible outside the assembly
			if (type.IsVisible ()) {
				string msg = String.Format ("'{0}' should not be visible outside the assembly.", type);
				Runner.Report (type, Severity.High, Confidence.Total, msg);
			}

			if (CanInstantiateType (type)) {
				string msg = String.Format ("'{0}' should not be static or sealed with no visible constructor.", type);
				Runner.Report (type, Severity.High, Confidence.Total, msg);
			}
		}

		private void CheckSuppressUnmanagedCodeSecurity (TypeDefinition type, bool required)
		{
			string msg = null;
			if (type.HasCustomAttributes && type.CustomAttributes.ContainsType ("System.Security.SuppressUnmanagedCodeSecurityAttribute")) {
				if (!required)
					 msg = "Remove [SuppressUnmanagedCodeSecurity] attribute on the type declaration.";
			} else {
				// no [SuppressUnmanagedCodeSecurity] attribute
				if (required)
					msg = "Add missing [SuppressUnmanagedCodeSecurity] attribute on the type declaration.";
			}

			if (msg != null)
				Runner.Report (type, Severity.Critical, Confidence.Total, msg);
		}

		// if the "right" type names is used then it's very important to follow 100% of the convention
		public RuleResult CheckType (TypeDefinition type)
		{
			switch (type.Name) {
			case "NativeMethods":
				// type must NOT be visible...
				CheckTypeVisibility (type);
				// and NOT decorated with [SuppressUnmanagedCodeSecurity]
				CheckSuppressUnmanagedCodeSecurity (type, false);
				break;
			case "SafeNativeMethods":
				// type must NOT be visible...
				CheckTypeVisibility (type);
				// and decorated with [SuppressUnmanagedCodeSecurity]
				CheckSuppressUnmanagedCodeSecurity (type, true);
				break;
			case "UnsafeNativeMethods":
				// type must NOT be visible...
				CheckTypeVisibility (type);
				// and decorated with [SuppressUnmanagedCodeSecurity]
				CheckSuppressUnmanagedCodeSecurity (type, true);
				// always report an audit-level defects for UnsafeNativeMethods
				Runner.Report (type, Severity.Audit, Confidence.Total, Audit);
				return RuleResult.Failure;
			default:
				// if p/invokes exists in this type then CheckMethod will spot them
				return RuleResult.DoesNotApply;
			}
			return Runner.CurrentRuleResult;
		}

		// note: all p/invokes methods should not be visible - but 
		// this is already checked by PInvokeShouldNotBeVisibleRule, 
		// which is important as a standalone rule for people that 
		// do not follow this, more complete, convention
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.IsPInvokeImpl)
				return RuleResult.DoesNotApply;

			// are the p/invoke declaration in a "well-named" type ?
			switch (method.DeclaringType.Name) {
			case "NativeMethods":
			case "SafeNativeMethods":
			case "UnsafeNativeMethods":
				return RuleResult.Success;
			default:
				// method does not follow the type-naming convention
				Runner.Report (method, Severity.Medium, Confidence.Total);
				return RuleResult.Failure;
			}
		}
	}
}

