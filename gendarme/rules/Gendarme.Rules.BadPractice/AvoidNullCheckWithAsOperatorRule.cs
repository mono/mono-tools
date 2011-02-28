//
// Gendarme.Rules.BadPractice.AvoidNullCheckWithAsOperatorRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// The rule will detect if a null check is done before using the <c>as</c> operator. 
	/// This null check is not needed, a <c>null</c> instance will return <c>null</c>,
	/// and the code will need to deal with <c>as</c> returning a null value anyway.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public string AsString (object obj)
	/// {
	///	return (o == null) ? null : o as string;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public string AsString (object obj)
	/// {
	///	return (o as string);
	/// }
	/// </code>
	/// </example>
	// as suggested in https://bugzilla.novell.com/show_bug.cgi?id=651305
	[Problem ("An unneeded null check is done before using the 'as' operator.")]
	[Solution ("Remove the extraneous null check")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class AvoidNullCheckWithAsOperatorRule : Rule, IMethodRule {

		OpCodeBitmask mask = new OpCodeBitmask (0x100000, 0x10000000000000, 0x0, 0x0);

		static bool CheckFalseBranch (Instruction ins)
		{
			Instruction next = ins.Next;
			if (!next.Is (ins.Previous.OpCode.Code))
				return false;

			if (!(ins.Operand as Instruction).Is (Code.Ldnull))
				return false;

			return CheckIsinst (next.Next);
		}

		static bool CheckTrueBranch (Instruction ins)
		{
			if (!ins.Next.Is (Code.Ldnull))
				return false;

			Instruction br = (ins.Operand as Instruction);
			if (ins.Previous.OpCode.Code != br.OpCode.Code)
				return false;

			return CheckIsinst (br.Next);
		}

		static bool CheckIsinst (Instruction ins)
		{
			if (!ins.Is (Code.Isinst))
				return false;
			return (ins.Next.OpCode.FlowControl != FlowControl.Cond_Branch);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule only applies to methods with IL...
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// and when the IL contains both a isinst and ldnull
			if (!mask.IsSubsetOf (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				bool detected = false;
				switch (ins.OpCode.Code) {
				case Code.Brfalse_S:
				case Code.Brfalse:
					detected = CheckFalseBranch (ins);
					break;
				case Code.Brtrue_S:
				case Code.Brtrue:
					detected = CheckTrueBranch (ins);
					break;
				}
				if (detected)
					Runner.Report (method, ins, Severity.Medium, Confidence.Normal);
			}

			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask mask = new OpCodeBitmask ();
			mask.Set (Code.Isinst);
			mask.Set (Code.Ldnull);
			Console.WriteLine (mask);
		}
#endif
	}
}
