//
// AvoidInt64ArgumentsInComVisibleMethodsTest.cs
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Rules.Interoperability.Com;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace Test.Rules.Interoperability.Com {

	[TestFixture]
	public class AvoidInt64ArgumentsInComVisibleMethodsTest : MethodRuleTestFixture<AvoidInt64ArgumentsInComVisibleMethodsRule> {

		[ComVisible (true)]
		public class ComVisibleClass {

			public void BadMethod (string a, long b)
			{
				return;
			}

			public void BadMethodWithTwoBadArgs (long a, string b, long c)
			{
				return;
			}

			public void GoodMethod (string a, int b)
			{
				return;
			}

			[ComVisible (false)]
			public void InvisibleMethod (string a, long b)
			{
				return;
			}
		}

		[ComVisible (false)]
		public class ComInvisibleClass {

			public void MethodWithInt64 (string a, long b)
			{
			}

			public void MethodWithInt32 (string a, int b)
			{
			}
		}

		public class ClassWithoutAttributes {

			public void MethodWithInt64 (string a, long b)
			{
			}

			public void MethodWithInt32 (string a, int b)
			{
			}
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<ComVisibleClass> ("InvisibleMethod");

			AssertRuleDoesNotApply<ComInvisibleClass> ("MethodWithInt64");
			AssertRuleDoesNotApply<ComInvisibleClass> ("MethodWithInt32");

			AssertRuleDoesNotApply<ClassWithoutAttributes> ("MethodWithInt64");
			AssertRuleDoesNotApply<ClassWithoutAttributes> ("MethodWithInt32");
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<ComVisibleClass> ("GoodMethod");
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ComVisibleClass> ("BadMethod", 1);
			AssertRuleFailure<ComVisibleClass> ("BadMethodWithTwoBadArgs", 2);
		}
	}
}
