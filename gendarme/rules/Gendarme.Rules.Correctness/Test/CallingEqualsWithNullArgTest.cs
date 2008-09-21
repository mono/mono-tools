// 
// Unit tests for CallingEqualsWithNullArgRule
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

using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class CallingEqualsWithNullArgTest : MethodRuleTestFixture<CallingEqualsWithNullArgRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no CALL[VIRT]
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public class CallToEqualsWithNullArg
		{
			public static void Main (string [] args)
			{
				CallToEqualsWithNullArg c = new CallToEqualsWithNullArg ();
				c.Equals (null);
			}
		}

		[Test]
		public void CallToEqualsWithNullArgTest ()
		{
			AssertRuleFailure<CallToEqualsWithNullArg> ("Main", 1);
		}
		
		public class CallingEqualsWithNonNullArg 
		{
			public static void Main (string [] args)
			{
				CallingEqualsWithNonNullArg c = new CallingEqualsWithNonNullArg ();
				CallingEqualsWithNonNullArg c1 = new CallingEqualsWithNonNullArg ();
				c.Equals (c1);
				c1 = null; // ensure an LDNULL instruction is inside the method
			}
		}

		[Test]
		public void CallingEqualsWithNonNullArgTest ()
		{
			AssertRuleSuccess<CallingEqualsWithNonNullArg> ("Main");
		}
		
		public class CallingEqualsOnEnum
		{
			enum Days { Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday };
			
			public bool Equals (Enum e)
			{
				if (e == null)
					return false;
				else
					return e.GetType () == typeof (Days);
			}
			
			public void PassingArgNullInEquals ()
			{
				Type e = typeof (Days);
				e.Equals (null);
			}
			
			public void NotPassingNullArgInEquals ()
			{
				Type e = typeof (Days);
				Type e1 = typeof (Days);
				e.Equals (e1);
				e1 = null; // ensure an LDNULL instruction is inside the method
			}
		}

		[Test]
		public void CallingEqualsOnEnumTest ()
		{
			AssertRuleFailure<CallingEqualsOnEnum> ("PassingArgNullInEquals", 1);
			AssertRuleSuccess<CallingEqualsOnEnum> ("NotPassingNullArgInEquals");
		}

		struct structure {

			public bool Equals (structure s)
			{
				return s.GetType () == typeof (structure);
			}
		}
		
		public class CallingEqualsOnStruct
		{			
			public void PassingNullArgument ()
			{
				structure s = new structure ();
				s.Equals (null);
			}
			
			public void PassingNonNullArg ()
			{
				structure s = new structure ();
				structure s1 = new structure ();
				s.Equals (s1);
			}
		}

		[Test]
		public void CallingEqualsOnStructTest ()
		{
			AssertRuleFailure<CallingEqualsOnStruct> ("PassingNullArgument", 1);
			// there's no LDNULL in the method, so the rule skip the analysis
			AssertRuleDoesNotApply<CallingEqualsOnStruct> ("PassingNonNullArg");
		}

		public class CallingEqualsOnArray
		{
			int [] a = new int [] {1, 2, 3};
			
			public bool Equals (int [] b)
			{
				if (b == null)
					return false;
				else
					return a.Length == b.Length;
			}
			
			public void PassingNullArg ()
			{
				int [] b = new int [] {1, 2, 3};
				b.Equals (null);
			}
			
			public void PassingNonNullArg ()
			{
				int [] b = new int [] {1, 2, 3};
				b.Equals (a);
				b = null;
			}
		}

		[Test]
		public void CallingEqualsOnArrayTest ()
		{
			AssertRuleFailure<CallingEqualsOnArray> ("PassingNullArg", 1);
			AssertRuleSuccess<CallingEqualsOnArray> ("PassingNonNullArg");
		}
	}
}
