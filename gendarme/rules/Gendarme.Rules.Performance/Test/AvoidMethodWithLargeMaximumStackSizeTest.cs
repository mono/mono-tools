//
// Unit tests for AvoidMethodWithLargeMaximumStackSizeRule
//
// Authors:
//	Jb Evain <jbevain@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class AvoidMethodWithLargeMaximumStackSizeTest : MethodRuleTestFixture<AvoidMethodWithLargeMaximumStackSizeRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		private void SmallMethod (int a)
		{
			int x = a * a - a;
			Console.WriteLine (x * x);
		}

		[Test]
		public void Small ()
		{
			AssertRuleSuccess<AvoidMethodWithLargeMaximumStackSizeTest> ("SmallMethod");
		}

		public void Bang (
			int a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9, int a10,
			int a11, int a12, int a13, int a14, int a15, int a16, int a17, int a18, int a19, int a20,
			int a21, int a22, int a23, int a24, int a25, int a26, int a27, int a28, int a29, int a30,
			int a31, int a32, int a33, int a34, int a35, int a36, int a37, int a38, int a39, int a40,
			int a41, int a42, int a43, int a44, int a45, int a46, int a47, int a48, int a49, int a50,
			int a51, int a52, int a53, int a54, int a55, int a56, int a57, int a58, int a59, int a60,
			int a61, int a62, int a63, int a64, int a65, int a66, int a67, int a68, int a69, int a70,
			int a71, int a72, int a73, int a74, int a75, int a76, int a77, int a78, int a79, int a80,
			int a81, int a82, int a83, int a84, int a85, int a86, int a87, int a88, int a89, int a90,
			int a91, int a92, int a93, int a94, int a95, int a96, int a97, int a98, int a99, int a100,
			int a101, int a102, int a103, int a104, int a105, int a106, int a107, int a108, int a109, int a110)
		{
		}

		private void MethodWithLargeMaximumStackSize (int a)
		{
			Bang (
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a,
				a, a, a, a, a, a, a, a, a, a);
		}

		[Test]
		public void TooLarge ()
		{
			AssertRuleFailure<AvoidMethodWithLargeMaximumStackSizeTest> ("MethodWithLargeMaximumStackSize", 1);
		}
	}
}
