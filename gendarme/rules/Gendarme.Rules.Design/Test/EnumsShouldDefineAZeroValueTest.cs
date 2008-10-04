// 
// Unit tests for EnumsShouldDefineAZeroValueRule
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
using System.Reflection;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Design {

	enum PrivateEnumWithZeroValue {
		Zero,
		One,
		Two
	}

	internal enum InternalEnumWithoutZeroValue {
		One = 1,
		Two,
		Three
	}

	[TestFixture]
	public class EnumsShouldDefineAZeroValueTest : TypeRuleTestFixture<EnumsShouldDefineAZeroValueRule> {

		public enum NestedPublicEnumWithZeroValue {
			Zero
		}

		[Flags]
		private enum NestedInternalFlagsWithZeroValue {
			GhostBit = 0,
			FirstBit,
		}

		[Flags]
		private enum NestedPrivateFlagsWithoutZeroValue {
			FirstBit = 1,
			SecondBit = 2,
			ThirdBit = 4
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		[Test]
		public void EnumWithZeroValue ()
		{
			AssertRuleSuccess<PrivateEnumWithZeroValue> ();
			AssertRuleSuccess<NestedPublicEnumWithZeroValue> ();
		}

		[Test]
		public void EnumWithoutZeroValue ()
		{
			AssertRuleFailure<InternalEnumWithoutZeroValue> ();
		}

		[Test]
		public void FlagWithoutZeroValue ()
		{
			AssertRuleDoesNotApply<NestedPrivateFlagsWithoutZeroValue> ();
		}

		[Test]
		public void FlagWithZeroValue ()
		{
			// flags are ignored by the rule
			AssertRuleDoesNotApply<NestedInternalFlagsWithZeroValue> ();
		}
	}
}
