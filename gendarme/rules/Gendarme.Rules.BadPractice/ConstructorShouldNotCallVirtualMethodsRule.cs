// 
// Gendarme.Rules.BadPractice.ConstructorShouldNotCallVirtualMethodsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
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
using System.Collections.Generic;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	public class ConstructorShouldNotCallVirtualMethodsRule : ITypeRule {

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			// sealed classes are ok
			if (type.IsSealed)
				return runner.RuleSuccess;

			MessageCollection messages = runner.RuleSuccess;

			// check each constructor
			foreach (MethodDefinition constructor in type.Constructors) {
				// early checks to avoid stack creation
				if (constructor.IsStatic || !constructor.HasBody)
					continue;

				CheckConstructor (constructor, ref messages);
			}
			return messages;
		}

		private static void CheckConstructor (MethodDefinition constructor, ref MessageCollection messages)
		{
			Stack<string> stack = new Stack<string> ();
			CheckMethod (constructor, ref messages, stack);
		}

		private static bool IsSubsclass (TypeReference sub, TypeReference type)
		{
			while (type != null) {
				if (sub == type)
					return true;
				TypeDefinition td = (type as TypeDefinition);
				type = (td == null) ? null : td.BaseType;
			}
			return false;
		}

		private static bool IsCallFromInstance (Instruction ins, int parameters)
		{
			while ((ins != null) && (parameters >= 0)) {
				if ((ins.OpCode.Code == Code.Ldarg_0) && (parameters == 0))
					return true; // this
				
				switch (ins.OpCode.StackBehaviourPush) {
				case StackBehaviour.Push0:
				case StackBehaviour.Push1:
				case StackBehaviour.Push1_push1:
				case StackBehaviour.Pushi:
				case StackBehaviour.Pushi8:
				case StackBehaviour.Pushr4:
				case StackBehaviour.Pushr8:
				case StackBehaviour.Pushref:
					parameters--;
					break;
				case StackBehaviour.Varpush:
					// call[i|virt]
					MethodReference mr = (ins.Operand as MethodReference);
					if (mr != null) {
						if (mr.HasThis)
							parameters++;
						parameters += mr.Parameters.Count;
						if (mr.ReturnType.ReturnType.FullName != "System.Void")
							parameters--;
					}
					break;
				}
				ins = ins.Previous;
			}
			return false;
		}

		private static void CheckMethod (MethodDefinition method, ref MessageCollection messages, Stack<string> stack)
		{
			if (!method.HasBody)
				return;

			string method_name = method.ToString ();
			// check to avoid constructors calling recursive methods
			if (stack.Contains (method_name))
				return;

			// check constructor for virtual method calls
			foreach (Instruction current in method.Body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					// we recurse into normal calls since they might be calling virtual methods
					MethodDefinition md = (current.Operand as MethodDefinition);
					if (md == null || md.IsConstructor || !md.HasThis)
						continue;

					// check that the method is it this class or a subclass
					if (!IsSubsclass (md.DeclaringType, method.DeclaringType))
						continue;

					// check that we're not calling the method on another object
					if (!IsCallFromInstance (current.Previous, md.Parameters.Count))
						continue;

					if (md.IsVirtual && !md.IsFinal) {
						Location loc = new Location (method, current.Offset);
						string s = stack.Count == 0 ? method.ToString () : stack.Aggregate ((a1, a2) => a1 + ", " + Environment.NewLine + a2);
						s = String.Format ("Calling a virtual method, '{0}', from {1} of a non-sealed class is a bad practice.", md, s);
						Message msg = new Message (s, loc, MessageType.Error);
						if (messages == null)
							messages = new MessageCollection (msg);
						else
							messages.Add (msg);
					} else {
						stack.Push (method_name);
						CheckMethod (md, ref messages, stack);
						stack.Pop ();
					}
					break;
				}
			}
		}
	}
}
