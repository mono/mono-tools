// 
// Test.Rules.NUnit.UnitTestsMissingTestFixtureTest
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

namespace Test.Rules.NUnit {

	[TestFixture]
	public class UnitTestsMissingTestFixtureTest : TypeRuleTestFixture<UnitTestsMissingTestFixtureRule> {

		abstract class AbstractClass {
		}

		[TestFixture]
		class TestFixtureClass {
			// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
			public void TestMethod ()
			{
			}
		}

		class InheritsFromTestFixtureClass : TestFixtureClass {
			// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
			public void AnotherTestMethod ()
			{
			}
		}

		class NoTestFixtureClass {
			// [Test] -- added later using Cecil to avoid NUnit treating this as a unit test
			public void TestMethod ()
			{
			}
		}


		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply<AbstractClass> ();
		}

		[Test]
		public void Good ()
		{
			// self-testing, TestFixture present
			AssertRuleSuccess<TestFixtureClass> ();
			AssertRuleSuccess<InheritsFromTestFixtureClass> ();

			DefinitionLoader.GetMethodDefinition<TestFixtureClass> ("TestMethod").AddTestAttribute ();
			DefinitionLoader.GetMethodDefinition<InheritsFromTestFixtureClass> ("AnotherTestMethod").AddTestAttribute ();
			AssertRuleSuccess<TestFixtureClass> ();
			AssertRuleSuccess<InheritsFromTestFixtureClass> ();

			// no TestFixture and no Test attributes
			AssertRuleSuccess (SimpleTypes.Class);

			// deep (more than one) hierarchy / loop
			AssertRuleSuccess<UnitTestsMissingTestFixtureRule> ();
		}

		[Test]
		public void Bad ()
		{
			DefinitionLoader.GetMethodDefinition<NoTestFixtureClass> ("TestMethod").AddTestAttribute ();
			AssertRuleFailure<NoTestFixtureClass> ();
		}
	}
}
