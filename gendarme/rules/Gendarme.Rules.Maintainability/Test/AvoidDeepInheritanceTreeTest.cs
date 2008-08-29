//
// Unit tests for AvoidDeepInheritanceTreeRule
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
using System.Security.Cryptography;

using Gendarme.Rules.Maintainability;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Maintainability {

	public class Internal : HMACSHA1 {
	}

	public class InternalOne : Internal {
	}

	public class InternalTwo : InternalOne {
	}

	public class InternalThree : InternalTwo {
	}

	public class InternalFour : InternalThree {
	}

	public class InternalFive : InternalFour {
	}

	[TestFixture]
	public class AvoidDeepInheritanceTreeTestWithCountExternalDepth : TypeRuleTestFixture<AvoidDeepInheritanceTreeRule> {

		[SetUp]
		public void SetUp ()
		{
			Rule.CountExternalDepth = true;
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Structure);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
		}

		[Test]
		public void Zero ()
		{
			AssertRuleSuccess<object> ();
		}

		[Test]
		public void One ()
		{
			// inherits from System.Object
			AssertRuleSuccess<string> ();
			AssertRuleSuccess<HashAlgorithm> ();
		}

		[Test]
		public void Two ()
		{
			AssertRuleSuccess<KeyedHashAlgorithm> ();
		}

		[Test]
		public void Three ()
		{
			AssertRuleSuccess<HMAC> ();
		}

		[Test]
		public void Four ()
		{
			AssertRuleSuccess<HMACSHA1> ();
		}

		class MyHMACSHA1 : HMACSHA1 {
		}

		[Test]
		public void More ()
		{
			AssertRuleFailure<MyHMACSHA1> ();
		}

		[Test]
		public void InternalOneWithCountExternalDepth ()
		{
			AssertRuleFailure<InternalOne> ();
		}

	}

	[TestFixture]
	public class AvoidDeepInheritanceTreeTest : TypeRuleTestFixture<AvoidDeepInheritanceTreeRule> {

		[Test]
		public void InternalZero ()
		{
			AssertRuleSuccess<Internal> ();
		}

		[Test]
		public void InternalOne ()
		{
			AssertRuleSuccess<InternalOne> ();
		}

		[Test]
		public void InternalFour ()
		{
			AssertRuleSuccess<InternalFour> ();
		}

		[Test]
		public void InternalFive ()
		{
			AssertRuleFailure<InternalFive> ();
		}

	}
}
