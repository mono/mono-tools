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

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule looks for complex cast operations (e.g. a <c>as</c>
	/// with a <c>null</c> check) that can be simplified using the <c>is</c> operator 
	/// (C# syntax). Note: in some case a compiler, like [g]mcs, can optimize the code and
	/// generate IL identical to a <c>is</c> operator. In this case the rule will not report 
	/// an error even if you could see one while looking the at source code.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// bool is_my_type = (my_instance as MyType) != null;
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// bool is_my_type = (my_instance is MyType);
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The method should use the \"is\" operator and avoid the cast and comparison to null.")]
	[Solution ("Replace the cast and compare to null with the simpler \"is\" operator.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class UseIsOperatorRule : Rule, IMethodRule {

		OpCodeBitmask bitmask = new OpCodeBitmask (0x100000, 0x10000000000000, 0x0, 0x1);

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// check if the method contains a Isinst, Ldnull *and* Ceq instruction
			if (!bitmask.IsSubsetOf (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			IList<Instruction> instructions = method.Body.Instructions;
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

				Runner.Report (method, instructions[i], Severity.High, Confidence.High);
			}

			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask bitmask = new OpCodeBitmask ();
			bitmask.Set (Code.Isinst);
			bitmask.Set (Code.Ldnull);
			bitmask.Set (Code.Ceq);
			Console.WriteLine (bitmask);
		}
#endif
	}
}
