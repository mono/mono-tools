//
// Unit tests for ImplementIComparableCorrectlyRule
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
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Design {

	[TestFixture]
	public class ImplementIComparableCorrectlyTest : TypeRuleTestFixture<ImplementIComparableCorrectlyRule> {

		[Test]
		public void NotApplicable ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);

			AssertRuleDoesNotApply<ImplementIComparableCorrectlyTest> ();
		}

		// tests for IComparable

		public struct ComparableStruct : IComparable {

			private int x;

			public int CompareTo (object obj)
			{
				if (obj is ComparableStruct)
					return x.CompareTo (((ComparableStruct) obj).x);
				throw new ArgumentException ("wrong type", "obj");
			}
		}

		public struct ComparableStructEquals : IComparable {

			private int x;

			public int CompareTo (object obj)
			{
				if (obj is ComparableStructEquals)
					return x.CompareTo (((ComparableStructEquals) obj).x);
				throw new ArgumentException ("wrong type", "obj");
			}

			public override bool Equals (object obj)
			{
				if (obj is ComparableStructEquals)
					return x == ((ComparableStructEquals) obj).x;
				return false;
			}
		}

		public struct ComparableStructComplete : IComparable {

			private int x;

			public int CompareTo (object obj)
			{
				if (obj is ComparableStructComplete)
					return x.CompareTo (((ComparableStructComplete) obj).x);
				throw new ArgumentException ("wrong type", "obj");
			}

			public override bool Equals (object obj)
			{
				if (obj is ComparableStructComplete)
					return x == ((ComparableStructComplete) obj).x;
				return false;
			}

			static public bool operator == (ComparableStructComplete left, ComparableStructComplete right)
			{
				return (left.x == right.x);
			}

			static public bool operator != (ComparableStructComplete left, ComparableStructComplete right)
			{
				return (left.x != right.x);
			}

			static public bool operator > (ComparableStructComplete left, ComparableStructComplete right)
			{
				return (left.x > right.x);
			}

			static public bool operator < (ComparableStructComplete left, ComparableStructComplete right)
			{
				return (left.x < right.x);
			}
		}

		[Test]
		public void NonGenericStruct ()
		{
			// missing Equals and operators
			AssertRuleFailure<ComparableStruct> (2);
			// missing operators
			AssertRuleFailure<ComparableStructEquals> (1);
			// complete
			AssertRuleSuccess<ComparableStructComplete> ();
		}

		public class ComparableClass : IComparable {

			private int x;

			public int CompareTo (object obj)
			{
				ComparableClass cc = (obj as ComparableClass);
				if (cc == null)
					throw new ArgumentException ("wrong type", "obj");

				return x.CompareTo (cc.x);
			}

			static public bool operator == (ComparableClass left, ComparableClass right)
			{
				return (left.x == right.x);
			}

			static public bool operator != (ComparableClass left, ComparableClass right)
			{
				return (left.x != right.x);
			}

			static public bool operator > (ComparableClass left, ComparableClass right)
			{
				return (left.x > right.x);
			}

			static public bool operator < (ComparableClass left, ComparableClass right)
			{
				return (left.x < right.x);
			}
		}

		public class ComparableClassEquals : IComparable {

			private int x;

			public int CompareTo (object obj)
			{
				ComparableClassEquals cc = (obj as ComparableClassEquals);
				if (cc == null)
					throw new ArgumentException ("wrong type", "obj");

				return x.CompareTo (cc.x);
			}

			public override bool Equals (object obj)
			{
				ComparableClassEquals cc = (obj as ComparableClassEquals);
				if (cc == null)
					return false;

				return x == cc.x;
			}

			static public bool operator == (ComparableClassEquals left, ComparableClassEquals right)
			{
				return (left.x == right.x);
			}

			static public bool operator != (ComparableClassEquals left, ComparableClassEquals right)
			{
				return (left.x != right.x);
			}
		}

		public class ComparableClassComplete : IComparable {

			private int x;

			public int CompareTo (object obj)
			{
				ComparableClassComplete cc = (obj as ComparableClassComplete);
				if (cc == null)
					throw new ArgumentException ("wrong type", "obj");

				return x.CompareTo (cc.x);
			}

			public override bool Equals (object obj)
			{
				ComparableClassComplete cc = (obj as ComparableClassComplete);
				if (cc == null)
					return false;

				return x == cc.x;
			}

			static public bool operator == (ComparableClassComplete left, ComparableClassComplete right)
			{
				return (left.x == right.x);
			}

			static public bool operator != (ComparableClassComplete left, ComparableClassComplete right)
			{
				return (left.x != right.x);
			}

			static public bool operator > (ComparableClassComplete left, ComparableClassComplete right)
			{
				return (left.x > right.x);
			}

			static public bool operator < (ComparableClassComplete left, ComparableClassComplete right)
			{
				return (left.x < right.x);
			}
		}

		[Test]
		public void NonGenericObject ()
		{
			// missing Equals (but has evey required operators)
			AssertRuleFailure<ComparableClass> (1);
			// missing some operators (> and <)
			AssertRuleFailure<ComparableClassEquals> (1);
			// complete
			AssertRuleSuccess<ComparableClassComplete> ();
		}

		// tests for IComparable<T>

		public struct GenericComparableStruct : IComparable<GenericComparableStruct> {

			private int x;

			public int CompareTo (GenericComparableStruct other)
			{
				return x.CompareTo (other.x);
			}
		}

		public struct GenericComparableStructEquals : IComparable<GenericComparableStructEquals> {

			private int x;

			public int CompareTo (GenericComparableStructEquals other)
			{
				return x.CompareTo (other.x);
			}

			public override bool Equals (object obj)
			{
				if (obj is GenericComparableStructEquals)
					return x == ((GenericComparableStructEquals) obj).x;
				return false;
			}
		}

		public struct GenericComparableStructComplete : IComparable<GenericComparableStructComplete> {

			private int x;

			public int CompareTo (GenericComparableStructComplete other)
			{
				return x.CompareTo (other.x);
			}

			public override bool Equals (object obj)
			{
				if (obj is GenericComparableStructComplete)
					return x == ((GenericComparableStructComplete) obj).x;
				return false;
			}

			static public bool operator == (GenericComparableStructComplete left, GenericComparableStructComplete right)
			{
				return (left.x == right.x);
			}

			static public bool operator != (GenericComparableStructComplete left, GenericComparableStructComplete right)
			{
				return (left.x != right.x);
			}

			static public bool operator > (GenericComparableStructComplete left, GenericComparableStructComplete right)
			{
				return (left.x > right.x);
			}

			static public bool operator < (GenericComparableStructComplete left, GenericComparableStructComplete right)
			{
				return (left.x < right.x);
			}
		}

		[Test]
		public void GenericStruct ()
		{
			// missing Equals and operators
			AssertRuleFailure<GenericComparableStruct> (2);
			// missing operators
			AssertRuleFailure<GenericComparableStructEquals> (1);
			// complete
			AssertRuleSuccess<GenericComparableStructComplete> ();
		}

		// both IComparable and IComparable<T>

		public class BothComparableStruct : IComparable, IComparable<BothComparableStruct> {

			private int x;

			public int CompareTo (object obj)
			{
				if (obj is BothComparableStruct)
					return x.CompareTo (((BothComparableStruct) obj).x);
				throw new ArgumentException ("wrong type", "obj");
			}

			public int CompareTo (BothComparableStruct other)
			{
				return x.CompareTo (other.x);
			}
		}

		public struct BothComparableStructComplete : IComparable, IComparable<BothComparableStructComplete> {

			private int x;

			public int CompareTo (object obj)
			{
				if (obj is BothComparableStructComplete)
					return x.CompareTo (((BothComparableStructComplete) obj).x);
				throw new ArgumentException ("wrong type", "obj");
			}

			public int CompareTo (BothComparableStructComplete other)
			{
				return x.CompareTo (other.x);
			}

			public override bool Equals (object obj)
			{
				if (obj is BothComparableStructComplete)
					return x == ((BothComparableStructComplete) obj).x;
				return false;
			}

			static public bool operator == (BothComparableStructComplete left, BothComparableStructComplete right)
			{
				return (left.x == right.x);
			}

			static public bool operator != (BothComparableStructComplete left, BothComparableStructComplete right)
			{
				return (left.x != right.x);
			}

			static public bool operator > (BothComparableStructComplete left, BothComparableStructComplete right)
			{
				return (left.x > right.x);
			}

			static public bool operator < (BothComparableStructComplete left, BothComparableStructComplete right)
			{
				return (left.x < right.x);
			}
		}

		[Test]
		public void BothClass ()
		{
			// missing Equals and operators
			AssertRuleFailure<BothComparableStruct> (2);
			// complete
			AssertRuleSuccess<GenericComparableStructComplete> ();
		}
	}
}
