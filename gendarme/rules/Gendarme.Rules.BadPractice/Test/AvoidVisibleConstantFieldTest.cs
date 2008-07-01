// 
// Unit tests for AvoidVisibleConstantFieldRule
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

using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class AvoidVisibleConstantFieldTest : TypeRuleTestFixture<AvoidVisibleConstantFieldRule> {

		public class PublicBad {
			public const double ZeroDouble = 0.0;			// 1
			protected const int ZeroInteger = 0;			// 2
			protected const ulong MaxUInt64 = UInt64.MaxValue;	// 3

			// ok, since it can only be null
			public const object Null = null;

			// not a const
			public static readonly object ReadOnlyNullObject = null;

			private const string Message = "Ok since it's not visible";
		}

		public class PublicGood {
			public static readonly double ZeroDouble = 0.0;
			public const object Null = null;
		}

		public class PublicEmpty {
			// no fields
		}

		[Test]
		public void PublicType ()
		{
			AssertRuleFailure<PublicBad> (3);
			AssertRuleSuccess<PublicGood> ();
			AssertRuleDoesNotApply<PublicEmpty> ();
		}

		public class ProtectedBad {
			public const string Message = "Oops";		// 1

			// ok, since it can only be null
			public const ProtectedBad Reference = null;

			// not a const
			public static readonly float Pi = 3.14f;
		}

		public class ProtectedGood {
			public static readonly string Empty = String.Empty;
			public const object Null = null;
			protected int count = 1;
		}

		public class ProtectedEmpty {
			// no fields
		}

		[Test]
		public void ProtectedType ()
		{
			AssertRuleFailure<ProtectedBad> (1);
			AssertRuleSuccess<ProtectedGood> ();
			AssertRuleDoesNotApply<ProtectedEmpty> ();
		}

		private class Private {
			public const double ZeroDouble = 0.0;
		}

		private class Internal {
			protected const int ZeroInteger = 0;
		}

		[Test]
		public void NonVisibleType ()
		{
			AssertRuleDoesNotApply<Private> ();
			AssertRuleDoesNotApply<Internal> ();
		}

		[Test]
		public void Others ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Enum);
		}
	}
}
