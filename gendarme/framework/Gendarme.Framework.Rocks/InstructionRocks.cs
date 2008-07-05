//
// Gendarme.Framework.Rocks.InstructionRocks
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Andreas Noever <andreas.noever@gmail.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// (C) 2008 Andreas Noever
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
						return method.DeclaringType; // this
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
			case Code.Ldc_I4_M1:
			case Code.Ldc_I4_0:
			case Code.Ldc_I4_1:
			case Code.Ldc_I4_2:
			case Code.Ldc_I4_3:
			case Code.Ldc_I4_4:
			case Code.Ldc_I4_5:
			case Code.Ldc_I4_6:
			case Code.Ldc_I4_7:
			case Code.Ldc_I4_8:
				return (code - Code.Ldc_I4_0); 
			default:
				return self.Operand;
			}
		}

		/// <summary>
		/// Get the ParameterDefinition associated with the Instruction.
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <param name="method">The method inside which the instruction comes from. 
		/// Needed for the macro instruction where only the variable index is available.</param>
		/// <returns>The ParameterDefinition associated with the instruction 
		/// or null if the instruction does apply to arguments.</returns>
		public static ParameterDefinition GetParameter (this Instruction self, MethodDefinition method)
		{
			if ((self == null) || (method == null))
				return null;

			int index;
			switch (self.OpCode.Code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				index = self.OpCode.Code - Code.Ldarg_0;
				if (!method.IsStatic) {
					index--;
					if (index < 0)
						return null;
				}
				return method.Parameters [index];
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
			case Code.Starg:
			case Code.Starg_S:
				return (self.Operand as ParameterDefinition);
			default:
				return null;
			}
		}

		/// <summary>
		/// Get the number of values removed on the stack for this instruction.
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <param name="method">The method inside which the instruction comes from 
		/// (needed for StackBehaviour.Varpop).
		/// <returns>The number of value removed (pop) from the stack for this instruction.</returns>
		public static int GetPopCount (this Instruction self, MethodDefinition method)
		{
			switch (self.OpCode.StackBehaviourPop) {
			case StackBehaviour.Pop0:
				return 0;

			case StackBehaviour.Pop1:
			case StackBehaviour.Popi:
			case StackBehaviour.Popref:
				return 1;

			case StackBehaviour.Pop1_pop1:
			case StackBehaviour.Popi_pop1:
			case StackBehaviour.Popi_popi8:
			case StackBehaviour.Popi_popr4:
			case StackBehaviour.Popi_popr8:
			case StackBehaviour.Popref_pop1:
			case StackBehaviour.Popref_popi:
			case StackBehaviour.Popi_popi:
				return 2;

			case StackBehaviour.Popi_popi_popi:
			case StackBehaviour.Popref_popi_popi:
			case StackBehaviour.Popref_popi_popi8:
			case StackBehaviour.Popref_popi_popr4:
			case StackBehaviour.Popref_popi_popr8:
			case StackBehaviour.Popref_popi_popref:
				return 3;

			case StackBehaviour.Varpop:
				switch (self.OpCode.FlowControl) {
				case FlowControl.Return:
					return method.ReturnType.ReturnType.FullName == "System.Void" ? 0 : 1;

				case FlowControl.Call:
					IMethodSignature calledMethod = (IMethodSignature) self.Operand;
					int n = calledMethod.Parameters.Count;
					if (self.OpCode.Code != Code.Newobj) {
						if (calledMethod.HasThis)
							n++;
					}
					return n;

				default:
					throw new NotImplementedException ("Varpop not supported for this Instruction.");
				}

			case StackBehaviour.PopAll:
				throw new NotImplementedException ("PopAll not supported for this Instruction.");
			default:
				string unknown = String.Format ("'{0}' is not a valid value for instruction '{1}'.",
					self.OpCode.StackBehaviourPush, self.OpCode);
				throw new InvalidOperationException (unknown);
			}
		}

		/// <summary>
		/// Get the number of values placed on the stack by this instruction.
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns>The number of value added (push) to the stack by this instruction.</returns>
		public static int GetPushCount (this Instruction self)
		{
			switch (self.OpCode.StackBehaviourPush) {
			case StackBehaviour.Push0:
				return 0;

			case StackBehaviour.Push1:
			case StackBehaviour.Pushi:
			case StackBehaviour.Pushi8:
			case StackBehaviour.Pushr4:
			case StackBehaviour.Pushr8:
			case StackBehaviour.Pushref:
				return 1;

			case StackBehaviour.Push1_push1:
				return 2;

			case StackBehaviour.Varpush:
				IMethodSignature calledMethod = (IMethodSignature) self.Operand;
				if (calledMethod != null)
					return (calledMethod.ReturnType.ReturnType.FullName == "System.Void") ? 0 : 1;

				throw new NotImplementedException ("Varpush not supported for this Instruction.");
			default:
				string unknown = String.Format ("'{0}' is not a valid value for instruction '{1}'.",
					self.OpCode.StackBehaviourPush, self.OpCode);
				throw new InvalidOperationException (unknown);
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
		/// Return if the Instruction is a load of an argument (ldarg* family).
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns>True if the instruction is a load argument/parameter, False otherwise</returns>
		public static bool IsLoadArgument (this Instruction self)
		{
			if (self == null)
				return false;

			switch (self.OpCode.Code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Return if the Instruction is the load of an element (ldelem* family)
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns>True if the instruction is a load element, False otherwise</returns>
		public static bool IsLoadElement (this Instruction self)
		{
			if (self == null)
				return false;

			switch (self.OpCode.Code) {
			case Code.Ldelem_Any:
			case Code.Ldelem_I:
			case Code.Ldelem_I1:
			case Code.Ldelem_I2:
			case Code.Ldelem_I4:
			case Code.Ldelem_I8:
			case Code.Ldelem_R4:
			case Code.Ldelem_R8:
			case Code.Ldelem_Ref:
			case Code.Ldelem_U1:
			case Code.Ldelem_U2:
			case Code.Ldelem_U4:
			case Code.Ldelema:
				return true;
			default:
				return false;
			}
		}

		/// <summary>
		/// Return if the Instruction is a load indirect (ldind* family)
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns>True if the instruction is a load indirect, False otherwise</returns>
		public static bool IsLoadIndirect (this Instruction self)
		{
			if (self == null)
				return false;

			switch (self.OpCode.Code) {
			case Code.Ldind_I:
			case Code.Ldind_I1:
			case Code.Ldind_I2:
			case Code.Ldind_I4:
			case Code.Ldind_I8:
			case Code.Ldind_R4:
			case Code.Ldind_R8:
			case Code.Ldind_Ref:
			case Code.Ldind_U1:
			case Code.Ldind_U2:
			case Code.Ldind_U4:
				return true;
			default:
				return false;
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
		/// Return if the Instruction is a store of an argument (starg* family).
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns>True if the instruction is a store of a parameter, False otherwise</returns>
		public static bool IsStoreArgument (this Instruction self)
		{
			if (self == null)
				return false;

			switch (self.OpCode.Code) {
			case Code.Starg:
			case Code.Starg_S:
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

		/// <summary>
		/// Return the instruction that match the current instruction. This is computed by 
		/// substracting push and adding pop counts until the total becomes zero.
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <param name="method">The method from which the instruction was extracted.</param>
		/// <returns>The instruction that match the current instruction.</returns>
		public static Instruction TraceBack (this Instruction self, MethodDefinition method)
		{
			int n = self.GetPopCount (method);
			self = self.Previous;
			while (self != null) {
				n -= self.GetPushCount ();
				if (n == 0)
					return self;
				n += self.GetPopCount (method);
				self = self.Previous;
			}
			return null;
		}
	}
}
