//
// Unit tests for ProvideValidXPathExpressionRule
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

	class XPathCases {
		const string good1 = "//book/@title";
		const string bad1 = "\\book/@title";

		string DoesNotApply1 () {
			return bad1;
		}

		void Success0 () {
			var xpath = XPathExpression.Compile ("/book[@npages = 100]/@title");
		}

		void Failure0 () {
			var xpath = XPathExpression.Compile ("/book[@npages == 100]/@title");
		}

		void Success1 () {
			var xpath = XPathExpression.Compile (good1);
		}

		void Failure1 () {
			var xpath = XPathExpression.Compile (bad1);
		}

		void Success2 () {
			var xpath = XPathExpression.Compile ("/book[@npages = 100]/@title", null);
		}

		void Success3 (string expression) {
			var xpath = XPathExpression.Compile (expression);
		}

		void Success4 (string expression) {
			var xnav = new XmlDocument ().CreateNavigator ();
			xnav.Compile (expression);
		}

		void Failure4 () {
			var xnav = new XmlDocument ().CreateNavigator ();
			xnav.Compile (bad1);
		}

		void Success5 (XmlDocument document) {
			document.SelectNodes (good1);
			document.SelectNodes (good1, null);
		}

		void Failure5 (XmlDocument document) {
			document.SelectNodes (bad1);
			document.SelectNodes (bad1, null);
		}

		void Success6 (XmlDocument document) {
			document.SelectSingleNode (good1);
			document.SelectSingleNode (good1, null);
		}

		void Failure6 (XmlDocument document) {
			document.SelectSingleNode (bad1);
			document.SelectSingleNode (bad1, null);
		}

		void Success7 (string expression) {
			var xnav = new XmlDocument ().CreateNavigator ();
			xnav.Evaluate (expression);
			var xpe = XPathExpression.Compile (good1);
			xnav.Evaluate (xpe);
		}

		void Failure7 () {
			var xnav = new XmlDocument ().CreateNavigator ();
			xnav.Evaluate (bad1);
		}

		void Success8 (string expression) {
			var xnav = new XmlDocument ().CreateNavigator ();
			xnav.Select (expression);
			xnav.SelectSingleNode (expression);
			var xpe = XPathExpression.Compile (good1);
			xnav.Select (xpe);
			xnav.SelectSingleNode (xpe);
		}

		void Failure8 () {
			var xnav = new XmlDocument ().CreateNavigator ();
			xnav.Select (bad1);
			xnav.SelectSingleNode (bad1);
		}

		void FailureNull () {
			var xpath = XPathExpression.Compile (null);
			var xnav = new XmlDocument ().CreateNavigator ();
			xnav.Evaluate ((string) null);
		}

		void FailureEmpty () {
			var xpath = XPathExpression.Compile ("");
			var xnav = new XmlDocument ().CreateNavigator ();
			xnav.Select (string.Empty);
		}
	}

	[TestFixture]
	public class ProvideValidXPathExpressionTest : MethodRuleTestFixture<ProvideValidXPathExpressionRule> {

		static bool raisedAnalyzeModuleEvent;

		[SetUp]
		public void RaiseAnalyzeModuleEvent ()
		{
			if (raisedAnalyzeModuleEvent)
				return;

			raisedAnalyzeModuleEvent = true;
			((TestRunner) Runner).OnModule (DefinitionLoader.GetTypeDefinition<XPathCases> ().Module);
		}

		[Test]
		public void DoesNotApply0 ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<XPathCases> ("DoesNotApply1");
		}

		[Test]
		public void Success0 ()
		{
			AssertRuleSuccess<XPathCases> ("Success0");
		}

		[Test]
		public void Success1 ()
		{
			AssertRuleSuccess<XPathCases> ("Success1");
		}

		[Test]
		public void Success2 ()
		{
			AssertRuleSuccess<XPathCases> ("Success2");
		}

		[Test]
		public void Success3 ()
		{
			AssertRuleSuccess<XPathCases> ("Success3");
		}

		[Test]
		public void Success4 ()
		{
			AssertRuleSuccess<XPathCases> ("Success4");
		}

		[Test]
		public void Success5 ()
		{
			AssertRuleSuccess<XPathCases> ("Success5");
		}

		[Test]
		public void Success6 ()
		{
			AssertRuleSuccess<XPathCases> ("Success6");
		}

		[Test]
		public void Success7 ()
		{
			AssertRuleSuccess<XPathCases> ("Success7");
		}

		[Test]
		public void Success8 ()
		{
			AssertRuleSuccess<XPathCases> ("Success8");
		}

		[Test]
		public void Failure0 ()
		{
			AssertRuleFailure<XPathCases> ("Failure0", 1);
		}

		[Test]
		public void Failure1 ()
		{
			AssertRuleFailure<XPathCases> ("Failure1", 1);
		}

		[Test]
		public void Failure4 ()
		{
			AssertRuleFailure<XPathCases> ("Failure4", 1);
		}

		[Test]
		public void Failure5 ()
		{
			AssertRuleFailure<XPathCases> ("Failure5", 2);
		}

		[Test]
		public void Failure6 ()
		{
			AssertRuleFailure<XPathCases> ("Failure6", 2);
		}

		[Test]
		public void Failure7 ()
		{
			AssertRuleFailure<XPathCases> ("Failure7", 1);
		}

		[Test]
		public void Failure8 ()
		{
			AssertRuleFailure<XPathCases> ("Failure8", 2);
		}

		[Test]
		public void FailureNullOrEmpty ()
		{
			AssertRuleFailure<XPathCases> ("FailureNull", 2);
			AssertRuleFailure<XPathCases> ("FailureEmpty", 2);
		}
	}
}
