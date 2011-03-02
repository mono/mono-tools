//
// Unit tests for PreferStringComparisonOverrideRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

using Gendarme.Rules.Globalization;
using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Tests.Rules.Globalization {

	[TestFixture]
	public class PreferStringComparisonOverrideTest : MethodRuleTestFixture<PreferStringComparisonOverrideRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		public class InstanceWithoutOverride {

			public bool Compare (string s1, string s2)
			{
				return false;
			}

			public void Test ()
			{
				if (Compare ("a", "b"))
					Console.WriteLine ();
			}
		}

		public class Base {

			public bool Compare (string s1, string s2)
			{
				return false;
			}

			public void TestBase ()
			{
				if (Compare ("a", "b"))
					Console.WriteLine ();
			}
		}

		public class Inherited : Base {
			public bool Compare (string s1, string s2, StringComparison comparison)
			{
				return true;
			}

			public void TestInherited ()
			{
				// from IL this is a call to Base.Compare so the override is not seen
				// note: fxcop also miss this one
				if (Compare ("a", "b"))
					Console.WriteLine ();
			}
		}

		public class StaticHelperWithoutOverride {

			// no alternative
			public static bool StaticCompare (string s1, string s2)
			{
				return false;
			}

			public void Test ()
			{
				if (StaticCompare ("a", "b"))
					Console.WriteLine ();
			}
		}

		public class StaticHelperWithExtraParameterInOverride {

			public static bool StaticCompare (string s1, string s2)
			{
				return false;
			}

			// the extra parameter disqualify the override
			public static bool StaticCompare (string s1, string s2, bool value, StringComparison comparison)
			{
				return value;
			}

			// the extra parameter disqualify the override
			public static bool StaticCompare (string s1, string s2, StringComparison comparison, bool value)
			{
				return value;
			}

			public void Test ()
			{
				if (StaticCompare ("a", "b"))
					Console.WriteLine ();
			}
		}

		public class Weird {
			public bool Compare (StringComparison a, StringComparison b)
			{
				return (a == b);
			}

			public void Test ()
			{
				if (Compare (StringComparison.CurrentCulture, StringComparison.CurrentCultureIgnoreCase))
					Console.WriteLine ();
			}
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<InstanceWithoutOverride> ("Test");

			AssertRuleSuccess<Base> ("TestBase");
			AssertRuleSuccess<Inherited> ("TestInherited");

			AssertRuleSuccess<StaticHelperWithoutOverride> ("Test");
			AssertRuleSuccess<StaticHelperWithExtraParameterInOverride> ("Test");

			AssertRuleSuccess<Weird> ("Test");
		}

		public class InstanceWithOverride {

			public bool Compare (string s1, string s2)
			{
				return false;
			}

			public bool Compare (string s1, string s2, StringComparison comparison)
			{
				return true;
			}

			// bad
			public void Test ()
			{
				if (Compare ("a", "b"))
					Console.WriteLine ();
			}
		}

		public class StaticHelper {

			public static bool StaticCompare (string s1, string s2)
			{
				return false;
			}

			// we have an alternative
			public static bool StaticCompare (string s1, string s2, StringComparison comparison)
			{
				return true;
			}

			// bad
			public void Test ()
			{
				if (StaticHelper.StaticCompare ("a", "b"))
					Console.WriteLine ();
			}
		}

		public class NonString {

			public bool Kompare (char [] s1, char [] s2)
			{
				return false;
			}

			public bool Kompare (char [] s1, char [] s2, StringComparison comparison)
			{
				return true;
			}

			// bad
			public void TestCharArray ()
			{
				if (Kompare (new char [] { }, new char [] { }))
					Console.WriteLine ();
			}

			public bool KomparInt (int a, int b)
			{
				return false;
			}

			public bool KomparInt (int a, int b, StringComparison comparison)
			{
				return true;
			}

			// bad
			public void TestInt ()
			{
				if (KomparInt (0, 0))
					Console.WriteLine ();
			}
		}

		public class ExtraParameters {

			public bool Kompare (int level, string s1, string s2)
			{
				return false;
			}

			public bool Kompare (int level, string s1, string s2, StringComparison comparison)
			{
				return true;
			}

			// bad
			public void TestExtraFirst ()
			{
				if (Kompare (0, "a", "B"))
					Console.WriteLine ();
			}

			public bool Kompar (string s1, int start, string s2)
			{
				return false;
			}

			public bool Kompar (string s1, int start, string s2, StringComparison comparison)
			{
				return true;
			}

			// bad
			public void TestExtraMid ()
			{
				if (Kompar ("a", 0, "B"))
					Console.WriteLine ();
			}

			public bool Komparz (string s1, string s2, int end)
			{
				return false;
			}

			// note: parameter name mismatch
			public bool Komparz (string s1, string s2, int start, StringComparison comparison)
			{
				return true;
			}

			// bad
			public void TestExtraEnd ()
			{
				if (Komparz ("a", "B", 0))
					Console.WriteLine ();
			}
		}

		public class FewParameters {

			public bool Compare ()
			{
				return false;
			}

			public bool Compare (StringComparison comparison)
			{
				return true;
			}

			// bad
			public void TestNone ()
			{
				if (Compare ())
					Console.WriteLine ();
			}

			public bool Compare (object o)
			{
				return (o == null);
			}

			public bool Compare (object o, StringComparison comparison)
			{
				return true;
			}

			// bad
			public void TestSingle ()
			{
				if (Compare (null))
					Console.WriteLine ();
			}

			public bool Compare (short a, long b)
			{
				return (a == b);
			}

			public bool Compare (short a, long b, StringComparison comparison)
			{
				return true;
			}

			// bad
			public void TestDifferent ()
			{
				if (Compare (1, 1))
					Console.WriteLine ();
			}
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure<InstanceWithOverride> ("Test");
			
			AssertRuleFailure<StaticHelper> ("Test");

			AssertRuleFailure<NonString> ("TestCharArray");
			AssertRuleFailure<NonString> ("TestInt");

			AssertRuleFailure<ExtraParameters> ("TestExtraFirst");
			AssertRuleFailure<ExtraParameters> ("TestExtraMid");
			AssertRuleFailure<ExtraParameters> ("TestExtraEnd");

			AssertRuleFailure<FewParameters> ("TestNone");
			AssertRuleFailure<FewParameters> ("TestSingle");
			AssertRuleFailure<FewParameters> ("TestDifferent");
		}
	}
}
