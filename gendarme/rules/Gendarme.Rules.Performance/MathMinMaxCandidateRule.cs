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
using Gendarme.Framework.Rocks;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Performance {

	// suggestion from https://bugzilla.novell.com/show_bug.cgi?id=373269

	[Problem ("This method seems to include code duplicating Math.Min or Math.Max functionality.")]
	[Solution ("The JIT can inline the Math.Min and Math.Max methods which provides better performance compared to custom, inline, implementations.")]
	public class MathMinMaxCandidateRule : Rule, IMethodRule {

		// Math.[Min|Max] has overloads for Byte, Double, Int16, 
		// Int32, Int64, SByte, Single, UInt16, UInt32 and Uint64
		//
		// Note: an overload also exists for Decimal but it's 
		// unlikely than any JIT inlines it
		private static bool IsSupported (TypeReference type)
		{
			// GetOriginalType will remove the '&' for references
			switch (type.GetOriginalType ().FullName) {
			case "System.Byte":
			case "System.Double":
			case "System.Int16":
			case "System.Int32":
			case "System.Int64":
			case "System.SByte":
			case "System.Single":
			case "System.UInt16":
			case "System.UInt32":
			case "System.UInt64":
				return true;
			default:
				return false;
			}
		}

		// is it a convertion
		private static bool IsConvertion (OpCode opcode)
		{
			switch (opcode.Code) {
			case Code.Conv_I:
			case Code.Conv_I1:
			case Code.Conv_I2:
			case Code.Conv_I4:
			case Code.Conv_I8:
			case Code.Conv_Ovf_I:
			case Code.Conv_Ovf_I_Un:
			case Code.Conv_Ovf_I1:
			case Code.Conv_Ovf_I1_Un:
			case Code.Conv_Ovf_I2:
			case Code.Conv_Ovf_I2_Un:
			case Code.Conv_Ovf_I4:
			case Code.Conv_Ovf_I4_Un:
			case Code.Conv_Ovf_I8:
			case Code.Conv_Ovf_I8_Un:
			case Code.Conv_Ovf_U:
			case Code.Conv_Ovf_U_Un:
			case Code.Conv_Ovf_U1:
			case Code.Conv_Ovf_U1_Un:
			case Code.Conv_Ovf_U2:
			case Code.Conv_Ovf_U2_Un:
			case Code.Conv_Ovf_U4:
			case Code.Conv_Ovf_U4_Un:
			case Code.Conv_Ovf_U8:
			case Code.Conv_Ovf_U8_Un:
			case Code.Conv_R_Un:
			case Code.Conv_R4:
			case Code.Conv_R8:
			case Code.Conv_U:
			case Code.Conv_U1:
			case Code.Conv_U2:
			case Code.Conv_U4:
			case Code.Conv_U8:
				return true;
			default:
				return false;
			}
		}

		// indirect load of a integral or fp value (i.e. no Ldind_Ref)
		// used for ref parameters
		private static bool IsLoadIndirect (OpCode opcode)
		{
			switch (opcode.Code) {
			case Code.Ldind_I:
			case Code.Ldind_I1:
			case Code.Ldind_I4:
			case Code.Ldind_I8: // same used for U8
			case Code.Ldind_R4:
			case Code.Ldind_R8:
			case Code.Ldind_U1:
			case Code.Ldind_U2:
			case Code.Ldind_U4:
				return true;
			default:
				return false;
			}
		}

		// indirect store of a integral or fp value (i.e. no Stind_Ref)
		// used for ref and out parameters
		private static bool IsStoreIndirect (OpCode opcode)
		{
			switch (opcode.Code) {
			case Code.Stind_I:
			case Code.Stind_I1:
			case Code.Stind_I2:
			case Code.Stind_I4:
			case Code.Stind_I8:
			case Code.Stind_R4:
			case Code.Stind_R8:
				return true;
			default:
				return false;
			}
		}

		// we return strings (instead of instructions) since they will be easier
		// to compare (ldarg_1 is easy but ldfld would need more checks)
		private static string GetPrevious (MethodDefinition method, ref Instruction ins)
		{
			OpCode opcode = ins.OpCode;

			// if the opcode is a Conv_* or a Ldind_* (except for an object reference)
			if (IsConvertion (opcode) || IsLoadIndirect (opcode)) {
				ins = ins.Previous;
				return GetPrevious (method, ref ins);
			}

			switch (opcode.Code) {
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
				int index = ins.OpCode.Code - (method.HasThis ? Code.Ldarg_1 : Code.Ldarg_0);
				if (IsSupported (method.Parameters [index].ParameterType))
					return ins.OpCode.Name;
				break;
			case Code.Ldfld:
				FieldReference field = (ins.Operand as FieldReference);
				if (IsSupported (field.FieldType))
					return field.Name;
				break;
			}
			return null;
		}

		private static bool IsOk (Instruction ins)
		{
			OpCode opcode = ins.OpCode;

			// if the opcode is a Conv_*, a Ldind_* or a Stind_* (except for object references)
			if (IsConvertion (opcode) || IsLoadIndirect (opcode) || IsStoreIndirect (opcode))
				return IsOk (ins.Next);

			switch (opcode.Code) {
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
			return null;
		}

		private static bool GreaterOrLesserThan (OpCode opcode)
		{
			switch (opcode.Code) {
			case Code.Bge:
			case Code.Bge_S:
			case Code.Bge_Un:
			case Code.Bge_Un_S:
			case Code.Bgt:
			case Code.Bgt_S:
			case Code.Bgt_Un:
			case Code.Bgt_Un_S:
			case Code.Ble:
			case Code.Ble_S:
			case Code.Ble_Un:
			case Code.Ble_Un_S:
			case Code.Blt:
			case Code.Blt_S:
			case Code.Blt_Un:
			case Code.Blt_Un_S:
				return true;
			default:
				return false;
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule applies only if the method has a body
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				// skip if this is not a branch
				if (ins.OpCode.FlowControl != FlowControl.Cond_Branch)
					continue;
				// check for specific cases like: '<', '>', '<=', '>='
				if (!GreaterOrLesserThan (ins.OpCode))
					continue;

				// find the two values on stack
				Instruction current = ins.Previous;
				string op1 = GetPrevious (method, ref current);
				if (op1 == null)
					continue;
				current = current.Previous;
				string op2 = GetPrevious (method, ref current);
				if (op2 == null)
					continue;

				// check value used immediately on both sides on the branch
				string next = GetNext (method, ins.Next);
				string branch = GetNext (method, ins.Operand as Instruction);

				// if value before and after the branch match then we have a candidate
				if (((op1 == next) && (op2 == branch)) || ((op2 == next) && (op1 == branch))) {
					Runner.Report (method, ins, Severity.Medium, Confidence.Normal, String.Empty);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
