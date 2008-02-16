//
// Unit test for DoNotCompareWithNaNRule
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
using Gendarme.Rules.Correctness;

using Mono.Cecil;

using NUnit.Framework;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class DoNotCompareWithNaNTest {

		public class SingleCases {

			public bool EqualityOperatorLeft (float a)
			{
				return (Single.NaN == a);
			}

			public bool EqualityOperatorRight (float a)
			{
				return (a == Single.NaN);
			}

			public bool Equality (float a, float b)
			{
				// note: ok for this rule (not for another one)
				return (a == b);
			}

			public bool InequalityOperatorLeft (float a)
			{
				return (Single.NaN != a);
			}

			public bool InequalityOperatorRight (float a)
			{
				return (a != Single.NaN);
			}

			public bool Inequality (float a, float b)
			{
				// note: ok for this rule (not for another one)
				return (a != b);
			}

			public bool NaNEquals (float a)
			{
				return Single.NaN.Equals (a);
			}

			public bool EqualsNaN (float a)
			{
				return a.Equals (Single.NaN);
			}

			public bool Equals (float a, float b)
			{
				// note: ok for this rule (not for another one)
				return (a.Equals (b) && b.Equals (a));
			}
		}

		public class DoubleCases {

			public bool EqualityOperatorLeft (double a)
			{
				return (Double.NaN == a);
			}

			public bool EqualityOperatorRight (double a)
			{
				return (a == Double.NaN);
			}

			public bool Equality (double a, double b)
			{
				// note: ok for this rule (not for another one)
				return (a == b);
			}

			public bool InequalityOperatorLeft (double a)
			{
				return (Double.NaN != a);
			}

			public bool InequalityOperatorRight (double a)
			{
				return (a != Double.NaN);
			}

			public bool Inequality (double a, double b)
			{
				// note: ok for this rule (not for another one)
				return (a != b);
			}

			public bool NaNEquals (double a)
			{
				return Double.NaN.Equals (a);
			}

			public bool EqualsNaN (double a)
			{
				return a.Equals (Double.NaN);
			}

			public bool Equals (double a, double b)
			{
				// note: ok for this rule (not for another one)
				return (a.Equals (b) && b.Equals (a));
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
			rule = new DoNotCompareWithNaNRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string typeName, string name)
		{
			type = assembly.MainModule.Types ["Test.Rules.Correctness.DoNotCompareWithNaNTest/" + typeName + "Cases"];
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			Assert.Fail ("name '{0}' was not found inside '{1}'.", name, typeName);
			return null;
		}

		[Test]
		public void EqualityOperator ()
		{
			MethodDefinition method = GetTest ("Single", "EqualityOperatorLeft");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Single-EqualityOperatorLeft");

			method = GetTest ("Single", "EqualityOperatorRight");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Single-EqualityOperatorRight");

			method = GetTest ("Single", "Equality");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "Single-Equality");

			method = GetTest ("Double", "EqualityOperatorLeft");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Double-EqualityOperatorLeft");

			method = GetTest ("Double", "EqualityOperatorRight");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Double-EqualityOperatorRight");

			method = GetTest ("Double", "Equality");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "Double-Equality");
		}

		[Test]
		public void InequalityOperator ()
		{
			MethodDefinition method = GetTest ("Single", "InequalityOperatorLeft");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Single-InequalityOperatorLeft");

			method = GetTest ("Single", "InequalityOperatorRight");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Single-InequalityOperatorRight");

			method = GetTest ("Single", "Inequality");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "Single-Inequality");

			method = GetTest ("Double", "InequalityOperatorLeft");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Double-InequalityOperatorLeft");

			method = GetTest ("Double", "InequalityOperatorRight");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Double-InequalityOperatorRight");

			method = GetTest ("Double", "Inequality");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "Double-Inequality");
		}

		[Test]
		public void NaNEquals ()
		{
			MethodDefinition method = GetTest ("Single", "NaNEquals");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Single-NaNEquals");

			method = GetTest ("Double", "NaNEquals");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Double-NaNEquals");
		}

		[Test]
		public void EqualsNaN ()
		{
			MethodDefinition method = GetTest ("Single", "EqualsNaN");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Single-EqualsNaN");

			method = GetTest ("Double", "EqualsNaN");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Double-EqualsNaN");
		}

		[Test]
		public void Equals ()
		{
			MethodDefinition method = GetTest ("Single", "Equals");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "Single-Equals");

			method = GetTest ("Double", "Equals");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "Double-Equals");
		}
	}
}
