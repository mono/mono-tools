//
// Unit test for ReviewDoubleAssignmentRule
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

using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class ReviewDoubleAssignmentTest : MethodRuleTestFixture<ReviewDoubleAssignmentRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		private string s;
		protected byte b;
		internal ulong ul1, ul2;

		public void SingleAssignmentOnField ()
		{
			s = "42";
			Console.WriteLine (s);
		}

		public void DoubleAssignmentOnField ()
		{
			s = s = "42";
			Console.WriteLine (s);
			b = b = 42;
			Console.WriteLine (b);
		}

		public void DoubleAssignementOnDifferentFields ()
		{
			ul1 = ul2 = 42;
			Console.WriteLine ("{0} - {1}", ul1, ul2);
		}

		struct Struct {
			public long Field;
		}

		public void DoubleAssignementOnDifferentInstanceFields ()
		{
			Struct s1, s2;
			s1.Field = s2.Field = 42;
			Console.WriteLine ("{0} - {1}", s1.Field, s2.Field);
		}

		[Test]
		public void Fields ()
		{
			// no DUP instruction
			AssertRuleDoesNotApply<ReviewDoubleAssignmentTest> ("SingleAssignmentOnField");
			AssertRuleFailure<ReviewDoubleAssignmentTest> ("DoubleAssignmentOnField", 2);
			AssertRuleSuccess<ReviewDoubleAssignmentTest> ("DoubleAssignementOnDifferentFields");
			AssertRuleSuccess<ReviewDoubleAssignmentTest> ("DoubleAssignementOnDifferentInstanceFields");
		}

		static DateTime dt;
		static DateTimeKind dtk;
		static short s1;
		static short s2;

		static void SingleAssignmentOnStaticField ()
		{
			dt = DateTime.UtcNow;
			Console.WriteLine (dt);
		}

		static void DoubleAssignmentOnStaticField ()
		{
			dt = dt = DateTime.UtcNow;
			Console.WriteLine (dt);
			dtk = dtk = DateTimeKind.Unspecified;
			Console.WriteLine (dtk);
		}

		public void DoubleAssignementOnDifferentStaticFields ()
		{
			s1 = s2 = 42;
			Console.WriteLine ("{0} - {1}", s1, s2);
		}

		[Test]
		public void StaticFields ()
		{
			// no DUP instruction
			AssertRuleDoesNotApply<ReviewDoubleAssignmentTest> ("SingleAssignmentOnStaticField");
			AssertRuleFailure<ReviewDoubleAssignmentTest> ("DoubleAssignmentOnStaticField", 2);
			AssertRuleSuccess<ReviewDoubleAssignmentTest> ("DoubleAssignementOnDifferentStaticFields");
		}

		public void SingleAssignmentOnLocal ()
		{
			int x = 42;
			Console.WriteLine (x);
		}

		public void DoubleAssignmentOnLocal ()
		{
			long x = x = -42;
			Console.WriteLine (x);
			ulong y = y = 42;
			Console.WriteLine (y);
		}

		public void DoubleAssignementOnDifferentLocals ()
		{
			byte b1, b2;
			b1 = b2 = 42;
			Console.WriteLine ("{0} - {1}", b1, b2);
		}

		[Test]
		public void Locals ()
		{
			// no DUP instruction
			AssertRuleDoesNotApply<ReviewDoubleAssignmentTest> ("SingleAssignmentOnLocal");
			AssertRuleFailure<ReviewDoubleAssignmentTest> ("DoubleAssignmentOnLocal", 2);
			AssertRuleSuccess<ReviewDoubleAssignmentTest> ("DoubleAssignementOnDifferentLocals");
		}

		static void SingleAssignmentOnStaticLocal ()
		{
			float f = 42.0f;
			Console.WriteLine (f);
		}

		static void DoubleAssignmentOnStaticLocal ()
		{
			double d = d = 42.0;
			Console.WriteLine (d);
		}

		static void DoubleAssignementOnDifferentStaticLocals ()
		{
			float f1, f2;
			f1 = f2 = 42.0f;
			Console.WriteLine ("{0} - {1}", f1, f2);
		}

		[Test]
		public void StaticLocals ()
		{
			// no DUP instruction
			AssertRuleDoesNotApply<ReviewDoubleAssignmentTest> ("SingleAssignmentOnStaticLocal");
			AssertRuleFailure<ReviewDoubleAssignmentTest> ("DoubleAssignmentOnStaticLocal", 1);
			AssertRuleSuccess<ReviewDoubleAssignmentTest> ("DoubleAssignementOnDifferentStaticLocals");
		}

		static byte bs;

		public void MixedDoubleAssignments ()
		{
			// instance = static = value
			b = bs = 42;
			Console.WriteLine ("{0} - {1}", b, bs);
			bs = b = 21;
			Console.WriteLine ("{0} - {1}", bs, b);
		}

		public void SingleAssignments (byte x, byte y)
		{
			b = x;
			Console.WriteLine (b);
			bs = y;
			Console.WriteLine (bs);
		}

		public void Call ()
		{
			SingleAssignments (42, 42);
		}

		[Test]
		public void Others ()
		{
			AssertRuleSuccess<ReviewDoubleAssignmentTest> ("MixedDoubleAssignments");
			// no DUP instruction
			AssertRuleDoesNotApply<ReviewDoubleAssignmentTest> ("SingleAssignments");
		}
	}
}
