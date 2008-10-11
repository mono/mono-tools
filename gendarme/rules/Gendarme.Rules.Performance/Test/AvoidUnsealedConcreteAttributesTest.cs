// 
// Unit tests for AvoidUnsealedConcreteAttributesRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) Daniel Abramov
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	internal class NotAttribute {
	}

	internal class AnAttribute : Attribute {
	}

	internal sealed class SealedAttributeInheritsAnAttribute : AnAttribute {
	}

	internal sealed class SealedAttribute : Attribute {
	}

	internal abstract class AbstractAttribute : Attribute {
	}

	[TestFixture]
	public class AvoidUnsealedConcreteAttributesTest : TypeRuleTestFixture<AvoidUnsealedConcreteAttributesRule> {

		[Test]
		public void TestAbstractAttribute ()
		{
			AssertRuleSuccess<AbstractAttribute> ();
		}

		[Test]
		public void TestAnAttribute ()
		{
			AssertRuleFailure<AnAttribute> (1);
		}

		[Test]
		public void TestNotAttribute ()
		{
			AssertRuleDoesNotApply<NotAttribute> ();
		}

		[Test]
		public void TestSealedAttribute ()
		{
			AssertRuleSuccess<SealedAttribute> ();
		}

		[Test]
		public void TestSealedAttributeInheritsAnAttribute ()
		{
			AssertRuleSuccess<SealedAttributeInheritsAnAttribute> ();
		}
	}
}
