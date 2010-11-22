// 
// Unit tests for MissingAttributeUsageOnCustomAttributeRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) Daniel Abramov
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Design {

	internal class NotAttribute {
	}

	internal class NoUsageDefinedAttribute : Attribute {
	}

	[AttributeUsage (AttributeTargets.Method)]
	internal class UsageDefinedInheritsNoUsageDefinedAttribute : NoUsageDefinedAttribute {
	}

	[AttributeUsage (AttributeTargets.Method)]
	internal class UsageDefinedAttribute : Attribute {
	}

	internal class NoUsageDefinedInheritsUsageDefinedAttribute : UsageDefinedAttribute {
	}


	[TestFixture]
	public class MissingAttributeUsageOnCustomAttributeTest {

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			rule = new MissingAttributeUsageOnCustomAttributeRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.GetType (typeof (T).FullName);
		}

		[Test]
		public void TestNotAttribute ()
		{
			TypeDefinition type = GetTest<NotAttribute> ();
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNoUsageDefinedAttribute ()
		{
			TypeDefinition type = GetTest<NoUsageDefinedAttribute> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNoUsageDefinedInheritsUsageDefinedAttribute ()
		{
			TypeDefinition type = GetTest<NoUsageDefinedInheritsUsageDefinedAttribute> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestUsageDefinedAttribute ()
		{
			TypeDefinition type = GetTest<UsageDefinedAttribute> ();
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestUsageDefinedInheritsNoUsageDefinedAttribute ()
		{
			TypeDefinition type = GetTest<UsageDefinedInheritsNoUsageDefinedAttribute> ();
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
