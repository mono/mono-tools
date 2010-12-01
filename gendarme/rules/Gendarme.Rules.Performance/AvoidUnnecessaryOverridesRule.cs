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
	public class AvoidUnnecessaryOverridesRule : Rule, ITypeRule {

		public RuleResult CheckType(TypeDefinition type)
		{
			if (!type.HasMethods)
				return RuleResult.DoesNotApply;

			// Iterate through all methods...
			foreach (MethodDefinition method in type.Methods) {
				// ...throw them out if they're not an override.
				if (!method.IsOverride ())
					continue;

				// We need to check for a simple IL code pattern.
				// load args (if necessary), call method, return.
				// CSC emits some superflulous nops, starg, jmp, ldarg, return in debug mode, 
				// so that has to be checked as well.
				if (!method.HasBody)
					continue;

				int i = 0;
				var instrs = method.Body.Instructions;

				// Skip over the nops and load arguments.
				while (i < instrs.Count && (instrs[i].Is (Code.Nop) || instrs[i].IsLoadArgument ()))
					i++;

				// If the next instruction is not a call we are good.
				if (!instrs[i].Is (Code.Call) && !instrs[i].Is (Code.Callvirt))
					continue;
				// Check to make sure the call is to the base class, and the same method name...
				MethodReference mr = instrs[i].Operand as MethodReference;
				bool isBase = false;
				foreach (TypeDefinition baseType in method.DeclaringType.AllSuperTypes ())
					if (mr.DeclaringType.FullName == baseType.FullName)
						isBase = true;
				if (!isBase)
					continue;
				if (mr.Name != method.Name)
					continue;
				// Account for calling overrides.
				if (mr.HasParameters != method.HasParameters)
					continue;
				// Check the parameter count and type.
				if (mr.HasParameters && (mr.Parameters.Count != method.Parameters.Count))
					continue;
				// Check for equality.
				bool sameParam = true;
				for (int o = 0; o < mr.Parameters.Count; o++) {
					var p1 = mr.Parameters[o];
					var p2 = method.Parameters[o];
					sameParam = true;
					sameParam &= p1.Attributes == p2.Attributes;
					sameParam &= p1.ParameterType == p2.ParameterType;
					if (!sameParam)
						break;
				}
				if (!sameParam)
					continue;

				i++;
				// If the return type is void, all we should have is nop and return.
				if (method.ReturnType.FullName == "System.Void") {
					if (!instrs[i].Is (Code.Nop) || !instrs[i].Next.Is (Code.Ret))
						continue;
				} else {
					while (i < instrs.Count &&
						(instrs[i].Is (Code.Nop) || instrs[i].Is (Code.Jmp) || instrs[i].IsStoreLocal () || instrs[i].IsLoadLocal ()))
						i++;

					if (!instrs[i].Is (Code.Ret))
						continue;
				}

				// If we've gotten this far, that means the code is just a call to the base method.
				// We need to check for attributes/security declarations that aren't in the
				// base.
				bool same = true;
				MethodDefinition md = mr.Resolve ();
				// If we can't resolve the definition of the original method, we can't get
				// the original attributes, so we'll say something with low confidence.
				if (md == null) {
					Runner.Report (method, Severity.Low, Confidence.Low);
					continue;
				}

				foreach (SecurityDeclaration sec in method.SecurityDeclarations)
					if (!md.SecurityDeclarations.Contains (sec))
						same = false;
				foreach (CustomAttribute attr in method.CustomAttributes)
					if (!md.CustomAttributes.Contains (attr))
						same = false;
				if (!same)
					continue;

				Runner.Report (method, Severity.Low, Confidence.High);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
