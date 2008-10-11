// 
// Unit tests for AvoidUnsealedUninheritedInternalClassesRule
//
// Authors:
//	Scott Peterson <lunchtimemama@gmail.com>
//
// Copyright (C) 2008 Scott Peterson
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

	public class Visible {
	}

	public class Outer {
		internal class UnsealedInner {
		}

		internal sealed class SealedInner {
		}
	}

	internal abstract class Abstract {
	}

	internal class Concrete : Abstract {
	}

	internal class Base : Abstract {
	}

	internal sealed class Final : Base {
	}

	internal sealed class Sealed {
	}

	internal class Unsealed {
	}

	[TestFixture]
	public class AvoidUnsealedUninheritedInternalTypeTest : TypeRuleTestFixture<AvoidUnsealedUninheritedInternalTypeRule>{

		[Test]
		public void TestVisable ()
		{
			AssertRuleSuccess<Visible> ();
		}

		[Test]
		public void TestUnsealedInner ()
		{
			AssertRuleFailure<Outer.UnsealedInner> (1);
		}

		[Test]
		public void TestSealedInner ()
		{
			AssertRuleSuccess<Outer.SealedInner> ();
		}

		[Test]
		public void TestAbstract ()
		{
			AssertRuleSuccess<Abstract> ();
		}

		[Test]
		public void TestConcrete ()
		{
			AssertRuleFailure<Concrete> (1);
			AssertRuleSuccess<Base> ();
		}

		[Test]
		public void TestSealed ()
		{
			AssertRuleSuccess<Sealed> ();
			AssertRuleSuccess<Final> ();
		}

		[Test]
		public void TestUnsealed ()
		{
			AssertRuleFailure<Unsealed> (1);
		}
	}
}
