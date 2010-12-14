// 
// Test.Rules.NUnit.ProvideMessageOnAssertCallsTest
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.Reflection;

using Mono.Cecil;
using Gendarme.Rules.NUnit;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;
using System.Runtime.InteropServices;

namespace Test.Rules.NUnit {

	[TestFixture]
	public class ProvideMessageOnAssertCallsTest : MethodRuleTestFixture<ProvideMessageOnAssertCallsRule> {

		private void DoesNotApplyNoAttributes ()
		{
			Assert.AreEqual (10, 20);
			Assert.AreNotEqual (20, 30);
		}

		// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
		private void BadTestAttribute ()
		{
			Assert.AreEqual (10, 20);
			Assert.AreNotEqual (20, 30);
		}

		// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
		private void FourBadAsserts ()
		{
			Assert.AreEqual (10, 15);
			Assert.AreEqual (10, 15, "message string");

			// unrelated code
			System.Collections.Generic.List<string> ls = new System.Collections.Generic.List<string> { "a", "b" };
			ls.Clear ();

			Assert.IsInstanceOfType (typeof (System.Reflection.Assembly), new object ());
			Assert.IsNull (null);
			Assert.IsNotNull (null, "message string");
			Assert.ReferenceEquals (new object (), new object ()); // should be ignored
			Assert.Fail ();
		}

		// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
		private void GoodOneBadAssert ()
		{
			Assert.LessOrEqual (10, 20);
		}

		// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
		private void GoodExceptions ()
		{
			Assert.ReferenceEquals (1, 2);
			Assert.Equals (3, 4);
		}

		// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
		private void GoodWithMessages ()
		{
			Assert.IsNull (new object (), "Test to check whether new object is null");
			Assert.IsFalse (true, "Test to check whether true is false");
			Assert.Fail ("Failing the test");
		}

		// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
		[DllImport ("libc.so")]
		private static extern void DoesNotApplyExternal ();

		// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
		private void DoesNotApplyEmpty ()
		{
		}


		[Test]
		public void DoesNotApply ()
		{
			MethodDefinition m = DefinitionLoader.GetMethodDefinition<ProvideMessageOnAssertCallsTest> ("DoesNotApplyExternal");
			m.AddTestAttribute ();
			AssertRuleDoesNotApply (m);

			m = DefinitionLoader.GetMethodDefinition<ProvideMessageOnAssertCallsTest> ("DoesNotApplyEmpty");
			m.AddTestAttribute ();
			AssertRuleDoesNotApply (m);

			AssertRuleDoesNotApply<ProvideMessageOnAssertCallsTest> ("DoesNotApplyNoAttributes");
		}

		[Test]
		public void Good ()
		{
			MethodDefinition m = DefinitionLoader.GetMethodDefinition<ProvideMessageOnAssertCallsTest> ("GoodOneBadAssert");
			m.AddTestAttribute ();
			AssertRuleSuccess (m);

			m = DefinitionLoader.GetMethodDefinition<ProvideMessageOnAssertCallsTest> ("GoodExceptions");
			m.AddTestAttribute ();
			AssertRuleSuccess (m);

			m = DefinitionLoader.GetMethodDefinition<ProvideMessageOnAssertCallsTest> ("GoodWithMessages");
			m.AddTestAttribute ();
			AssertRuleSuccess (m);
		}

		[Test]
		public void Bad ()
		{
			MethodDefinition m = DefinitionLoader.GetMethodDefinition<ProvideMessageOnAssertCallsTest> ("BadTestAttribute");
			m.AddTestAttribute ();
			AssertRuleFailure (m, 2);

			m = DefinitionLoader.GetMethodDefinition<ProvideMessageOnAssertCallsTest> ("FourBadAsserts");
			m.AddTestAttribute ();
			AssertRuleFailure (m, 4);
		}
	}
}
