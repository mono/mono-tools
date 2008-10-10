//
// Unit tests for AvoidReturningArraysOnPropertiesRule
//
// Authors:
//	Adrian Tsai <adrian_tsai@hotmail.com>
//
// Copyright (c) 2007 Adrian Tsai
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

#pragma warning disable 649

	[TestFixture]
	public class AvoidReturningArraysOnPropertiesTest : MethodRuleTestFixture<AvoidReturningArraysOnPropertiesRule> {

		public class ShouldBeCaught {
			byte [] foo;

			byte [] Foo {
				get { return foo; }
			}
		}

		public class ShouldBeIgnored {
			byte [] Bar ()
			{
				return new byte [10];
			}

			int foo;
			int Foo  {
				get { return foo; }
				set { foo = value; }
			}
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ShouldBeCaught> ("get_Foo", 1);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<ShouldBeIgnored> ("get_Foo");
		}

		[Test]
		public void DoesNotApply ()
		{
			// not a property
			AssertRuleDoesNotApply<ShouldBeIgnored> ("Bar");
		}
	}
}
