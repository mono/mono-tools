//
// Unit test for ReplaceIncompleteOddnessCheckRule
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

using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class ReplaceIncompleteOddnessCheckTest : MethodRuleTestFixture<ReplaceIncompleteOddnessCheckRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		// common, but incomplete, oddness check
		public bool IsIntOddBad (int x)
		{
			// this won't work for negative numbers (returns -1)
			return ((x % 2) == 1);
		}

		public bool IsLongOddBad (long x)
		{
			// this won't work for negative numbers (returns -1)
			return ((x % 2) == 1);
		}

		[Test]
		public void Bad ()
		{
			Assert.IsTrue (IsIntOddBad (1), "1");
			Assert.IsFalse (IsIntOddBad (-1), "-1");	// uho
			Assert.IsFalse (IsIntOddBad (2), "2");
			Assert.IsFalse (IsIntOddBad (-2), "-2");
			AssertRuleFailure<ReplaceIncompleteOddnessCheckTest> ("IsIntOddBad", 1);

			Assert.IsFalse (IsLongOddBad (Int64.MinValue), "Int64.MinValue");
			Assert.IsTrue (IsLongOddBad (Int64.MaxValue), "Int64.MaxValue");
			AssertRuleFailure<ReplaceIncompleteOddnessCheckTest> ("IsLongOddBad", 1);
		}

		public bool IsUnsignedByteOddBad (byte x)
		{
			// this can't be a negative number since it's unsigned
			return ((x % 2) == 1);
		}

		public bool IsUnsignedLongOddBad (ulong x)
		{
			// this can't be a negative number since it's unsigned
			return ((x % 2) == 1);
		}

		[Test]
		public void Unsigned ()
		{
			Assert.IsFalse (IsUnsignedByteOddBad (Byte.MinValue), "Byte.MinValue");
			Assert.IsTrue (IsUnsignedByteOddBad (Byte.MaxValue), "Byte.MaxValue");
			AssertRuleFailure<ReplaceIncompleteOddnessCheckTest> ("IsUnsignedByteOddBad", 1);

			Assert.IsFalse (IsUnsignedLongOddBad (UInt64.MinValue), "UInt64.MinValue");
			Assert.IsTrue (IsUnsignedLongOddBad (UInt64.MaxValue), "UInt64.MaxValue");
			AssertRuleFailure<ReplaceIncompleteOddnessCheckTest> ("IsUnsignedLongOddBad", 1);
		}

		// fixed version, i.e. what the developer expected
		public bool IsOddGood (int x)
		{
			return ((x % 2) != 0);
		}

		[Test]
		public void Good ()
		{
			Assert.IsTrue (IsOddGood (1), "1");
			Assert.IsTrue (IsOddGood (-1), "-1");
			Assert.IsFalse (IsOddGood (2), "2");
			Assert.IsFalse (IsOddGood (-2), "-2");
			AssertRuleSuccess<ReplaceIncompleteOddnessCheckTest> ("IsOddGood");
		}

		// better version, without modulo, that works on all integer
		public bool IsOddBest (int x)
		{
			return ((x & 1) == 1);
		}

		[Test]
		public void Best ()
		{
			Assert.IsTrue (IsOddBest (1), "1");
			Assert.IsTrue (IsOddBest (-1), "-1");
			Assert.IsFalse (IsOddBest (2), "2");
			Assert.IsFalse (IsOddBest (-2), "-2");
			// no REM[_UN] instruction is used so the rule does not apply
			AssertRuleDoesNotApply <ReplaceIncompleteOddnessCheckTest> ("IsOddBest");
		}

		public bool IsEvenGood (int x)
		{
			return ((x % 2) == 0);
		}

		[Test]
		public void EvenGood ()
		{
			Assert.IsFalse (IsEvenGood (1), "1");
			Assert.IsFalse (IsEvenGood (-1), "-1");
			Assert.IsTrue (IsEvenGood (2), "2");
			Assert.IsTrue (IsEvenGood (-2), "-2");
			AssertRuleSuccess<ReplaceIncompleteOddnessCheckTest> ("IsEvenGood");
		}

		public bool IsEvenBad (int x)
		{
			return ((x % 2) != 1);
		}

		[Test]
		public void EvenBad ()
		{
			Assert.IsFalse (IsEvenBad (1), "1");
			Assert.IsTrue (IsEvenBad (-1), "-1"); // uho
			Assert.IsTrue (IsEvenBad (2), "2");
			Assert.IsTrue (IsEvenBad (-2), "-2");
			AssertRuleFailure<ReplaceIncompleteOddnessCheckTest> ("IsEvenBad");
		}

		public bool ModuloThree (int x)
		{
			return ((x % 3) == 0);
		}

		public bool Compare (int x)
		{
			return ((x % 2) >= 1);
		}

		public bool SByteMax (long x)
		{
			return ((x % SByte.MaxValue) == 1);
		}

		public bool Int64Max (long x)
		{
			return ((x % Int64.MaxValue) == 1);
		}

		public bool Int32Max (int x)
		{
			return ((x % Int32.MaxValue) == 1);
		}

		[Test]
		public void NonOddnessVariations ()
		{
			AssertRuleSuccess<ReplaceIncompleteOddnessCheckTest> ("ModuloThree");
			AssertRuleSuccess<ReplaceIncompleteOddnessCheckTest> ("Compare");
			AssertRuleSuccess<ReplaceIncompleteOddnessCheckTest> ("SByteMax");
			AssertRuleSuccess<ReplaceIncompleteOddnessCheckTest> ("Int64Max");
			AssertRuleSuccess<ReplaceIncompleteOddnessCheckTest> ("Int32Max");
		}
	}
}
