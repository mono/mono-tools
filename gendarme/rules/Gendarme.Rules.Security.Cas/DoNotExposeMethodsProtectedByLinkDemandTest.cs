//
// Unit tests for DoNotExposeMethodsProtectedByLinkDemandRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005-2006, 2008 Novell, Inc (http://www.novell.com)
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
	public class DoNotExposeMethodsProtectedByLinkDemandTest : MethodRuleTestFixture<DoNotExposeMethodsProtectedByLinkDemandRule> {

		public class BaseClass {

			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
			public virtual void VirtualMethod ()
			{
			}

			[SecurityPermission (SecurityAction.LinkDemand, ControlAppDomain = true)]
			protected void ProtectedMethod ()
			{
			}
		}

		public class SubsetInheritClass: BaseClass  {

			[SecurityPermission (SecurityAction.LinkDemand, Unrestricted = true)]
			public override void VirtualMethod ()
			{
				base.VirtualMethod ();
			}

			[SecurityPermission (SecurityAction.LinkDemand, Unrestricted = true)]
			public void CallProtectedMethod ()
			{
				base.ProtectedMethod ();
			}
		}

		[Test]
		public void SubsetInherit ()
		{
			AssertRuleSuccess<SubsetInheritClass> ("VirtualMethod");
			AssertRuleSuccess<SubsetInheritClass> ("CallProtectedMethod");
		}

		public class NotASubsetInheritClass: BaseClass {

			[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
			public override void VirtualMethod ()
			{
				base.VirtualMethod ();
			}

			[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
			public void CallProtectedMethod ()
			{
				base.ProtectedMethod ();
			}
		}

		[Test]
		public void NotASubsetInherit ()
		{
			AssertRuleFailure<NotASubsetInheritClass> ("VirtualMethod", 1);
			AssertRuleFailure<NotASubsetInheritClass> ("VirtualMethod", 1);
		}

		public class SubsetCallClass {

			[SecurityPermission (SecurityAction.LinkDemand, Unrestricted = true)]
			public void Method ()
			{
				new BaseClass ().VirtualMethod ();
			}
		}

		[Test]
		public void SubsetCall ()
		{
			AssertRuleSuccess<SubsetCallClass> ("Method");
		}

		public class NotASubsetCallClass {

			[SecurityPermission (SecurityAction.LinkDemand, ControlThread = true)]
			public void Method ()
			{
				new BaseClass ().VirtualMethod ();
			}
		}

		[Test]
		public void NotASubsetCall ()
		{
			AssertRuleFailure<NotASubsetCallClass> ("Method", 1);
		}
	}
}
