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
using System.Reflection;
using System.Security.Permissions;

using Gendarme.Framework;
using Gendarme.Rules.Security;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Security {

	[TestFixture]
	public class SealedTypeWithInheritanceDemandTest {

		[SecurityPermission (System.Security.Permissions.SecurityAction.InheritanceDemand, Unrestricted = true)]
		class NonSealedClass {

			public NonSealedClass ()
			{
			}
		}

		sealed class SealedClassWithoutSecurity {

			public SealedClassWithoutSecurity ()
			{
			}
		}

		[SecurityPermission (System.Security.Permissions.SecurityAction.LinkDemand, Unrestricted = true)]
		sealed class SealedClassWithoutInheritanceDemand {

			public SealedClassWithoutInheritanceDemand ()
			{
			}
		}

		[SecurityPermission (System.Security.Permissions.SecurityAction.InheritanceDemand, Unrestricted = true)]
		sealed class SealedClassWithInheritanceDemand {

			public SealedClassWithInheritanceDemand ()
			{
			}
		}

		private ITypeRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new SealedTypeWithInheritanceDemandRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Security.SealedTypeWithInheritanceDemandTest/" + name;
			return assembly.MainModule.Types [fullname];
		}

		[Test]
		public void NonSealed ()
		{
			TypeDefinition type = GetTest ("NonSealedClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void SealedWithoutSecurity ()
		{
			TypeDefinition type = GetTest ("SealedClassWithoutSecurity");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void SealedWithoutInheritanceDemand ()
		{
			TypeDefinition type = GetTest ("SealedClassWithoutInheritanceDemand");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void SealedWithInheritanceDemand ()
		{
			TypeDefinition type = GetTest ("SealedClassWithInheritanceDemand");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}
	}
}
