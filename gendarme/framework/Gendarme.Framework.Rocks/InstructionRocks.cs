//
// Gendarme.Framework.Rocks.InstructionRocks
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

namespace Gendarme.Framework.Rocks {

	// add Instruction extensions methods here
	// only if:
	// * you supply minimal documentation for them (xml)
	// * you supply unit tests for them
	// * they are required somewhere to simplify, even indirectly, the rules
	//   (i.e. don't bloat the framework in case of x, y or z in the future)

	/// <summary>
	/// InstructionRocks contains extensions methods for Instruction
	/// and the related collection classes.
	/// </summary>
	public static class InstructionRocks {

		/// <summary>
		/// Get the FieldDefinition associated with the Instruction.
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns>The FieldDefinition associated with the instruction 
		/// or null if the instruction does apply to fields.</returns>
		public static FieldDefinition GetField (this Instruction self)
		{
			if (self == null)
				return null;

			switch (self.OpCode.Code) {
			case Code.Ldfld:
			case Code.Ldflda:
			case Code.Ldsfld:
			case Code.Ldsflda:
			case Code.Stfld:
			case Code.Stsfld:
				return (self.Operand as FieldReference).Resolve ();
			default:
				return null;
			}
		}

		/// <summary>
		/// Return the operand of the Instruction. For macro instruction the operand is constructed.
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <param name="method">The method inside which the instruction comes from.</param>
		/// <returns>Return the operand that the non-macro version of this Instruction would have.</returns>
		public static object GetOperand (this Instruction self, MethodDefinition method)
		{
			if ((self == null) || (method == null))
				return null;

			Code code = self.OpCode.Code;
			int index;
			switch (code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				index = code - Code.Ldarg_0;
				if (!method.IsStatic) {
					index--;
					if (index < 0)
						return null;
				}
				return method.Parameters [index];
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
				return method.Body.Variables [code - Code.Ldloc_0];
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
				return method.Body.Variables [code - Code.Stloc_0];
			default:
				// TODO - complete converting macro
				if ((self.Operand == null) && (self.OpCode.OpCodeType == OpCodeType.Macro))
					throw new NotImplementedException (self.OpCode.ToString ());
				return self.Operand;
			}
		}

		/// <summary>
		/// Get the VariableDefinition associated with the Instruction.
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <param name="method">The method inside which the instruction comes from. 
		/// Needed for the macro instruction where only the variable index is available.</param>
		/// <returns>The VariableDefinition associated with the instruction 
		/// or null if the instruction does apply to local variables.</returns>
		public static VariableDefinition GetVariable (this Instruction self, MethodDefinition method)
		{
			if ((self == null) || (method == null) || !method.HasBody)
				return null;

			int index;
			switch (self.OpCode.Code) {
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
				index = self.OpCode.Code - Code.Ldloc_0;
				return method.Body.Variables [index];
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
				index = self.OpCode.Code - Code.Stloc_0;
				return method.Body.Variables [index];
			case Code.Ldloc:
			case Code.Ldloc_S:
			case Code.Ldloca:
			case Code.Ldloca_S:
			case Code.Stloc:
			case Code.Stloc_S:
				return (self.Operand as VariableDefinition);
			default:
				return null;
			}
		}

		/// <summary>
		/// Return if the Instruction is a load of a local variable (ldloc* family).
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns>True if the instruction is a load local variable, False otherwise</returns>
		public static bool IsLoadLocal (this Instruction self)
		{
			if (self == null)
				return false;

			switch (self.OpCode.Code) {
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
			case Code.Ldloc:
			case Code.Ldloc_S:
			case Code.Ldloca:
			case Code.Ldloca_S:
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Return if the Instruction is a store of a local variable (stloc* family).
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns>True if the instruction is a store local variable, False otherwise</returns>
		public static bool IsStoreLocal (this Instruction self)
		{
			if (self == null)
				return false;

			switch (self.OpCode.Code) {
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
			case Code.Stloc:
			case Code.Stloc_S:
				return true;
			default:
				return false;
			}
		}
	}
}
