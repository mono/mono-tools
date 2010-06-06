//
// Unit tests for PreferEmptyInstanceOverNullRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2008 Cedric Vivier
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
using System.Collections;
using System.Collections.Generic;

using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class PreferEmptyInstanceOverNullTest : MethodRuleTestFixture<PreferEmptyInstanceOverNullRule> {

		public class DoesNotApplyCases {
			bool foo;
			string x;

			public void Void () {
				Console.WriteLine ("Foo");
			}

			public int Int () {
				return 0;
			}

			public Type Type () {
				return null;
			}

			public string StringProperty {
				set {
					x = value;
				}
			}

			public IEnumerable<int> GetOffsets () {
				if (!foo)
					yield break;
				//store.LoadOffsets ();
				foreach (int offset in GetOffsets ())
					yield return offset;
			}

			public override string ToString () {
				return null;
			}
		}

		public class SuccessCases {
			bool foo;
			string x;

			public int [] IntArray () {
				x = null;
				return new int [0];
			}

			public string StringProperty {
				get {
					x = null;
					return "foo";
				}
			}

			public string String (string s) {
				x = null;
				return "foo";
			}

			public string String2 () {
				x = null;
				return string.Empty;
			}

			public string String3 () {
				x = String (null);
				return this.ToString ();
			}

			public ArrayList Collection () {
				if (!foo)
					return new ArrayList ();
				var list = new ArrayList ();
				list.Add (4);
				list.Add (8);
				String (null);
				return list;
			}

			public List<int> GenericCollection () {
				if (!foo)
					return new List<int> ();
				String (null);
				var list = new List<int> ();
				list.Add (4);
				list.Add (8);
				return list;
			}
		}

		public class FailureCases {
			bool foo = false;
			string s;

			public int [] IntArray () {
				if (!foo)
					return null;
				return new int [1];
			}

			public string StringProperty {
				get {
					return null;
				}
			}

			public string String () {
				return null;
			}

			public string String2 () {
				if (!foo)
					return null;
				return "foo";
			}

			public string String3 () {
				if (!foo)
					return null;
				if (s == "N/A")
					return null;
				return s;
			}

			public IEnumerable<int> GetOffsets () {
				if (!foo)
					return null;
				//store.LoadOffsets ();
				return GetOffsets ();
			}

			public ArrayList Collection () {
				if (!foo)
					return null;
				var list = new ArrayList ();
				list.Add (4);
				list.Add (8);
				return list;
			}

			public List<int> GenericCollection () {
				if (!foo)
					return null;
				var list = new List<int> ();
				list.Add (4);
				list.Add (8);
				return list;
			}

			public string InlineIf {
				get { return s.Length == 0 ? null : s; }
			}

			public string InlineIf2 {
				get { return s.Length == 0 ? s : null; }
			}
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply<DoesNotApplyCases> ("Void");
			AssertRuleDoesNotApply<DoesNotApplyCases> ("Int");
			AssertRuleDoesNotApply<DoesNotApplyCases> ("Type");
			AssertRuleDoesNotApply<DoesNotApplyCases> ("set_StringProperty");
			AssertRuleDoesNotApply<DoesNotApplyCases> ("GetOffsets");
			AssertRuleDoesNotApply<DoesNotApplyCases> ("ToString");
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<SuccessCases> ("IntArray");
			AssertRuleSuccess<SuccessCases> ("get_StringProperty");
			AssertRuleSuccess<SuccessCases> ("String");
			AssertRuleSuccess<SuccessCases> ("String2");
			AssertRuleSuccess<SuccessCases> ("String3");
			AssertRuleSuccess<SuccessCases> ("Collection");
			AssertRuleSuccess<SuccessCases> ("GenericCollection");
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure<FailureCases> ("IntArray", 1);
			AssertRuleFailure<FailureCases> ("get_StringProperty", 1);
			AssertRuleFailure<FailureCases> ("String", 1);
			AssertRuleFailure<FailureCases> ("String2", 1);
			AssertRuleFailure<FailureCases> ("String3", 2);
			AssertRuleFailure<FailureCases> ("GetOffsets", 1);
			AssertRuleFailure<FailureCases> ("Collection", 1);
			AssertRuleFailure<FailureCases> ("GenericCollection", 1);
			AssertRuleFailure<FailureCases> ("get_InlineIf", 1);
			AssertRuleFailure<FailureCases> ("get_InlineIf2", 1);
		}
	}
}
