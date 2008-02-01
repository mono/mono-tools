//
// Gendarme.Rules.BadPractice.ToStringReturnsNullRule
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
using System.Collections;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	public class ToStringReturnsNullRule: ITypeRule {

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			// rules applies to types that overrides System.Object.Equals(object)
			MethodDefinition method = type.GetMethod (MethodSignatures.ToString);
			if ((method == null) || !method.HasBody)
				return runner.RuleSuccess;

			// rule applies

			if (!CheckIfMethodReturnsNull (method))
				return runner.RuleSuccess;

			// FIXME: location
			Location location = new Location (method, -1);
			Message message = new Message ("Method 'ToString()' seems to returns null under some conditions.", location, MessageType.Error);
			return new MessageCollection (message);
		}

		private static bool CheckIfMethodReturnsNull (MethodDefinition method)
		{
			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Ret:
					switch (ins.Previous.OpCode.Code) {
					case Code.Ldarg_0:
						// very special case where String.ToString() return "this"
						return false;
					case Code.Ldloc_0:
					case Code.Ldloc_1:
					case Code.Ldloc_2:
					case Code.Ldloc_S:
						// FIXME: we could detect some NULL in there
						break;
					case Code.Newobj:
						// we're sure it's not null, e.g. new string ('!', 2)
						return false;
					case Code.Ldnull:
						// we're sure it's null
						return true;
					case Code.Ldstr:
						return (ins.Previous.Operand == null);
					case Code.Ldfld:
					case Code.Ldsfld:
						// we could be sure for read-only fields with
						//	if ((ins.Previous.Operand as FieldDefinition).IsInitOnly)
						// but since it's unlikely we can track null in them...
						// ...better not return false positives
						return false;
					case Code.Call:
					case Code.Callvirt:
						// note: calling other ToString() is safe since the rule applies to them too
						// FIXME: we could check if the method can possibly return a null string
						break;
					default:
						throw new NotSupportedException (String.Format ("{0} inside {1}", ins.Previous.OpCode.Code, method));
					}
					break;
				}
			}
			return false;
		}
	}
}
