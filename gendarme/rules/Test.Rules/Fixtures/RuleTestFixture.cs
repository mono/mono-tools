//
// Test.Rules.Fixtures.RuleTestFixture<T>
// Base class for rule test fixtures that simplifies writing unit tests for Gendarme.
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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
using System.Reflection;

using Gendarme.Framework;
using Test.Rules.Helpers;

using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Fixtures {
	
	/// <summary>
	/// Base class inRuleTestFixture helpers hierarchy providing methods for making assertions on rule execution results.
	/// </summary>
	/// <typeparam name="TRule">Type of rule to be tested (e.g. IMethodRule, ITypeRule).</typeparam>
	/// <typeparam name="TMetadataToken">Type of Cecil metadata token that TRule handles (e.g. MethodDefinition for IMethodRule).</typeparam>
	public abstract class RuleTestFixture<TRule, TMetadataToken>
		where TRule : IRule, new () {

		private TRule rule;
		private TestRunner runner;

		public TRule Rule {
			get { return rule; }
		}

		public Runner Runner {
			get { return runner; }
		}

		public bool FireEvents { get; set; }

		/// <summary>
		/// Creates a RuleTestFixture instance, the runner and the rule itself.
		/// </summary>
		protected RuleTestFixture ()		{
			rule = new TRule ();
			runner = new TestRunner (rule);
		}
		
		/// <summary>
		/// Asserts that the rule does not apply to a particular token. 
		/// </summary>
		/// <param name="token">Cecil metadata token to check.</param>
		protected void AssertRuleDoesNotApply (TMetadataToken token)
		{
			RunRuleAndCheckResults (token, RuleResult.DoesNotApply, null);
		}		
		
		/// <summary>
		/// Asserts that the rule has been executed successfully. 
		/// </summary>
		/// <param name="token">Cecil metadata token to check.</param>
		protected void AssertRuleSuccess (TMetadataToken token)
		{
			RunRuleAndCheckResults (token, RuleResult.Success, null);
		}		
						
		/// <summary>
		/// Asserts that the rule has failed to execute successfully. 
		/// </summary>
		/// <param name="token">Cecil metadata token to check.</param>
		protected void AssertRuleFailure (TMetadataToken token)
		{
			RunRuleAndCheckResults (token, RuleResult.Failure, null);
		}

		/// <summary>
		/// Asserts that the rule has failed to execute successfully. 
		/// </summary>
		/// <param name="token">Cecil metadata token to check.</param>
		/// <param name="expectedCount">Expected defects count.</param>
		protected void AssertRuleFailure (TMetadataToken token, int expectedCount)
		{
			RunRuleAndCheckResults (token, RuleResult.Failure, expectedCount);
		}
					
		/// <summary>
		/// Runs the rule and checks the results against the specified matcher.
		/// </summary>
		private void RunRuleAndCheckResults (TMetadataToken token, RuleResult expectedResult, int? expectedDefectCount)
		{
			RuleResult result = RunRule (token);
			Assert.AreEqual (expectedResult, result, "{0} failed on {1}: result should be {2} but got {3}.", 
				typeof (TRule).Name, token, expectedResult, result);

			if (expectedDefectCount.HasValue) {
				Assert.AreEqual (expectedDefectCount.Value, runner.Defects.Count, 
					"{0} failed on {1}: should have {2} defects but got {3}.", 
					typeof (TRule).Name, token, expectedDefectCount.Value, runner.Defects.Count);
			}
		}
						
		/// <summary>
		/// Runs the rule, depending on its type.
		/// </summary>
		private RuleResult RunRule (TMetadataToken token)
		{
			if (token == null)
				throw new ArgumentNullException ("token");

			MethodDefinition md = (token as MethodDefinition);
			if (md != null) {
				if (FireEvents) {
					runner.OnAssembly (md.DeclaringType.Module.Assembly);
					runner.OnType (md.DeclaringType);
					runner.OnMethod (md);
				}
				return runner.CheckMethod (token as MethodDefinition);
			}

			TypeDefinition td = (token as TypeDefinition);
			if (td != null) {
				if (FireEvents) {
					runner.OnAssembly (md.DeclaringType.Module.Assembly);
					runner.OnType (md.DeclaringType);
				}
				return runner.CheckType (td);
			}

			AssemblyDefinition ad = (token as AssemblyDefinition);
			if (ad != null) {
				if (FireEvents)
					runner.OnAssembly (td.Module.Assembly);
				return runner.CheckAssembly (ad);
			}

			throw new NotImplementedException (token.GetType ().ToString ());
		}
	}
}
