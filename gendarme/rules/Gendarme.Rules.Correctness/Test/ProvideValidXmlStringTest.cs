//
// Unit tests for ProvideValidXmlStringRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2009 Cedric Vivier
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
using NUnit.Framework;

using Gendarme.Rules.Correctness;

using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

using System.Xml;
using System.Xml.XPath;


namespace Test.Rules.Correctness {

	class XmlCases {
		const string good1 = "<author>Robert J. Sawyer</author>";
		const string bad1 = "<author>Robert J. Sawyer</authr>";

		string DoesNotApply1 () {
			return bad1;
		}

		void Success0 () {
			var doc = new XmlDocument ();
			doc.LoadXml ("<book />");
		}

		void Failure0 () {
			var doc = new XmlDocument ();
			doc.LoadXml ("<book>");
		}

		void Success1 () {
			var doc = new XmlDocument ();
			doc.LoadXml (good1);
		}

		void Failure1 () {
			var doc = new XmlDocument ();
			doc.LoadXml (bad1);
		}

		void Success2 (XmlDocumentFragment doc, string xml) {
			doc.InnerXml = xml;
		}

		void Failure2 (XmlDocumentFragment doc) {
			doc.InnerXml = bad1;
		}

		void Success3 (XPathNavigator nav, string xml) {
			nav.OuterXml = xml;
		}

		void Failure3 (XPathNavigator nav) {
			nav.OuterXml = bad1;
		}

		void Success4 (XPathNavigator nav, string xml) {
			nav.AppendChild (xml);
			nav.AppendChild ();
			nav.AppendChild (nav);
			nav.InsertAfter (xml);
			nav.InsertAfter ();
			nav.InsertAfter (nav);
		}

		void Failure4 (XPathNavigator nav) {
			nav.AppendChild (bad1);
		}

		void Failure4b (XPathNavigator nav) {
			nav.InsertAfter (bad1);
		}

		void FailureNull () {
			var doc = new XmlDocument ();
			doc.LoadXml (null);
			doc.InnerXml = null;
			doc.CreateNavigator ().AppendChild ((string) null);
		}

		void FailureEmpty () {
			var doc = new XmlDocument ();
			doc.LoadXml ("");
			doc.InnerXml = "";
			doc.CreateNavigator ().AppendChild (string.Empty);
		}
	}

	[TestFixture]
	public class ProvideValidXmlStringTest : MethodRuleTestFixture<ProvideValidXmlStringRule> {

		static bool raisedAnalyzeModuleEvent;

		[SetUp]
		public void RaiseAnalyzeModuleEvent ()
		{
			if (raisedAnalyzeModuleEvent)
				return;

			raisedAnalyzeModuleEvent = true;
			((TestRunner) Runner).OnModule (DefinitionLoader.GetTypeDefinition<XmlCases> ().Module);
		}

		[Test]
		public void DoesNotApply0 ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<XmlCases> ("DoesNotApply1");
		}

		[Test]
		public void Success0 ()
		{
			AssertRuleSuccess<XmlCases> ("Success0");
		}

		[Test]
		public void Success1 ()
		{
			AssertRuleSuccess<XmlCases> ("Success1");
		}

		[Test]
		public void Success2 ()
		{
			AssertRuleSuccess<XmlCases> ("Success2");
		}

		[Test]
		public void Success3 ()
		{
			AssertRuleSuccess<XmlCases> ("Success3");
		}

		[Test]
		public void Success4 ()
		{
			AssertRuleSuccess<XmlCases> ("Success4");
		}

		[Test]
		public void Failure0 ()
		{
			AssertRuleFailure<XmlCases> ("Failure0", 1);
		}

		[Test]
		public void Failure1 ()
		{
			AssertRuleFailure<XmlCases> ("Failure1", 1);
		}

		[Test]
		public void Failure2 ()
		{
			AssertRuleFailure<XmlCases> ("Failure2", 1);
		}

		[Test]
		public void Failure3 ()
		{
			AssertRuleFailure<XmlCases> ("Failure3", 1);
		}

		[Test]
		public void Failure4 ()
		{
			AssertRuleFailure<XmlCases> ("Failure4", 1);
			AssertRuleFailure<XmlCases> ("Failure4b", 1);
		}

		[Test]
		public void FailureNullOrEmpty ()
		{
			AssertRuleFailure<XmlCases> ("FailureNull", 3);
			AssertRuleFailure<XmlCases> ("FailureEmpty", 3);
		}
	}
}
