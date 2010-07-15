//
// Unit tests for NewLineLiteralRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006-2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Rules.Portability;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Portability {

	[TestFixture]
	public class NewLineLiteralTest : MethodRuleTestFixture<NewLineLiteralRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		public string GetNewLineLiteral_13 ()
		{
			return "Hello\nMono";
		}

		public string GetNewLineLiteral_10 ()
		{
			return "\rHello Mono";
		}

		public string GetNewLineLiteral ()
		{
			return "Hello Mono\r\n";
		}

		public string Tab ()
		{
			return "\tHello Mono\r\n";
		}

		public string Control ()
		{
			return "\xbHello Mono\r\n";
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<NewLineLiteralTest> ("GetNewLineLiteral_13", 1);
			AssertRuleFailure<NewLineLiteralTest> ("GetNewLineLiteral_10", 1);
			AssertRuleFailure<NewLineLiteralTest> ("GetNewLineLiteral", 1);

			AssertRuleFailure<NewLineLiteralTest> ("Tab", 1);
			Assert.IsTrue (Runner.Defects [0].Text.Contains ("\\t"), "visible tab");

			AssertRuleFailure<NewLineLiteralTest> ("Control", 1);
			Assert.IsTrue (Runner.Defects [0].Text.Contains ("\\xb"), "visible 0xb");
		}

		public string GetNewLine ()
		{
			return String.Concat ("Hello Mono", Environment.NewLine);
		}

		public string GetNull ()
		{
			return null;
		}

		public string GetEmpty ()
		{
			// note: this does a LDSTR with CSC and a LDSFLD String.Empty 
			// with [g]mcs when optimizations are enabled
			return "";
		}

		[Test]
		public void Correct ()
		{
			AssertRuleSuccess<NewLineLiteralTest> ("GetNewLine");
			// see note ^^^
			AssertRuleSuccess<NewLineLiteralTest> ("GetEmpty");
			// no LDSTR instruction (LDNULL is used)
			AssertRuleDoesNotApply<NewLineLiteralTest> ("GetNull");
		}
	}
}
