//
// Unit tests for PreferXmlAbstractionsRule
//
// Authors:
//	Cedric Vivier  <cedricv@neonux.com>
//
// Copyright (C) 2009 Cedric Vivier
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

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

using System.Xml;
using System.Xml.XPath;

namespace Test.Rules.Design {

	[TestFixture]
	public class PreferXmlAbstractionsTest : MethodRuleTestFixture<PreferXmlAbstractionsRule> {

		static bool raisedAnalyzeModuleEvent;

		[SetUp]
		public void RaiseAnalyzeModuleEvent ()
		{
			if (raisedAnalyzeModuleEvent)
				return;

			raisedAnalyzeModuleEvent = true;
			((TestRunner) Runner).OnModule (DefinitionLoader.GetTypeDefinition<PreferXmlAbstractionsTest> ().Module);
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<PreferXmlAbstractionsTest> ("PrivateBadReturn");
			AssertRuleDoesNotApply<PreferXmlAbstractionsTest> ("PrivateBadParameter");
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<PreferXmlAbstractionsTest> ("VisibleReturn");
			AssertRuleSuccess<PreferXmlAbstractionsTest> ("VisibleParameter");
			AssertRuleSuccess<PreferXmlAbstractionsTest> ("VisibleParameter2");
			AssertRuleSuccess<PreferXmlAbstractionsTest> ("VisibleOutParameter");
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure<PreferXmlAbstractionsTest> ("VisibleBadReturn", 1);
			AssertRuleFailure<PreferXmlAbstractionsTest> ("VisibleBadParameter", 1);
			AssertRuleFailure<PreferXmlAbstractionsTest> ("VisibleBadParameters", 2);
			AssertRuleFailure<PreferXmlAbstractionsTest> ("VisibleBadReturnAndParameter", 2);
			AssertRuleFailure<PreferXmlAbstractionsTest> ("VisibleBadReturnAndParameter2", 2);
		}

		private XmlDocument PrivateBadReturn ()
		{
			return null;
		}

		private void PrivateBadParameter (XmlNode input)
		{
		}

		public IXPathNavigable VisibleReturn ()
		{
			return null;
		}

		protected void VisibleParameter (XmlReader input)
		{
		}

		public void VisibleParameter2 (IXPathNavigable input)
		{
		}

		public void VisibleOutParameter (out XmlDocument output)
		{
			output = null;
		}

		public XmlDocument VisibleBadReturn ()
		{
			return null;
		}

		public void VisibleBadParameter (XmlNode input)
		{
		}

		protected void VisibleBadParameters (XmlNode input, XmlDocument doc)
		{
		}

		public XmlNode VisibleBadReturnAndParameter (XmlDocument input)
		{
			return null;
		}

		public XPathDocument VisibleBadReturnAndParameter2 (XPathDocument doc)
		{
			return doc;
		}
	}
}
