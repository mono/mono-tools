//
// Gendarme.Rules.BadPractice.CloneMethodShouldNotReturnNullRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	[Problem ("The implementation ICloneable.Clone () seems to return null in some circumstances.")]
	[Solution ("Return an appropriate object instead of returning null.")]
	public class CloneMethodShouldNotReturnNullRule: Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to types implementing System.ICloneable
			if (!type.Implements ("System.ICloneable"))
				return RuleResult.DoesNotApply;

			// rule applies only if a body is available (e.g. not for pinvokes...)
			MethodDefinition method = type.GetMethod (MethodSignatures.Clone);
			if ((method == null) || (!method.HasBody))
				return RuleResult.DoesNotApply;

			// FIXME: still *very* incomplete, but that will handle non-optimized code
			// from MS CSC where "return null" == "nop | ldnull | stloc.0 | br.s | ldloc.0 | ret"
			bool return_null = false;
			Instruction previous = null;
			foreach (Instruction instruction in method.Body.Instructions) {
				switch (instruction.OpCode.Code) {
				case Code.Nop:
				case Code.Constrained:
					// don't update previous
					break;
				case Code.Ldnull:
					return_null = true;
					previous = instruction;
					break;
				case Code.Br:
				case Code.Br_S:
					// don't update previous if branching to next instruction
					if (instruction.Operand != instruction.Next)
						previous = instruction;
					break;
				case Code.Ldloc_0:
					if ((previous != null) && (previous.OpCode.Code == Code.Stloc_0))
						break;
					return_null = false;
					break;
				case Code.Ret:
					if (return_null) {
						Runner.Report (method, instruction, Severity.Medium, Confidence.Normal, String.Empty);
						return RuleResult.Failure;
					}
					previous = instruction;
					break;
				case Code.Stloc_0:
					// return_null doesn't change state
				default:
					previous = instruction;
					break;
				}
			}

			return RuleResult.Success;
		}
	}
}
