//
// Unit test for ReviewSelfAssignmentTest
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

#pragma warning disable 169, 1717

	[TestFixture]
	public class ReviewSelfAssignmentTest : MethodRuleTestFixture<ReviewSelfAssignmentRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no store instruction
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		int x;

		public void InstanceField ()
		{
			// note: C# compilers will *warn* about this
			// warning CS1717: Assignment made to same variable; did you mean to assign something else?
			x = x;
		}

		struct Struct {
			public long Field;
		}

		Struct s1, s2;

		public void DifferentInstanceField ()
		{
			s1.Field = s2.Field;
		}

		class Class : ICloneable {

			public int x;

			public Class ()
			{
			}

			public object Clone ()
			{
				Class c = new Class ();
				c.x = x;
				return c;
			}
		}

		public void SameInstanceField ()
		{
			// note: C# compilers will *warn* about this
			// warning CS1717: Assignment made to same variable; did you mean to assign something else?
			s1.Field = s1.Field;
			s2.Field = s2.Field;
		}

		private long counter;

		public void AutoIncrement ()
		{
			Console.WriteLine ("a" + (counter++).ToString ());
		}

		[Test]
		public void InstanceFields ()
		{
			AssertRuleFailure<ReviewSelfAssignmentTest> ("InstanceField", 1);
			AssertRuleFailure<ReviewSelfAssignmentTest> ("SameInstanceField", 2);
			AssertRuleSuccess<ReviewSelfAssignmentTest> ("DifferentInstanceField");
			AssertRuleSuccess<Class> ("Clone");
			AssertRuleSuccess<ReviewSelfAssignmentTest> ("AutoIncrement");
		}

		static string ss = "empty";

		public void SameStaticField ()
		{
			// note: C# compilers will *warn* about this
			// warning CS1717: Assignment made to same variable; did you mean to assign something else?
			ss = ss;
		}

		public void StaticField ()
		{
			ss = "";
		}

		[Test]
		public void StaticFields ()
		{
			AssertRuleFailure<ReviewSelfAssignmentTest> ("SameStaticField", 1);
			AssertRuleSuccess<ReviewSelfAssignmentTest> ("StaticField");
		}

		public void Variable (int y)
		{
			// note: C# compilers will *warn* about this
			// warning CS1717: Assignment made to same variable; did you mean to assign something else?
			int z = y * 3;
			z = z;
		}

		[Test]
		[Ignore ("too much false positives since compilers introduce their own variables and we can't differentiate them from user's variables")]
		public void Variables ()
		{
			AssertRuleFailure<ReviewSelfAssignmentTest> ("SameVariable", 1);
		}

		public void SameParameter (int y)
		{
			// note: C# compilers will *warn* about this
			// warning CS1717: Assignment made to same variable; did you mean to assign something else?
			y = y;
			x = y * 2;
		}

		public void Parameter (int y)
		{
			x = y * 2;
		}

		static public void StaticSameParameter (string s)
		{
			// note: C# compilers will *warn* about this
			// warning CS1717: Assignment made to same variable; did you mean to assign something else?
			s = s;
			ss += s;
		}

		static public void StaticParameter (string s)
		{
			ss += s;
		}

		[Test]
		public void Parameters ()
		{
			AssertRuleFailure<ReviewSelfAssignmentTest> ("SameParameter", 1);
			AssertRuleSuccess<ReviewSelfAssignmentTest> ("Parameter");

			AssertRuleFailure<ReviewSelfAssignmentTest> ("StaticSameParameter", 1);
			AssertRuleSuccess<ReviewSelfAssignmentTest> ("StaticParameter");
		}

		public struct Link {
			public int HashCode;
			public int Next;
		}
		private Link [] links;

		private void HashSetGood (int a, int b)
		{
			links [a].Next = links [b].Next;
		}

		private void HashSetBad_Parameter (int a)
		{
			links [a].Next = links [a].Next;
		}

		private void HashSetBad_Value ()
		{
			links [411].Next = links [411].Next;
		}

		[Test]
		public void Arrays ()
		{
			AssertRuleSuccess<ReviewSelfAssignmentTest> ("HashSetGood");
			AssertRuleFailure<ReviewSelfAssignmentTest> ("HashSetBad_Parameter");
			AssertRuleFailure<ReviewSelfAssignmentTest> ("HashSetBad_Value");
		}

		public class Chain {
			public Link [] Previous;
			public Link [] Next;
		}

		public void Close (Chain c, int pos)
		{
			c.Previous [pos].HashCode = c.Next [pos].HashCode;
		}

		public void CloseBad (Chain c, int pos)
		{
			c.Previous [pos].HashCode = c.Previous [pos].HashCode;
		}

		public void SetLinkChain (Link l, Chain c, int pos)
		{
			l.HashCode = c.Previous [pos].HashCode;
		}

		public void SetChainLink (Link l, Chain c, int pos)
		{
			c.Previous [pos].HashCode = l.HashCode;
		}

		[Test]
		public void ChainWithIndexer ()
		{
			AssertRuleSuccess<ReviewSelfAssignmentTest> ("Close");
			AssertRuleFailure<ReviewSelfAssignmentTest> ("CloseBad", 1);
			AssertRuleSuccess<ReviewSelfAssignmentTest> ("SetLinkChain");
			AssertRuleSuccess<ReviewSelfAssignmentTest> ("SetChainLink");
		}
	}
}
