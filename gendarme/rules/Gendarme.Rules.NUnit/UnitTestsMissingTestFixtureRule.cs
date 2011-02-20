// 
// Gendarme.Rules.NUnit.UnitTestsMissingTestFixtureRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.NUnit {

	/// <summary>
	/// This rule checks that all types which have methods with TestAttribute, TestCaseAttribute
	/// or TestCaseSourceAttribute are marked with the TestFixtureAttribute. NUnit &lt; 2.5 will not run
	/// tests located in types without TestFixtureAttribute.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class Test {
	///	[Test]
	///	public void TestMethod ()
	///	{
	///		Assert.AreEqual (0, 0);
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// [TestFixture]
	/// class Test {
	///	[Test]
	///	public void TestMethod ()
	///	{
	///		Assert.AreEqual (0, 0);
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("Type contains methods with Test, TestCase or TestCaseSource attributes, but the type itself is not marked with the TestFixture attribute")]
	[Solution ("Add a TestFixture attribute to the type")]
	public class UnitTestsMissingTestFixtureRule : NUnitRule, ITypeRule {

		private Version Version25 = new Version (2, 5);

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsClass || type.IsAbstract || !type.HasMethods)
				return RuleResult.DoesNotApply;

			// check if TestFixture is applied to any type in the hierarchy
			TypeDefinition testingType = type;
			while (testingType != null) {
				if (testingType.HasAttribute ("NUnit.Framework", "TestFixtureAttribute"))
					return RuleResult.Success;
				if (testingType.BaseType != null)
					testingType = testingType.BaseType.Resolve ();
				else
					break;
			}

			foreach (MethodDefinition method in type.Methods) {
				if (method.IsTest ()) {
					Severity severity = (NUnitVersion == null || NUnitVersion < Version25) ? Severity.High : Severity.Low;
					Runner.Report (method, severity, Confidence.High);
					return RuleResult.Failure;
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
