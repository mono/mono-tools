//
// Unit tests for AvoidMultidimensionalIndexerRule
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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class AvoidMultidimensionalIndexerTest : MethodRuleTestFixture<AvoidMultidimensionalIndexerRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		Type [] intint = new Type [] { typeof (int), typeof (int) };
		Type [] stringstring = new Type [] { typeof (string), typeof (string) };

		public int this [int index] {
			get { return 0; }
		}

		protected int this [string name] {
			get { return 0; }
		}

		public int this [int x, int y] {
			get { return 0; }
		}

		protected int this [string name, string subname] {
			get { return 0; }
		}

		[Test]
		public void Visible ()
		{
			// single argument indexers
			AssertRuleSuccess<AvoidMultidimensionalIndexerTest> ("get_Item", new Type [] { typeof (int) });
			AssertRuleSuccess<AvoidMultidimensionalIndexerTest> ("get_Item", new Type [] { typeof (string) });
			// multiple argument indexers
			AssertRuleFailure<AvoidMultidimensionalIndexerTest> ("get_Item", intint, 1);
			AssertRuleFailure<AvoidMultidimensionalIndexerTest> ("get_Item", stringstring, 1);
		}

		class NonVisibleType {

			public int this [int x, int y] {
				get { return 0; }
			}

			protected int this [string name, string subname] {
				get { return 0; }
			}
		}

		public class VisibleType {

			private int this [int x, int y] {
				get { return 0; }
			}

			internal int this [string name, string subname]	{
				get { return 0; }
			}
		}


		[Test]
		public void NonVisible ()
		{
			AssertRuleSuccess<NonVisibleType> ("get_Item", intint);
			AssertRuleSuccess<NonVisibleType> ("get_Item", stringstring);
			// visible type with non visible members
			AssertRuleSuccess<VisibleType> ("get_Item", intint);
			AssertRuleSuccess<VisibleType> ("get_Item", stringstring);
		}
	}
}
