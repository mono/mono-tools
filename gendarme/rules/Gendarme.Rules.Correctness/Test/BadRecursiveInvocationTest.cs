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
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

using Gendarme.Rules.Correctness;
using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

#pragma warning disable 162

	[TestFixture]
	public class BadRecursiveInvocationTest : MethodRuleTestFixture<BadRecursiveInvocationRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no CALL[VIRT] or NEWOBJ
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

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

			public static int StaticFooPlusOne {
				get { return StaticFooPlusOne + 1; }
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

			public static int StaticGoodOverload (object obzekt)
			{
				return StaticGoodOverload ((string) obzekt);
			}

			public static int StaticGoodOverload (string s)
			{
				return 0;
			}

			public static int StaticBad (object obzekt)
			{
				return StaticBad (obzekt);
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
		
		[Test]
		public void RecursiveProperties ()
		{
			AssertRuleFailure<BadRec> ("get_Foo", 1);
			AssertRuleFailure<BadRec> ("get_OnePlusFoo", 1);
			AssertRuleFailure<BadRec> ("get_FooPlusOne", 1);
			AssertRuleFailure<BadRec> ("get_StaticFooPlusOne", 1);
		}
		
		[Test]
		public void Property ()
		{
			AssertRuleDoesNotApply<BadRec> ("get_Bar"); // no method call
		}

		[Test, Ignore ("uncaught by rule")]
		public void IndirectRecursiveProperty ()
		{
			AssertRuleFailure<BadRec> ("get_FooBar", 1);
		}

		[Test]
		public void OverriddenMethod ()
		{
			AssertRuleSuccess<BadRec> ("GetHashCode");
		}
		
		[Test]
		public void BadRecursiveMethods ()
		{
			AssertRuleFailure<BadRec> ("Equals", 1);
			AssertRuleFailure<BadRec> ("StaticBad", 1);
		}

		[Test]
		public void BadFibo ()
		{
			AssertRuleFailure<BadRec> ("BadFibo", 1);
			AssertRuleFailure<BadRec> ("StaticBadFibo", 1);
		}

		[Test]
		public void Fibonacci ()
		{
			AssertRuleSuccess<BadRec> ("Fibonacci");
			AssertRuleSuccess<BadRec> ("StaticFibonacci");
		}

		[Test, Ignore ("uncaught by rule")]
		public void CodeUsingAnInstanceOfItself ()
		{
			AssertRuleFailure<BadRec> ("AnotherInstance", 1);
		}

		[Test]
		public void TestAssert ()
		{
			AssertRuleSuccess<BadRec> ("Assert");
		}

		[Test]
		public void TestStaticCallingAnotherClassWithSameMethodName ()
		{
			AssertRuleSuccess<BadRec> ("Write");
		}

		[Test]
		public void TestUnreachable ()
		{
			AssertRuleSuccess<BadRec> ("Unreachable");
		}

		// test case provided by Richard Birkby
		internal sealed class FalsePositive7 {

			public void Run ()
			{
				GetType ();
				Console.WriteLine (Select ());
			}

			private static T Select<T> ()
			{
				Console.WriteLine ();
				return default (T);
			}

			private static string Select ()
			{
				return Select<string> ();
			}
		}

		[Test]
		public void Generics ()
		{
			AssertRuleSuccess<FalsePositive7> ();
		}

		internal class InterfaceCallGood : IDeserializationCallback {

			protected virtual void OnDeserialization (object sender)
			{
				((IDeserializationCallback) this).OnDeserialization (sender);
			}

			void IDeserializationCallback.OnDeserialization (object sender)
			{
				throw new NotImplementedException ();
			}
		}

		internal class InterfaceCallBad : IDeserializationCallback {

			void IDeserializationCallback.OnDeserialization (object sender)
			{
				// uho
				((IDeserializationCallback) this).OnDeserialization (sender);
			}
		}

		[Test]
		public void Interfaces ()
		{
			AssertRuleSuccess<InterfaceCallGood> ("OnDeserialization");
			AssertRuleSuccess<InterfaceCallGood> ("System.Runtime.Serialization.IDeserializationCallback.OnDeserialization");
			AssertRuleFailure<InterfaceCallBad> ("System.Runtime.Serialization.IDeserializationCallback.OnDeserialization", 1);
		}

		// since we detect dots for interfaces... we test .ctor and .cctor
		public class MyObject : ICloneable {

			static MyObject ()
			{
			}

			public MyObject ()
			{
				Clone ();
			}

			public object Clone ()
			{
				throw new NotImplementedException ();
			}

			object ICloneable.Clone ()
			{
				return new MyObject ();
			}
		}

		[Test]
		public void Dots ()
		{
			AssertRuleDoesNotApply<MyObject> (".cctor"); // no call in method
			AssertRuleSuccess<MyObject> (".ctor");
			AssertRuleSuccess<MyObject> ("System.ICloneable.Clone");
		}

		public class Coverage {
			public int FewParameters (int a, int b, int c)
			{
				return FewParameters (a, b, c);
			}

			public int ManyParameters (int a, int b, int c, int d, int e)
			{
				return ManyParameters (a, b, c, d, e);
			}

			static int StaticFewParameters (int a, int b, int c)
			{
				return StaticFewParameters (a, b, c);
			}

			static int StaticManyParameters (int a, int b, int c, int d, int e)
			{
				return StaticManyParameters (a, b, c, d, e);
			}
		}

		[Test]
		public void MoreCoverage_Static ()
		{
			AssertRuleFailure<Coverage> ("StaticFewParameters", 1);
			AssertRuleFailure<Coverage> ("StaticManyParameters", 1);
		}

		[Test]
		public void MoreCoverage_Instance ()
		{
			AssertRuleFailure<Coverage> ("FewParameters", 1);
			AssertRuleFailure<Coverage> ("ManyParameters", 1);
		}

		[Test]
		public void StaticGoodOverload ()
		{
			AssertRuleSuccess<BadRec> ("StaticGoodOverload", new Type [] { typeof (object) });
		}

		class Array {

			public virtual void SetProperty (string name, object value)
			{
				Console.WriteLine ("{0}: {1}", name, value);
			}

			internal virtual void SetProperty (string name, object [] args)
			{
				SetProperty (name, args [0]);
			}
		}

		[Test]
		public void ArrayOverload ()
		{
			AssertRuleSuccess<Array> ("SetProperty", new Type [] { typeof (string), typeof (object[]) });
		}
	}
}
