//
// Unit tests for UseIsOperatorRule
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class UseIsOperatorTest : MethodRuleTestFixture<UseIsOperatorRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		private bool ReturnEqualityBad (object value)
		{
			return ((value as UseIsOperatorTest) == null);
		}

		private bool ReturnInequalityBad (object value)
		{
			return ((value as UseIsOperatorTest) != null);
		}

		private bool ReturnEqualityOk (object value)
		{
			return (value is UseIsOperatorTest);
		}

		private bool ReturnInequalityOk (object value)
		{
			return !(value is UseIsOperatorTest);
		}

		[Test]
		public void Return ()
		{
			AssertRuleFailure<UseIsOperatorTest> ("ReturnEqualityBad", 1);
			AssertRuleFailure<UseIsOperatorTest> ("ReturnInequalityBad", 1);
			AssertRuleDoesNotApply<UseIsOperatorTest> ("ReturnEqualityOk");
			AssertRuleSuccess<UseIsOperatorTest> ("ReturnInequalityOk");
		}

		private void ConditionIsOk (object value)
		{
			if (value is UseIsOperatorTest) {
				Console.WriteLine ("Ok");
			}
		}

		private void ConditionAsOk (object value)
		{
			UseIsOperatorTest test = (value as UseIsOperatorTest);
			if (test != null) {
				// 'is' would not be optimal since we use the 'as' result
				Console.WriteLine (test.ToString ());
			}
		}

		[Test]
		public void Conditions ()
		{
			AssertRuleDoesNotApply<UseIsOperatorTest> ("ConditionIsOk");
			AssertRuleDoesNotApply<UseIsOperatorTest> ("ConditionAsOk");
		}

		private void ConditionSplitBad (object value)
		{
			UseIsOperatorTest test = (value as UseIsOperatorTest);
			// 'test' is unused after the test
			if (test != null) {
				Console.WriteLine ("Bad");
			}
		}

		// [g]mcs compiles this like an 'is', csc does too when compiling with optimizations
		private void ConditionEqualityBad (object value)
		{
			if ((value as UseIsOperatorTest) == null) {
				Console.WriteLine ("Bad");
			}
		}

		private void ConditionInequalityBad (object value)
		{
			if ((value as UseIsOperatorTest) != null) {
				Console.WriteLine ("Bad");
			}
		}

		[Test]
		[Ignore ("Compiler optimization (default for [g]mcs) can fix this")]
		public void ConditionsOptimized ()
		{
			// missed opportunities are less problematic than false positives ;-)
			AssertRuleFailure<UseIsOperatorTest> ("ConditionEqualityBad", 1);
			AssertRuleFailure<UseIsOperatorTest> ("ConditionInequalityBad", 1);
			AssertRuleFailure<UseIsOperatorTest> ("ConditionSplitBad", 1);
		}

		private object ReturnEqualityThis (object value)
		{
			if ((value as UseIsOperatorTest) == this)
				return false;
			return (this == null);
		}

		[Test]
		public void BetterCoverage ()
		{
			AssertRuleSuccess<UseIsOperatorTest> ("ReturnEqualityThis");
		}
	}
}
