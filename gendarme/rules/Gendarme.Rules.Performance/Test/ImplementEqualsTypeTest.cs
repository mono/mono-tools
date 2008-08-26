//
// Unit tests for ImplementEqualsTypeRule
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Performance {

	[TestFixture]
	public class ImplementEqualsTypeTest : TypeRuleTestFixture<ImplementEqualsTypeRule> {

		[Test]
		public void Enum ()
		{
			// rule does not apply to enums
			AssertRuleDoesNotApply (SimpleTypes.Enum);
		}

		public struct StructWithoutEqualsObject {
			int i;
		}

		public struct StructWithEqualsObject {
			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}

			// only to avoid compiler warnings - not part of the rule/test
			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
		}

		public struct StructWithEqualsType {
			public bool Equals (StructWithEqualsType type)
			{
				return Equals (type);
			}
		}

		public struct StructWithBothEquals {
			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}

			public bool Equals (StructWithBothEquals type)
			{
				return Equals (type);
			}

			// only to avoid compiler warnings - not part of the rule/test
			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
		}

		[Test]
		public void Structure ()
		{
			AssertRuleDoesNotApply<StructWithoutEqualsObject> ();
			AssertRuleDoesNotApply<StructWithEqualsType> ();

			AssertRuleFailure<StructWithEqualsObject> ();

			AssertRuleSuccess<StructWithBothEquals> ();
		}

		public class ClassWithoutEqualsObject {
			int i;
		}

		public class ClassWithEqualsObject {
			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}

			// only to avoid compiler warnings - not part of the rule/test
			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
		}

		public class ClassWithEqualsType {
			public bool Equals (ClassWithEqualsType type)
			{
				return Equals (type);
			}
		}

		public class ClassWithBothEquals {
			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}

			public bool Equals (ClassWithBothEquals type)
			{
				return Equals (type);
			}

			// only to avoid compiler warnings - not part of the rule/test
			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
		}

		[Test]
		public void Class ()
		{
			AssertRuleDoesNotApply<ClassWithoutEqualsObject> ();
			AssertRuleDoesNotApply<ClassWithEqualsType> ();

			AssertRuleFailure<ClassWithEqualsObject> ();

			AssertRuleSuccess<ClassWithBothEquals> ();
		}

		public interface InterfaceWithoutEqualsObject {
			int GetHashCode ();
		}

		public interface InterfaceWithEqualsObject {
			bool Equals (object obj);
		}

		public interface InterfaceWithEqualsType {
			bool Equals (InterfaceWithEqualsType type);
		}

		public interface InterfaceWithBothEquals {
			bool Equals (object obj);
			bool Equals (InterfaceWithBothEquals type);
		}

		[Test]
		public void Interface ()
		{
			AssertRuleDoesNotApply<InterfaceWithoutEqualsObject> ();
			AssertRuleDoesNotApply<InterfaceWithEqualsType> ();

			AssertRuleFailure<InterfaceWithEqualsObject> ();

			AssertRuleSuccess<InterfaceWithBothEquals> ();
		}

		public class Generic<X> {

			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}

			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}

			public bool Equals (Generic<X> other)
			{
				return true;
			}
		}

		[Test]
		public void Generics ()
		{
			AssertRuleSuccess<Generic<int>> ();
		}
	}
}
