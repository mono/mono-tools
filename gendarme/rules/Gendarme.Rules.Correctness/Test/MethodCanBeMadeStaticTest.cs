//
// Unit tests for MethodCanBeMadeStatic rule
//
// Authors:
//	Jb Evain <jbevain@gmail.com>
//
// Copyright (C) 2007 Jb Evain
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
using System.Diagnostics;

using Gendarme.Framework;
using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class MethodCanBeMadeStaticTest : MethodRuleTestFixture<MethodCanBeMadeStaticRule> {

		public class Item {

			public int Foo ()
			{
				return 42;
			}

			public int _bar;

			public int Bar ()
			{
				return _bar = 42;
			}

			public static int Baz ()
			{
				return 42;
			}

			public virtual void Gazonk ()
			{
			}

			public void OnItemBang (object sender, EventArgs ea)
			{
			}
		}

		[Test]
		public void TestGoodCandidate ()
		{
			AssertRuleFailure<Item> ("Foo");
			Assert.AreEqual (1, Runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNotGoodCandidate ()
		{
			AssertRuleSuccess<Item> ("Bar");
			AssertRuleDoesNotApply<Item> ("Baz");
			AssertRuleDoesNotApply<Item> ("Gazonk");
			AssertRuleDoesNotApply<Item> ("OnItemBang");
			Assert.AreEqual (0, Runner.Defects.Count, "Count");
		}

		[Conditional ("DO_NOT_DEFINE")]
		void WriteLine (string s)
		{
			Console.WriteLine (s);
		}

		[Test]
		public void ConditionalCode ()
		{
			AssertRuleDoesNotApply<MethodCanBeMadeStaticTest> ("WriteLine");
		}
	}
}
