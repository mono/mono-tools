//
// Unit tests for DoNotUseEnumIsAssignableFromRule
//
// Authors:
//	Jb Evain  <jbevain@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class DoNotUseEnumIsAssignableFromTest : MethodRuleTestFixture<DoNotUseEnumIsAssignableFromRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no CALL[VIRT] instruction
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public bool EnumIsAssignableFromType (Type type)
		{
			return typeof (Enum).IsAssignableFrom (type);
		}

		public bool EnumIsAssignableFromBaseType (Type type)
		{
			return typeof (Enum).IsAssignableFrom (type.BaseType);
		}

		public bool EnumIsEnum (Type type)
		{
			return type.IsEnum;
		}

		enum Foo {
			Bar,
			Baz,
			Gaz,
		}

		[Test]
		public void Bad ()
		{
			Assert.IsTrue (EnumIsAssignableFromType (typeof (Foo)), "EnumIsAssignableFromType");
			AssertRuleFailure<DoNotUseEnumIsAssignableFromTest> ("EnumIsAssignableFromType", 1);
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "1");

			Assert.IsTrue (EnumIsAssignableFromBaseType (typeof (Foo)), "EnumIsAssignableFromBaseType");
			AssertRuleFailure<DoNotUseEnumIsAssignableFromTest> ("EnumIsAssignableFromBaseType", 1);
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "1");
		}

		[Test]
		public void Good ()
		{
			Assert.IsTrue (EnumIsEnum (typeof (Foo)), "EnumIsEnum");
			AssertRuleSuccess<DoNotUseEnumIsAssignableFromTest> ("EnumIsEnum");
		}
	}
}
