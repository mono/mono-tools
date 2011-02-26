// 
// Gendarme.Rules.Exceptions.NewExceptionsRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Exceptions {

	[EngineDependency (typeof (OpCodeEngine))]
	abstract public class NewExceptionsRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);
			// if the module does not reference any of these types, don't analyze it
			// (unless this is corlib itself since they're defined in it :-)
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentAssembly.Name.Name == "mscorlib" ||
					e.CurrentModule.AnyTypeReference ((TypeReference tr) => {
						return CheckException (tr);
					}));
			};
		}

		abstract protected bool CheckException (TypeReference type);
		abstract protected Severity Severity { get; }

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// if method has no IL, the rule doesn't apply
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// and when the IL contains a NewObj instruction
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Newobj))
				return RuleResult.DoesNotApply;

			// look for newobj instructions
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.Code != Code.Newobj)
					continue;

				// obtain a reference to the constructor's type
				TypeReference ctype = (ins.Operand as MethodReference).DeclaringType;

				// report a defect if an offending exception type is found
				if (CheckException (ctype))
					Runner.Report (method, ins, Severity, Confidence.High, ctype.Name);
			}

			return Runner.CurrentRuleResult;
		}
	}
}
