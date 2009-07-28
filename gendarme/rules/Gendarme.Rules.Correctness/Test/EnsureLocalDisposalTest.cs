//
// Unit tests for EnsureLocalDisposalRule
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
using System.IO;
using System.Xml;
using System.Collections.Generic;
using NUnit.Framework;

using Gendarme.Rules.Correctness;

using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace Test.Rules.Correctness {

	class DisposalCases {
		string foo;
		StreamReader sr;

		string DoesNotApply1 () { //no call/newobj/stloc
			return foo;
		}

		StreamReader DoesNotApply2 () { //returns IDisposable
			var sr = new StreamReader ("bar.xml");
			return sr;
		}

		void DoesNotApply3 () {
			sr = new StreamReader ("bar.xml"); //field
		}

		string Success0 () {
			var o = new object ();
			return o.ToString ();
		}

		void Success1 () {
			using (var reader = XmlReader.Create ("foo.xml")) {
				Console.WriteLine (reader.ToString ());
			}
		}

		void Success2 (string pattern) {
			using (var reader = XmlReader.Create ("foo.xml")) {
				Console.WriteLine (reader.ToString ());
				StreamReader reader2 = null;
				try {
					reader2 = new StreamReader ("bar.xml"); //newobj
				} catch (InvalidOperationException e) {
					Console.WriteLine (e.Message);
				} finally {
					if (reader2 != null)
						reader2.Dispose ();
				}
			}
		}

		void Success3 (IDisposable disposable) {
			int x = 0;
			disposable.Dispose ();
		}

		void Success4 () {
			int x = 0;
			sr = new StreamReader ("bar.xml"); //field
		}

		//foreach(enumerator)
		static string Success5 (IEnumerable<string> strings)
		{
			foreach (var s in strings) {
				if (s.Length > 0)
					return s;
			}
			return null;
		}

		//foreach(valuetype_enumerator) || valuetype disposable
		static string ValueTypeDisposable (List<string> strings)
		{
			foreach (var s in strings) {
				if (s.Length > 0)
					return s;
			}
			return null;
		}

		//generator
		//csc/gmcs for some reason build an IDisposable iterator class that is never disposed
		static IEnumerable<string> Success6 ()
		{
			yield return "foo";
			yield return "bar";
		}

		void Failure0 () {
			var reader = XmlReader.Create ("foo.xml");
			Console.WriteLine (reader.ToString ());
		}

		void Failure1 () {
			var reader = XmlReader.Create ("foo.xml");
			Console.WriteLine (reader.ToString ());
			((IDisposable) reader).Dispose (); //not guaranteed
		}

		void Failure2 () {
			XmlReader reader = null;
			try {
				reader = XmlReader.Create ("foo.xml");
				Console.WriteLine (reader.ToString ());
			} catch (InvalidOperationException e) {
				Console.WriteLine (e.Message);
			}
			((IDisposable) reader).Dispose ();
		}

		void Failure3 () {
			using (var reader = XmlReader.Create ("foo.xml")) {
				Console.WriteLine (reader.ToString ());
				try {
					var reader2 = new StreamReader ("bar.xml"); //newobj
				} finally {
					((IDisposable) reader).Dispose (); //reader != reader2 !!!
				}
			}
		}

		void Failure4 () {
			var reader = XmlReader.Create ("foo.xml");
			reader.ToString ();
			Success3 (reader);
		}

		bool Failure5 () {
			using (var reader = XmlReader.Create ("foo.xml")) {
				var reader2 = new StreamReader ("bar.xml"); //newobj
				var reader3 = new StreamReader ("baz.xml"); //newobj
				if (reader2.ReadLine () == reader3.ReadLine ())
					return true;
			}
			return false;
		}

		void Failure6 () {
			var reader = XmlReader.Create ("foo.xml");
			reader.ToString ();
			Success3 (reader);
			reader = XmlReader.Create ("bar.xml");
		}
	}

	[TestFixture]
	public class EnsureLocalDisposalTest : MethodRuleTestFixture<EnsureLocalDisposalRule> {

		[Test]
		public void DoesNotApply0 ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void DoesNotApply1 ()
		{
			AssertRuleDoesNotApply<DisposalCases> ("DoesNotApply1");
		}

		[Test]
		public void DoesNotApply2 ()
		{
			AssertRuleDoesNotApply<DisposalCases> ("DoesNotApply2");
		}

		[Test]
		public void DoesNotApply3 ()
		{
			AssertRuleDoesNotApply<DisposalCases> ("DoesNotApply3");
		}

		[Test]
		public void Success0 ()
		{
			AssertRuleSuccess<DisposalCases> ("Success0");
		}

		[Test]
		public void Success1 ()
		{
			AssertRuleSuccess<DisposalCases> ("Success1");
		}

		[Test]
		public void Success2 ()
		{
			AssertRuleSuccess<DisposalCases> ("Success2");
		}

		[Test]
		public void Success3 ()
		{
			AssertRuleSuccess<DisposalCases> ("Success3");
		}

		[Test]
		public void Success4 ()
		{
			AssertRuleSuccess<DisposalCases> ("Success4");
		}

		[Test]
		public void Success5 ()
		{
			AssertRuleSuccess<DisposalCases> ("Success5");
		}

		[Test]
		public void Success6 ()
		{
			AssertRuleSuccess<DisposalCases> ("Success6");
		}

		[Test]
		public void ValueTypeDisposable ()
		{
			AssertRuleSuccess<DisposalCases> ("ValueTypeDisposable");
		}

		[Test]
		public void Failure0 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure0", 1);
		}

		[Test]
		public void Failure1 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure1", 1);
		}

		[Test]
		public void Failure2 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure2", 1);
		}

		[Test]
		public void Failure3 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure3", 2);
		}

		[Test]
		public void Failure4 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure4", 1);
		}

		[Test]
		public void Failure5 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure5", 2);
		}

		[Test]
		public void Failure6 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure6", 2);
		}
	}
}
