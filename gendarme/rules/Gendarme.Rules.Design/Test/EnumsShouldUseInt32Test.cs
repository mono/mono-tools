// 
// Unit tests for EnumsShouldUseInt32Rule
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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class EnumsShouldUseInt32Test : TypeRuleTestFixture<EnumsShouldUseInt32Rule> {

		public enum DefaultEnum {
		}

		public enum ByteEnum : byte {
		}

		public enum SignedByteEnum : sbyte {
		}

		public enum ShortEnum : short {
		}

		public enum UnsignedShortEnum : ushort {
		}

		public enum IntEnum : int {
		}

		public enum UnsignedIntEnum : uint {
		}

		public enum LongEnum : long {
		}

		public enum UnsignedLongEnum : ulong {
		}

		[Test]
		public void NotAnEnum ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		[Test]
		public void Ok ()
		{
			AssertRuleSuccess<DefaultEnum> ();
			AssertRuleSuccess<IntEnum> ();
		}

		[Test]
		public void Bad ()
		{
			// CLS compliant types: Byte, Int16 or Int64 (Int32 is CLS but Ok)
			AssertRuleFailure<ByteEnum> (1);
			AssertRuleFailure<ShortEnum> (1);
			AssertRuleFailure<LongEnum> (1);
		}

		[Test]
		public void ReallyBad ()
		{
			// i.e. using non-CLS compliant types, SByte, UInt16, UInt32 or UInt64
			AssertRuleFailure<SignedByteEnum> (1);
			AssertRuleFailure<UnsignedShortEnum> (1);
			AssertRuleFailure<UnsignedIntEnum> (1);
			AssertRuleFailure<UnsignedLongEnum> (1);
		}
	}
}
