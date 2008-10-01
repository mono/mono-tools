// 
// Unit tests for AbstractTypesShouldNotHavePublicConstructorsRule
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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	public abstract class PublicAbstractClass {
		public abstract int X { get; }
	}

	public abstract class PublicAbstractClassWithPublicCtor {

		public PublicAbstractClassWithPublicCtor ()
		{
		}
	}

	public abstract class PublicAbstractClassWithProtectedCtor {

		protected PublicAbstractClassWithProtectedCtor ()
		{
		}
	}

	[TestFixture]
	public class AbstractTypesShouldNotHavePublicConstructorsTest : TypeRuleTestFixture<AbstractTypesShouldNotHavePublicConstructorsRule> {

		[Test]
		public void DoesNotApply ()
		{
			// non-abstract class
			AssertRuleDoesNotApply<AbstractTypesShouldNotHavePublicConstructorsTest> ();
		}

		public abstract class NestedPublicAbstractClass {
			public abstract int X { get; }
		}

		public abstract class NestedPublicAbstractClassWithPublicCtors {

			public NestedPublicAbstractClassWithPublicCtors ()
			{
			}

			public NestedPublicAbstractClassWithPublicCtors (int x)
			{
			}
		}

		public abstract class NestedPublicAbstractClassWithProtectedCtors {

			protected NestedPublicAbstractClassWithProtectedCtors ()
			{
			}

			protected NestedPublicAbstractClassWithProtectedCtors (int x)
			{
			}
		}

		[Test]
		public void WithNoConstructors ()
		{
			AssertRuleSuccess<PublicAbstractClass> ();
			AssertRuleSuccess<AbstractTypesShouldNotHavePublicConstructorsTest.NestedPublicAbstractClass> ();
		}

		[Test]
		public void WithPublicConstructors ()
		{
			AssertRuleFailure<PublicAbstractClassWithPublicCtor> (1);
			AssertRuleFailure<AbstractTypesShouldNotHavePublicConstructorsTest.NestedPublicAbstractClassWithPublicCtors> (2);
		}

		[Test]
		public void WithProtectedConstructors ()
		{
			AssertRuleSuccess<PublicAbstractClassWithProtectedCtor> ();
			AssertRuleSuccess<AbstractTypesShouldNotHavePublicConstructorsTest.NestedPublicAbstractClassWithProtectedCtors> ();
		}
	}
}
