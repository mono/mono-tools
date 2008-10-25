// 
// Unit tests for CompareWithEmptyStringEfficientlyRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
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
using System.Collections.Specialized;

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class CompareWithEmptyStringEfficientlyTest : MethodRuleTestFixture<CompareWithEmptyStringEfficientlyRule> {

		[Test]
		public void DoesNotApply ()
		{
			// has no body
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// has no call to other methods
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public class UsingStringEquals {
			string s = "";
			public static void Main (string [] args)
			{
				UsingStringEquals u = new UsingStringEquals ();
				if (u.s.Equals ("")) {
				}
			}
		}

		public class UsingStringEqualsEmpty {
			string s = "";
			public static void Main (string [] args)
			{
				UsingStringEqualsEmpty u = new UsingStringEqualsEmpty ();
				if (u.s.Equals (String.Empty)) {
				}
			}
		}

		public class UsingStringLength {
			string s = "";
			public static void Main (string [] args)
			{
				UsingStringLength u = new UsingStringLength ();
				if (u.s.Length == 0) {
				}
			}
		}

		public class UsingEqualsWithNonStringArg {
			int i = 0;
			public static void Main (string [] args)
			{
				UsingEqualsWithNonStringArg u = new UsingEqualsWithNonStringArg ();
				if (u.i.Equals (1)) {
				}
			}
		}

		public class AnotherUseOfEqualsWithEmptyString {
			string s = "abc";
			public static void Main (string [] args)
			{
				AnotherUseOfEqualsWithEmptyString a = new AnotherUseOfEqualsWithEmptyString ();
				bool b = a.s.Equals ("");
			}
		}

		public class AnotherUseOfEqualsWithStringEmpty {
			string s = "abc";
			public static void Main (string [] args)
			{
				AnotherUseOfEqualsWithStringEmpty a = new AnotherUseOfEqualsWithStringEmpty ();
				bool b = a.s.Equals (String.Empty);
			}
		}

		public class OneMoreUseOfEqualsWithEmptyString {
			string s = "";
			public static void Main (string [] args)
			{
				OneMoreUseOfEqualsWithEmptyString o = new OneMoreUseOfEqualsWithEmptyString ();
				if (o.s.Equals ("")) {
					bool b = o.s.Equals ("");
				}
			}
		}

		public class UsingEqualsWithNonEmptyString {
			string s = "";
			public static void Main (string [] args)
			{
				UsingEqualsWithNonEmptyString u = new UsingEqualsWithNonEmptyString ();
				if (u.s.Equals ("abc")) {
				}
			}
		}

		[Test]
		public void MainComparingStrings ()
		{
			AssertRuleFailure<UsingStringEquals> ("Main");
			AssertRuleFailure<UsingStringEqualsEmpty> ("Main");
			
			AssertRuleSuccess<UsingStringLength> ("Main");
			AssertRuleSuccess<UsingEqualsWithNonStringArg> ("Main");

			AssertRuleFailure<AnotherUseOfEqualsWithEmptyString> ("Main");
			AssertRuleFailure<AnotherUseOfEqualsWithStringEmpty> ("Main");
			AssertRuleFailure<OneMoreUseOfEqualsWithEmptyString> ("Main");
			AssertRuleSuccess<UsingEqualsWithNonEmptyString> ("Main");
		}


		public bool WrapperLiteral (string s)
		{
			return s.Equals ("");
		}

		public bool WrapperEmpty (string s)
		{
			return s.Equals (String.Empty);
		}

		[Test]
		public void WrapperEqualsString ()
		{
			AssertRuleFailure<CompareWithEmptyStringEfficientlyTest> ("WrapperLiteral");
			AssertRuleFailure<CompareWithEmptyStringEfficientlyTest> ("WrapperEmpty");
		}

		public bool WrapperObjectLiteral (string s)
		{
			return s.Equals ((object) String.Empty);
		}

		public bool WrapperObjectEmpty (string s)
		{
			return s.Equals ((object) "");
		}

		[Test]
		public void WrapperEqualsObject ()
		{
			// [g]mcs emit a "callvirt System.Boolean System.String::Equals(System.Object)"
			// while csc emits "callvirt System.Boolean System.Object::Equals(System.Object)"
			AssertRuleFailure<CompareWithEmptyStringEfficientlyTest> ("WrapperObjectLiteral");
			AssertRuleFailure<CompareWithEmptyStringEfficientlyTest> ("WrapperObjectEmpty");
		}

		public bool OperatorEqualsLiteral (string s)
		{
			return (s == "");
		}

		public bool OperatorEqualsStringEmpty (string s)
		{
			return (s == String.Empty);
		}

		public bool OperatorInequalsLiteral (string s)
		{
			return (s != "");
		}

		public bool OperatorInequalsStringEmpty (string s)
		{
			return (s != String.Empty);
		}

		public bool OperatorEqualsString (string s)
		{
			return (s == "gendarme");
		}

		public bool OperatorInequalsString (string s)
		{
			return (s != "gendarme");
		}

		[Test]
		public void Operators ()
		{
			AssertRuleFailure<CompareWithEmptyStringEfficientlyTest> ("OperatorEqualsLiteral");
			AssertRuleFailure<CompareWithEmptyStringEfficientlyTest> ("OperatorEqualsStringEmpty");
			AssertRuleFailure<CompareWithEmptyStringEfficientlyTest> ("OperatorInequalsLiteral");
			AssertRuleFailure<CompareWithEmptyStringEfficientlyTest> ("OperatorInequalsStringEmpty");

			AssertRuleSuccess<CompareWithEmptyStringEfficientlyTest> ("OperatorEqualsString");
			AssertRuleSuccess<CompareWithEmptyStringEfficientlyTest> ("OperatorInequalsString");
		}

		public bool TwoParameters ()
		{
			return Object.Equals ("", String.Empty);
		}

		static string static_string;
		string instance_string;

		public bool Fields ()
		{
			return ("".Equals (static_string) || String.Empty.Equals (instance_string));
		}

		public bool NewobjAndEquality ()
		{
			DateTime dt = new DateTime ();
			return dt == DateTime.UtcNow;
		}

		[Test]
		public void BetterCoverage ()
		{
			AssertRuleSuccess<CompareWithEmptyStringEfficientlyTest> ("TwoParameters");
			AssertRuleSuccess<CompareWithEmptyStringEfficientlyTest> ("Fields");
			AssertRuleSuccess<CompareWithEmptyStringEfficientlyTest> ("NewobjAndEquality");
		}

		static bool CheckString (string val)
		{
			return (val.Length % 2 == 0);
		}

		static void ThrowValidationException (string a, string b, string c)
		{
		}

		// from mcs/class/System.Web/System.Web/HttpRequest.cs
		static void ValidateNameValueCollection (string name, NameValueCollection coll)
		{
			if (coll == null)
				return;

			foreach (string key in coll.Keys) {
				string val = coll [key];
				if (val != null && val != "" && CheckString (val))
					ThrowValidationException (name, key, val);
			}
		}

		[Test]
		public void MultipleChecks ()
		{
			AssertRuleFailure<CompareWithEmptyStringEfficientlyTest> ("ValidateNameValueCollection", 1);
		}
	}
}
