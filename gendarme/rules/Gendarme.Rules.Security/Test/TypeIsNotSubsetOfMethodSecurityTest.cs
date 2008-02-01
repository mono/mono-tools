//
// Unit tests for TypeIsNotSubsetOfMethodSecurityRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace Test.Rules.Security {

	[TestFixture]
	public class TypeIsNotSubsetOfMethodSecurityTest {

		public class NoSecurityClass {

			public NoSecurityClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		public class LinkDemandClass {

			public LinkDemandClass ()
			{
			}

			[EnvironmentPermission (SSP.SecurityAction.LinkDemand, Unrestricted = true)]
			public void Method ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.InheritanceDemand, ControlThread = true)]
		public class InheritanceDemandClass {

			public InheritanceDemandClass ()
			{
			}

			[EnvironmentPermission (SSP.SecurityAction.InheritanceDemand, Unrestricted = true)]
			public void Method ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.Assert, ControlThread = true)]
		public class AssertNotSubsetClass {

			public AssertNotSubsetClass ()
			{
			}

			[EnvironmentPermission (SSP.SecurityAction.Assert, Unrestricted = true)]
			public void Method ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.Demand, ControlThread = true)]
		public class DemandSubsetClass {

			public DemandSubsetClass ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.Demand, ControlThread = true)]
			public void Method ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.Deny, Unrestricted = true)]
		public class DenyNotSubsetClass {

			public DenyNotSubsetClass ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.Deny, ControlThread = true)]
			public void Method ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.PermitOnly, ControlThread = true)]
		public class PermitOnlySubsetClass {

			public PermitOnlySubsetClass ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.PermitOnly, Unrestricted = true)]
			public void Method ()
			{
			}
		}

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private ModuleDefinition module;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			module = assembly.MainModule;
			rule = new TypeIsNotSubsetOfMethodSecurityRule ();
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Security.TypeIsNotSubsetOfMethodSecurityTest/" + name;
			return assembly.MainModule.Types[fullname];
		}

		[Test]
		public void NoSecurity ()
		{
			TypeDefinition type = GetTest ("NoSecurityClass");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void LinkDemand ()
		{
			TypeDefinition type = GetTest ("LinkDemandClass");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void InheritanceDemand ()
		{
			TypeDefinition type = GetTest ("InheritanceDemandClass");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void AssertNotSubset ()
		{
			TypeDefinition type = GetTest ("AssertNotSubsetClass");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void DemandSubset ()
		{
			TypeDefinition type = GetTest ("DemandSubsetClass");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void DenyNotSubset ()
		{
			TypeDefinition type = GetTest ("DenyNotSubsetClass");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void PermitOnlySubset ()
		{
			TypeDefinition type = GetTest ("PermitOnlySubsetClass");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}
	}
}
