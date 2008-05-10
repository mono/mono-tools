//
// Unit tests for TypeExposeFieldsRule
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
using System.Reflection;
using System.Security.Permissions;
using SSP = System.Security.Permissions;

using Gendarme.Framework;
using Gendarme.Rules.Security;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Security {

	[TestFixture]
	public class TypeExposeFieldsTest {

		[SecurityPermission (System.Security.Permissions.SecurityAction.InheritanceDemand, Unrestricted = true)]
		class NonPublicClass {

			public object Field = new object ();

			public NonPublicClass ()
			{
			}
		}

		public class NoSecurityClass {

			public object Field = new object ();

			public NoSecurityClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.Deny, Unrestricted = true)]
		public class NoDemandClass {

			public object Field = new object ();

			public NoDemandClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, Unrestricted = true)]
		public class NoPublicFieldClass {

			protected object Field = new object ();

			public NoPublicFieldClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, Unrestricted = true)]
		public class LinkDemandWithFieldClass {

			public object Field = new object ();

			public LinkDemandWithFieldClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, Unrestricted = true)]
		public class DemandWithFieldClass {

			public object Field = new object ();

			public DemandWithFieldClass ()
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
			rule = new TypeExposeFieldsRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Security.TypeExposeFieldsTest/" + name;
			return assembly.MainModule.Types[fullname];
		}

		[Test]
		public void NonPublic ()
		{
			TypeDefinition type = GetTest ("NonPublicClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void NoSecurity ()
		{
			TypeDefinition type = GetTest ("NoSecurityClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void NoDemand ()
		{
			TypeDefinition type = GetTest ("NoDemandClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void NoPublicField ()
		{
			TypeDefinition type = GetTest ("NoPublicFieldClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void LinkDemandWithField ()
		{
			TypeDefinition type = GetTest ("LinkDemandWithFieldClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}

		[Test]
		public void DemandWithField ()
		{
			TypeDefinition type = GetTest ("DemandWithFieldClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}
	}
}
