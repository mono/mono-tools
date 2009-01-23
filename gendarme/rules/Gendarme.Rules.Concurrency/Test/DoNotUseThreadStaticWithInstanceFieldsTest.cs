//
// Unit tests for DoNotUseThreadStaticWithInstanceFieldsRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
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

using Gendarme.Rules.Concurrency;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public sealed class DoNotUseThreadStaticWithInstanceFieldsTest : TypeRuleTestFixture<DoNotUseThreadStaticWithInstanceFieldsRule> {

		private sealed class Good1 {
			[ThreadStatic]
			public static string name;
		}
		
		private sealed class Bad1 {
			[ThreadStatic]
			public string name1;
		}
		
		private sealed class Bad2 {
			[ThreadStatic]
			public string name1;

			[ThreadStatic]
			public static string name;		// this one is OK

			[ThreadStatic]
			public string name2;
		}
		
		[Test]
		public void NotApplicable ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		[Test]
		public void Cases ()
		{
			AssertRuleSuccess<Good1> ();
			
			AssertRuleFailure<Bad1> ();
			AssertRuleFailure<Bad2> (2);
		}
	}
}
