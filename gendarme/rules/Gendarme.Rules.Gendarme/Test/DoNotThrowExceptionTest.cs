// 
// Test.Rules.Gendarme.DoNotThrowExceptionTest
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
	public class DoNotThrowExceptionTest : MethodRuleTestFixture<DoNotThrowExceptionRule> {

		public DoNotThrowExceptionTest ()
		{
			FireEvents = true;
		}

		class NotRule {
			public void DoSomething ()
			{
				throw new DivideByZeroException ();
			}
		}

		class GoodRule1 : Rule, IMethodRule {

			public RuleResult CheckMethod (MethodDefinition method)
			{
				return RuleResult.Success;
			}
		}

		class GoodRule2 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				return RuleResult.Success;
			}

			private int property_value;

			[System.ComponentModel.Description ("Description")]
			public int RuleProperty
			{
				get { return property_value; }
				set
				{
					if (value < 0)
						throw new ArgumentException ("Minimum value for RuleProperty is 0", "RuleProperty");
					property_value = value;
				}
			}
		}


		class BadRule1 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new ArgumentException ("method");
			}
		}


		class BadRule2 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				return RuleResult.Success;
			}

			private int property_value;

			public int RuleProperty
			{
				get { return property_value; }
				set
				{
					if (value < 0)
						throw new ArgumentException ("Minimum value for RuleProperty is 0", "RuleProperty");
					property_value = value;
				}
			}

			[System.ComponentModel.Description ("Description")]
			private int RulePrivateProperty
			{
				get { return property_value; }
				set
				{
					if (value < 0)
						throw new ArgumentException ("Minimum value for RuleProperty is 0", "RuleProperty");
					property_value = value;
				}
			}
		}


		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<NotRule> ("DoSomething");
		}

		[Test]
		public void DoesNotThrowExceptions ()
		{
			AssertRuleSuccess<GoodRule1> ("CheckMethod");
			AssertRuleSuccess<GoodRule2> ("set_RuleProperty"); // such properties setters will be called in a runner
			
		}

		[Test]
		public void ThrowsExceptions ()
		{
			AssertRuleFailure<BadRule1> ("CheckMethod", 1);
			AssertRuleFailure<BadRule2> ("set_RuleProperty", 1);
			AssertRuleFailure<BadRule2> ("set_RulePrivateProperty", 1);
		}
	}
}
