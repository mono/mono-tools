//
// Unit tests for ReviewNonVirtualMethodWithInheritanceDemandRule
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
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SSP = System.Security.Permissions;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Security.Cas;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Security.Cas {

	[TestFixture]
	public class ReviewNonVirtualMethodWithInheritanceDemandTest {

		public abstract class AbstractMethodsClass {

			[SecurityPermission (SSP.SecurityAction.InheritanceDemand, ControlAppDomain = true)]
			public abstract void Abstract ();
		}

		public class VirtualMethodsClass: AbstractMethodsClass  {

			public override void Abstract ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.InheritanceDemand, ControlAppDomain = true)]
			public virtual void Virtual ()
			{
			}
		}

		public class NoVirtualMethodsClass {

			[SecurityPermission (SSP.SecurityAction.InheritanceDemand, ControlAppDomain = true)]
			public void Method ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.InheritanceDemand, ControlAppDomain = true)]
			static public void StaticMethod ()
			{
			}
		}

		public abstract class NotInheritanceDemandClass {

			[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlAppDomain = true)]
			public abstract void Asbtract ();

			[SecurityPermission (SSP.SecurityAction.Demand, ControlAppDomain = true)]
			public virtual void Virtual ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.LinkDemand, ControlAppDomain = true)]
			public void Method ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.Demand, ControlAppDomain = true)]
			static public void StaticMethod ()
			{
			}
		}

		private IMethodRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			rule = new ReviewNonVirtualMethodWithInheritanceDemandRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Security.Cas.ReviewNonVirtualMethodWithInheritanceDemandTest/" + name;
			return assembly.MainModule.GetType (fullname);
		}

		[Test]
		public void AbstractMethods ()
		{
			TypeDefinition type = GetTest ("AbstractMethodsClass");
			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor)
					continue;
				Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), method.ToString ());
			}
		}

		[Test]
		public void VirtualMethods ()
		{
			TypeDefinition type = GetTest ("VirtualMethodsClass");
			foreach (MethodDefinition method in type.Methods) {
				switch (method.Name) {
				case "Abstract":
					Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), method.Name);
					break;
				case "Virtual":
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), method.Name);
					break;
				}
			}
		}

		[Test]
		public void NoVirtualMethods ()
		{
			TypeDefinition type = GetTest ("NoVirtualMethodsClass");
			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor)
					continue;
				Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), method.ToString ());
			}
		}

		[Test]
		public void NotInheritanceDemand ()
		{
			TypeDefinition type = GetTest ("NotInheritanceDemandClass");
			foreach (MethodDefinition method in type.Methods) {
				if (method.IsConstructor)
					continue;
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), method.ToString ());
			}
		}
	}
}
