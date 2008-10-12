//
// Unit tests for UseTypeEmptyTypesRule
//
// Authors:
//	Jb Evain <jbevain@novell.com>
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class UseTypeEmptyTypesTest : MethodRuleTestFixture<UseTypeEmptyTypesRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no body
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no newarr 
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}
	
		public class TestCase {

			Type [] empty = new Type [0];
			
			public bool CreateEmptyTypeArray ()
			{
				Type [] array = new Type [0];
				return (array == null);
			}

			public bool CreateNotEmptyTypeArray ()
			{
				Type [] array = new Type [42];
				return (array == null);
			}

			public byte [] CreateEmptyNonTypeArray ()
			{
				return new byte [0];
			}

			public bool CreateUnknownSizedTypeArray (int x)
			{
				Type [] array = new Type [x];
				return (array == null);
			}
		}

		[Test]
		public void CreateEmptyTypeArray ()
		{
			AssertRuleFailure<TestCase> ("CreateEmptyTypeArray");
		}

		[Test]
		public void CreateEmptyTypeArrayInCtor ()
		{
			AssertRuleFailure<TestCase> (".ctor");
		}

		[Test]
		public void CreateNotEmptyTypeArray ()
		{
			AssertRuleSuccess<TestCase> ("CreateNotEmptyTypeArray");
		}

		[Test]
		public void CreateEmptyNonTypeArray ()
		{
			AssertRuleSuccess<TestCase> ("CreateEmptyNonTypeArray");
		}

		[Test]
		public void CreateUnknownSizedTypeArray ()
		{
			// no Ldc_I4[_S|_0]
			AssertRuleDoesNotApply<TestCase> ("CreateUnknownSizedTypeArray");
		}
	}
}
