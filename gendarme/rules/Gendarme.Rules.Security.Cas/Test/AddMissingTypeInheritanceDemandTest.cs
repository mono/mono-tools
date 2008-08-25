//
// Unit tests for AddMissingTypeInheritanceDemandRule
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
	public class AddMissingTypeInheritanceDemandTest : TypeRuleTestFixture<AddMissingTypeInheritanceDemandRule> {

		[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
		class NonVisibleClass {

			public NonVisibleClass ()
			{
			}
		}

		[Test]
		public void NonVisible ()
		{
			AssertRuleDoesNotApply<NonVisibleClass> ();
		}

		[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
		public sealed class SealedClass {

			public SealedClass ()
			{
			}
		}

		[Test]
		public void Sealed ()
		{
			AssertRuleDoesNotApply<SealedClass> ();
		}

		[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
		public class LinkDemandClass {

			public LinkDemandClass ()
			{
			}
		}

		[Test]
		public void LinkDemand ()
		{
			AssertRuleDoesNotApply<LinkDemandClass> ();
		}

		[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
		public class LinkDemandVirtualMethodClass {

			public LinkDemandVirtualMethodClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[Test]
		public void LinkDemandVirtualMethod ()
		{
			AssertRuleFailure<LinkDemandVirtualMethodClass> (1);
		}

		[EnvironmentPermission (SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class InheritanceDemandClass {

			public InheritanceDemandClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[Test]
		public void InheritanceDemand ()
		{
			AssertRuleDoesNotApply<InheritanceDemandClass> ();
		}

		[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
		[EnvironmentPermission (SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class NoIntersectionClass {

			public NoIntersectionClass ()
			{
			}
		}

		[Test]
		public void NoIntersection ()
		{
			AssertRuleDoesNotApply<NoIntersectionClass> ();
		}

		[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
		[EnvironmentPermission (SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class NoIntersectionVirtualMethodClass {

			public NoIntersectionVirtualMethodClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[Test]
		public void NoIntersectionVirtualMethod ()
		{
			AssertRuleFailure<NoIntersectionVirtualMethodClass> (1);
		}

		[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
		[SecurityPermission (SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class IntersectionClass {

			public IntersectionClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[Test]
		public void Intersection ()
		{
			AssertRuleSuccess<IntersectionClass> ();
		}
	}
}
