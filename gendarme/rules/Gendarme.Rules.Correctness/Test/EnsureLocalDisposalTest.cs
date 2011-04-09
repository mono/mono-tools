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

using Gendarme.Framework;
using Gendarme.Rules.Correctness;

using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace Test.Rules.Correctness {

	class DisposalCases {
		string foo;
		StreamReader sr;

		StreamReader StreamReader { get; set; }

		string DoesNotApply1 ()
		{
			//no call/newobj/stloc
			return foo;
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

		void Success7 () {
			sr = new StreamReader ("bar.xml"); //field
		}

		void Success8 () {
			StreamReader = new StreamReader ("bar.xml"); //property
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

		void Failure7 () {
			new XmlTextReader ("foo.xml");
		}

		void Failure8 () {
			var xslt = new System.Xml.Xsl.XslCompiledTransform ();
			xslt.Load (new XmlTextReader ("foo.xml"));
		}

		void Failure9 () {
			var xslt = new System.Xml.Xsl.XslCompiledTransform ();
			xslt.Load (XmlTextReader.Create ("foo.xml"));
		}
	}

	[TestFixture]
	public class EnsureLocalDisposalTest : MethodRuleTestFixture<EnsureLocalDisposalRule> {

		[Test]
		public void DoesNotApply0 ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		[Test]
		public void DoesNotApply1 ()
		{
			AssertRuleDoesNotApply<DisposalCases> ("DoesNotApply1");
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
		public void Success7 ()
		{
			AssertRuleSuccess<DisposalCases> ("Success7");
		}

		[Test]
		public void Success8 ()
		{
			AssertRuleSuccess<DisposalCases> ("Success8");
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
			AssertRuleFailure<DisposalCases> ("Failure3", 1);
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

		[Test]
		public void Failure7 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure7", 1);
		}

		[Test]
		public void Failure8 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure8", 1);
		}

		[Test]
		public void Failure9 ()
		{
			AssertRuleFailure<DisposalCases> ("Failure9", 1);
		}

		// test case based on:
		// https://github.com/Iristyle/mono-tools/commit/2cccfd0efd406434e1309d0740826ff06d32de20

		string FluentTestCase ()
		{
			using (StringWriter sw = new StringWriter ()) {
				// while analyzing 'FluentTestCase' we cannot know what's inside
				// 'NestedFluentCall[Two|Three]' except that they _looks_like_ fluent APIs
				return NestedFluentCall (sw).ToString () + 
					NestedFluentCallTwo (sw).ToString () +
					NestedFluentCallThree (sw).ToString ();
			}
		}

		StringWriter NestedFluentCall (StringWriter stringWriter)
		{
			// same instance is returned and does not need to be disposed (the caller does it)
			return stringWriter;
		}

		StringWriter NestedFluentCallTwo (StringWriter stringWriter)
		{
			// a new instance is being returned and someone should be disposing it
			// without source code or (good) documentation it behaves exactly like
			// the previous method
			StringWriter sw = new StringWriter ();
			sw.Write (stringWriter);
			return sw;
		}

		StringWriter NestedFluentCallThree (StringWriter stringWriter)
		{
			// really bad code to show that we cannot determine with 100% exactitude
			// what some methods returns to us
			if (stringWriter.GetHashCode () % 2 == 1)
				return NestedFluentCall (stringWriter);
			else
				return NestedFluentCallTwo (stringWriter);
		}

		[Test]
		public void FluentApi ()
		{
			AssertRuleSuccess<EnsureLocalDisposalTest> ("FluentTestCase");
		}

		// adapted (without locals variants) from https://bugzilla.novell.com/show_bug.cgi?id=666403

		void OutParameter1 (out StreamReader stream)
		{
			var new_stream = new StreamReader ("baz.xml"); //out param
			stream = new_stream;
		}

		bool OutParameter2 (out StreamReader stream)
		{
			stream = new StreamReader ("baz.xml"); //out param, no locals
			return true;
		}

		[Test]
		public void OutParameters ()
		{
			AssertRuleSuccess<EnsureLocalDisposalTest> ("OutParameter1");
			AssertRuleSuccess<EnsureLocalDisposalTest> ("OutParameter2");
		}

		class SomeClassThatContainsADisposableProperty {
			public StreamReader Reader { get; set; }
		}

		void OtherInstanceProperty1 (SomeClassThatContainsADisposableProperty someObj)
		{
			var reader = new StreamReader ("foobaz.xml");
			someObj.Reader = reader; //property in param
		}

		void OtherInstanceProperty2 (SomeClassThatContainsADisposableProperty someObj)
		{
			someObj.Reader = new StreamReader ("foobaz.xml"); //property in param, no locals
		}

		[Test]
		public void OtherInstance ()
		{
			AssertRuleSuccess<EnsureLocalDisposalTest> ("OtherInstanceProperty1");
			AssertRuleSuccess<EnsureLocalDisposalTest> ("OtherInstanceProperty2");
		}

		StreamReader ReturnIDisposable1 ()
		{
			var ret = new StreamReader ("baz.xml"); //return value
			return ret;
		}

		StreamReader ReturnIDisposable2 ()
		{
			return new StreamReader ("baz.xml"); //return value, no locals
		}

		[Test]
		public void ReturnValue ()
		{
			// csc 10 (without /o optimize) will fail this as it introduce extra compiler generated locals
#if __MonoCS__
			AssertRuleSuccess<EnsureLocalDisposalTest> ("ReturnIDisposable1");
#endif
			AssertRuleSuccess<EnsureLocalDisposalTest> ("ReturnIDisposable2");
		}
	}
}
