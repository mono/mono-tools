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
	/// This rule verifies that finalizers are only visible to the type's family (e.g. protected in C#).
	/// If they are not family then they can be called from user code which could lead to
	/// problems. Note that this restriction is enforced by the C# and VB.NET compilers
	/// but other compilers may not do so.
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

	[Problem ("The finalizer for this type isn't protected (family) and can therefore be called by user level code.")]
	[Solution ("Change the finalizer visibility to protected (family).")]
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
