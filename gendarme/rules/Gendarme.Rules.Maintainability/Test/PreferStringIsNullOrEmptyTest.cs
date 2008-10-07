// 
// Unit tests for PreferStringIsNullOrEmptyRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Maintainability;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Maintainability {

	[TestFixture]
	public class PreferStringIsNullOrEmptyTest : MethodRuleTestFixture<PreferStringIsNullOrEmptyRule> {

		public bool ArgumentNullAndLengthCheck (string s)
		{
			return ((s == null) || (s.Length == 0));
		}

		public bool ArgumentIsNullOrEmpty (string s)
		{
			return String.IsNullOrEmpty (s);
		}

		[Test]
		public void Arguments ()
		{
			AssertRuleFailure<PreferStringIsNullOrEmptyTest> ("ArgumentNullAndLengthCheck");
			AssertRuleSuccess<PreferStringIsNullOrEmptyTest> ("ArgumentIsNullOrEmpty");
		}

		private string str;

		public bool FieldNullAndLengthCheck ()
		{
			return ((str == null) || (str.Length == 0));
		}

		public bool FieldIsNullOrEmpty ()
		{
			return String.IsNullOrEmpty (str);
		}

		[Test]
		public void Fields ()
		{
			AssertRuleFailure<PreferStringIsNullOrEmptyTest> ("FieldNullAndLengthCheck");
			AssertRuleSuccess<PreferStringIsNullOrEmptyTest> ("FieldIsNullOrEmpty");
		}

		public bool FieldIsNotNullAndNotEmpty ()
		{
			return ((str != null) && (str.Length >= 0));
		}

		[Test]
		[Ignore ("we don't support the inverted condition, i.e. not null and not empty")]
		public void Field_InvertedCondition ()
		{
			AssertRuleFailure<PreferStringIsNullOrEmptyTest> ("FieldIsNotNullAndNotEmpty");
		}

		private static string static_str;

		public void StaticFieldNullAndLengthCheck ()
		{
			if ((static_str == null) || (static_str.Length == 0))
				Console.WriteLine ("Empty");
		}

		public void StaticFieldIsNullOrEmpty ()
		{
			if (String.IsNullOrEmpty (static_str))
				Console.WriteLine ("Empty");
		}

		[Test]
		public void StaticFields ()
		{
			AssertRuleFailure<PreferStringIsNullOrEmptyTest> ("StaticFieldNullAndLengthCheck");
			AssertRuleSuccess<PreferStringIsNullOrEmptyTest> ("StaticFieldIsNullOrEmpty");
		}

		public bool LocalNullAndLengthCheck ()
		{
			string s = String.Format ("{0}", 1);
			return ((s == null) || (s.Length == 0));
		}

		public bool LocalIsNullOrEmpty ()
		{
			string s = String.Format ("{0}", 1);
			return String.IsNullOrEmpty (s);
		}

		[Test]
		public void Locals ()
		{
			AssertRuleFailure<PreferStringIsNullOrEmptyTest> ("LocalNullAndLengthCheck");
			AssertRuleSuccess<PreferStringIsNullOrEmptyTest> ("LocalIsNullOrEmpty");
		}

		public static void StaticLocalNullAndLengthCheck ()
		{
			string s = String.Format ("{0}", 1);
			if ((s == null) || (s.Length == 0))
				Console.WriteLine ("Empty");
		}

		public static void StaticLocalIsNullOrEmpty ()
		{
			string s = String.Format ("{0}", 1);
			if (String.IsNullOrEmpty (s))
				Console.WriteLine ("Empty");
		}

		[Test]
		public void StaticLocals ()
		{
			AssertRuleFailure<PreferStringIsNullOrEmptyTest> ("StaticLocalNullAndLengthCheck");
			AssertRuleSuccess<PreferStringIsNullOrEmptyTest> ("StaticLocalIsNullOrEmpty");
		}

		// two different checks are ok if we're doing different stuff, like throwing exceptions, based on each one
		public void CommonExceptionPattern (string s)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
			if (s.Length == 0)
				throw new ArgumentException ("empty");
		}

		public void ConfusingStringInstance (string s1, string s2)
		{
			// this is bad code but it's not what the rule looks for
			if ((s1 == null) || (s2.Length == 0))
				throw new InvalidOperationException ("confusing s1 and s2!");
		}

		[Test]
		public void Others ()
		{
			AssertRuleSuccess<PreferStringIsNullOrEmptyTest> ("CommonExceptionPattern");
			AssertRuleSuccess<PreferStringIsNullOrEmptyTest> ("ConfusingStringInstance");
		}

		public static bool StringIsStrictlyEmpty (string value)
		{
			return value != null && value.Length == 0;
		}

		public void ThereShouldBeNoStringIsNullOrEmptySuggestionHere (string realm)
		{
			if (realm != null && realm.Length == 0)
				throw new ArgumentException ();
			Console.WriteLine (realm);
		}

		[Test]
		[Ignore ("FIXME: false positive")]
		public void StrictlyEmpty ()
		{
			AssertRuleSuccess<PreferStringIsNullOrEmptyTest> ("StringIsStrictlyEmpty");
			AssertRuleSuccess<PreferStringIsNullOrEmptyTest> ("ThereShouldBeNoStringIsNullOrEmptySuggestionHere");
		}
	}
}
