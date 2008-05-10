// 
// Unit tests for StaticConstructorsShouldBePrivateRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) Daniel Abramov
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Security;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Security {

	internal class NoStaticCtorDefinedClass {
	}

	internal class PrivateStaticCtorDefinedClass {
		static PrivateStaticCtorDefinedClass ()
		{
		}
	}

	internal interface InterfaceHasNoConstructor {
		int GetMe { get; }
	}

	[TestFixture]
	public class StaticConstructorsShouldBePrivateTest {

		private ITypeRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new StaticConstructorsShouldBePrivateRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.Types [typeof (T).FullName];
		}

		[Test]
		public void TestNoStaticCtorDefinedClass ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckType (GetTest<NoStaticCtorDefinedClass> ()));
		}

		[Test]
		public void TestPrivateStaticCtorDefinedClass ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckType (GetTest<PrivateStaticCtorDefinedClass> ()));
		}

		[Test]
		public void TestNonPrivateStaticCtorDefinedClass ()
		{
			TypeDefinition inspectedType = GetTest<PrivateStaticCtorDefinedClass> ();
			MethodDefinition static_ctor = null;
			foreach (MethodDefinition ctor in inspectedType.Constructors) {
				if (ctor.IsStatic) {
					static_ctor = ctor;
					break;
				}
			}
			try {
				static_ctor.IsPublic = true; // change it from private to public
				Assert.AreEqual (RuleResult.Failure, runner.CheckType (inspectedType), inspectedType.FullName);
				Assert.AreEqual (1, runner.Defects.Count, "Count");
			}
			finally {
				static_ctor.IsPublic = false;
			}
		}

		[Test]
		public void TestInterface ()
		{
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (GetTest<InterfaceHasNoConstructor> ()));
		}
	}
}
