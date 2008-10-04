// 
// Gendarme.Rules.Design.FinalizersShouldBeProtectedRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks for every finalizers are visible to family only (protected in C#) 
	/// because otherwise they can be called from the user code. In C# and VB.NET this rule
	/// is enforced by the compiler, but some languages (like IL) may not have such a 
	/// restriction, thus making developer able to declare non-family finalizers.
	/// </summary>
	/// <example>
	/// Bad example (IL):
	/// <code>
	/// .class family auto ansi beforefieldinit BadPublicFinalizer extends
	/// [mscorlib]System.Object
	/// {
	///	.method public hidebysig instance void Finalize() cil managed
	///	{
	///		// ...
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (C#):
	/// <code>
	/// public class GoodProtectedFinalizer {
	///	// compiler makes it protected
	///	~GoodProtectedFinalizer ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (IL):
	/// <code>
	/// .class family auto ansi beforefieldinit GoodProtectedFinalizer extends
	/// [mscorlib]System.Object
	/// {
	///	.method family hidebysig instance void Finalize() cil managed
	///	{
	///		// ...
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("The finalizer for this type isn't protected (family) and is not callable only from the runtime.")]
	[Solution ("Change finalizer visibility to protected (family).")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2221:FinalizersShouldBeProtected")]
	public class FinalizersShouldBeProtectedRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			MethodDefinition finalizer = type.GetMethod (MethodSignatures.Finalize);
			if (finalizer == null) // no finalizer found
				return RuleResult.DoesNotApply;

			// good finalizer:
			if (finalizer.IsFamily && !finalizer.IsFamilyAndAssembly && !finalizer.IsFamilyOrAssembly)
				return RuleResult.Success;

			Runner.Report (finalizer, Severity.High, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
