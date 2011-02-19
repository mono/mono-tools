//
// Gendarme.Rules.Performance.MathMinMaxCandidateRule
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

namespace Gendarme.Rules.Performance {

	// suggestion from https://bugzilla.novell.com/show_bug.cgi?id=373269

	/// <summary>
	/// This rule checks methods for code which seems to duplicate <c>Math.Min</c> or 
	/// <c>Math.Max</c>. The JIT can inline these methods and generate
	/// better code for, at least some types, than it can for a custom inline
	/// implementation.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// int max = (a > b) ? a : b;
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// int max = Math.Max (a, b);
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method seems to include code duplicating Math.Min or Math.Max functionality.")]
	[Solution ("The JIT can (sometimes) generate better code for Math.Min and Math.Max methods than it can for hand-written versions.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class MathMinMaxCandidateRule : Rule, IMethodRule {

		// see inactive code to regenerate the bitmask if needed

		static OpCodeBitmask GreaterOrLesserThan = new OpCodeBitmask (0x787BC00000000000, 0xF, 0x0, 0x0);

		// note: does not include Ldind_Ref
		static OpCodeBitmask LoadIndirect = new OpCodeBitmask (0x0, 0x7FE0, 0x0, 0x0);

		// note: does not include Ldind_Ref
		static OpCodeBitmask StoreIndirect = new OpCodeBitmask (0x0, 0x7E0000, 0x2000000000000000, 0x0);


		// Math.[Min|Max] has overloads for Byte, Double, Int16, 
		// Int32, Int64, SByte, Single, UInt16, UInt32 and Uint64
		//
		// Note: an overload also exists for Decimal but it's 
		// unlikely than any JIT inlines it
		private static bool IsSupported (TypeReference type)
		{
			if (type.Namespace != "System")
				return false;
			// GetElementType will remove the '&' for references
			TypeReference tr = type.GetElementType ();
			switch (tr.Name) {
			case "Byte":
			case "Double":
			case "Int16":
			case "Int32":
			case "Int64":
			case "SByte":
			case "Single":
			case "UInt16":
			case "UInt32":
			case "UInt64":
				return true;
			default:
				return false;
			}
		}

		// we return strings (instead of instructions) since they will be easier
		// to compare (ldarg_1 is easy but ldfld would need more checks)
		private static string GetPrevious (MethodDefinition method, ref Instruction ins)
		{
			Code code = ins.OpCode.Code;

			// if the opcode is a Conv_* or a Ldind_* (except for an object reference)
			if (OpCodeBitmask.Conversion.Get (code) || LoadIndirect.Get (code)) {
				ins = ins.Previous;
				return GetPrevious (method, ref ins);
			}

			switch (code) {
			case Code.Ldarg_0:
				if (method.HasThis) {
					ins = ins.Previous;
					return GetPrevious (method, ref ins);
				}
				if (IsSupported (method.Parameters [0].ParameterType))
					return ins.OpCode.Name;
				break;
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				int index = code - (method.HasThis ? Code.Ldarg_1 : Code.Ldarg_0);
				if (IsSupported (method.Parameters [index].ParameterType))
					return ins.OpCode.Name;
				break;
			case Code.Ldfld:
				FieldReference field = (ins.Operand as FieldReference);
				if (IsSupported (field.FieldType))
					return field.Name;
				break;
			}
			return String.Empty;
		}

		private static bool IsOk (Instruction ins)
		{
			Code code = ins.OpCode.Code;

			// if the opcode is a Conv_*, a Ldind_* or a Stind_* (except for object references)
			if (OpCodeBitmask.Conversion.Get (code) || LoadIndirect.Get (code) || StoreIndirect.Get (code))
				return IsOk (ins.Next);

			switch (code) {
			case Code.Ret:
			case Code.Stloc:
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
			case Code.Stloc_S:
				return true;
			case Code.Br:
			case Code.Br_S:
				return IsOk (ins.Operand as Instruction);
			default:
				return false;
			}
		}

		private static string GetNext (MethodDefinition method, Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Ldarg_0:
				if (method.HasThis) {
					return GetNext (method, ins.Next);
				}
				if (IsOk (ins.Next))
					return ins.OpCode.Name;
				break;
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				if (IsOk (ins.Next))
					return ins.OpCode.Name;
				break;
			case Code.Ldfld:
				FieldReference field = (ins.Operand as FieldReference);
				if (IsOk (ins.Next))
					return field.Name;
				break;
			}
			return String.Empty;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule applies only if the method has a body
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any bge*, bgt*, ble* or blt* instructions in the method ?
			if (!GreaterOrLesserThan.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				// check for specific cases like: '<', '>', '<=', '>='
				if (!GreaterOrLesserThan.Get (ins.OpCode.Code))
					continue;

				// find the two values on stack
				Instruction current = ins.Previous;
				string op1 = GetPrevious (method, ref current);
				if (op1.Length == 0)
					continue;
				current = current.Previous;
				string op2 = GetPrevious (method, ref current);
				if (op2.Length == 0)
					continue;

				// check value used immediately on both sides on the branch
				string next = GetNext (method, ins.Next);
				string branch = GetNext (method, ins.Operand as Instruction);

				// if value before and after the branch match then we have a candidate
				if (((op1 == next) && (op2 == branch)) || ((op2 == next) && (op1 == branch))) {
					Runner.Report (method, ins, Severity.Medium, Confidence.Normal);
				}
			}

			return Runner.CurrentRuleResult;
		}
#if false
		public void BuildBitmask ()
		{
			OpCodeBitmask bitmask = new OpCodeBitmask ();
			bitmask.Set (Code.Ldind_I);
			bitmask.Set (Code.Ldind_I1);
			bitmask.Set (Code.Ldind_I2);
			bitmask.Set (Code.Ldind_I4);
			bitmask.Set (Code.Ldind_I8);
			bitmask.Set (Code.Ldind_R4);
			bitmask.Set (Code.Ldind_R8);
			bitmask.Set (Code.Ldind_U1);
			bitmask.Set (Code.Ldind_U2);
			bitmask.Set (Code.Ldind_U4);
			Console.WriteLine ("LoadIndirect = {0}", bitmask);

			bitmask.ClearAll ();
			bitmask.Set (Code.Stind_I);
			bitmask.Set (Code.Stind_I1);
			bitmask.Set (Code.Stind_I2);
			bitmask.Set (Code.Stind_I4);
			bitmask.Set (Code.Stind_I8);
			bitmask.Set (Code.Stind_R4);
			bitmask.Set (Code.Stind_R8);
			Console.WriteLine ("StoreIndirect = {0}", bitmask);

			bitmask.ClearAll ();
			bitmask.Set (Code.Bge);
			bitmask.Set (Code.Bge_S);
			bitmask.Set (Code.Bge_Un);
			bitmask.Set (Code.Bge_Un_S);
			bitmask.Set (Code.Bgt);
			bitmask.Set (Code.Bgt_S);
			bitmask.Set (Code.Bgt_Un);
			bitmask.Set (Code.Bgt_Un_S);
			bitmask.Set (Code.Ble);
			bitmask.Set (Code.Ble_S);
			bitmask.Set (Code.Ble_Un);
			bitmask.Set (Code.Ble_Un_S);
			bitmask.Set (Code.Blt);
			bitmask.Set (Code.Blt_S);
			bitmask.Set (Code.Blt_Un);
			bitmask.Set (Code.Blt_Un_S);
			Console.WriteLine ("GreaterOrLesserThan = {0}", bitmask);
		}
#endif
	}
}
