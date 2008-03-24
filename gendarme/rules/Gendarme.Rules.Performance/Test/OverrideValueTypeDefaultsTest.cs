//
// Unit tests for OverrideValueTypeDefaultsRule
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
	public class OverrideValueTypeDefaultsTest : TypeRuleTestFixture<OverrideValueTypeDefaultsRule> {

		[Test]
		public void Class ()
		{
			// rule does not apply to class
			AssertRuleDoesNotApply<OverrideValueTypeDefaultsTest> ();
		}

		[Test]
		public void Enum ()
		{
			// rule does not apply to enums
			AssertRuleDoesNotApply (SimpleTypes.Enum);
		}

		[Test]
		public void Interface ()
		{
			// rule does not apply to interface
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		public struct EmptyStruct {
		}

		public struct PublicStruct {
			int i;
		}

		protected struct ProtectedStruct {
			object obj;
		}

		private struct PrivateStruct {
			EmptyStruct empty;
			object obj;
		}

		internal struct InternalStruct {
			string s;
		}

		public struct EqualsOnly {
			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}
		}

		public struct GetHashCodeOnly {
			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
		}

		public struct OverridesOnly {

			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}

			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
		}

		public struct OperatorsOnly {

			public static bool operator== (OperatorsOnly left, OperatorsOnly right)
			{
				return left.Equals (right);
			}

			public static bool operator != (OperatorsOnly left, OperatorsOnly right)
			{
				return !left.Equals (right);
			}
		}

		public struct Ok {
			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}

			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}

			public static bool operator == (Ok left, Ok right)
			{
				return left.Equals (right);
			}

			public static bool operator != (Ok left, Ok right)
			{
				return !left.Equals (right);
			}
		}

		[Test]
		public void Structure ()
		{
			AssertRuleFailure<EmptyStruct> ();
			AssertRuleFailure<PublicStruct> ();
			AssertRuleFailure<ProtectedStruct> ();
			AssertRuleFailure<PrivateStruct> ();
			AssertRuleFailure<InternalStruct> ();

			AssertRuleFailure<EqualsOnly> ();
			AssertRuleFailure<GetHashCodeOnly> ();
			AssertRuleFailure<OverridesOnly> ();
			AssertRuleFailure<OperatorsOnly> ();

			AssertRuleSuccess<Ok> ();
		}
	}
}
