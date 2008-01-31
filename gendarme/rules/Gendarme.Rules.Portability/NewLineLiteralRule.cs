//
// Gendarme.Rules.Portability.NewLineLiteralRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006-2008 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Portability {

	public class NewLineLiteralRule: IMethodRule {

		private static char[] InvalidChar = { '\r', '\n' };

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			// methods can be empty (e.g. p/invoke declarations)
			if (!method.HasBody)
				return runner.RuleSuccess;

			// rule applies

			MessageCollection results = null;
			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Ldstr:
					// check the string being referenced by the instruction
					string s = (ins.Operand as string);
					if (s == null)
						continue;

					if (s.IndexOfAny (InvalidChar) >= 0) {
						Location loc = new Location (method, ins.Offset);
						// make the invalid char visible on output
						s = s.Replace ("\n", "\\n");
						s = s.Replace ("\r", "\\r");
						Message msg = new Message (String.Format ("Found string: \"{0}\"", s),
							loc, MessageType.Warning);

						if (results == null)
							results = new MessageCollection (msg);
						else
							results.Add (msg);
					}
					break;
				default:
					break;
				}
			}

			return results;
		}
	}
}
