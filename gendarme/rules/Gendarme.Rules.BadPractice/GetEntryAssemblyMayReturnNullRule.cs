// 
// Gendarme.Rules.BadPractice.GetEntryAssemblyMayReturnNullRule
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

namespace Gendarme.Rules.BadPractice {

	public class GetEntryAssemblyMayReturnNullRule : IMethodRule {

		public MessageCollection CheckMethod (MethodDefinition methodDefinition, Runner runner)
		{
			// rule doesn't not apply to methods without code (e.g. p/invokes)
			if (!methodDefinition.HasBody)
				return runner.RuleSuccess;

			AssemblyDefinition assembly = methodDefinition.DeclaringType.Module.Assembly;

			// not for executables
			if (assembly.EntryPoint != null)
				return runner.RuleSuccess;

			// go!
			MessageCollection messages = runner.RuleSuccess;
			foreach (Instruction current in methodDefinition.Body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					MethodReference mr = (current.Operand as MethodReference);
					if (mr != null && mr.Name == "GetEntryAssembly"
					    && mr.DeclaringType.FullName == "System.Reflection.Assembly") { // that's it
						Location loc = new Location (methodDefinition, current.Offset);
						Message msg = new Message ("Assembly.GetEntryAssembly () method returns null when it is called not from the root application domain.", loc, MessageType.Warning);
						if (messages == runner.RuleSuccess)
							messages = new MessageCollection (msg);
						else
							messages.Add (msg);
					}
					break;
				}
			}
			return messages;
		}
	}
}
