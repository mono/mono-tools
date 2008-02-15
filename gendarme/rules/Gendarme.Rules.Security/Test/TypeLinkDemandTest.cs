//
// Unit tests for TypeLinkDemandRule
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

namespace Test.Rules.Security {

	[TestFixture]
	public class TypeLinkDemandTest {

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		class NonPublicClass {

			public NonPublicClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		public sealed class SealedClass {

			public SealedClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		public class LinkDemandClass {

			public LinkDemandClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		public class LinkDemandVirtualMethodClass {

			public LinkDemandVirtualMethodClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[EnvironmentPermission (SSP.SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class InheritanceDemandClass {

			public InheritanceDemandClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		[EnvironmentPermission (SSP.SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class NoIntersectionClass {

			public NoIntersectionClass ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlThread = true)]
		[EnvironmentPermission (SSP.SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class NoIntersectionVirtualMethodClass {

			public NoIntersectionVirtualMethodClass ()
			{
			}

			public virtual void Virtual ()
			{
			}
		}

		[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlAppDomain = true)]
		[SecurityPermission (SSP.SecurityAction.InheritanceDemand, Unrestricted = true)]
		public class IntersectionClass {

			public IntersectionClass ()
			{
			}

			public virtual void Virtual ()
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
			rule = new TypeLinkDemandRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Security.TypeLinkDemandTest/" + name;
			return assembly.MainModule.Types[fullname];
		}

		[Test]
		public void NonPublic ()
		{
			TypeDefinition type = GetTest ("NonPublicClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void Sealed ()
		{
			TypeDefinition type = GetTest ("SealedClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void LinkDemand ()
		{
			TypeDefinition type = GetTest ("LinkDemandClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void LinkDemandVirtualMethod ()
		{
			TypeDefinition type = GetTest ("LinkDemandVirtualMethodClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}

		[Test]
		public void InheritanceDemand ()
		{
			TypeDefinition type = GetTest ("InheritanceDemandClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void NoIntersection ()
		{
			TypeDefinition type = GetTest ("NoIntersectionClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void NoIntersectionVirtualMethod ()
		{
			TypeDefinition type = GetTest ("NoIntersectionVirtualMethodClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}

		[Test]
		public void Intersection ()
		{
			TypeDefinition type = GetTest ("IntersectionClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}
	}
}
