//
// Unit tests for SealedTypeWithInheritanceDemandRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005-2006,2008 Novell, Inc (http://www.novell.com)
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
	public class ReviewSealedTypeWithInheritanceDemandTest : TypeRuleTestFixture<ReviewSealedTypeWithInheritanceDemandRule> {

		[SecurityPermission (System.Security.Permissions.SecurityAction.InheritanceDemand, Unrestricted = true)]
		class NonSealedClass {

			public NonSealedClass ()
			{
			}
		}

		[Test]
		public void NonSealed ()
		{
			AssertRuleDoesNotApply<NonSealedClass> ();
		}

		sealed class SealedClassWithoutSecurity {

			public SealedClassWithoutSecurity ()
			{
			}
		}

		[Test]
		public void SealedWithoutSecurity ()
		{
			AssertRuleDoesNotApply<SealedClassWithoutSecurity> ();
		}

		[SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Unrestricted = true)]
		sealed class SealedClassWithoutInheritanceDemand {

			public SealedClassWithoutInheritanceDemand ()
			{
			}
		}

		[Test]
		public void SealedWithoutInheritanceDemand ()
		{
			AssertRuleSuccess<SealedClassWithoutInheritanceDemand> ();
		}

		[SecurityPermission (System.Security.Permissions.SecurityAction.InheritanceDemand, Unrestricted = true)]
		sealed class SealedClassWithInheritanceDemand {

			public SealedClassWithInheritanceDemand ()
			{
			}
		}

		[Test]
		public void SealedWithInheritanceDemand ()
		{
			AssertRuleFailure<SealedClassWithInheritanceDemand> (1);
		}
	}
}
