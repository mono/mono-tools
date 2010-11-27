// 
// Unit tests for ConstructorShouldNotCallVirtualMethodsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.BadPractice {

	internal class ClassWithStaticCtor {
		static ClassWithStaticCtor ()
		{
			ClassWithStaticCtor test = new ClassWithStaticCtor ();
			// this is ok, since we calling the instance virtual method
			test.Method ();
		}

		public virtual void Method ()
		{
		}
	}

	internal class ClassWithInstanceOfItself {

		private ClassWithInstanceOfItself itself;

		public ClassWithInstanceOfItself ()
		{
			itself = null;
		}

		public ClassWithInstanceOfItself (ClassWithInstanceOfItself self)
		{
			itself = self;
			itself.Method (0);
		}

		public virtual void Method (int x)
		{
		}
	}

	internal class ClassNotCallingVirtualMethods {

		public ClassNotCallingVirtualMethods ()
		{
			this.NormalMethod ();
		}

		private void NormalMethod () 
		{
		}

		protected virtual void VirtualMethod ()
		{
		}

		public virtual bool VirtualProperty {
			get { return true; }
			set { ; }
		}
	}

	internal sealed class SealedClassWithVirtualCall : ClassNotCallingVirtualMethods {
		public SealedClassWithVirtualCall ()
		{
			VirtualMethod ();
		}
	}

	internal class ClassWithRecursiveVirtualCall {
		public ClassWithRecursiveVirtualCall ()
		{
			VirtualMethod (5);
		}

		protected virtual void VirtualMethod (int n)
		{
			while (n > 0)
				VirtualMethod (n--);
		}
	}

	internal class ClassCallingVirtualMethodOnce {

		public ClassCallingVirtualMethodOnce ()
		{
			this.NormalMethod ();
			// call virtual method - bad thing
			this.VirtualMethod ();
			this.NormalMethod ();
		}

		protected virtual void VirtualMethod ()
		{
		}

		public void NormalMethod ()
		{
		}

		public void CallingVirtualMethod ()
		{
			VirtualMethod ();
		}
	}

	internal class ClassCallingVirtualMethodThreeTimes {

		public ClassCallingVirtualMethodThreeTimes ()
		{
			this.NormalMethod ();
			// call virtual method - bad thing
			this.VirtualMethod ();
			this.VirtualMethod2 ();
			this.VirtualMethod ();
			this.NormalMethod ();
		}

		protected virtual void VirtualMethod () 
		{
		}

		protected virtual void VirtualMethod2 ()
		{
		}

		private void NormalMethod ()
		{
		}
	}

	internal class ClassCallingVirtualMethodFromBaseClass : ClassCallingVirtualMethodOnce {

		public ClassCallingVirtualMethodFromBaseClass ()
		{
			base.VirtualMethod ();
		}
	}

	internal class ClassCallingVirtualPropertyFromBaseClass : ClassNotCallingVirtualMethods {

		public ClassCallingVirtualPropertyFromBaseClass ()
		{
			this.VirtualProperty = false;
		}
	}

	internal class ClassIndirectlyCallingVirtualMethod {

		public ClassIndirectlyCallingVirtualMethod ()
		{
			NormalMethod ();
		}

		[DllImport ("liberty.so")]
		extern static void Erty ();

		public virtual void VirtualMethod ()
		{
			Erty ();
		}

		private void NormalMethod ()
		{
			VirtualMethod ();
		}
	}

	internal class ClassIndirectlyCallingVirtualMethodFromBaseClass : ClassNotCallingVirtualMethods {

		public ClassIndirectlyCallingVirtualMethodFromBaseClass ()
		{
			Method ();
		}

		private void Method ()
		{
			base.VirtualMethod ();
		}
	}

	internal class ClassIndirectlyCallingVirtualPropertyFromBaseClass : ClassNotCallingVirtualMethods {

		public ClassIndirectlyCallingVirtualPropertyFromBaseClass ()
		{
			Method ();
		}

		public bool NonVirtualProperty {
			get { return false; }
		}

		private void Method ()
		{
			base.VirtualProperty = NonVirtualProperty;
			Method (); // check recursion
		}
	}

	internal class ClassCallingVirtualMethodFromOtherClasses {

		ClassIndirectlyCallingVirtualMethod test;

		public ClassCallingVirtualMethodFromOtherClasses ()
		{
			test = new ClassIndirectlyCallingVirtualMethod ();
			test.VirtualMethod ();
		}
	}

	[TestFixture]
	public class ConstructorShouldNotCallVirtualMethodsTest : TypeRuleTestFixture<ConstructorShouldNotCallVirtualMethodsRule> {

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			rule = new ConstructorShouldNotCallVirtualMethodsRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.GetType (typeof (T).FullName);
		}

		[Test]
		public void TestClassWithStaticCtor ()
		{
			TypeDefinition type = GetTest<ClassWithStaticCtor> ();
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestSealedClassWithVirtualCall ()
		{
			TypeDefinition type = GetTest<SealedClassWithVirtualCall> ();
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassWithInstanceOfItself ()
		{
			TypeDefinition type = GetTest<ClassWithInstanceOfItself> ();
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassCallingVirtualMethodOnce ()
		{
			TypeDefinition type = GetTest<ClassCallingVirtualMethodOnce> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassCallingVirtualMethodThreeTimes ()
		{
			TypeDefinition type = GetTest<ClassCallingVirtualMethodThreeTimes> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (3, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassNotCallingVirtualMethods ()
		{
			TypeDefinition type = GetTest<ClassNotCallingVirtualMethods> ();
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassCallingVirtualMethodFromBaseClass ()
		{
			TypeDefinition type = GetTest<ClassCallingVirtualMethodFromBaseClass> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassCallingVirtualPropertyFromBaseClass ()
		{
			TypeDefinition type = GetTest<ClassCallingVirtualPropertyFromBaseClass> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassIndirectlyCallingVirtualMethod ()
		{
			TypeDefinition type = GetTest<ClassIndirectlyCallingVirtualMethod> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassIndirectlyCallingVirtualMethodFromBaseClass ()
		{
			TypeDefinition type = GetTest<ClassIndirectlyCallingVirtualMethodFromBaseClass> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassIndirectlyCallingVirtualPropertyFromBaseClass ()
		{
			TypeDefinition type = GetTest<ClassIndirectlyCallingVirtualPropertyFromBaseClass> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassCallingVirtualMethodFromOtherClasses ()
		{
			TypeDefinition type = GetTest<ClassCallingVirtualMethodFromOtherClasses> ();
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassWithRecursiveVirtualCall ()
		{
			TypeDefinition type = GetTest<ClassWithRecursiveVirtualCall> ();
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
	}
}
