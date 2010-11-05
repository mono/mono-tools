// 
// Unit tests for AvoidUncalledPrivateCodeRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2007-2008,2010 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

using Gendarme.Rules.Performance;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Performance {

#pragma warning disable 169, 414

	// from Mono.Rocks
	// note: error CS1109: Extension methods must be defined in a top level static class; StaticType is a nested class
	public static class StaticType {
		public static IEnumerable<TSource> Repeat<TSource> (this IEnumerable<TSource> self, int number)
		{
			//				Check.Self (self);

			return CreateRepeatIterator (self, number);
		}

		private static IEnumerable<TSource> CreateRepeatIterator<TSource> (IEnumerable<TSource> self, int number)
		{
			for (int i = 0; i < number; i++) {
				foreach (var element in self)
					yield return element;
			}
		}
	}

	[TestFixture]
	public class AvoidUncalledPrivateCodeTest : MethodRuleTestFixture<AvoidUncalledPrivateCodeRule> {
		
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

		[Test]
		public void PrivateMethodsInPublicType ()
		{
			AssertRuleFailure<UncalledPrivateMethod> ("display", 1);
			AssertRuleSuccess<CalledPrivateMethod> ("display");
			AssertRuleDoesNotApply<CalledPrivateMethod> ("Main");
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

		[Test]
		public void InternalMethodsInPublicTypes ()
		{
			AssertRuleFailure<UncalledInternalMethod> ("print", 1);
			AssertRuleSuccess<CalledInternalMethod> ("CalledMethod");
			AssertRuleDoesNotApply<MethodCallingClass> ("Main");
		}

		private class PrivateMethodInPrivateClass {
			private void privateMethodNotCalled ()
			{
				privateMethodCalled ();
			}

			// only called by uncalled method - but we don't report that
			private void privateMethodCalled ()
			{
			}
		}

		[Test]
		public void PrivateMethodsInPrivateType ()
		{
			AssertRuleFailure<PrivateMethodInPrivateClass> ("privateMethodNotCalled", 1);
			AssertRuleSuccess<PrivateMethodInPrivateClass> ("privateMethodCalled");
		}

		private class PublicMethodsInPrivateClass {
			public void PublicCalledMethod ()
			{
			}

			public virtual void PublicVirtualUncalledMethod ()
			{
			}

			public void PublicUncalledMethod ()
			{
			}

			public static void Main (string [] args)
			{
				PublicMethodsInPrivateClass p = new PublicMethodsInPrivateClass ();
				p.PublicCalledMethod ();
			}
		}

		internal class PublicMethodsInInternalClass {
			public void PublicCalledMethod ()
			{
			}

			public virtual void PublicVirtualUncalledMethod ()
			{
			}

			public void PublicUncalledMethod ()
			{
			}

			public static void Main (string [] args)
			{
				PublicMethodsInInternalClass p = new PublicMethodsInInternalClass ();
				p.PublicCalledMethod ();
			}
		}

		[Test]
		public void PublicMethods ()
		{
			AssertRuleFailure<PublicMethodsInPrivateClass> ("PublicUncalledMethod");
			AssertRuleSuccess<PublicMethodsInPrivateClass> ("PublicVirtualUncalledMethod");
			AssertRuleSuccess<PublicMethodsInPrivateClass> ("PublicCalledMethod");
			AssertRuleFailure<PublicMethodsInInternalClass> ("PublicUncalledMethod");
			AssertRuleSuccess<PublicMethodsInInternalClass> ("PublicVirtualUncalledMethod");
			AssertRuleSuccess<PublicMethodsInInternalClass> ("PublicCalledMethod");
		}

		public class AnonymousMethod {
			private void MethodWithAnonymousMethod ()
			{
				string [] values = new string [] { "one", "two", "three" };
				if (Array.Exists (values, delegate (string myString) { return myString.Length == 3; }))
					Console.WriteLine ("Exists strings with length == 3");
			}
		}

		internal class NestedClasses {
			public class AnotherClass {
				private void UncalledPrivateInNestedInternal ()
				{
				}

				internal void UncalledInternalInNestedInternal ()
				{
				}

				// public method but not visible outside the assembly
				public void UncalledPublicInNestedInternal ()
				{
				}

				// protected method but not visible outside the assembly
				protected void UncalledProtectedInNestedInternal ()
				{
				}
			}
		}

		public class NestedCalledMethods {
			class Nested {
				void callParentDisplay ()
				{
					display ();
					display2 ();
				}
			}

			private static void display ()
			{
			}

			internal static void display2 ()
			{
			}
		}

		[Test]
		public void NestedMethods ()
		{
			AssertRuleFailure<NestedClasses.AnotherClass> ("UncalledPrivateInNestedInternal");
			AssertRuleFailure<NestedClasses.AnotherClass> ("UncalledInternalInNestedInternal");
			AssertRuleFailure<NestedClasses.AnotherClass> ("UncalledPublicInNestedInternal");
			AssertRuleFailure<NestedClasses.AnotherClass> ("UncalledProtectedInNestedInternal");

			AssertRuleSuccess<NestedCalledMethods> ("display");
		}

		interface Iface1 {
			void IfaceMethod1 ();
		}

		interface Iface2 {
			void IfaceMethod2 ();
		}

		// both methods are unused but needed to satisfy interface requirements
		public class ImplementingExplicitInterfacesMembers : Iface1, Iface2 {
			void Iface1.IfaceMethod1 ()
			{
			}

			void Iface2.IfaceMethod2 ()
			{
			}
		}

		[Test]
		public void ImplementingExplicitInterfaces ()
		{
			AssertRuleSuccess<ImplementingExplicitInterfacesMembers> ();
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

		[Test]
		public void Constructors ()
		{
			AssertRuleSuccess<PrivateConstructorNotCalled> ();
			AssertRuleDoesNotApply<StaticConstructorNotCalled> (".cctor");
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

		[Test]
		public void SerializationConstructors ()
		{
			AssertRuleSuccess<PublicSerializableConstructorNotCalled> ();
			AssertRuleSuccess<PrivateSerializableConstructorNotCalled> ();
			AssertRuleSuccess<ProtectedSerializableConstructorNotCalled> ();
			AssertRuleSuccess<InternalSerializableConstructorNotCalled> ();
		}

		public class UncalledPublicOverriddenMethod {
			public override string ToString ()
			{
				return ToStringToo ();
			}

			internal virtual string ToStringToo ()
			{
				return String.Empty;
			}
		}

		public class UncalledInternalOverriddenMethod : UncalledPublicOverriddenMethod {
			// this can be accessed thru UncalledPublicOverriddenMethod 
			internal override string ToStringToo ()
			{
				return "aha!";
			}
		}

		[Test]
		public void UncalledOverriddenMethodTest ()
		{
			AssertRuleSuccess<UncalledPublicOverriddenMethod> ();
			AssertRuleSuccess<UncalledInternalOverriddenMethod> ();
		}

		public class UsingComRegisterAndUnRegisterFunctionAttribute {
			[ComRegisterFunction]
			private void register ()
			{
			}

			[ComUnregisterFunction]
			private void unregister ()
			{
			}
		}

		[Test]
		public void ComRegisterFunctions ()
		{
			AssertRuleDoesNotApply<UsingComRegisterAndUnRegisterFunctionAttribute> ("register");
			AssertRuleDoesNotApply<UsingComRegisterAndUnRegisterFunctionAttribute> ("unregister");
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

		[Test]
		public void Delegates ()
		{
			AssertRuleSuccess<CallingPrivateMethodsThroughDelegates> ("privateMethod");
		}

		public class ClassWithFinalizer {

			// finalizer is private but we can't report it as unused
			~ClassWithFinalizer ()
			{
			}

			// note: don't add anything else in this class or the test will break
		}

		[Test]
		public void Finalizer ()
		{
			AssertRuleSuccess<ClassWithFinalizer> ();
		}

		internal class MyList : IList {

			public MyList ()
			{
			}

			public int Add (object value)
			{
				throw new NotImplementedException ();
			}

			public void Clear ()
			{
				throw new NotImplementedException ();
			}

			public bool Contains (object value)
			{
				throw new NotImplementedException ();
			}

			public int IndexOf (object value)
			{
				throw new NotImplementedException ();
			}

			public void Insert (int index, object value)
			{
				throw new NotImplementedException ();
			}

			public bool IsFixedSize {
				get { throw new NotImplementedException (); }
			}

			public bool IsReadOnly {
				get { throw new NotImplementedException (); }
			}

			public void Remove (object value)
			{
				throw new NotImplementedException ();
			}

			public void RemoveAt (int index)
			{
				throw new NotImplementedException ();
			}

			public object this [int index] {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}

			public void CopyTo (Array array, int index)
			{
				throw new NotImplementedException ();
			}

			public int Count {
				get { throw new NotImplementedException (); }
			}

			public bool IsSynchronized {
				get { throw new NotImplementedException (); }
			}

			public object SyncRoot {
				get { throw new NotImplementedException (); }
			}

			public IEnumerator GetEnumerator ()
			{
				throw new NotImplementedException ();
			}
		}

		public class MyExplicitList : IList {

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

		[Test]
		public void InterfaceImplementations ()
		{
			new MyList ();
			AssertRuleSuccess<MyList> ();
			AssertRuleSuccess<MyExplicitList> ();
		}

		private AssemblyDefinition assembly;

		private TypeDefinition GetTest (string name)
		{
			if (assembly == null) {
				string unit = Assembly.GetExecutingAssembly ().Location;
				assembly = AssemblyDefinition.ReadAssembly (unit);
			}
			string fullname = "Test.Rules.Performance.AvoidUncalledPrivateCodeTest/" + name;
			return assembly.MainModule.GetType (fullname);
		}

		[Test]
		public void AnonymousMethodTest ()
		{
			// compiler generated code is compiler dependant, check for [g]mcs (inner type)
			TypeDefinition type = GetTest ("AnonymousMethod/<>c__CompilerGenerated0");
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
					if (!method.IsConstructor)
						AssertRuleDoesNotApply (method);
					break;
				}
			}
		}

		// from libanculus
		// http://code.google.com/p/libanculus-sharp/source/browse/trunk/src/Anculus.Core/Sorting/QuickSorter.cs

		public class Anculus {
			public void Sort<T> (IList<T> list, IComparer<T> comparer)
			{
				Sort<T> (list, comparer, 0, list.Count - 1);
			}

			private static void Sort<T> (IList<T> list, IComparer<T> comparer, int lower, int upper)
			{
				if (lower < upper) {
					int split = Pivot<T> (list, comparer, lower, upper);
					Sort<T> (list, comparer, lower, split - 1);
					Sort<T> (list, comparer, split + 1, upper);
				}
			}

			private static int Pivot<T> (IList<T> list, IComparer<T> comparer, int lower, int upper)
			{
				return 0;
			}
		}

		[Test]
		[Ignore ("Mono bug #320901")]
		// https://bugzilla.novell.com/show_bug.cgi?id=320901
		public void Generics ()
		{
			AssertRuleSuccess<Anculus> ();
		}

		internal abstract class X<T> {
			protected static bool a ()
			{
				return false;
			}
			public T b ()
			{
				return default(T);
			}
		}

		internal class Y : X<float> {

			public override string ToString ()
			{
				return a () ? String.Empty : base.ToString ();
			}
		}

		[Test]
		public void InheritedGenerics ()
		{
			Y y = new Y ();
			Assert.AreEqual (0.0f, y.b (), "float");
			AssertRuleSuccess<Y> ();
			AssertRuleSuccess<X<int>> ("a");
			AssertRuleSuccess<X<int>> ("b");
		}

		// test case from gmcs - bug #410000
		class EmptyAddressOf /*: EmptyExpression, IMemoryLocation */ {
			// THIS PROPERTY IS NEVER USED
			public bool IsFixed { get { return true; } }
		}

		[Test]
		public void UnusedProperty ()
		{
			AssertRuleFailure<EmptyAddressOf> ("get_IsFixed");
		}

		// test case provided by Richard Birkby
		internal sealed class FalsePositive1 {
			public void Run ()
			{
				GetType ();
				Foo<object> f = new Foo<object> ();
				f.FalsePositive1 ();
			}

			sealed class Foo<T> {
				public void FalsePositive1 ()
				{
					GetType ();
				}
			}
		}

		public void RunFalsePositive ()
		{
			new FalsePositive1 ().Run ();
		}

		[Test]
		public void MoreGenerics ()
		{
			AssertRuleSuccess<FalsePositive1> ();
		}

		interface IFoo {
			event EventHandler Foo;
		}

		interface IBar {
			event EventHandler Bar;
		}

		private class EventCases : IFoo, IBar {
			private event EventHandler unused;

			event EventHandler Unused {
				add { unused += value; }
				remove { unused -= value; }
			}

			private event EventHandler used;

			event EventHandler Used {
				add { used += value; }
				remove { used -= value; }
			}

			public EventCases ()
			{
				Used += new EventHandler (Common);
				(this as IFoo).Foo += new EventHandler (Common);
				Bar += new EventHandler (Common);
			}

			void Common (object sender, EventArgs e)
			{
				Used -= new EventHandler (Common);
			}

			private event EventHandler foo, bar;

			event EventHandler IFoo.Foo {
				add { foo += value; }
				remove { foo -= value; }
			}

			public event EventHandler Bar {
				add { bar += value; }
				remove { bar -= value; }
			}
		}

		[Test]
		public void Events ()
		{
			AssertRuleFailure<EventCases> ("add_Unused", 1);
			AssertRuleFailure<EventCases> ("remove_Unused", 1);

			AssertRuleSuccess<EventCases> ("add_Used");
			AssertRuleSuccess<EventCases> ("remove_Used");

			AssertRuleSuccess<EventCases> ("Test.Rules.Performance.AvoidUncalledPrivateCodeTest.IFoo.add_Foo");
			// not used but we ignore explicit interfaces
			AssertRuleSuccess<EventCases> ("Test.Rules.Performance.AvoidUncalledPrivateCodeTest.IFoo.remove_Foo");

			AssertRuleSuccess<EventCases> ("add_Bar");
			AssertRuleSuccess<EventCases> ("remove_Bar");
		}

		[Test]
		public void Events_Autogenerated ()
		{
			MethodDefinition md = DefinitionLoader.GetMethodDefinition<EventCases> ("add_foo");
			if (!md.IsSynchronized)
				Assert.Ignore ("newer versions of CSC (e.g. 10.0) does not set the Synchronized");
			// older CSC (and xMCS) compilers still generate it's own add/remove methods that set the
			// synchronize flags and we simply ignore them since they are out of the developer's control
			AssertRuleDoesNotApply (md);
			AssertRuleDoesNotApply<EventCases> ("remove_foo");
		}

		[Test]
		[Ignore ("Mono bug #320901")]
		// https://bugzilla.novell.com/show_bug.cgi?id=320901
		public void MonoRocks ()
		{
			AssertRuleSuccess (typeof (StaticType), "CreateRepeatIterator");
		}

		// Test cases devired from bug #458178

		void MultidimArray ()
		{
			const int LengthA = 2;
			const int LengthB = 2;

			bool [,] array = new bool [LengthA, LengthB];

			for (int i = 0; i < LengthA; i++) {
				for (int j = 0; j < LengthB; j++) {
					array [i, j] = true;
				}
			}
		}

		void JaggedArray ()
		{
			const int LengthA = 2;
			const int LengthB = 2;

			bool [][] array = new bool [LengthA][];

			for (int i = 0; i < LengthA; i++) {
				for (int j = 0; j < LengthB; j++) {
					array [i][j] = true;
				}
			}
		}

		void NullableArray ()
		{
			const int LengthA = 2;
			const int LengthB = 2;

			bool? [,] array = new bool? [LengthA, LengthB];

			for (int i = 0; i < LengthA; i++) {
				for (int j = 0; j < LengthB; j++) {
					array [i, j] = true;
				}
			}
		}

		[Test]
		// https://bugzilla.novell.com/show_bug.cgi?id=458178
		public void Arrays ()
		{
			AssertRuleFailure<AvoidUncalledPrivateCodeTest> ("MultidimArray", 1);
			AssertRuleFailure<AvoidUncalledPrivateCodeTest> ("JaggedArray", 1);
			AssertRuleFailure<AvoidUncalledPrivateCodeTest> ("NullableArray", 1);
		}

		[Conditional ("DO_NOT_DEFINE")]
		static void WriteLine (string s)
		{
			Console.WriteLine (s);
		}

		[Test]
		public void ConditionalCode ()
		{
			AssertRuleDoesNotApply<AvoidUncalledPrivateCodeTest> ("WriteLine");
		}
	}
}
