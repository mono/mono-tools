// 
// Gendarme.Rules.Design.FinalizersShouldCallBaseClassFinalizerRule
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
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	public class FinalizersShouldCallBaseClassFinalizerRule : ITypeRule {

		public MessageCollection CheckType (TypeDefinition typeDefinition, Runner runner)
		{
			// handle System.Object (which can't call base class)
			if (typeDefinition.BaseType == null)
				return runner.RuleSuccess;

			MethodDefinition finalizer = typeDefinition.GetMethod (MethodSignatures.Finalize);

			if (finalizer == null) // no finalizer found
				return runner.RuleSuccess;

			if (IsBaseFinalizeCalled (finalizer))
				return runner.RuleSuccess;

			Location loc = new Location (finalizer);
			Message msg = new Message ("Base class finalizer must be called just before the method exits.", loc, MessageType.Error);
			return new MessageCollection (msg);
		}

		private static bool IsBaseFinalizeCalled (MethodDefinition finalizer)
		{
			foreach (Instruction current in finalizer.Body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
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
