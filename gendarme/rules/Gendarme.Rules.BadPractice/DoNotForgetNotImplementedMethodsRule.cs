//
// Gendarme.Rules.BadPractice.DoNotForgetNotImplementedMethodsRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2008 Cedric Vivier
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

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.BadPractice {

	[Problem ("This method look like it is not implemented or incomplete.")]
	[Solution ("Implement the method and/or make sure it's limitation are well documented.")]
	public class DoNotForgetNotImplementedMethodsRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			bool encounteredNIE = false;

			int n = 0;
			foreach (Instruction inst in method.Body.Instructions) {
				if (FlowControl.Cond_Branch == inst.OpCode.FlowControl)
					break;//if the method has a condition consider it implemented asap

				if (OpCodes.Newobj == inst.OpCode) {
					MethodReference ctor = (MethodReference) inst.Operand;
					if (".ctor" == ctor.Name 
						&& "System.NotImplementedException" == ctor.DeclaringType.FullName) {
						if (inst.Next.OpCode.Code == Code.Throw) {
							encounteredNIE = true;
							break;
						}
					}
				}
				n++;
				if (n > 10)
					break;//we assume it is implemented then
			}

			if (encounteredNIE) {
				// the defect is more severe if the method is visible outside it's assembly
				Severity severity = method.IsPublic ? Severity.High : Severity.Medium;
				Runner.Report (method, severity, Confidence.Normal);
				return RuleResult.Failure;
			}

			return RuleResult.Success;
		}
	}
}
