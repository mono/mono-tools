// 
// Unit tests for ImplementEqualsAndGetHashCodeInPairRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
	
	[TestFixture]
	public class ImplementEqualsAndGetHashCodeInPairTest : TypeRuleTestFixture<ImplementEqualsAndGetHashCodeInPairRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		public class ImplementsEqualsButNotGetHashCode {
			public override bool Equals (Object obj)
			{
				return this == obj;
			}
		}

		[Test]
		public void EqualsButNotGetHashCodeTest ()
		{
			AssertRuleFailure<ImplementsEqualsButNotGetHashCode> (1);
		}
			
		public class ImplementsGetHashCodeButNotEquals {
			public override int GetHashCode ()
			{
				return 2;
			}
		}

		[Test]
		public void GetHashCodeButNotEqualsTest ()
		{
			AssertRuleFailure<ImplementsGetHashCodeButNotEquals> (1);
		}
		
		public class ImplementsNoneOfThem {
			public void test ()
			{
			}
		}

		[Test]
		public void NoneOfThemTest ()
		{
			AssertRuleSuccess<ImplementsNoneOfThem> ();
		}

		public class ImplementsBothOfThem {
			public override int GetHashCode ()
			{
				return 2;
			}
			public new bool Equals (Object obj)
			{
				return this == obj;
			}
		}

		[Test]
		public void BothOfThemTest ()
		{
			AssertRuleSuccess<ImplementsBothOfThem> ();
		}

		public class ImplementsEqualsUsesObjectGetHashCode {
			public override bool Equals (Object obj)
			{
				return this == obj;
			}
			public static void Main (string [] args)
			{
				int j = 0;
				ImplementsEqualsUsesObjectGetHashCode i = new ImplementsEqualsUsesObjectGetHashCode ();
				j = i.GetHashCode ();
			}
		}

		[Test]
		public void ImplementsEqualsUsesObjectGetHashCodeTest ()
		{
			AssertRuleFailure<ImplementsEqualsUsesObjectGetHashCode> (1);
		}

		public class ImplementsEqualsReuseBaseGetHashCode {
			public override bool Equals (Object obj)
			{
				return this == obj;
			}
			public override int  GetHashCode()
			{
 				 return base.GetHashCode();
			}
			public static void Main (string [] args)
			{
				int j = 0;
				ImplementsEqualsUsesObjectGetHashCode i = new ImplementsEqualsUsesObjectGetHashCode ();
				j = i.GetHashCode ();
			}
		}

		[Test]
		public void ImplementsEqualsReuseBaseGetHashCodeTest ()
		{
			AssertRuleSuccess<ImplementsEqualsReuseBaseGetHashCode> ();
		}

		public class ImplementsGetHashCodeUsesObjectEquals {
			public override int GetHashCode ()
			{
				return 1;
			}
			public static void Main (string [] args)
			{
				ImplementsGetHashCodeUsesObjectEquals i = new ImplementsGetHashCodeUsesObjectEquals ();
				ImplementsGetHashCodeUsesObjectEquals i1 = new ImplementsGetHashCodeUsesObjectEquals ();
				i.Equals (i1);
			}
		}

		[Test]
		public void ImplementsGetHashCodeUsesObjectEqualsTest ()
		{
			AssertRuleFailure<ImplementsGetHashCodeUsesObjectEquals> (1);
		}

		public class ImplementingEqualsWithTwoArgs {
			public bool Equals (Object obj1, Object obj2)
			{
				return obj1 == obj2;
			}
		}

		[Test]
		public void EqualsWithTwoArgsTest ()
		{
			AssertRuleSuccess<ImplementingEqualsWithTwoArgs> ();
		}

		public class ImplementingGetHashCodeWithOneArg {
			public int GetHashCode (int j)
			{
				return j*2;
			}
		}

		[Test]
		public void GetHashCodeWithOneArgTest ()
		{
			AssertRuleSuccess<ImplementingGetHashCodeWithOneArg> ();
		}
	}
}
