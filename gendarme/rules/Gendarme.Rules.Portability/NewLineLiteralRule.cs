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

	[Problem ("The method use some literal values for new lines (e.g. \\r\\n) which aren't portable across operating systems.")]
	[Solution ("Replace literals with Environment.NewLine.")]
	public class NewLineLiteralRule : Rule, IMethodRule {

		private static char[] InvalidChar = { '\r', '\n' };

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// methods can be empty (e.g. p/invoke declarations)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// rule applies

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Ldstr:
					// check the string being referenced by the instruction
					string s = (ins.Operand as string);
					if (s == null)
						continue;

					if (s.IndexOfAny (InvalidChar) >= 0) {
						// make the invalid char visible on output
						s = s.Replace ("\n", "\\n");
						s = s.Replace ("\r", "\\r");
						s = String.Format ("Found string: \"{0}\"", s);
						Runner.Report (method, ins, Severity.Low, Confidence.High, s);
					}
					break;
				default:
					break;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
