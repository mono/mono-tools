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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;

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

		public virtual void VirtualMethod ()
		{
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
	public class ConstructorShouldNotCallVirtualMethodsTest {

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private Runner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new ConstructorShouldNotCallVirtualMethodsRule ();
			runner = new MinimalRunner ();
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.Types [typeof (T).FullName];
		}


		[Test]
		public void TestClassWithStaticCtor ()
		{
			Assert.IsNull (rule.CheckType (GetTest<ClassWithStaticCtor> (), runner));
		}

		[Test]
		public void TestSealedClassWithVirtualCall ()
		{
			Assert.IsNull (rule.CheckType (GetTest<SealedClassWithVirtualCall> (), runner));
		}

		[Test]
		public void TestClassWithInstanceOfItself ()
		{
			Assert.IsNull (rule.CheckType (GetTest<ClassWithInstanceOfItself> (), runner));
		}

		[Test]
		public void TestClassCallingVirtualMethodOnce ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassCallingVirtualMethodOnce> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestClassCallingVirtualMethodThreeTimes ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassCallingVirtualMethodThreeTimes> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (3, messages.Count);
		}

		[Test]
		public void TestClassNotCallingVirtualMethods ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassNotCallingVirtualMethods> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestClassCallingVirtualMethodFromBaseClass ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassCallingVirtualMethodFromBaseClass> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestClassCallingVirtualPropertyFromBaseClass ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassCallingVirtualPropertyFromBaseClass> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestClassIndirectlyCallingVirtualMethod ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassIndirectlyCallingVirtualMethod> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestClassIndirectlyCallingVirtualMethodFromBaseClass ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassIndirectlyCallingVirtualMethodFromBaseClass> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestClassIndirectlyCallingVirtualPropertyFromBaseClass ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassIndirectlyCallingVirtualPropertyFromBaseClass> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestClassCallingVirtualMethodFromOtherClasses ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassCallingVirtualMethodFromOtherClasses> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestClassWithRecursiveVirtualCall ()
		{
			MessageCollection messages = rule.CheckType (GetTest<ClassWithRecursiveVirtualCall> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}
	}
}
