//
// Gendarme.Rules.Performance.RemoveUnneededFinalizerRule
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	// similar to findbugs
	// FI: Empty finalizer should be deleted (FI_EMPTY)
	// FI: Finalizer does nothing but call superclass finalizer (FI_USELESS)
	// FI: Finalizer only nulls fields (FI_FINALIZER_ONLY_NULLS_FIELDS)

	/// <summary>
	/// This rule looks for types that have an empty finalizer (a.k.a. destructor in C# or 
	/// <c>Finalize</c> method). Finalizers that simply set fields to null are considered to be
	/// empty because this does not help the garbage collection. You should remove the empty 
	/// finalizer to alleviate pressure on the garbage collector and finalizer thread.
	/// </summary>
	/// <example>
	/// Bad example (empty):
	/// <code>
	/// class Bad {
	///	~Bad ()
	///	{
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (only nulls fields):
	/// <code>
	/// class Bad {
	///	object o;
	///	
	///	~Bad ()
	///	{
	///		o = null;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class Good {
	///	object o;
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.2 this rule was named EmptyDestructorRule</remarks>

	[Problem ("The type has an useless (empty or only nullifying fields) finalizer.")]
	[Solution ("Remove the finalizer from this type to reduce the GC workload.")]
	[FxCopCompatibility ("Microsoft.Performance", "CA1821:RemoveEmptyFinalizers")]
	public class RemoveUnneededFinalizerRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to type with a finalizer
			MethodDefinition finalizer = type.GetMethod (MethodSignatures.Finalize);
			if (finalizer == null)
				return RuleResult.DoesNotApply;

			// rule applies

			// finalizer is present, look if it has any code within it
			// i.e. look if is does anything else than calling it's base class
			int nullify_fields = 0;
			foreach (Instruction ins in finalizer.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Callvirt:
					// it's empty if we're calling the base class finalizer
					MethodReference mr = (ins.Operand as MethodReference);
					if ((mr == null) || !mr.IsFinalizer ())
						return RuleResult.Success;
					break;
				case Code.Nop:
				case Code.Leave:
				case Code.Leave_S:
				case Code.Ldarg_0:
				case Code.Endfinally:
				case Code.Ret:
				case Code.Ldnull:
					// ignore
					break;
				case Code.Stfld:
					// considered as empty as long as it's only to nullify them
					if (ins.Previous.OpCode.Code == Code.Ldnull) {
						nullify_fields++;
						continue;
					}
					return RuleResult.Success;
				default:
					// finalizer isn't empty (normal)
					return RuleResult.Success;
				}
			}

			// finalizer is empty (bad / useless)
			string msg = nullify_fields == 0 ? String.Empty : 
				String.Format (CultureInfo.InvariantCulture, 
					"Contains {0} fields being nullified needlessly", nullify_fields);
			Runner.Report (type, Severity.Medium, Confidence.Normal, msg);
			return RuleResult.Failure;
		}
	}
}
