//
// Gendarme.Framework.StackEntryUsageResult
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// Represents a usage of a StackEntry
	/// </summary>
	public struct StackEntryUsageResult : IEquatable<StackEntryUsageResult> {

		/// <summary>
		/// The instruction that uses the StackEntry
		/// </summary>
		public readonly Instruction Instruction;

		/// <summary>
		/// The positive offset of the StackEntry before the instruction executes. 0 means right on top.
		/// </summary>
		public readonly int StackOffset;

		public StackEntryUsageResult (Instruction ins, int offset)
		{
			this.Instruction = ins;
			this.StackOffset = offset;
		}

		public override bool Equals (object obj)
		{
			if (obj is StackEntryUsageResult)
				return Equals ((StackEntryUsageResult) obj);
			return false;
		}

		public bool Equals (StackEntryUsageResult other)
		{
			return (Instruction == other.Instruction) && (StackOffset == other.StackOffset);
		}

		public override int GetHashCode ()
		{
			return Instruction.GetHashCode () ^ StackOffset;
		}

		public static bool operator == (StackEntryUsageResult left, StackEntryUsageResult right)
		{
			return left.Equals (right);
		}

		public static bool operator != (StackEntryUsageResult left, StackEntryUsageResult right)
		{
			return !left.Equals (right);
		}
	}
}
