// 
// Tests.Rules.Gendarme.DefectsMustBeReportedTest
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
using Gendarme.Framework;
using Gendarme.Rules.Gendarme;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace Test.Rules.Gendarme {

	[TestFixture]
	public class DefectsMustBeReportedTest : TypeRuleTestFixture<DefectsMustBeReportedRule> {

		class GoodRule : Rule {
			public void Test ()
			{
				Runner.Report (null, Severity.Critical, Confidence.Total);
			}
		}

		class GoodRuleTwoMethods : Rule {
			public void Test ()
			{
			}

			private void Report ()
			{
				Runner.Report (null, Severity.Audit, Confidence.Low);
			}
		}

		class BadRule : Rule, ITypeRule {
			public RuleResult CheckType (TypeDefinition type)
			{
				return RuleResult.Failure;
			}
		}

		abstract class DoesNotApplyAbstract : Rule {
			public void Test ()
			{
			}
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply<DoesNotApplyAbstract> ();
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodRule> ();
			AssertRuleSuccess<GoodRuleTwoMethods> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadRule> ();
		}
	}
}
