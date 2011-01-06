// 
// Test.Rules.Gendarme.ReviewAttributesOnRulesTest
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
using System.ComponentModel;

using Mono.Cecil;
using Gendarme.Rules.Gendarme;
using Gendarme.Framework;
using Gendarme.Framework.Engines;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace Test.Rules.Gendarme {

	[TestFixture]
	public class ReviewAttributesOnRulesTest : TypeRuleTestFixture<ReviewAttributesOnRulesRule> {

		class Case1 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		class Case2 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem (null)]
		class Case3 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Solution ("")]
		class Case4 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("")]
		[Solution (null)]
		class Case5 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution (null)]
		class Case6 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		class Case7 {
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		class Case8 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		abstract class Case9 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		abstract class Case10 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		class Case11 : Case10 {
		}

		[Test]
		public void ProblemSolution ()
		{
			AssertRuleFailure<Case1> (1); // required attributes missing
			AssertRuleFailure<Case2> (1); // Solution attribute missing
			AssertRuleFailure<Case3> (2); // null argument and Solution attribute missing
			AssertRuleFailure<Case4> (2); // empty string argument and Problem attribute missing
			AssertRuleFailure<Case5> (2); // null and empty string arguments
			AssertRuleFailure<Case6> (1); // null argument on Solution attribute
			AssertRuleFailure<Case7> (2); // two attributes are used on non-rule

			AssertRuleSuccess<Case8> (); // concrete rule has both attributes with correct arguments
			AssertRuleSuccess<Case9> (); // abstract rule has no attributes
			AssertRuleSuccess<Case10> (); // abstract rule has both required attributes
			AssertRuleSuccess<Case11> (); // concrete rule inherits attributes from abstract rule

		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[FxCopCompatibility ("", null)]
		class Case12 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[FxCopCompatibility ("Microsoft.Test", "12345")]
		class Case13 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}


		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[FxCopCompatibility ("Microsoft.Test", "CA12345")]
		class Case14 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[FxCopCompatibility ("Microsoft.Test", "CA1234")]
		class Case15 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[FxCopCompatibility ("Microsoft.Test", "CA1234:RuleName")]
		class Case16 {
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[FxCopCompatibility ("Microsoft.Test", "CA1234:RuleName")]
		class Case17 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}



		[Test]
		public void FxCopCompatibility ()
		{
			AssertRuleFailure<Case12> (1); // arguments has null or empty string values
			AssertRuleFailure<Case13> (1); // second argument has wrong format
			AssertRuleFailure<Case14> (1); // second argument has wrong format
			AssertRuleFailure<Case15> (1); // second argument has correct format but no FxCop rule name is provided
			AssertRuleFailure<Case16> (1); // attribute used on non-rule

			AssertRuleSuccess<Case17> (); // arguments have correct formats and FxCop rule name is provided
		}

		[EngineDependency (typeof (OpCodeEngine))]
		class Case18 {
		}


		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[EngineDependency ("")]
		class Case19 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[EngineDependency (typeof (ReviewAttributesOnRulesRule))]
		class Case20 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[EngineDependency (typeof (OpCodeEngine))]
		class Case21 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[EngineDependency ("Gendarme.Framework.Engines.OpCodeEngine")]
		class Case22 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void EngineDependency ()
		{
			AssertRuleFailure<Case18> (1); // attribute used on non-rule
			AssertRuleFailure<Case19> (1); // empty string argument
			AssertRuleFailure<Case20> (1); // argument does not inherit from Engine

			AssertRuleSuccess<Case21> (); // argument type inherit from Engine
			AssertRuleSuccess<Case22> (); // non-empty string argument provided
		}

		[DocumentationUri ("http://www.mono-project.com/Gendarme")]
		class Case23 {
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[DocumentationUri ("http://www.mono-project.com/Gendarme")]
		class Case24 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void DocumentationUri ()
		{
			AssertRuleFailure<Case23> (1); // attribute used on non-rule

			AssertRuleSuccess<Case24> (); // attribute used on rule
		}


		[Problem ("Problem description")]
		[Solution ("Solution description")]
		[System.ComponentModel.Description ("Description")]
		[DefaultValue (null)]
		class Case25 : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		class Case26 {
			[System.ComponentModel.Description ("Description")]
			[DefaultValue (null)]
			public int Property { get; set; }
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		class Case27 : Rule, IMethodRule {
			[System.ComponentModel.Description ("")]
			[DefaultValue (null)]
			public int Property { get; set; }

			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		class Case28 : Rule, IMethodRule {
			[System.ComponentModel.Description ("Description")]
			[DefaultValue (null)]
			private int Property { get; set; }

			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Problem ("Problem description")]
		[Solution ("Solution description")]
		class Case29 : Rule, IMethodRule {
			[System.ComponentModel.Description ("Description")]
			[DefaultValue (42)]
			public int Property { get; set; }

			public RuleResult CheckMethod (MethodDefinition method)
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void DescriptionDefaultValue ()
		{
			AssertRuleFailure<Case25> (2); // two attributes used in wrong place (on rule)
			AssertRuleFailure<Case26> (2); // two attributes used in wrong place (on non-rule property)
			AssertRuleFailure<Case27> (1); // Description argument is an empty string
			AssertRuleFailure<Case28> (2); // two attributes used on private property

			AssertRuleSuccess<Case29> (); // both attributes are used in correct place and Desciprion argument is not empty
		}

		[Test]
		public void Runner ()
		{
			AssertRuleSuccess<TestRunner> ();
		}
	}
}
