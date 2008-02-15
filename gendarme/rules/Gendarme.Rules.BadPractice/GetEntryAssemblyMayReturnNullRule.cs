// 
// Gendarme.Rules.BadPractice.GetEntryAssemblyMayReturnNullRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.BadPractice {

	[Problem ("This method calls Assembly.GetEntryAssembly which may returns null if not called from the root application domain.")]
	[Solution ("Avoid depending on Assembly.GetEntryAssembly inside reusable code.")]
	public class GetEntryAssemblyMayReturnNullRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule doesn't not apply to methods without code (e.g. p/invokes)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// not for executables
			if (method.DeclaringType.Module.Assembly.EntryPoint != null)
				return RuleResult.DoesNotApply;

			// go!

			foreach (Instruction current in method.Body.Instructions) {
				switch (current.OpCode.Code) {
				case Code.Call:
				case Code.Calli:
				case Code.Callvirt:
					MethodReference mr = (current.Operand as MethodReference);
					if ((mr != null) && (mr.Name == "GetEntryAssembly")
						&& (mr.DeclaringType.FullName == "System.Reflection.Assembly")) {
						Runner.Report (method, current, Severity.Medium, Confidence.Total, String.Empty);
					}
					break;
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
