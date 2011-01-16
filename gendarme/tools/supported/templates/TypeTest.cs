// 
// $rootnamespace$.$safeitemname$
//
// Authors:
//	$name$ <$email$>
//
// Copyright (C) $year$ $name$
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
// TODO: Add using for the project of the rule being tested.
// using Gendarme.Rules. ;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace $rootnamespace$ {

	[TestFixture]
	public class $safeitemname$ : TypeRuleTestFixture</* TODO: Add rule's type */> {
		[Test]
		public void DoesNotApply ()
		{
			// TODO: Write tests that don't apply.
			// AssertRuleDoesNotApply<type> ();
		}

		[Test]
		public void Good ()
		{
			// TODO: Write tests that should succeed.
			// AssertRuleSuccess<type> ();
		}

		[Test]
		public void Bad ()
		{
			// TODO: Write tests that should fail.
			// AssertRuleFailure<type> ();
		}
	}
}
