// 
// Unit tests for AttributeArgumentsShouldHaveAccessorsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) Daniel Abramov
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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {
	internal class JustClass {
		public JustClass (int data) { }
	}

	internal class EmptyAttribute : Attribute {
		public EmptyAttribute () { }
	}

	internal class NearlyEmptyAttribute : Attribute {
		public NearlyEmptyAttribute () { }

		public string SomeAnotherAccessor {
			get { return "hello, world"; }
		}
	}

	internal sealed class OneAccessorMissingAttribute : Attribute {
		private string foo;
		private int bar;

		public OneAccessorMissingAttribute (string foo, int bar)
		{
			this.foo = foo;
			this.bar = bar;
		}

		public string Foo {
			get { return foo; }
		}

		public string SomeAnotherAccessor {
			get { return "hello, world"; }
		}
	}

	internal sealed class TwoAccessorsMissingAttribute : Attribute {
		private string foo;
		private int bar;

		public TwoAccessorsMissingAttribute (string foo, int bar)
		{
			this.foo = foo;
			this.bar = bar;
		}

		public string SomeAnotherAccessor {
			get { return "hello, world"; }
		}
	}


	internal sealed class NoAccessorsMissingAttribute : Attribute {
		private string foo;
		private int bar;

		public NoAccessorsMissingAttribute (string foo, int bar)
		{
			this.foo = foo;
			this.bar = bar;
		}

		public string Foo {
			get { return foo; }
		}

		public int Bar {
			get { return bar; }
		}

		public string SomeAnotherAccessor {
			get { return "hello, world"; }
		}
	}

	internal sealed class MultiConstructorNoAccessorsMissingAttribute : Attribute {
		private string foo;
		private int bar;
		private bool foobar;

		public MultiConstructorNoAccessorsMissingAttribute (string foo, int bar)
		{
			this.foo = foo;
			this.bar = bar;
		}

		public MultiConstructorNoAccessorsMissingAttribute (string foo, bool foobar)
		{
			this.foo = foo;
			this.foobar = foobar;
		}

		public string Foo {
			get { return foo; }
		}

		public int Bar {
			get { return bar; }
		}

		public bool Foobar {
			get { return foobar; }
		}

		public string SomeAnotherAccessor {
			get { return "hello, world"; }
		}
	}

	internal sealed class MultiConstructorOneAccessorMissingAttribute : Attribute {
		private string foo;
		private int bar;
		private bool foobar;

		public MultiConstructorOneAccessorMissingAttribute (string foo, int bar)
		{
			this.foo = foo;
			this.bar = bar;
		}

		public MultiConstructorOneAccessorMissingAttribute (string foo, bool foobar)
		{
			this.foo = foo;
			this.foobar = foobar;
		}

		public int Bar {
			get { return bar; }
		}

		public bool Foobar {
			get { return foobar; }
		}

		public string SomeAnotherAccessor {
			get { return "hello, world"; }
		}
	}

	internal sealed class MultiConstructorTwoAccessorsMissingAttribute : Attribute {
		private string foo;
		private int bar;
		private bool foobar;

		public MultiConstructorTwoAccessorsMissingAttribute (string foo, int bar)
		{
			this.foo = foo;
			this.bar = bar;
		}

		public MultiConstructorTwoAccessorsMissingAttribute (string foo, bool foobar)
		{
			this.foo = foo;
			this.foobar = foobar;
		}

		public bool Foobar {
			get { return foobar; }
		}

		public string SomeAnotherAccessor {
			get { return "hello, world"; }
		}
	}

	internal abstract class FooAttribute : Attribute
	{
		protected FooAttribute (string foo)
		{
			this.Foo = foo;
		}

		public string Foo
		{
			get;
			private set;
		}
	}

	internal class FooBarAttribute : FooAttribute
	{
		protected FooBarAttribute (string foo, string bar) : base (foo)
		{
			this.Bar = bar;
		}

		public string Bar
		{
			get;
			private set;
		}
	}


	[TestFixture]
	public class AttributeArgumentsShouldHaveAccessorsTest : TypeRuleTestFixture<AttributeArgumentsShouldHaveAccessorsRule> {

		[Test]
		public void TestEmptyAttribute ()
		{
			AssertRuleSuccess<EmptyAttribute> ();
		}

		[Test]
		public void TestJustClass ()
		{
			AssertRuleDoesNotApply<JustClass> ();
		}

		[Test]
		public void TestMultiConstructorNoAccessorsMissingAttribute ()
		{
			AssertRuleSuccess<MultiConstructorNoAccessorsMissingAttribute> ();
		}

		[Test]
		public void TestMultiConstructorOneAccessorMissingAttribute ()
		{
			AssertRuleFailure<MultiConstructorOneAccessorMissingAttribute> (1);
		}

		[Test]
		public void TestMultiConstructorTwoAccessorsMissingAttribute ()
		{
			AssertRuleFailure<MultiConstructorTwoAccessorsMissingAttribute> (2);
		}

		[Test]
		public void TestNearlyEmptyttribute ()
		{
			AssertRuleSuccess<NearlyEmptyAttribute> ();
		}

		[Test]
		public void TestNoAccessorsMissingAttribute ()
		{
			AssertRuleSuccess<NoAccessorsMissingAttribute> ();
		}

		[Test]
		public void TestOneAccessorMissingAttribute ()
		{
			AssertRuleFailure<OneAccessorMissingAttribute> (1);
		}

		[Test]
		public void TestTwoAccessorsMissingAttribute ()
		{
			AssertRuleFailure<TwoAccessorsMissingAttribute> (2);
		}

		[Test]
		public void TestInheritedPropertiesAttribute ()
		{
			AssertRuleSuccess<FooBarAttribute> ();
		}
	}
}
