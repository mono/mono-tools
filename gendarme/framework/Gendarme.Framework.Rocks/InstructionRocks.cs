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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework.Helpers;

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

			if (self.OpCode.OperandType == OperandType.InlineField)
				return (self.Operand as FieldReference).Resolve ();

			return null;
		}

		/// <summary>
		/// Get the MethodReference or MethodDefinition (but not a CallSite) associated with the Instruction
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns></returns>
		/// <remarks>Older (pre 0.9) Cecil CallSite did not inherit from MethodReference so this was not an issue</remarks>
		public static MethodReference GetMethod (this Instruction self)
		{
			if ((self == null) || (self.OpCode.FlowControl != FlowControl.Call))
				return null;
			// we want to avoid InlineSig which is a CallSite (inheriting from MethodReference) 
			// but without a DeclaringType
			if (self.OpCode.OperandType != OperandType.InlineMethod)
				return null;

			return (self.Operand as MethodReference);
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
			switch (code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
			case Code.Starg:
			case Code.Starg_S:
				ParameterDefinition p = self.GetParameter (method);
				// handle 'this' for instance methods
				if (p == null)
					return method.DeclaringType;
				return p;
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
				return self.GetVariable (method);
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
			case Code.Ldc_I4_S:
				return (int) (sbyte) self.Operand;
			default:
				return self.Operand;
			}
		}

		/// <summary>
		/// Return the type associated with the instruction's operand (INCOMPLETE).
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <param name="method">The method inside which the instruction comes from.</param>
		/// <returns>Return a TypeReference compatible with the instruction operand or null.</returns>
		public static TypeReference GetOperandType (this Instruction self, MethodDefinition method)
		{
			if ((self == null) || (method == null))
				return null;

			Code code = self.OpCode.Code;
			switch (code) {
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarga:
			case Code.Ldarga_S:
			case Code.Starg:
			case Code.Starg_S:
				ParameterDefinition pd = self.GetParameter (method);
				// special case for 'this'
				return pd == null ? method.DeclaringType : pd.ParameterType;
			case Code.Conv_R4:
			case Code.Ldc_R4:
			case Code.Ldelem_R4:
			case Code.Ldind_R4:
			case Code.Stelem_R4:
			case Code.Stind_R4:
				return PrimitiveReferences.GetSingle (method);
			case Code.Conv_R8:
			case Code.Ldc_R8:
			case Code.Ldelem_R8:
			case Code.Ldind_R8:
			case Code.Stelem_R8:
				return PrimitiveReferences.GetDouble (method);
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
			case Code.Ldloc:
			case Code.Ldloc_S:
			case Code.Ldloca:
			case Code.Ldloca_S:
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
			case Code.Stloc:
			case Code.Stloc_S:
				return self.GetVariable (method).VariableType;
			case Code.Ldfld:
			case Code.Ldflda:
			case Code.Ldsfld:
			case Code.Ldsflda:
			case Code.Stfld:
			case Code.Stsfld:
				return (self.Operand as FieldReference).FieldType;
			case Code.Call:
			case Code.Callvirt:
			case Code.Newobj:
				return (self.Operand as MethodReference).ReturnType;
			default:
				return null;
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
				// will not be reached if no parameters exists (hence it won't allocate empty collections)
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
		/// (needed for StackBehaviour.Varpop).</param>
		/// <returns>The number of value removed (pop) from the stack for this instruction.</returns>
		public static int GetPopCount (this Instruction self, IMethodSignature method)
		{
			if (self == null)
				throw new ArgumentException ("self");
			if (method == null)
				throw new ArgumentException ("method");

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
					return method.ReturnType.IsNamed ("System", "Void") ? 0 : 1;

				case FlowControl.Call:
					IMethodSignature calledMethod = (IMethodSignature) self.Operand;
					// avoid allocating empty ParameterDefinitionCollection
					int n = calledMethod.HasParameters ? calledMethod.Parameters.Count : 0;
					if (self.OpCode.Code != Code.Newobj) {
						if (calledMethod.HasThis)
							n++;
					}
					return n;

				default:
					throw new NotImplementedException ("Varpop not supported for this Instruction.");
				}

			case StackBehaviour.PopAll:
				return -1;
			default:
				string unknown = String.Format (CultureInfo.InvariantCulture,
					"'{0}' is not a valid value for instruction '{1}'.",
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
			if (self == null)
				throw new ArgumentException ("self");

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
					return calledMethod.ReturnType.IsNamed ("System", "Void") ? 0 : 1;

				throw new NotImplementedException ("Varpush not supported for this Instruction.");
			default:
				string unknown = String.Format (CultureInfo.InvariantCulture,
					"'{0}' is not a valid value for instruction '{1}'.",
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
				break;
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
				index = self.OpCode.Code - Code.Stloc_0;
				break;
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
			return method.Body.Variables [index];
		}

		/// <summary>
		/// Helper method to avoid patterns like "ins.Previous != null &amp;&amp; ins.Previous.OpCode.Code == Code.Newobj"
		/// and replace it with a shorter "ins.Previous.Is (Code.Newobj)".
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <param name="code">The Code to compare to.</param>
		/// <returns>True if the instruction's code match the specified argument, False otherwise</returns>
		public static bool Is (this Instruction self, Code code)
		{
			if (self == null)
				return false;
			return (self.OpCode.Code == code);
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

			return OpCodeBitmask.LoadArgument.Get (self.OpCode.Code);
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

			return OpCodeBitmask.LoadElement.Get (self.OpCode.Code);
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

			return OpCodeBitmask.LoadIndirect.Get (self.OpCode.Code);
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

			return OpCodeBitmask.LoadLocal.Get (self.OpCode.Code);
		}

		/// <summary>
		/// Determine if the instruction operand contains the constant zero (0).
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <returns>True if the operand contains the constant zero (0), False otherwise</returns>
		public static bool IsOperandZero (this Instruction self)
		{
			if (self == null)
				return false;

			switch (self.OpCode.Code) {
			case Code.Ldc_I4:
				return ((int) self.Operand == 0);
			case Code.Ldc_I4_S:
				return ((sbyte) self.Operand == 0);
			case Code.Ldc_I4_0:
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

			return OpCodeBitmask.StoreArgument.Get (self.OpCode.Code);
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

			return OpCodeBitmask.StoreLocal.Get (self.OpCode.Code);
		}

		/// <summary>
		/// Return the instruction that match the current instruction. This is computed by 
		/// substracting push and adding pop counts until the total becomes zero.
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <param name="method">The method from which the instruction was extracted.</param>
		/// <returns>The instruction that match the current instruction.</returns>
		public static Instruction TraceBack (this Instruction self, IMethodSignature method)
		{
			return TraceBack (self, method, 0);
		}

		/// <summary>
		/// Return the instruction that match the current instruction. This is computed by 
		/// substracting push and adding pop counts until the total becomes zero.
		/// </summary>
		/// <param name="self">The Instruction on which the extension method can be called.</param>
		/// <param name="method">The method from which the instruction was extracted.</param>
		/// <param name="offset">Offset to add the the Pop count. Useful to track several parameters to a method.</param>
		/// <returns>The instruction that match the current instruction.</returns>
		public static Instruction TraceBack (this Instruction self, IMethodSignature method, int offset)
		{
			int n = offset + self.GetPopCount (method);
			while (n > 0 && self.Previous != null) {
				self = self.Previous;
				// we cannot "simply" trace backward over a unconditional branch
				if (self.OpCode.FlowControl == FlowControl.Branch)
					return null;
				n -= self.GetPushCount ();
				if (n == 0)
					return self;
				int pop = self.GetPopCount (method);
				if (pop == -1)
					return null; // PopAll
				n += pop;
			}
			return null;
		}
	}
}
