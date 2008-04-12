// 
// Unit tests for AvoidUncalledPrivateCodeRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

using Gendarme.Framework;
using Gendarme.Rules.Performance;
using Mono.Cecil;
using NUnit.Framework;
using System.Collections;

namespace Test.Rules.Performance {
	
	[TestFixture]
	public class AvoidUncalledPrivateCodeTest {
		
		public class AnonymousMethod {
			private void MethodWithAnonymousMethod ()
			{
				string[] values = new string[] {"one", "two", "three"};
				if (Array.Exists (values, delegate (string  myString) { return myString.Length == 3;}))
					Console.WriteLine ("Exists strings with length == 3");
			}
		}

		public class UncalledPrivateMethod {
			private void display ()
			{
			}
		}

		public class CalledPrivateMethod {
			private void display ()
			{
			}

			public static void Main (string [] args)
			{
				CalledPrivateMethod c = new CalledPrivateMethod ();
				c.display ();
			}
		}

		public class NestedCalledPrivateMethod {
			class Nested {
				void callParentDisplay ()
				{
					display ();
				}
			}

			private static void display ()
			{
			}
		}

		public class UncalledInternalMethod {
			internal void print ()
			{
			}
		}

		public class CalledInternalMethod {
			internal void CalledMethod ()
			{
			}
		}

		public class MethodCallingClass {
			public static void Main (string [] args)
			{
				CalledInternalMethod c = new CalledInternalMethod ();
				c.CalledMethod ();
			}
		}

		private class PublicMethodNotCalledInPrivateClass {
			public void publicMethod ()
			{
			}
		}

		internal class PublicMethodNotCalledInInternalClass {
			public void publicMethod ()
			{
			}
		}

		private class PublicMethodCalledInPrivateClass {
			public void publicCalledMethod ()
			{
			}

			public static void Main (string [] args)
			{
				PublicMethodCalledInPrivateClass p = new PublicMethodCalledInPrivateClass ();
				p.publicCalledMethod ();
			}
		}

		internal class PublicMethodCalledInInternalClass {
			public void publicMethodCalled ()
			{
			}

			public static void Main (string [] args)
			{
				PublicMethodCalledInInternalClass p = new PublicMethodCalledInInternalClass ();
				p.publicMethodCalled ();
			}
		}

		private class PrivateMethodInPrivateClassNotCalled {
			private void privateMethodNotCalled ()
			{
			}
		}

		internal class NestedClasses {
			public class AnotherClass {
				public void publicMethodNotCalledInNestedInternalClass ()
				{
				}
			}
		}

		interface Iface1 {
			void IfaceMethod1 ();
		}

		interface Iface2 {
			void IfaceMethod2 ();
		}

		public class ImplementingExplicitInterfacesMembers : Iface1, Iface2 {
			void Iface1.IfaceMethod1 ()
			{
			}

			void Iface2.IfaceMethod2 ()
			{
			}

			public static void Main (string [] args)
			{
				ImplementingExplicitInterfacesMembers i = new ImplementingExplicitInterfacesMembers ();
				Iface1 iobject = i;
				iobject.IfaceMethod1 ();
			}
		}

		public class PrivateConstructorNotCalled {
			private PrivateConstructorNotCalled ()
			{
			}
		}

		public class StaticConstructorNotCalled {
			static int i = 0;
			static StaticConstructorNotCalled ()
			{
				i = 5;
			}
		}

		[Serializable]
		public class PublicSerializableConstructorNotCalled {
			private int i;
			public PublicSerializableConstructorNotCalled (SerializationInfo info, StreamingContext context)
			{
				i = 0;
			}
		}

		[Serializable]
		public class PrivateSerializableConstructorNotCalled {
			private int i;
			private PrivateSerializableConstructorNotCalled (SerializationInfo info, StreamingContext context)
			{
				i = 0;
			}
		}

		[Serializable]
		public class ProtectedSerializableConstructorNotCalled {
			private int i;
			protected ProtectedSerializableConstructorNotCalled (SerializationInfo info, StreamingContext context)
			{
				i = 0;
			}
		}

		[Serializable]
		public class InternalSerializableConstructorNotCalled {
			private int i;
			internal InternalSerializableConstructorNotCalled (SerializationInfo info, StreamingContext context)
			{
				i = 0;
			}
		}

		public class UncalledOverriddenMethod {
			public override string ToString ()
			{
				return String.Empty;
			}
		}

		public class UsingComRegisterAndUnRegisterFunctionAttribute {
			[ComRegisterFunctionAttribute]
			private void register ()
			{
			}
			[ComUnregisterFunctionAttribute]
			private void unregister ()
			{
			}
		}

		public class CallingPrivateMethodsThroughDelegates {
			delegate string delegateExample ();

			private string privateMethod ()
			{
				return String.Empty;
			}

			public static void Main (string [] args)
			{
				CallingPrivateMethodsThroughDelegates c = new CallingPrivateMethodsThroughDelegates ();
				delegateExample d = new delegateExample (c.privateMethod);
			}
		}

		public class ClassWithFinalizer {

			// finalizer is private but we can't report it as unused
			~ClassWithFinalizer ()
			{
			}

			// note: don't add anything else in this class or the test will break
		}

		public class MyList : IList {

			int IList.Add (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.Clear ()
			{
				throw new NotImplementedException ();
			}

			bool IList.Contains (object value)
			{
				throw new NotImplementedException ();
			}

			int IList.IndexOf (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.Insert (int index, object value)
			{
				throw new NotImplementedException ();
			}

			bool IList.IsFixedSize {
				get { throw new NotImplementedException (); }
			}

			bool IList.IsReadOnly {
				get { throw new NotImplementedException (); }
			}

			void IList.Remove (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.RemoveAt (int index)
			{
				throw new NotImplementedException ();
			}

			object IList.this [int index] {
				get { throw new NotImplementedException (); }
				set { throw new NotImplementedException (); }
			}

			void ICollection.CopyTo (Array array, int index)
			{
				throw new NotImplementedException ();
			}

			int ICollection.Count {
				get { throw new NotImplementedException (); }
			}

			bool ICollection.IsSynchronized  {
				get { throw new NotImplementedException (); }
			}

			object ICollection.SyncRoot {
				get { throw new NotImplementedException (); }
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				throw new NotImplementedException ();
			}
		}

		private IMethodRule methodRule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			methodRule = new AvoidUncalledPrivateCodeRule ();
			runner = new TestRunner (methodRule);
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Performance.AvoidUncalledPrivateCodeTest/" + name;
			return assembly.MainModule.Types [fullname];
		}

		[Test]
		public void UncalledPrivateMethodTest ()
		{
			type = GetTest ("UncalledPrivateMethod");
			Assert.AreEqual (1, type.Methods.Count, "Methods.Count");
			Assert.AreEqual (RuleResult.Failure , runner.CheckMethod (type.Methods [0]));
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void CalledPrivateMethodTest ()
		{
			type = GetTest ("CalledPrivateMethod");
			foreach (MethodDefinition md in type.Methods) {
				switch (md.Name) {
				case "Main":
					// rule does not apply to Main
					Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (md));
					break;
				case "display":
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (md));
					break;
				}
			}
		}

		[Test]
		public void NestedCalledPrivateMethodTest ()
		{
			type = GetTest ("NestedCalledPrivateMethod");
			foreach (MethodDefinition md in type.Methods) {
				switch (md.Name) {
				case "display":
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (md));
					break;
				}
			}
		}

		[Test]
		public void UncalledInternalMethodTest ()
		{
			type = GetTest ("UncalledInternalMethod");
			Assert.AreEqual (1, type.Methods.Count, "Methods.Count");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (type.Methods [0]));
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void CalledInternalMethodTest ()
		{
			type = GetTest ("CalledInternalMethod");
			Assert.AreEqual (1, type.Methods.Count, "Methods.Count");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (type.Methods [0]));
		}

		[Test]
		public void CheckingForMainMethodTest () {
			type = GetTest ("CalledInternalMethod");
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == "Main")
					Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void PublicMethodNotCalledInPrivateClassTest ()
		{
			type = GetTest ("PublicMethodNotCalledInPrivateClass");
			foreach (MethodDefinition method in type.Methods) {
				switch (method.Name) {
				case "publicMethod":
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), method.Name);
					break;
				default:
					Assert.Fail ("Test case for method {0} is not handled", method.Name);
					break;
				}
			}
		}

		[Test]
		public void PublicMethodCalledInPrivateClassTest ()
		{
			type = GetTest ("PublicMethodCalledInPrivateClass");
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == "publicCalledMethod")
					Assert.AreEqual (RuleResult.Success ,runner.CheckMethod (method));
		}

		[Test]
		public void PublicMethodCalledInInternalClassTest ()
		{
			type = GetTest ("PublicMethodCalledInInternalClass");
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == "publicMethodCalled")
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void PrivateMethodInPrivateClassNotCalledTest ()
		{
			type = GetTest ("PrivateMethodInPrivateClassNotCalled");
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == "privateMethodNotCalled")
					Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
			Assert.AreEqual (1, runner.Defects.Count);
		}

		[Test]
		public void PublicMethodNotCalledInNestedInternalClassTest ()
		{
			type = GetTest ("NestedClasses");
			foreach (MethodDefinition method in type.Methods) {
				switch (method.Name) {
				case "publicMethodNotCalledInNestedInternalClass":
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), method.Name);
					break;
				default:
					Assert.Fail ("Test case for method {0} is not handled", method.Name);
					break;
				}
			}
		}

		[Test]
		public void ImplementingInterfacesMembersTest ()
		{
			type = GetTest ("ImplementingExplicitInterfacesMembers");
			foreach (MethodDefinition method in type.Methods) {
				switch (method.Name) {
				case "Main":
					Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), method.Name);
					break;
				case "Test.Rules.Performance.AvoidUncalledPrivateCodeTest.Iface1.IfaceMethod1":
// mono bug #343465
				case "Test.Rules.Performance.AvoidUncalledPrivateCodeTest+Iface1.IfaceMethod1":
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), method.Name);
					break;
				case "Test.Rules.Performance.AvoidUncalledPrivateCodeTest.Iface2.IfaceMethod2":
				// mono bug #343465
				case "Test.Rules.Performance.AvoidUncalledPrivateCodeTest+Iface2.IfaceMethod2":
					Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), method.Name);
					break;
				default:
					Assert.Fail ("Test case for method {0} is not handled", method.Name);
					break;
				}
			}
		}

		[Test]
		public void PrivateConstructorNotCalledTest ()
		{
			type = GetTest ("PrivateConstructorNotCalled");
			foreach (MethodDefinition method in type.Constructors) {
				Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), method.Name);
			}
		}

		[Test]
		public void StaticConstructorNotCalledTest ()
		{
			type = GetTest ("StaticConstructorNotCalled");
			foreach (MethodDefinition method in type.Constructors)
				if (method.Name == ".cctor")
					Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void SerializationConstructors ()
		{
			type = GetTest ("PublicSerializableConstructorNotCalled");
			foreach (MethodDefinition ctor in type.Constructors)
				Assert.AreEqual (RuleResult.Success, runner.CheckMethod (ctor), ctor.ToString ());
			type = GetTest ("PrivateSerializableConstructorNotCalled");
			foreach (MethodDefinition ctor in type.Constructors)
				Assert.AreEqual (RuleResult.Success, runner.CheckMethod (ctor), ctor.ToString ());
			type = GetTest ("ProtectedSerializableConstructorNotCalled");
			foreach (MethodDefinition ctor in type.Constructors)
				Assert.AreEqual (RuleResult.Success, runner.CheckMethod (ctor), ctor.ToString ());
			type = GetTest ("InternalSerializableConstructorNotCalled");
			foreach (MethodDefinition ctor in type.Constructors)
				Assert.AreEqual (RuleResult.Success, runner.CheckMethod (ctor), ctor.ToString ());
		}

		[Test]
		public void UncalledOverriddenMethodTest ()
		{
			type = GetTest ("UncalledOverriddenMethod");
			foreach (MethodDefinition method in type.Methods) {
				switch (method.Name) {
				case "ToString":
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), method.Name);
					break;
				default:
					Assert.Fail ("Test case not handled");
					break;
				}
			}
		}

		[Test]
		public void ImplementingComRegisterFunctionAttributeTest ()
		{
			type = GetTest ("UsingComRegisterAndUnRegisterFunctionAttribute");
			foreach (MethodDefinition method in type.Constructors)
				if (method.Name == "register")
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void ImplementingComUnregisterFunctionAttributeTest ()
		{
			type = GetTest ("UsingComRegisterAndUnRegisterFunctionAttribute");
			foreach (MethodDefinition method in type.Constructors)
				if (method.Name == "unregister")
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void CallingPrivateMethodsThroughDelegatesTest ()
		{
			type = GetTest ("CallingPrivateMethodsThroughDelegates");
			foreach (MethodDefinition method in type.Constructors)
				if (method.Name == "privateMethod")
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void CheckClassWithFinalizer ()
		{
			type = GetTest ("ClassWithFinalizer");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (type.Methods [0]));
		}

		[Test]
		public void CheckExplicitInterfaceImplementation ()
		{
			type = GetTest ("MyList");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), method.Name);
			}
		}

		[Test]
		public void AnonymousMethodTest ()
		{
			// compiler generated code is compiler dependant, check for [g]mcs (inner type)
			type = GetTest ("AnonymousMethod/<>c__CompilerGenerated0");
			// otherwise try for csc (inside same class)
			if (type == null)
				type = GetTest ("AnonymousMethod");

			Assert.IsNotNull (type, "type not found");
			foreach (MethodDefinition method in type.Methods) {
				switch (method.Name) {
				case "MethodWithAnonymousMethod":
					// this isn't part of the test (but included with CSC)
					break;
				default:
					Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
					break;
				}
			}
		}
	}
}
