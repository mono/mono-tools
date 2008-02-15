//
// Gendarme.Rules.Performance.UseIsOperatorRule class
//
// Authors:
//	Seo Sanghyeon  <sanxiyn@gmail.com>
//
// Copyright (c) 2008 Seo Sanghyeon
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

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Performance {

	[Problem ("The method should use the \"is\" operator and avoid the cast and compare to null.")]
	[Solution ("Replace the cast and compare to null with the simpler \"is\" operator.")]
	public class UseIsOperatorRule : Rule, IMethodRule {

		private const string ErrorText = "Using the 'is' operator would produce better (less) IL and would be easier to understand.";

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			bool notContainsIsOperator = false;
			InstructionCollection instructions = method.Body.Instructions;
			int n = instructions.Count - 2;
			for (int i = 0; i < n; i++) {
				Code code0 = instructions [i].OpCode.Code;
				if (code0 != Code.Isinst)
					continue;
				Code code1 = instructions [i + 1].OpCode.Code;
				if (code1 != Code.Ldnull)
					continue;
				Code code2 = instructions [i + 2].OpCode.Code;
				if (code2 != Code.Ceq)
					continue;

				notContainsIsOperator = true;
				Runner.Report (method, instructions[i], Severity.High, Confidence.High, ErrorText);	
			}

			return notContainsIsOperator? RuleResult.Failure : RuleResult.Success;
		}
	}
}
