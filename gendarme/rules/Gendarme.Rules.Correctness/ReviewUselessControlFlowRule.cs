//
// Gendarme.Rules.Correctness.ReviewUselessControlFlowRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	// rule idea credits to FindBug - http://findbugs.sourceforge.net/
	// UCF: Useless control flow to next line (UCF_USELESS_CONTROL_FLOW_NEXT_LINE)
	// UCF: Useless control flow (UCF_USELESS_CONTROL_FLOW)

	/// <summary>
	/// This rule checks for empty blocks that produce useless control flow inside IL. 
	/// This usually occurs when a block is left incomplete or when a typo is made.
	/// </summary>
	/// <example>
	/// Bad example (empty):
	/// <code>
	/// if (x == 0) {
	///	// TODO - ever seen such a thing ? ;-)
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (typo):
	/// <code>
	/// if (x == 0); {
	///	Console.WriteLine ("always printed");
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// if (x == 0) {
	///	Console.WriteLine ("printed only if x == 0");
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method contains conditional code which does not change the flow of execution.")]
	[Solution ("Verify the code logic. This is likely a typo (e.g. an extra ';') or dead code (empty condition).")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ReviewUselessControlFlowRule : Rule, IMethodRule {

		private static OpCodeBitmask Branches = new OpCodeBitmask (0x8704380000000000, 0x0, 0x0, 0x0);

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// exclude methods that don't have any conditional branches
			if (!Branches.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				switch (ins.OpCode.Code) {
				case Code.Brfalse:
				case Code.Brfalse_S:
				case Code.Brtrue:
				case Code.Brtrue_S:
				// BNE is used by [G]MCS
				case Code.Bne_Un:
				case Code.Bne_Un_S:
				case Code.Beq:
				case Code.Beq_S:
					Instruction br = (ins.Operand as Instruction);
					int delta = br.Offset - ins.Next.Offset;
					if (delta == 0) {
						// Medium: since compiler already warned about this
						Runner.Report (method, ins, Severity.Medium, Confidence.Normal);
					}  else if (delta <= 2) {
						// is the block (between the jumps) small and empty ?
						// CSC does this, probably to help the debugger.
						// [G]MCS does not
						while (delta > 0) {
							br = br.Previous;
							if (br.OpCode.Code != Code.Nop)
								break;
							delta--;
						}
						if (delta == 0)
							Runner.Report (method, ins, Severity.Low, Confidence.Normal);
					}
					break;
				}
			}
			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask branches = new OpCodeBitmask ();
			branches.Set (Code.Brfalse);
			branches.Set (Code.Brfalse_S);
			branches.Set (Code.Brtrue);
			branches.Set (Code.Brtrue_S);
			branches.Set (Code.Bne_Un);
			branches.Set (Code.Bne_Un_S);
			branches.Set (Code.Beq);
			branches.Set (Code.Beq_S);
			Console.WriteLine (branches);
		}
#endif
	}
}
