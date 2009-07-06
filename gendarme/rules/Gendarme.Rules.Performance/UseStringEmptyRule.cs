//
// Gendarme.Rules.Performance.UseStringEmpty
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule checks for methods that are using the literal <c>""</c> instead of the
	/// <c>String.Empty</c> field. You'll get slighly better performance by using 
	/// <c>String.Empty</c>. Note that in some cases, e.g. in a <c>switch/case</c> statement,
	/// you cannot use a field, so <c>""</c> must be used instead of <c>String.Empty</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// string s = "";
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// string s = String.Empty;
	/// </code>
	/// </example>

	[Problem ("The method uses literal \"\" instead of String.Empty.")]
	[Solution ("Replace the empty string literal with String.Empty.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class UseStringEmptyRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// check if the method loads some string (Ldstr)
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Ldstr))
				return RuleResult.DoesNotApply;

			// *** ok, the rule applies! ***

			// look for string references
			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.OperandType) {
				case OperandType.InlineString:
					string s = (ins.Operand as string);
					if (s.Length == 0)
						Runner.Report (method, ins, Severity.Medium, Confidence.High);
					break;
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
