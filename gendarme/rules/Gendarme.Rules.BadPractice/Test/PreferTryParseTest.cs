// 
// Unit tests for PreferTryParseRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

using Gendarme.Framework;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class PreferTryParseTest : MethodRuleTestFixture<PreferTryParseRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no CALL[VIRT] instruction
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		class ParseWithTryCatch {
			private int Int32Parse ()
			{
				try {
					return Int32.Parse ("12") + Int32.Parse ("-12", NumberStyles.AllowCurrencySymbol);
				}
				catch {
					return 0;
				}
			}
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ParseWithTryCatch> ("Int32Parse", 2);
			Assert.AreEqual (Severity.Medium, Runner.Defects [0].Severity, "Severity");
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "Confidence");
		}

		class ParseWithoutTryCatch {
			private int Int32Parse ()
			{
				return Int32.Parse ("12") + Int32.Parse ("-12", NumberStyles.AllowCurrencySymbol);
			}
		}

		class ParseOutsideTryCatch {
			private int Int32Parse (string s)
			{
				try {
					if (String.IsNullOrEmpty (s))
						return 0;

					return (int) (s [0] - '0');
				}
				catch (Exception) {
					return Int32.Parse (s);
				}
			}
		}

		[Test]
		public void Worse ()
		{
			AssertRuleFailure<ParseWithoutTryCatch> ("Int32Parse", 2);
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "Severity-1");
			Assert.AreEqual (Confidence.High, Runner.Defects [0].Confidence, "Confidence-1");

			AssertRuleFailure<ParseOutsideTryCatch> ("Int32Parse", 1);
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "Severity-2");
			Assert.AreEqual (Confidence.High, Runner.Defects [0].Confidence, "Confidence-2");
		}

		class TryParse {
			private int Int32Parse ()
			{
				int i;
				if (!Int32.TryParse ("12", out i))
					i = 0;
				return i;
			}
		}

		class AtypicalParse {
			private bool Parse ()
			{
				return true;
			}

			public void CallParse ()
			{
				if (!Parse ())
					Console.WriteLine ();
			}
		}

		class NoTryParse {
			static NoTryParse Parse (string s)
			{
				return new NoTryParse ();
			}

			public void CallParse ()
			{
				NoTryParse.Parse (null);
			}
		}

		class BadTryParse {
			// bad candidate - does not return 'bool'
			static void TryParse (string s, out BadTryParse btp)
			{
				btp = new BadTryParse ();
			}

			// bad candidate - first parameter is not 'string'
			static bool TryParse (char c, out BadTryParse btp)
			{
				btp = new BadTryParse ();
				return true;
			}

			// bad candidate - last parameter is not 'out <type>'
			static bool TryParse (string s, BadTryParse btp)
			{
				btp = new BadTryParse ();
				return true;
			}

			static BadTryParse Parse (string s)
			{
				return new BadTryParse ();
			}

			public void CallParse ()
			{
				BadTryParse btp = null;
				BadTryParse.TryParse (String.Empty, out btp);
				BadTryParse.TryParse ('a', out btp);
				BadTryParse.TryParse (String.Empty, btp);
				// below is valid since there was no valid TryParse cnadidate
				BadTryParse.Parse (null);
			}
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<TryParse> ("Int32Parse");
			AssertRuleSuccess<AtypicalParse> ("CallParse");
			AssertRuleSuccess<NoTryParse> ("CallParse");
			AssertRuleSuccess<BadTryParse> ("CallParse");
		}
	}
}

