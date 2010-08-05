// 
// Gendarme.Framework.Bitmask<T> class
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

using System;
using System.Text;

namespace Gendarme.Framework {

	/// <summary>
	/// Provide a bitmask, up to 64 bits, based on an enum or integral value
	/// (sadly this can't be restricted to enums/integral types using 'where').
	/// </summary>
	/// <typeparam name="T">Type on which the bitmask is based.</typeparam>
	public class Bitmask<T> : IEquatable<Bitmask<T>> where T : struct, IConvertible {

		ulong mask;

		/// <summary>
		/// Construct a bitmask will all bits clear.
		/// </summary>
		public Bitmask ()
		{
		}

		/// <summary>
		/// Construct a bitmask will all bits set (true) or clear (false)
		/// </summary>
		/// <param name="initialValues"></param>
		public Bitmask (bool initialValues)
		{
			if (initialValues)
				mask = UInt64.MaxValue;
		}


		// note: the GetHashCode works because both enums and integral
		// types values are identical to their hash code

		/// <summary>
		/// Clear the bit represent by the parameter.
		/// </summary>
		/// <param name="bit">Value to clear in the bitmask</param>
		public void Clear (T bit)
		{
			unchecked {
				mask &= ~((ulong) 1 << bit.ToInt32 (null));
			}
		}

		/// <summary>
		/// Clear all bits
		/// </summary>
		public void ClearAll ()
		{
			mask = 0;
		}

		public int Count ()
		{
			if (mask == 0)
				return 0;

			int count = 0;
			for (int i = 0; i < 64; i++) {
				if (((mask >> i) & 1) == 1)
					count++;
			}
			return count;
		}

		/// <summary>
		/// Get the bit represented by the parameter
		/// </summary>
		/// <param name="bit">Value to get in the bitmask</param>
		/// <returns>True if the bit is set, false otherwise.</returns>
		public bool Get (T bit)
		{
			return (((mask >> bit.ToInt32 (null)) & 1) == 1);
		}

		/// <summary>
		/// Set the bit represented by the parameter
		/// </summary>
		/// <param name="bit">Value to set in the bitmask</param>
		public void Set (T bit)
		{
			mask |= ((ulong) 1 << bit.ToInt32 (null));
		}

		/// <summary>
		/// Set all bits
		/// </summary>
		public void SetAll ()
		{
			mask = UInt64.MaxValue;
		}

		private void SetRange (int start, int end)
		{
			while (start < end) {
				mask |= ((ulong) 1 << start++);
			}
		}

		/// <summary>
		/// Set all bits, from 'value' down to 0.
		/// </summary>
		/// <param name="bit"></param>
		public void SetDown (T bit)
		{
			SetRange (0, bit.ToInt32 (null) + 1);
		}

		/// <summary>
		/// Set all bits, from 'value' up to the maximum.
		/// </summary>
		/// <param name="bit"></param>
		public void SetUp (T bit)
		{
			SetRange (bit.ToInt32 (null), 64);
		}

		/// <summary>
		/// Does this bitmask intersects with the specified bitmask.
		/// </summary>
		/// <param name="bitmask">Bitmask to compare</param>
		/// <returns>True if the bitmask intersects with the specified bitmask, False otherwise.</returns>
		public bool Intersect (Bitmask<T> bitmask)
		{
			if (bitmask == null)
				return (mask == 0);
			return ((mask & bitmask.mask) != 0);
		}

		/// <summary>
		/// Is this bitmask a subset of the specified bitmask.
		/// </summary>
		/// <param name="bitmask">Bitmask to compare (potential superset)</param>
		/// <returns>True if the bitmask is a subset of the specified bitmask, False otherwise.</returns>
		public bool IsSubsetOf (Bitmask<T> bitmask)
		{
			if (bitmask == null)
				return false;
			return ((mask & bitmask.mask) == mask);
		}

		public override bool Equals (object obj)
		{
			Bitmask<T> other = (obj as Bitmask<T>);
			return Equals (other);
		}

		public bool Equals (Bitmask<T> other)
		{
			if (other == null)
				return false;
			return (mask == other.mask);
		}

		public override int GetHashCode ()
		{
			return unchecked ((int) (mask ^ (mask >> 32)));
		}

		public override string ToString ()
		{
			if (mask == 0)
				return "0";

			StringBuilder sb = new StringBuilder ();
			bool value = false;
			for (int i = 63; i >= 0; i--) {
				if (((mask >> i) & 1) == 1) {
					sb.Append ("1");
					value = true;
				} else if (value) {
					sb.Append ("0");
				}
			}
			return sb.ToString ();
		}
	}
}
