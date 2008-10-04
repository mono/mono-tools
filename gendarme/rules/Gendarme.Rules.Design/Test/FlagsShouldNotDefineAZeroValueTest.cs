// 
// Unit tests for FlagsShouldNotDefineAZeroValueTest
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[Flags]
	enum PrivateFlagsWithZeroValue {
		Zero = 0,
		One  = 1,
		Two  = 2
	}

	[Flags]
	internal enum InternalFlagsWithoutZeroValue {
		Zero = 1,
		One  = 2,
		Two  = 4
	}

	[TestFixture]
	public class FlagsShouldNotDefineAZeroValueTest : TypeRuleTestFixture<FlagsShouldNotDefineAZeroValueRule> {

		public enum NestedPublicEnumWithZeroValue {
			Zero
		}

		private enum NestedPrivateEnumWithoutZeroValue {
			FirstBit = 1,
			SecondBit = 2,
			ThirdBit = 4
		}

		[Flags]
		private enum NestedInternalFlagsWithZeroValue {
			GhostBit = 0,
			FirstBit,
		}

		[Test]
		public void NotAFlag ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);

			AssertRuleDoesNotApply<FlagsShouldNotDefineAZeroValueTest.NestedPublicEnumWithZeroValue> ();
			AssertRuleDoesNotApply<FlagsShouldNotDefineAZeroValueTest.NestedPrivateEnumWithoutZeroValue> ();
		}

		[Test]
		public void FlagsWithoutZeroValue ()
		{
			AssertRuleSuccess<InternalFlagsWithoutZeroValue> ();
		}

		[Test]
		public void FlagsWithZeroValue ()
		{
			AssertRuleFailure<PrivateFlagsWithZeroValue> (1);
			AssertRuleFailure<FlagsShouldNotDefineAZeroValueTest.NestedInternalFlagsWithZeroValue> (1);
		}
	}
}
