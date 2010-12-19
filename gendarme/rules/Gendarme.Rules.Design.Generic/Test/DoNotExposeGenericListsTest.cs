// 
// Test.Rules.Design.Generic.DoNotExposeGenericListsTest
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
using System.Collections.Generic;

using Mono.Cecil;
using Gendarme.Rules.Design.Generic;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;


namespace Test.Rules.Design.Generic {
	public class BadField {
		public List<string> field;
	}
	public class BadProperty {
		public List<string> property
		{
			get;
			set;
		}
	}
	public class BadMethods {
		public List<string> method ()
		{
			return null;
		}
		public void method (List<string> a, List<int> b)
		{
		}
	}

	[TestFixture]
	public class DoNotExposeGenericListsTest : TypeRuleTestFixture<DoNotExposeGenericListsRule> {
		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess (SimpleTypes.Class);
			AssertRuleSuccess (SimpleTypes.Structure);
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadField> (1);
			AssertRuleFailure<BadMethods> (3);
			AssertRuleFailure<BadProperty> (1);
		}
	}
}
