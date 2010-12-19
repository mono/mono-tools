// 
// Test.Rules.Design.Generic.DoNotDeclareStaticMembersOnGenericTypesTest
//
// Authors:
//	Nicholas Rioux
//
// Copyright (C) 2010 Nicholas Rioux
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
using Gendarme.Rules.Design.Generic;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;


namespace Test.Rules.Design.Generic {
	public class GoodGenericClass<T> {
		public T member;
	}

	public class BadGenericClass<T> {
		public static T member;
	}

	public class BadGenericClass2<T> {
		public static T member1;
		public static void member2 ()
		{
		}
	}

	[TestFixture]
	public class DoNotDeclareStaticMembersOnGenericTypesTest : TypeRuleTestFixture<DoNotDeclareStaticMembersOnGenericTypesRule> {
		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodGenericClass<string>> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadGenericClass<string>> (1);
			AssertRuleFailure<BadGenericClass2<string>> (2);
		}
	}
}
