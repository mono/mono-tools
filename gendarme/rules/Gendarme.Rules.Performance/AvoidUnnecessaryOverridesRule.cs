//
// Gendarme.Rules.Performance.AvoidUnnecessaryOverridesRule
//
// Authors:
//	N Lum <nol888@gmail.com>
//
// Copyright (C) 2010 N Lum
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


using Gendarme.Framework;
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule looks for overriding methods which just call the base method, and don't
	/// define any additional attributes or security declarations.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public override string ToString ()
	/// {
	///		return base.ToString ();
	///	}
	/// </code>
	/// </example>
	/// <example>
	/// Good example (1):
	/// <code>
	/// [FileIOPermission (SecurityAction.Demand, @"c:\dir\file")]
	/// public override string ToString ()
	/// {
	///		return base.ToString ();
	///	}
	/// </code>
	/// </example>
	/// <example>
	/// Good example (2):
	/// <code>
	/// /*public override string ToString ()
	/// {
	///		return base.ToString ();
	///	}*/
	/// </code>
	/// </example>

	[Problem ("This override of a base class method is unnecessary.")]
	[Solution ("Remove the override method or extend the functionality of the method.")]
	public class AvoidUnnecessaryOverridesRule : Rule, IMethodRule {

		static bool IsBase (MethodReference method, MethodReference mr)
		{
			if (mr.Name != method.Name)
				return false;

			if (!mr.CompareSignature (method))
				return false;

			foreach (TypeDefinition baseType in method.DeclaringType.AllSuperTypes ()) {
				if (mr.DeclaringType.FullName == baseType.FullName)
					return true;
			}
			return false;
		}

		static bool CompareCustomAttributes (ICustomAttributeProvider a, ICustomAttributeProvider b)
		{
			bool ha = a.HasCustomAttributes;
			bool hb = b.HasCustomAttributes;
			// if only one of them has custom attributes
			if (ha != hb)
				return false;
			// if both do not have custom attributes
			if (!ha && !hb)
				return true;
			// compare attributes
			foreach (CustomAttribute attr in a.CustomAttributes) {
				if (!b.CustomAttributes.Contains (attr))
					return false;
			}
			return true;
		}

		static bool CompareSecurityDeclarations (ISecurityDeclarationProvider a, ISecurityDeclarationProvider b)
		{
			bool ha = a.HasSecurityDeclarations;
			bool hb = b.HasSecurityDeclarations;
			// if only one of them has custom attributes
			if (ha != hb)
				return false;
			// if both do not have custom attributes
			if (!ha && !hb)
				return true;
			// compare attributes
			foreach (SecurityDeclaration sd in a.SecurityDeclarations) {
				if (!b.SecurityDeclarations.Contains (sd))
					return false;
			}
			return true;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// default ctor is non-virtual
			// static ctor is also not virtual
			// abstract methods do not have a body
			if (!method.HasBody || !method.IsVirtual)
				return RuleResult.DoesNotApply;

			// We need to check for a simple IL code pattern.
			// load args (if necessary), call method, return.
			// CSC emits some superflulous nops, starg, jmp, ldarg, return in debug mode, 
			// so that has to be checked as well.

			int i = 0;
			var instrs = method.Body.Instructions;

			Instruction ins = null;
			// Skip over the nops and load arguments.
			while (i < instrs.Count) {
				ins = instrs [i++];
				if (!ins.Is (Code.Nop) && !ins.IsLoadArgument ())
					break;
			}

			// If the next instruction is not a call we are good.
			if (!ins.Is (Code.Call) && !ins.Is (Code.Callvirt))
				return RuleResult.Success;
			// Check to make sure the call is to the base class, and the same method name...
			MethodReference mr = ins.Operand as MethodReference;
			if (!IsBase (method, mr))
				return RuleResult.Success;

			ins = instrs [i++];
			// If the return type is void, all we should have is nop and return.
			if (method.ReturnType.FullName == "System.Void") {
				if (!ins.Is (Code.Nop) || !ins.Next.Is (Code.Ret))
					return RuleResult.Success;
			} else {
				while (i < instrs.Count) {
					if (ins.Is (Code.Nop) || ins.Is (Code.Jmp) || ins.IsStoreLocal () || ins.IsLoadLocal ())
						break;
					ins = instrs [i++];
				}
				if (!ins.Is (Code.Ret))
					return RuleResult.Success;
			}

			// If we've gotten this far, that means the code is just a call to the base method.
			// We need to check for attributes/security declarations that aren't in the
			// base.
			MethodDefinition md = mr.Resolve ();
			// If we can't resolve the definition of the original method, we can't get
			// the original attributes, so we'll say something with low confidence.
			if (md == null) {
				Runner.Report (method, Severity.Medium, Confidence.Low);
				return RuleResult.Success;
			}

			if (!CompareCustomAttributes (method, md) || !CompareSecurityDeclarations (method, md))
				return RuleResult.Success;
			
			Runner.Report (method, Severity.Medium, Confidence.High);
			return Runner.CurrentRuleResult;
		}
	}
}

