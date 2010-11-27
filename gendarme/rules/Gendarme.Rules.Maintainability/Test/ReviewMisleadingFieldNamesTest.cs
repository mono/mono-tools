//
// Unit Tests for ReviewMisleadingFieldNamesRule
//
// Authors:
//	N Lum <nol888@gmail.com>
//
// Copyright (C) 2010 N Lum
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Gendarme.Rules.Maintainability;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Maintainability {

	[TestFixture]
	public class ReviewMisleadingFieldNamesTest : TypeRuleTestFixture<ReviewMisleadingFieldNamesRule> {

		private class GoodClass {
			int m_SomeValue;
			static int s_SomeValue;
		}

		private class Bad1 {
			int s_SomeValue;
			static int s_SomeOtherValue;
		}

		private class Bad2 {
			int s_SomeValue;
			static int m_SomeValue;
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodClass> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<Bad1> (1);
			AssertRuleFailure<Bad2> (2);
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
		}
	}
}
