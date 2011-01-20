//
// Unit tests for AvoidVisibleFieldsRule
//
// Authors:
//	Adrian Tsai <adrian_tsai@hotmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) 2007 Adrian Tsai
// Copyright (C) 2008, 2011 Novell, Inc (http://www.novell.com)
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
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

#pragma warning disable 169, 649

	[TestFixture]
	public class AvoidVisibleFieldsTest : TypeRuleTestFixture<AvoidVisibleFieldsRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		public class ClassProtectedField {
			protected int x;
		}

		public struct StructPublicField {
			public long y;
		}

		[Test]
		public void TestShouldBeCaught ()
		{
			AssertRuleFailure<ClassProtectedField> (1);
			AssertRuleFailure<ClassProtectedField> (1);
		}

		public class ShouldBeIgnored {
			internal int y;
			private int z;
		}

		[Test]
		public void TestShouldBeIgnored ()
		{
			AssertRuleSuccess<ShouldBeIgnored> ();
		}

		public class ArraySpecialCase {
			public byte [] key;
			public string [] files;
		}

		[Test]
		public void TestArraySpecialCase ()
		{
			AssertRuleFailure<ArraySpecialCase> (2);
		}

		public class StaticFieldCase {
			public static int x;
		}

		[Test]
		public void TestStaticCase ()
		{
			AssertRuleFailure<StaticFieldCase> (1);
		}

		public class ConstSpecialCase {
			public const int x = 1;
		}

		public class ReadOnlySpecialCase {
			public readonly int x = 1;
		}

		[Test]
		public void TestConstSpecialCase ()
		{
			AssertRuleSuccess<ConstSpecialCase> ();
		}

		[Test]
		public void TestReadOnlySpecialCase ()
		{
			AssertRuleSuccess<ReadOnlySpecialCase> ();
		}

		public class NestedPublic {
			public string name;
		}

		protected class NestedProtected {
			public string name;
		}

		private class NestedPrivate {
			public string name;
		}

		internal class NestedInternal {
			public string name;
		}

		[Test]
		public void TestNested ()
		{
			AssertRuleFailure<NestedPublic> (1);
			AssertRuleFailure<NestedProtected> (1);
			AssertRuleDoesNotApply<NestedPrivate> ();
			AssertRuleDoesNotApply<NestedInternal> ();
		}

		public class EventOnly {
			public event EventHandler<EventArgs> Event;
		}

		public class NonEvent {
			public /* event */ EventHandler<EventArgs> Event;
		}

		[Test]
		public void Events ()
		{
			// a backing-field name 'Event' (like the event itself) will be added
			// by the compiler but it will not be visible (it will be private)
			AssertRuleSuccess<EventOnly> ();
			// but there will be a failure if 'event' keyword is missing
			AssertRuleFailure<NonEvent> (1);
		}
	}
}
