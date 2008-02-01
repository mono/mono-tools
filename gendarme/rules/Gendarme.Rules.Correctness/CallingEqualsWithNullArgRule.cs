//
// Gendarme.Rules.Correctness.CallingEqualsWithNullArgRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using System.Collections;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Correctness {

	public class CallingEqualsWithNullArgRule: IMethodRule {

		private static bool IsEquals (MethodReference md)
		{
			if ((md == null) || (md.Name != "Equals"))
				return false;

			return (md.ReturnType.ReturnType.FullName == "System.Boolean");
		}

		private static bool IsCall (Instruction ins)
		{
			OpCode oc = ins.OpCode;
			return ((oc == OpCodes.Call) || (oc == OpCodes.Calli) || (oc == OpCodes.Callvirt));
		}

		private static bool IsPreviousLdnull (Instruction ins)
		{
			while (ins.Previous != null) {
				OpCode oc = ins.Previous.OpCode;
				if (oc == OpCodes.Ldnull) {
					return true;
				} else if ((oc == OpCodes.Constrained) || (oc == OpCodes.Nop)) {
					ins = ins.Previous;
				} else {
					return false;
				}
			}
			return false;
		}

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			if (!method.HasBody)
				return null;

			MessageCollection mc = null;
			foreach (Instruction ins in method.Body.Instructions) {
				// if we're calling bool type.Equals()
				if (IsCall (ins) && IsEquals (ins.Operand as MethodReference)) {
					// and that the previous, real, instruction is loading a null value
					if (IsPreviousLdnull (ins)) {
						Location location = new Location (method, ins.Offset);
						Message message = new Message ("You should not call Equals (null), i.e., argument should not be null", location, MessageType.Error);

						if (mc == null)
							mc = new MessageCollection (message);
						else
							mc.Add (message);
					}
				}
			}

			return mc;
		}
	}
}
