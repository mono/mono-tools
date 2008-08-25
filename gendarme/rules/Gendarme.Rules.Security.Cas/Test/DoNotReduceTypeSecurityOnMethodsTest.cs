//
// Unit tests for DoNotReduceTypeSecurityOnMethodsRule
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

namespace Test.Rules.Security.Cas {

	[TestFixture]
	public class DoNotReduceTypeSecurityOnMethodsTest : TypeRuleTestFixture<DoNotReduceTypeSecurityOnMethodsRule> {

		public class NoSecurityClass {

			public NoSecurityClass ()
			{
			}
		}

		[Test]
		public void NoSecurity ()
		{
			AssertRuleDoesNotApply<NoSecurityClass> ();
		}

		[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
		public class LinkDemandClass {

			public LinkDemandClass ()
			{
			}

			[EnvironmentPermission (SecurityAction.LinkDemand, Unrestricted = true)]
			public void Method ()
			{
			}
		}

		[Test]
		public void LinkDemand ()
		{
			AssertRuleDoesNotApply<LinkDemandClass> ();
		}

		[SecurityPermission (SecurityAction.InheritanceDemand, ControlThread = true)]
		public class InheritanceDemandClass {

			public InheritanceDemandClass ()
			{
			}

			[EnvironmentPermission (SecurityAction.InheritanceDemand, Unrestricted = true)]
			public void Method ()
			{
			}
		}

		[Test]
		public void InheritanceDemand ()
		{
			AssertRuleDoesNotApply<InheritanceDemandClass> ();
		}

		[SecurityPermission (SecurityAction.Assert, ControlThread = true)]
		public class AssertNotSubsetClass {

			public AssertNotSubsetClass ()
			{
			}

			[EnvironmentPermission (SecurityAction.Assert, Unrestricted = true)]
			public void Method ()
			{
			}
		}

		[Test]
		public void AssertNotSubset ()
		{
			AssertRuleFailure<AssertNotSubsetClass> (1);
		}

		[SecurityPermission (SecurityAction.Demand, ControlThread = true)]
		public class DemandSubsetClass {

			public DemandSubsetClass ()
			{
			}

			[SecurityPermission (SecurityAction.Demand, ControlThread = true)]
			public void Method ()
			{
			}
		}

		[Test]
		public void DemandSubset ()
		{
			AssertRuleSuccess<DemandSubsetClass> ();
		}

		[SecurityPermission (SecurityAction.Deny, Unrestricted = true)]
		public class DenyNotSubsetClass {

			public DenyNotSubsetClass ()
			{
			}

			[SecurityPermission (SecurityAction.Deny, ControlThread = true)]
			public void Method ()
			{
			}
		}

		[Test]
		public void DenyNotSubset ()
		{
			AssertRuleFailure<DenyNotSubsetClass> (1);
		}

		[SecurityPermission (SecurityAction.PermitOnly, ControlThread = true)]
		public class PermitOnlySubsetClass {

			public PermitOnlySubsetClass ()
			{
			}

			[SecurityPermission (SecurityAction.PermitOnly, Unrestricted = true)]
			public void Method ()
			{
			}
		}

		[Test]
		public void PermitOnlySubset ()
		{
			AssertRuleSuccess<PermitOnlySubsetClass> ();
		}
	}
}
