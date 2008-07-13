//
// Unit tests for AvoidRefAndOutParametersRule
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

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class AvoidRefAndOutParametersTest : MethodRuleTestFixture<AvoidRefAndOutParametersRule> {

		private bool PrivateByReference (ref DateTime date)
		{
			date = date.AddDays (1);
			return date.IsDaylightSavingTime ();
		}

		internal void InternalOut (string input, out string output)
		{
			output = input;
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<AvoidRefAndOutParametersTest> ("PrivateByReference");
			AssertRuleDoesNotApply<AvoidRefAndOutParametersTest> ("InternalOut");
		}

		public bool PublicByReference (ref DateTime date)
		{
			date = date.AddDays (1);
			return date.IsDaylightSavingTime ();
		}

		[Test]
		public void Reference ()
		{
			AssertRuleFailure<AvoidRefAndOutParametersTest> ("PublicByReference", 1);
			Assert.AreEqual (Severity.Medium, Runner.Defects [0].Severity, "Severity");
		}

		protected void ProtectedOut (string input, out string output)
		{
			output = input;
		}

		public bool TrySomething (string something, out DateTime date)
		{
			date = DateTime.ParseExact (something, null, null);
			return date.IsDaylightSavingTime ();
		}

		[Test]
		public void Out ()
		{
			AssertRuleFailure<AvoidRefAndOutParametersTest> ("ProtectedOut", 1);
			Assert.AreEqual (Severity.Low, Runner.Defects [0].Severity, "Severity");
			AssertRuleSuccess<AvoidRefAndOutParametersTest> ("TrySomething");
		}
	}
}
