// 
// Gendarme.Rules.NUnit.TestMethodsMustBePublicRule
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
	/// Test method (a method, marked with either TestAttribute, TestCaseAttribute 
	/// or TestCaseSourceAttribute) is not public. Most NUnit test runners won't 
	/// execute non-public unit tests.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// [Test]
	/// private void TestMethod () 
	/// {
	///	Assert.AreEqual (10, 20);
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public void TestMethod ()
	/// {
	///	Assert.AreEqual (10, 20);
	/// }
	/// </code>
	/// </example>

	[Problem ("Test method is not marked as public, which means it will not be executed by most test runners.")]
	[Solution ("Change method visibility to public.")]
	public class TestMethodsMustBePublicRule : NUnitRule, IMethodRule {
		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.IsTest ())
				return RuleResult.DoesNotApply;

			if (!method.IsPublic)
				Runner.Report (method, Severity.Critical, Confidence.High);

			return Runner.CurrentRuleResult;
		}
	}
}
