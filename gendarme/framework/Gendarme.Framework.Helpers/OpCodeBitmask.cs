// 
// Gendarme.Framework.Helpers.OpCodeBitmask
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
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
//

using System;
using System.Globalization;
using System.Text;

using Mono.Cecil.Cil;

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// This is a specialized Bitmask class for the Code enumeration. 
	/// Bitmask`1 can't be used since there are more than 64 opcodes defined.
	/// </summary>
	public sealed class OpCodeBitmask : IEquatable<OpCodeBitmask> {

		ulong [] mask = new ulong [4];

		/// <summary>
		/// Create a new, empty, OpCode bitmask
		/// </summary>
		public OpCodeBitmask ()
		{
		}

		public OpCodeBitmask (OpCodeBitmask bitmask)
		{
			if (bitmask == null)
				throw new ArgumentNullException ("bitmask");

			mask [0] = bitmask.mask [0];
			mask [1] = bitmask.mask [1];
			mask [2] = bitmask.mask [2];
			mask [3] = bitmask.mask [3];
		}

		public OpCodeBitmask (ulong a, ulong b, ulong c, ulong d)
		{
			mask [0] = a;
			mask [1] = b;
			mask [2] = c;
			mask [3] = d;
		}


		public void Clear (Code code)
		{
			int index = (int) code;
			mask [index >> 6] &= ~((ulong) 1 << (index & 63));
		}

		public void ClearAll ()
		{
			mask [0] = mask [1] = mask [2] = mask [3] = 0;
		}

		public bool Get (Code code)
		{
			int index = (int) code;
			return ((mask [index >> 6] & ((ulong) 1 << (index & 63))) != 0);
		}

		public void Set (Code code)
		{
			int index = (int) code;
			mask [index >> 6] |= ((ulong) 1 << (index & 63));
		}

		public void SetAll ()
		{
			mask [0] = mask [1] = mask [2] = mask [3] = UInt64.MaxValue;
		}

		public void UnionWith (OpCodeBitmask bitmask)
		{
			if (bitmask == null)
				throw new ArgumentNullException ("bitmask");

			mask [0] |= bitmask.mask [0];
			mask [1] |= bitmask.mask [1];
			mask [2] |= bitmask.mask [2];
			mask [3] |= bitmask.mask [3];
		}

		/// <summary>
		/// Compute if an intersection exists between the bitmasks
		/// </summary>
		/// <param name="bitmask">Note: a null value is interpreted as a full (set) mask.</param>
		/// <returns>True if there is an intersection (for any opcode) between the masks, False otherwise</returns>
		public bool Intersect (OpCodeBitmask bitmask)
		{
			if (bitmask == null)
				return true;

			if ((mask [0] & bitmask.mask [0]) != 0)
				return true;
			if ((mask [1] & bitmask.mask [1]) != 0)
				return true;
			if ((mask [2] & bitmask.mask [2]) != 0)
				return true;
			return ((mask [3] & bitmask.mask [3]) != 0);
		}

		public bool IsSubsetOf (OpCodeBitmask bitmask)
		{
			if (bitmask == null)
				return true;

			if ((mask [0] & bitmask.mask [0]) != mask [0])
				return false;
			if ((mask [1] & bitmask.mask [1]) != mask [1])
				return false;
			if ((mask [2] & bitmask.mask [2]) != mask [2])
				return false;
			return ((mask [3] & bitmask.mask [3]) == mask [3]);
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as OpCodeBitmask);
		}

		public bool Equals (OpCodeBitmask other)
		{
			if (other == null)
				return false;
			return ((mask [0] == other.mask [0]) || (mask [1] == other.mask [1]) ||
				(mask [2] == other.mask [2]) || (mask [3] == other.mask [3]));
		}

		public override int GetHashCode ()
		{
			return (mask [0] ^ mask [1] ^ mask [2] ^ mask [3]).GetHashCode ();
		}

		public override string ToString ()
		{
			return String.Format (CultureInfo.InvariantCulture, "0x{0:X}:0x{1:X}:0x{2:X}:0x{3:X}", 
				mask [0], mask [1], mask [2], mask [3]);
		}


		// Common masks
		private static OpCodeBitmask all;
		private static OpCodeBitmask calls;
		private static OpCodeBitmask conversion;
		private static OpCodeBitmask load_argument;
		private static OpCodeBitmask load_element;
		private static OpCodeBitmask load_indirect;
		private static OpCodeBitmask load_local;
		private static OpCodeBitmask store_argument;
		private static OpCodeBitmask store_local;

		private static OpCodeBitmask flow_control_branch;
		private static OpCodeBitmask flow_control_return;


		/// <summary>
		/// Mask with all bits sets so it includes all opcodes (and more)
		/// </summary>
		static public OpCodeBitmask All {
			get {
				if (all == null)
					all = new OpCodeBitmask (Int64.MaxValue, Int64.MaxValue, Int64.MaxValue, Int64.MaxValue);
				return all;
			}
		}

		/// <summary>
		/// Mask that includes Call and Callvirt.
		/// Does not include Calli since it's operand is an InlineSig (not InlineMethod)
		/// </summary>
		static public OpCodeBitmask Calls {
			get {
				if (calls == null) {
#if true
					calls = new OpCodeBitmask (0x8000000000, 0x400000000000, 0x0, 0x0);
#else
					calls = new OpCodeBitmask ();
					calls.Set (Code.Call);
					calls.Set (Code.Callvirt);
#endif
				}
				return calls;
			}
		}

		/// <summary>
		/// Mask that includes Conv_* instructions
		/// </summary>
		static public OpCodeBitmask Conversion {
			get {
				if (conversion == null) {
#if false
					conversion = new OpCodeBitmask (0x0, 0x80203FC000000000, 0x400F87F8000001FF, 0x0);
#else
					conversion = new OpCodeBitmask ();
					conversion.Set (Code.Conv_I);
					conversion.Set (Code.Conv_I1);
					conversion.Set (Code.Conv_I2);
					conversion.Set (Code.Conv_I4);
					conversion.Set (Code.Conv_I8);
					conversion.Set (Code.Conv_Ovf_I);
					conversion.Set (Code.Conv_Ovf_I_Un);
					conversion.Set (Code.Conv_Ovf_I1);
					conversion.Set (Code.Conv_Ovf_I1_Un);
					conversion.Set (Code.Conv_Ovf_I2);
					conversion.Set (Code.Conv_Ovf_I2_Un);
					conversion.Set (Code.Conv_Ovf_I4);
					conversion.Set (Code.Conv_Ovf_I4_Un);
					conversion.Set (Code.Conv_Ovf_I8);
					conversion.Set (Code.Conv_Ovf_I8_Un);
					conversion.Set (Code.Conv_Ovf_U);
					conversion.Set (Code.Conv_Ovf_U_Un);
					conversion.Set (Code.Conv_Ovf_U1);
					conversion.Set (Code.Conv_Ovf_U1_Un);
					conversion.Set (Code.Conv_Ovf_U2);
					conversion.Set (Code.Conv_Ovf_U2_Un);
					conversion.Set (Code.Conv_Ovf_U4);
					conversion.Set (Code.Conv_Ovf_U4_Un);
					conversion.Set (Code.Conv_Ovf_U8);
					conversion.Set (Code.Conv_Ovf_U8_Un);
					conversion.Set (Code.Conv_R_Un);
					conversion.Set (Code.Conv_R4);
					conversion.Set (Code.Conv_R8);
					conversion.Set (Code.Conv_U);
					conversion.Set (Code.Conv_U1);
					conversion.Set (Code.Conv_U2);
					conversion.Set (Code.Conv_U4);
					conversion.Set (Code.Conv_U8);
#endif
				}
				return conversion;
			}
		}

		static public OpCodeBitmask LoadArgument {
			get {
				if (load_argument == null) {
#if true
					load_argument = new OpCodeBitmask (0xC03C, 0x0, 0x0, 0x180);
#else
					load_argument = new OpCodeBitmask ();
					load_argument.Set (Code.Ldarg_0);
					load_argument.Set (Code.Ldarg_1);
					load_argument.Set (Code.Ldarg_2);
					load_argument.Set (Code.Ldarg_3);
					load_argument.Set (Code.Ldarg);
					load_argument.Set (Code.Ldarg_S);
					load_argument.Set (Code.Ldarga);
					load_argument.Set (Code.Ldarga_S);
#endif
				}
				return load_argument;
			}
		}

		static public OpCodeBitmask LoadElement {
			get {
				if (load_element == null) {
#if true
					load_element = new OpCodeBitmask (0x0, 0x0, 0x100FFF000, 0x0);
#else
					load_element = new OpCodeBitmask ();
					load_element.Set (Code.Ldelem_Any);
					load_element.Set (Code.Ldelem_I);
					load_element.Set (Code.Ldelem_I1);
					load_element.Set (Code.Ldelem_I2);
					load_element.Set (Code.Ldelem_I4);
					load_element.Set (Code.Ldelem_I8);
					load_element.Set (Code.Ldelem_R4);
					load_element.Set (Code.Ldelem_R8);
					load_element.Set (Code.Ldelem_Ref);
					load_element.Set (Code.Ldelem_U1);
					load_element.Set (Code.Ldelem_U2);
					load_element.Set (Code.Ldelem_U4);
					load_element.Set (Code.Ldelema);
#endif
				}
				return load_element;
			}
		}

		static public OpCodeBitmask LoadIndirect {
			get {
				if (load_indirect == null) {
#if true
					load_indirect = new OpCodeBitmask (0x0, 0xFFE0, 0x0, 0x0);
#else
					load_indirect = new OpCodeBitmask ();
					load_indirect.Set (Code.Ldind_I);
					load_indirect.Set (Code.Ldind_I1);
					load_indirect.Set (Code.Ldind_I2);
					load_indirect.Set (Code.Ldind_I4);
					load_indirect.Set (Code.Ldind_I8);
					load_indirect.Set (Code.Ldind_R4);
					load_indirect.Set (Code.Ldind_R8);
					load_indirect.Set (Code.Ldind_Ref);
					load_indirect.Set (Code.Ldind_U1);
					load_indirect.Set (Code.Ldind_U2);
					load_indirect.Set (Code.Ldind_U4);
#endif
				}
				return load_indirect;
			}
		}

		static public OpCodeBitmask LoadLocal {
			get {
				if (load_local == null) {
#if true
					load_local = new OpCodeBitmask (0x603C0, 0x0, 0x0, 0xC00);
#else
					load_local = new OpCodeBitmask ();
					load_local.Set (Code.Ldloc_0);
					load_local.Set (Code.Ldloc_1);
					load_local.Set (Code.Ldloc_2);
					load_local.Set (Code.Ldloc_3);
					load_local.Set (Code.Ldloc);
					load_local.Set (Code.Ldloc_S);
					load_local.Set (Code.Ldloca);
					load_local.Set (Code.Ldloca_S);
#endif
				}
				return load_local;
			}
		}

		static public OpCodeBitmask StoreArgument {
			get {
				if (store_argument == null) {
#if true
					store_argument = new OpCodeBitmask (0x10000, 0x0, 0x0, 0x200);
#else
					store_argument = new OpCodeBitmask ();
					store_argument.Set (Code.Starg);
					store_argument.Set (Code.Starg_S);
#endif
				}
				return store_argument;
			}
		}

		static public OpCodeBitmask StoreLocal {
			get {
				if (store_local == null) {
#if true
					store_local = new OpCodeBitmask (0x83C00, 0x0, 0x0, 0x1000);
#else
					store_local = new OpCodeBitmask ();
					store_local.Set (Code.Stloc_0);
					store_local.Set (Code.Stloc_1);
					store_local.Set (Code.Stloc_2);
					store_local.Set (Code.Stloc_3);
					store_local.Set (Code.Stloc);
					store_local.Set (Code.Stloc_S);
#endif
				}
				return store_local;
			}
		}

		static public OpCodeBitmask FlowControlBranch {
			get {
				if (flow_control_branch == null) {
#if true
					flow_control_branch = new OpCodeBitmask (0x80040000000000, 0x0, 0x1800000000000000, 0x1000);
#else
					flow_control_branch = new OpCodeBitmask ();
					flow_control_branch.Set (Code.Br);
					flow_control_branch.Set (Code.Br_S);
					flow_control_branch.Set (Code.Leave);
					flow_control_branch.Set (Code.Leave_S);
#endif
				}
				return flow_control_branch;
			}
		}

		static public OpCodeBitmask FlowControlReturn {
			get {
				if (flow_control_return == null) {
#if true
					flow_control_return = new OpCodeBitmask (0x20000000000, 0x0, 0x400000000000000, 0x4000);
#else
					flow_control_return = new OpCodeBitmask ();
					flow_control_return.Set (Code.Ret);
					flow_control_return.Set (Code.Endfinally);
					flow_control_return.Set (Code.Endfilter);
#endif
				}
				return flow_control_return;
			}
		}

	}
}
