//
// Gendarme.Rules.Correctness.DontCompareWithNaNRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Correctness {

	public class DontCompareWithNaNRule : IMethodRule {

		private const string EqualityMessage = "A floating point value is compared (== or !=) with [Single|Double].NaN.";
		private const string EqualsMessage = "[Single|Double].Equals is called using NaN.";

		private static bool CheckPrevious (InstructionCollection il, int index)
		{
			for (int i = index; i >= 0; i--) {
				Instruction ins = il [i];
				switch (ins.OpCode.Code) {
				case Code.Ldc_R4:
					// return false, invalid, is NaN is detected
					return !Single.IsNaN ((float) ins.Operand);
				case Code.Ldc_R8:
					// return false, invalid, is NaN is detected
					return !Double.IsNaN ((double) ins.Operand);
				case Code.Nop:
				case Code.Ldarg:
				case Code.Ldarg_1:
				case Code.Ldloca:
				case Code.Ldloca_S:
				case Code.Stloc:
				case Code.Stloc_0:
				case Code.Stloc_1:
					// continue
					break;
				default:
					return true;
				}
			}
			return true;
		}

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			MessageCollection messageCollection = null;
			if (!method.HasBody)
				return runner.RuleSuccess;

			MessageCollection mc = null;
			InstructionCollection il = method.Body.Instructions;
			for (int i = 0; i < method.Body.Instructions.Count; i++) {
				Instruction ins = il [i];
				switch (ins.OpCode.Code) {
				// handle == and !=
				case Code.Ceq:
					if (!CheckPrevious (il, i - 1)) {
						Message msg = new Message (EqualityMessage, new Location (method, ins.Offset), MessageType.Error);
						if (mc == null)
							mc = new MessageCollection (msg);
						else
							mc.Add (msg);
					}
					break;
				// handle calls to [Single|Double].Equals
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					MemberReference callee = ins.Operand as MemberReference;
					if (callee.Name.Equals ("Equals") && callee.DeclaringType.IsFloatingPoint ()) {
						if (!CheckPrevious (il, i - 1)) {
							Message msg = new Message (EqualsMessage, new Location (method, ins.Offset), MessageType.Error);
							if (mc == null)
								mc = new MessageCollection (msg);
							else
								mc.Add (msg);
						}
					}
					break;
				}
			}
			return mc;
		}
	}
}
