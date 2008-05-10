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
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Performance;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Performance {

	[TestFixture]
	public class UseIsOperatorTest {

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

		private void ConditionSplitBad (object value)
		{
			UseIsOperatorTest test = (value as UseIsOperatorTest);
			// 'test' is unused after the test
			if (test != null) {
				Console.WriteLine ("Bad");
			}
		}

		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Performance.UseIsOperatorTest"];
			rule = new UseIsOperatorRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition md in type.Methods) {
				if (md.Name == name)
					return md;
			}
			Assert.Fail ("Method '{0}' not found.");
			return null;
		}

		[Test]
		public void Return ()
		{
			MethodDefinition method = GetTest ("ReturnEqualityBad");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "ReturnEqualityBad");

			method = GetTest ("ReturnInequalityBad");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "ReturnInequalityBad");

			method = GetTest ("ReturnEqualityOk");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "ReturnEqualityOk");

			method = GetTest ("ReturnInequalityOk");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "ReturnInequalityOk");
		}

		[Test]
		public void Conditions ()
		{
			MethodDefinition method = GetTest ("ConditionIsOk");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "ConditionIsOk");

			method = GetTest ("ConditionAsOk");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "ConditionAsOk");
		}

		[Test]
		[Ignore ("Compiler optimization (default for [g]mcs) can fix this")]
		public void ConditionsOptimized ()
		{
			// missed opportunities are less problematic than false positives ;-)
			MethodDefinition method = GetTest ("ConditionEqualityBad");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "ConditionEqualityBad");

			method = GetTest ("ConditionInequalityBad");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "ConditionInequalityBad");

			method = GetTest ("ConditionSplitBad");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "ConditionSplitBad");
		}
	}
}
