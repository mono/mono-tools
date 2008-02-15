//
// Gendarme.Rules.BadPractice.EqualShouldHandleNullArgRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

	[Problem ("This Equals method does not handle null argument as it should.")]
	[Solution ("Modify the method implementation to return false if null argument found.")]
	public class EqualShouldHandleNullArgRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rules applies to types that overrides System.Object.Equals(object)
			MethodDefinition method = type.GetMethod (MethodSignatures.Equals);
			if ((method == null) || !method.HasBody)
				return RuleResult.DoesNotApply;

			// rule applies

			// scan IL to see if null is checked and false returned
			if (HandlesNullArg (method))
				return RuleResult.Success;

			Runner.Report (method, Severity.Medium, Confidence.High, String.Empty);
			return RuleResult.Failure;
		}

		// note: not perfect, in particular when calls to other methods are used
		private static bool HandlesNullArg (MethodDefinition method)
		{
			bool this_used = false;
			bool object_used = false;
			bool null_check = false;
			bool return_value = false;

			int n = 500; // avoid endless loop
			Instruction ins = method.Body.Instructions [0];
			while ((ins != null) && (n-- > 0)) {
				switch (ins.OpCode.Code) {
				case Code.Ldarg_0:
					this_used = true;
					break;
				case Code.Ldarg_1:
					// object parameter is used
					object_used = true;
					break;
				case Code.Ldc_I4_0:
					// it's possible that Equals returns false (without any more checks)
					return_value = false;
					break;
				case Code.Ldc_I4_1:
					// it's possible that Equals returns true (without any more checks)
					return_value = true;
					break;
				case Code.Isinst:
					if (object_used)
						null_check = true;
					break;
				case Code.Brtrue:
				case Code.Brtrue_S:
					// this could be the null check after Ldarg_1
					if (object_used && !null_check) {
						null_check = true;
						// we do not branch since ldarg_1 is null
					}
					break;
				case Code.Brfalse:
				case Code.Brfalse_S:
					if (object_used || null_check) {
						// this could be the null check after Ldarg_1
						if (!null_check)
							null_check = (ins.Previous.OpCode.Code == Code.Ldarg_1);
						ins = (Instruction) ins.Operand;
						continue;
					}
					break;
				case Code.Ceq:
					// if (this == obj), this cannot be null
					if (this_used && object_used)
						null_check = true;
					break;
				case Code.Ret:
					// if we return the value from a call
					if (ins.Previous.OpCode.FlowControl == FlowControl.Call)
						return true;
					ins = null;
					continue;
				}
				ins = ins.Next;
			}
			// case #1: object was not used
			if (!object_used)
				return !return_value;
			else
				return (null_check && !return_value);
			// not sure, but we don't want to bury results with false-negative
		}
	}
}
