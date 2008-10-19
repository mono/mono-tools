//
// Unit tests for PreferIntegerOrStringForIndexersRule
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
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class PreferIntegerOrStringForIndexersTest : MethodRuleTestFixture<PreferIntegerOrStringForIndexersRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		public int this [int index] {
			get { return 0; }
		}

		protected int this [string name] {
			get { return 0; }
		}

		public int this [object o] {
			get { return 0; }
		}

		protected int this [long index] {
			get { return 0; }
		}

		public int this [DateTime date] {
			get { return 0; }
		}

		[Test]
		public void Visible ()
		{
			AssertRuleSuccess<PreferIntegerOrStringForIndexersTest> ("get_Item", new Type [] { typeof (int) });
			AssertRuleSuccess<PreferIntegerOrStringForIndexersTest> ("get_Item", new Type [] { typeof (string) });
			AssertRuleSuccess<PreferIntegerOrStringForIndexersTest> ("get_Item", new Type [] { typeof (object) });
			AssertRuleSuccess<PreferIntegerOrStringForIndexersTest> ("get_Item", new Type [] { typeof (long) });
			AssertRuleFailure<PreferIntegerOrStringForIndexersTest> ("get_Item", new Type [] { typeof (DateTime) }, 1);
		}

		class NonVisibleType {

			public int this [DateTime date] {
				get { return 0; }
			}
		}

		public class VisibleType {

			internal int this [DateTime date] {
				get { return 0; }
			}
		}

		[Test]
		public void NonVisible ()
		{
			AssertRuleSuccess<NonVisibleType> ("get_Item", new Type [] { typeof (DateTime) });
			AssertRuleSuccess<VisibleType> ("get_Item", new Type [] { typeof (DateTime) });
		}
	}
}
