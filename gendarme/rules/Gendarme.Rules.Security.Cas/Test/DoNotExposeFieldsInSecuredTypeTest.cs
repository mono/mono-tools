//
// Unit tests for DoNotExposeFieldsInSecuredTypeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005,2008 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;

using Gendarme.Rules.Security.Cas;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Security {

	[TestFixture]
	public class DoNotExposeFieldsInSecuredTypeTest : TypeRuleTestFixture<DoNotExposeFieldsInSecuredTypeRule> {

		[SecurityPermission (System.Security.Permissions.SecurityAction.InheritanceDemand, Unrestricted = true)]
		class NonVisibleClass {

			public object Field = new object ();

			public NonVisibleClass ()
			{
			}
		}

		[Test]
		public void NonVisible ()
		{
			AssertRuleDoesNotApply<NonVisibleClass> ();
		}

		public class NoSecurityClass {

			public object Field = new object ();

			public NoSecurityClass ()
			{
			}
		}

		[Test]
		public void NoSecurity ()
		{
			AssertRuleDoesNotApply<NoSecurityClass> ();
		}

		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public class NoDemandClass {

			public object Field = new object ();

			public NoDemandClass ()
			{
			}
		}

		[Test]
		public void NoDemand ()
		{
			AssertRuleDoesNotApply<NoDemandClass> ();
		}

		[SecurityPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public class NoVisibleFieldClass {

			private object Field = new object ();
			internal string FieldToo = String.Empty;

			public NoVisibleFieldClass ()
			{
			}
		}

		[Test]
		public void NoVisibleField ()
		{
			AssertRuleSuccess<NoVisibleFieldClass> ();
		}

		[SecurityPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public class LinkDemandWithFieldClass {

			public object Field = new object ();

			public LinkDemandWithFieldClass ()
			{
			}
		}

		[Test]
		public void LinkDemandWithField ()
		{
			AssertRuleFailure<LinkDemandWithFieldClass> (1);
		}

		[SecurityPermission (SecurityAction.LinkDemand, Unrestricted = true)]
		public class DemandWithFieldClass {

			public object Field = new object ();

			public DemandWithFieldClass ()
			{
			}
		}

		[Test]
		public void DemandWithField ()
		{
			AssertRuleFailure<DemandWithFieldClass> (1);
		}
	}
}
