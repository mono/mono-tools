//
// Unit tests for BadRecursiveInvocationRule
//
// Authors:
//	Aaron Tomb <atomb@soe.ucsc.edu>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Aaron Tomb
// Copyright (C) 2006-2008 Novell, Inc (http://www.novell.com)
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
using System.Security;
using System.Security.Permissions;

using Gendarme.Framework;
using Gendarme.Rules.Correctness;
using Mono.Cecil;
using NUnit.Framework;

using Test.Rules.Helpers;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class BadRecursiveInvocationTest {

		class BadRec {

			/* This should be an error. */
			public int Foo {
				get { return Foo; }
			}

			/* This should be an error. */
			public int OnePlusFoo {
				get { return 1 + OnePlusFoo; }
			}

			/* This should be an error. */
			public int FooPlusOne {
				get { return FooPlusOne + 1; }
			}

			/* correct */
			public int Bar {
				get { return -1; }
			}

			/* a more complex recursion */
			public int FooBar {
				get { return BarFoo; }
			}

			public int BarFoo {
				get { return FooBar; }
			}

			/* This should be fine, as it uses 'base.' */
			public override int GetHashCode ()
			{
				return base.GetHashCode ();
			}
			
			/* not fine, missing 'base.' */
			public override bool Equals (object obzekt)
			{
				return Equals (obzekt);
			}

			public static int StaticBadFibo (int n)
			{
				return StaticBadFibo (n - 1) + StaticBadFibo (n - 2);
			}

			public int BadFibo (int n)
			{
				return BadFibo (n - 1) + BadFibo (n - 2);
			}
			
			public static int StaticFibonacci (int n)
			{
				if (n < 2)
					return n;

				return StaticFibonacci (n - 1) + StaticFibonacci (n - 2);
			}

			public int Fibonacci (int n)
			{
				if (n < 2)
					return n;

				return Fibonacci (n - 1) + Fibonacci (n - 2);
			}

			public void AnotherInstance ()
			{
				BadRec rec = new BadRec ();
				rec.AnotherInstance ();
			}

			public void Assert ()
			{
				new PermissionSet (PermissionState.None).Assert ();
			}

			static Helper help;
			public static void Write (bool value)
			{
				help.Write (value);
			}

			public void Unreachable ()
			{
				throw new NotImplementedException ();
				Unreachable ();
			}
		}

		class Helper {
			public void Write (bool value)
			{
			}
		}
		
		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private ModuleDefinition module;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			module = assembly.MainModule;
			type = module.Types["Test.Rules.Correctness.BadRecursiveInvocationTest/BadRec"];
			rule = new BadRecursiveInvocationRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}

		[Test]
		public void RecursiveProperties ()
		{
			MethodDefinition method = GetTest ("get_Foo");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (1, runner.Defects.Count, "Count1");

			method = GetTest ("get_OnePlusFoo");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (1, runner.Defects.Count, "Count2");

			method = GetTest ("get_FooPlusOne");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult3");
			Assert.AreEqual (1, runner.Defects.Count, "Count3");
		}
		
		[Test]
		public void Property ()
		{
			MethodDefinition method = GetTest ("get_Bar");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test, Ignore ("uncaught by rule")]
		public void IndirectRecursiveProperty ()
		{
			MethodDefinition method = GetTest ("get_FooBar");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void OverriddenMethod ()
		{
			MethodDefinition method = GetTest ("GetHashCode");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void BadRecursiveMethod ()
		{
			MethodDefinition method = GetTest ("Equals");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void BadFibo ()
		{
			MethodDefinition method = GetTest ("BadFibo");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (1, runner.Defects.Count, "Count1");

			method = GetTest ("StaticBadFibo");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (1, runner.Defects.Count, "Count2");
		}

		[Test]
		public void Fibonacci ()
		{
			MethodDefinition method = GetTest ("Fibonacci");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (0, runner.Defects.Count, "Count1");

			method = GetTest ("StaticFibonacci");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (0, runner.Defects.Count, "Count2");
		}

		[Test, Ignore ("uncaught by rule")]
		public void CodeUsingAnInstanceOfItself ()
		{
			MethodDefinition method = GetTest ("AnotherInstance");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestAssert ()
		{
			MethodDefinition method = GetTest ("Assert");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestStaticCallingAnotherClassWithSameMethodName ()
		{
			MethodDefinition method = GetTest ("Write");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestUnreachable ()
		{
			MethodDefinition method = GetTest ("Unreachable");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
