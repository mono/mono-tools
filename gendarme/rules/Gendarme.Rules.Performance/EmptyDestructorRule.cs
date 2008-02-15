//
// Gendarme.Rules.Performance.EmptyDestructorRule
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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {
	
	[Problem ("The type has an empty destructor (or Finalize method).")]
	[Solution ("Remove the empty destructor (or Finalize method) from the class.")]
	public class EmptyDestructorRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to type with a finalizer
			MethodDefinition finalizer = type.GetMethod (MethodSignatures.Finalize);
			if (finalizer == null)
				return RuleResult.DoesNotApply;

			// rule applies

			// finalizer is present, look if it has any code within it
			// i.e. look if is does anything else than calling it's base class
			foreach (Instruction ins in finalizer.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					// it's empty if we're calling the base class destructor
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
					// ignore
					break;
				default:
					// destructor isn't empty (normal)
					return RuleResult.Success;
				}
			}

			// finalizer is empty (bad / useless)
			Runner.Report (type, Severity.Medium, Confidence.Normal, "The type finalizer is empty and should be removed.");
			return RuleResult.Failure;
		}
	}
}
