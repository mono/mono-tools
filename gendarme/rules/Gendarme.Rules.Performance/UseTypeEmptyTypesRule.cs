//
// Gendarme.Rules.Performance.UseTypeEmptyTypesRule
//
// Authors:
//	Jb Evain <jbevain@novell.com>
//
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;

namespace Gendarme.Rules.Performance {

	[Problem ("The method creates an empty System.Type array instead of using Type.EmptyTypes.")]
	[Solution ("Change the array creation for Type.EmptyTypes.")]
	public class UseTypeEmptyTypesRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// look for array of Type creation
			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode != OpCodes.Newarr)
					continue;

				var type = (TypeReference) ins.Operand;

				if (type.FullName != Constants.Type)
					continue;

				var prev = ins.Previous;

				if (prev == null) // extremely unlikely
					continue;

				if ((prev.OpCode == OpCodes.Ldc_I4_0) ||
					(prev.OpCode == OpCodes.Ldc_I4 && prev.Operand == (int) 0) ||
					(prev.OpCode == OpCodes.Ldc_I4_S && prev.Operand == (byte) 0)) {

					Runner.Report (method, ins, Severity.Medium, Confidence.High, string.Empty);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
