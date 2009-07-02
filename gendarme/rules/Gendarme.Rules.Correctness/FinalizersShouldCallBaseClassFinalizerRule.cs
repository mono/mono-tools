// 
// Gendarme.Rules.Correctness.FinalizersShouldCallBaseClassFinalizerRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule is used to warn the developer that a finalizer does not call the base class 
	/// finalizer. In C#, this is enforced by compiler but some .NET languages (like IL)
	/// may allow such behavior.
	/// </summary>
	/// <example>
	/// Bad example (IL):
	/// <code>
	/// .assembly extern mscorlib
	/// {
	///	.ver 1:0:5000:0
	///	.publickeytoken = (B7 7A 5C 56 19 34 E0 89 )
	/// }
	/// .class public auto ansi beforefieldinit BadFinalizer extends [mscorlib]System.Object
	/// {
	///	.method family virtual hidebysig instance void Finalize() cil managed
	///	{
	///		// no base call so rule will fire here
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (C#):
	/// <code>
	/// public class GoodFinalizer {
	///	~GoodFinalizer ()
	///	{
	///		// C# compiler will insert base.Finalize () call here
	///		// so any compiler-generated code will be valid
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("The finalizer for this type does not call the base class finalizer.")]
	[Solution ("Since your language does not do this automatically, like C#, add a call to the base type finalizer just before the finalizer exits.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Usage", "CA2220:FinalizersShouldCallBaseClassFinalizer")]
	public class FinalizersShouldCallBaseClassFinalizerRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// handle System.Object (which can't call base class)
			if (type.BaseType == null)
				return RuleResult.DoesNotApply;

			MethodDefinition finalizer = type.GetMethod (MethodSignatures.Finalize);
			if (finalizer == null) // no finalizer found
				return RuleResult.DoesNotApply;

			if (IsBaseFinalizeCalled (finalizer))
				return RuleResult.Success;

			Runner.Report (finalizer, Severity.Critical, Confidence.Total);
			return RuleResult.Failure;
		}

		private static bool IsBaseFinalizeCalled (MethodDefinition finalizer)
		{
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (finalizer)))
				return false;

			foreach (Instruction current in finalizer.Body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					MethodReference mr = (current.Operand as MethodReference);
					if ((mr != null) && mr.IsFinalizer ())
						return true;
					break;
				}
			}
			return false;
		}
	}
}
