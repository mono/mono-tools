//
// Unit tests for PreferCharOverloadRule
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class PreferCharOverloadTest : MethodRuleTestFixture<PreferCharOverloadRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no body
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no call to methods
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public bool StringIndexOf_Char (string s)
		{
			return (s.IndexOf ('b') >= 0);
		}

		public bool StringIndexOf_ShortString (string s)
		{
			return (s.IndexOf ("b") >= 0);
		}

		public bool StringIndexOf_LongString (string s)
		{
			return (s.IndexOf ("bc") >= 0);
		}

		public bool StringIndexOf_UnknownString (string s)
		{
			return ("abc".IndexOf (s) >= 0);
		}

		public bool StringIndexOf_ShortStringInt (string s)
		{
			return (s.IndexOf ("b", 1) >= 0);
		}

		public bool StringIndexOf_LongStringInt (string s)
		{
			return (s.IndexOf ("bc", 2) >= 0);
		}

		public bool StringIndexOf_UnknownStringInt (string s)
		{
			return ("abc".IndexOf (s, 3) >= 0);
		}

		public bool StringIndexOf_ShortStringIntInt (string s)
		{
			int pos = "abc".IndexOf ("b", 1, 2);
			return (s.IndexOf ("a", pos, 1) == -1);
		}

		public bool StringIndexOf_LongStringIntInt (string s)
		{
			int pos = "abc".IndexOf ("bc", 1, 2);
			return (s.IndexOf ("ab", pos, 1) == -1);
		}

		public bool StringIndexOf_UnknownStringIntInt (string s)
		{
			int pos = "abc".IndexOf (s, 1, 2);
			return (s.IndexOf (s, pos, 1) == -1);
		}

		[Test]
		public void StringIndexOf ()
		{
			AssertRuleSuccess<PreferCharOverloadTest> ("StringIndexOf_Char");

			AssertRuleFailure<PreferCharOverloadTest> ("StringIndexOf_ShortString", 1);
			AssertRuleSuccess<PreferCharOverloadTest> ("StringIndexOf_LongString");
			AssertRuleSuccess<PreferCharOverloadTest> ("StringIndexOf_UnknownString");

			AssertRuleFailure<PreferCharOverloadTest> ("StringIndexOf_ShortStringInt", 1);
			AssertRuleSuccess<PreferCharOverloadTest> ("StringIndexOf_LongStringInt");
			AssertRuleSuccess<PreferCharOverloadTest> ("StringIndexOf_UnknownStringInt");

			AssertRuleFailure<PreferCharOverloadTest> ("StringIndexOf_ShortStringIntInt", 2);
			AssertRuleSuccess<PreferCharOverloadTest> ("StringIndexOf_LongStringIntInt");
			AssertRuleSuccess<PreferCharOverloadTest> ("StringIndexOf_UnknownStringIntInt");
		}

		public bool StringLastIndexOf_Char (string s)
		{
			return (s.LastIndexOf ('b') >= 0);
		}

		public bool StringLastIndexOf_ShortString (string s)
		{
			return (s.LastIndexOf ("b") >= 0);
		}

		public bool StringLastIndexOf_LongString (string s)
		{
			return (s.LastIndexOf ("bc") >= 0);
		}

		public bool StringLastIndexOf_UnknownString (string s)
		{
			return ("abc".LastIndexOf (s) >= 0);
		}

		public bool StringLastIndexOf_ShortStringInt (string s)
		{
			return (s.LastIndexOf ("b", 1) >= 0);
		}

		public bool StringLastIndexOf_LongStringInt (string s)
		{
			return (s.LastIndexOf ("bc", 2) >= 0);
		}

		public bool StringLastIndexOf_UnknownStringInt (string s)
		{
			return ("abc".LastIndexOf (s, 3) >= 0);
		}

		public bool StringLastIndexOf_ShortStringIntInt (string s)
		{
			int pos = "abc".LastIndexOf ("b", 1, 2);
			return (s.LastIndexOf ("a", pos, 1) == -1);
		}

		public bool StringLastIndexOf_LongStringIntInt (string s)
		{
			int pos = "abc".LastIndexOf ("bc", 1, 2);
			return (s.LastIndexOf ("ab", pos, 1) == -1);
		}

		public bool StringLastIndexOf_UnknownStringIntInt (string s)
		{
			int pos = "abc".LastIndexOf (s, 1, 2);
			return (s.LastIndexOf (s, pos, 1) == -1);
		}

		[Test]
		public void StringLastIndexOf ()
		{
			AssertRuleSuccess<PreferCharOverloadTest> ("StringLastIndexOf_Char");

			AssertRuleFailure<PreferCharOverloadTest> ("StringLastIndexOf_ShortString", 1);
			AssertRuleSuccess<PreferCharOverloadTest> ("StringLastIndexOf_LongString");
			AssertRuleSuccess<PreferCharOverloadTest> ("StringLastIndexOf_UnknownString");

			AssertRuleFailure<PreferCharOverloadTest> ("StringLastIndexOf_ShortStringInt", 1);
			AssertRuleSuccess<PreferCharOverloadTest> ("StringLastIndexOf_LongStringInt");
			AssertRuleSuccess<PreferCharOverloadTest> ("StringLastIndexOf_UnknownStringInt");

			AssertRuleFailure<PreferCharOverloadTest> ("StringLastIndexOf_ShortStringIntInt", 2);
			AssertRuleSuccess<PreferCharOverloadTest> ("StringLastIndexOf_LongStringIntInt");
			AssertRuleSuccess<PreferCharOverloadTest> ("StringLastIndexOf_UnknownStringIntInt");
		}

		public string ReplaceString_Char (string s)
		{
			return s.Replace ('a', 'b');
		}

		public string ReplaceString_ShortString (string s)
		{
			return s.Replace ("a", "b");
		}

		public string ReplaceString_LongString (string s)
		{
			return s.Replace ("ab", "ba");
		}

		public string ReplaceString_ShortLongString (string s)
		{
			return s.Replace ("a", "ba");
		}

		public string ReplaceString_UnknownShortString (string s)
		{
			return "abc".Replace (s, "b");
		}

		public string ReplaceString_UnknownLongString (string s)
		{
			return "abc".Replace (s, "ba");
		}

		[Test]
		public void StringReplace ()
		{
			AssertRuleSuccess<PreferCharOverloadTest> ("ReplaceString_Char");
			AssertRuleFailure<PreferCharOverloadTest> ("ReplaceString_ShortString", 1);
			AssertRuleSuccess<PreferCharOverloadTest> ("ReplaceString_LongString");
			AssertRuleSuccess<PreferCharOverloadTest> ("ReplaceString_ShortLongString");
			AssertRuleSuccess<PreferCharOverloadTest> ("ReplaceString_UnknownShortString");
			AssertRuleSuccess<PreferCharOverloadTest> ("ReplaceString_UnknownLongString");
		}

		public bool StringIndexOf_CurrentCulture (string s)
		{
			// a char would be compared as an ordinal value
			return (s.IndexOf ("a", StringComparison.CurrentCulture) == -1);
		}

		public bool LastStringIndexOf_CurrentCultureIgnoreCase (string s)
		{
			// a char would be compared as an ordinal value
			return (s.LastIndexOf ("a", StringComparison.CurrentCultureIgnoreCase) == -1);
		}

		public bool StringIndexOf_InvariantCulture (string s)
		{
			// a char would be compared as an ordinal value
			return (s.IndexOf ("a", StringComparison.InvariantCulture) == -1);
		}

		public bool LastStringIndexOf_InvariantCultureIgnoreCase (string s)
		{
			// a char would be compared as an ordinal value
			return (s.LastIndexOf ("a", StringComparison.InvariantCultureIgnoreCase) == -1);
		}

		public bool StringIndexOf_Ordinal (string s)
		{
			// a char would be compared as an ordinal value
			return (s.IndexOf ("a", StringComparison.Ordinal) == -1);
		}

		public bool LastStringIndexOf_OrdinalIgnoreCase (string s)
		{
			// a char would be compared as an ordinal value and this string
			// value is not case sensitive
			return (s.LastIndexOf ("_", StringComparison.OrdinalIgnoreCase) == -1);
		}

		[Test]
		public void StringComparisonChecks ()
		{
			AssertRuleSuccess<PreferCharOverloadTest> ("StringIndexOf_CurrentCulture");
			AssertRuleSuccess<PreferCharOverloadTest> ("LastStringIndexOf_CurrentCultureIgnoreCase");

			AssertRuleSuccess<PreferCharOverloadTest> ("StringIndexOf_InvariantCulture");
			AssertRuleSuccess<PreferCharOverloadTest> ("LastStringIndexOf_InvariantCultureIgnoreCase");

			AssertRuleFailure<PreferCharOverloadTest> ("StringIndexOf_Ordinal", 1);
			AssertRuleFailure<PreferCharOverloadTest> ("LastStringIndexOf_OrdinalIgnoreCase", 1);
		}

		class String {
			void LastIndexOf ()
			{
			}

			void TestLastIndexOf ()
			{
				String s = new String ();
				s.LastIndexOf ();
			}

			void Replace (string s)
			{
			}

			void TestReplace ()
			{
				String s = new String ();
				s.Replace ("a");
			}
		}

		[Test]
		public void SpecialCases ()
		{
			AssertRuleSuccess<String> ("TestLastIndexOf");
			AssertRuleSuccess<String> ("TestReplace");
		}
	}
}
