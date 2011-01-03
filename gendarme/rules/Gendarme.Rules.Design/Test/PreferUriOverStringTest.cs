//
// Unit Tests for PreferUriOverStringRule
//
// Authors:
//	Nicholas Rioux
//
// Copyright (C) 2010 Nicholas Rioux
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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Design {

	public class GoodUris {
		public Uri MyUri
		{
			get;
			set;
		}
		public int NotAUrl
		{
			get;
			set;
		}
		public string Urn
		{
			get;
			set;
		}

		public Uri GetUri ()
		{
			return null;
		}
		public Uri GetNewLink (Uri oldUrl)
		{
			return null;
		}

		public void OverloadedMethod (string uri)
		{
		}
		public void OverloadedMethod (Uri uri)
		{
		}
	}

	public class BadUris {
		public string SomeUri
		{
			get;
			set;
		}

		public string BadUrnMethod (string urlParam)
		{
			return null;
		}
	}

	public class UriAttribute : Attribute {

		public UriAttribute (string uri)
		{
			Uri = uri;
		}

		// note: automatic properties are, alas, decorated as generated code
		public string Uri { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute ("System.Web.Services", "2.0.50727.42")]
	public class WebService {
		private bool IsLocalFileSystemWebService (string url)
		{
			return false;
		}
	}

	[TestFixture]
	public class PreferUriOverStringTest : MethodRuleTestFixture<PreferUriOverStringRule> {

		[Test]
		public void DoesNotApply ()
		{
			// The rule doesn't apply to setters if a getter is present.
			AssertRuleDoesNotApply<BadUris> ("set_SomeUri");
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess (SimpleMethods.EmptyMethod);
			AssertRuleSuccess<GoodUris> ("GetUri");
			AssertRuleSuccess<GoodUris> ("GetNewLink");
			AssertRuleSuccess<GoodUris> ("OverloadedMethod", new Type [] { typeof (string) });
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadUris> ("get_SomeUri", 1);
			AssertRuleFailure<BadUris> ("BadUrnMethod", 2);
		}

		[Test]
		public void Attribute ()
		{
			AssertRuleSuccess<UriAttribute> (".ctor");
			AssertRuleFailure<UriAttribute> ("get_Uri", 1);
		}

		[Test]
		public void GeneratedCode ()
		{
			AssertRuleDoesNotApply<WebService> ("IsLocalFileSystemWebService");
		}
	}
}

