//
// Unit tests for DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest
//
// Authors:
//	Rolf Bjarne Kvinge <RKvinge@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Runtime.InteropServices;
using System.Text;

using Gendarme.Framework;
using Gendarme.Rules.Interoperability;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;
using Test.Rules.Fixtures;

namespace Test.Rules.Interoperability {

	[TestFixture]
	public class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest : MethodRuleTestFixture <DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule> 
	{

		private DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		
		private TestRunner runner {
			get {
				return new TestRunner (new DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule ());
			}
		}
		
		public delegate void CallbackDelegate ();

		[DllImport ("library.dll")]
		static extern void PInvokeNormal ();
		
		[DllImport ("library.dll")]
		static extern void PInvokeDelegate1 (CallbackDelegate d);
		
		[DllImport ("library.dll")]
		static extern void PInvokeDelegate2 (object obj, CallbackDelegate d);
		
		[DllImport ("library.dll")]
		static extern void PInvokeDelegate3 (CallbackDelegate d1, object obj, CallbackDelegate d3);
		
#region Callback methods
		private CallbackDelegate CallbackOK1_Field;
		private CallbackDelegate CallbackOK1_StaticField;
		private void CallbackOK1 ()
		{
			int a, b;
			try {
				a = 1;
			} catch {
				b = 2;
			}
		}
		
		private CallbackDelegate CallbackOKStatic_Field;
		private CallbackDelegate CallbackOKStatic_StaticField;
		private static void CallbackOKStatic ()
		{
			int a, b;
			try {
				a = 1;
			} catch {
				b = 2;
			}
		}
		
		private CallbackDelegate CallbackFailEmpty_Field;
		private CallbackDelegate CallbackFailEmpty_StaticField;
		private void CallbackFailEmpty ()
		{
		}

		private CallbackDelegate CallbackFailEmptyStatic_Field;
		private CallbackDelegate CallbackFailEmptyStatic_StaticField;
		private static void CallbackFailEmptyStatic ()
		{
		}
		
		private void CallbackFailNoCatch ()
		{
			try {
			} finally {
			}
		}

		private void CallbackFailNoEmptyCatch ()
		{
			try {
			} catch (Exception ex) {
			}
		}

		private void CallbackFailNotEntireMethod1 ()
		{
			int i = 0;
			try {
			} catch {
			}
		}
		
		private void CallbackFailNotEntireMethod2 ()
		{
			int i;
			try { 
			} catch {
			}
			i = 0;
		}
		
		private void CallbackFailNoEmptyCatchEntireMethod ()
		{
			try {
				int a = 1;
				try {
					int b = 2;
				} catch {
					int c = 3;
				}
			} catch (Exception ex) {
				int d = 4;
			}
		}
		#endregion
		
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Interoperability.DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest"];
		}

		private MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}
		
		private TypeDefinition GetTypeTest (string name)
		{
			Console.WriteLine ("GetTypeTest ('{1}') NestedType count: {0}", this.type.NestedTypes.Count, name);
			foreach (TypeDefinition type in this.type.NestedTypes) {
				Console.WriteLine (" Name: '{0}'", type.Name);
				if (type.Name == name)
					return type;
			}
			return null;
		}
		
		private void TestMethod_OK1 ()
		{
			CallbackDelegate c = CallbackFailEmpty;
			// Create a bad delegate but not pass it to a pinvoke.
			PInvokeNormal ();
		}

		private void TestMethod_AnonymousOK1 ()
		{
			CallbackDelegate c = delegate () {};
			// Create a bad delegate but not pass it to a pinvoke.
			PInvokeNormal ();
		}
		
		private void TestMethod_OK2 ()
		{
			CallbackDelegate a = CallbackFailEmpty;
			CallbackDelegate b = CallbackOK1;
			// Create a bad delegate and a good delegate, pass the good delegate.
			PInvokeDelegate1 (b);
		}
		
		private void TestMethod_AnonymousOK2 ()
		{
			CallbackDelegate a = delegate () { };
			CallbackDelegate b = delegate () { try {} catch {} };
			// Create a bad delegate and a good delegate, pass the good delegate.
			PInvokeDelegate1 (b);
		}
		
		private void TestMethod_CallbackOK1 ()
		{
			CallbackDelegate c = CallbackOK1;
			
			CallbackOK1_Field = CallbackOK1;
			CallbackOK1_StaticField = CallbackOK1;
			
			PInvokeDelegate1 (CallbackOK1);
			PInvokeDelegate1 (new CallbackDelegate (CallbackOK1));
			PInvokeDelegate1 (c);
			PInvokeDelegate1 (CallbackOK1_Field);
			PInvokeDelegate1 (CallbackOK1_StaticField);
			PInvokeDelegate2 (null, CallbackOK1);
			PInvokeDelegate2 (new CallbackDelegate (CallbackOK1), null);
			PInvokeDelegate3 (CallbackOK1_Field, null, CallbackOK1_StaticField);
		}
		
		private void TestMethod_CallbackOKStatic ()
		{
			CallbackDelegate c = CallbackOKStatic;
			
			CallbackOKStatic_Field = CallbackOKStatic;
			CallbackOKStatic_StaticField = CallbackOKStatic;
			PInvokeDelegate1 (CallbackOKStatic);
			PInvokeDelegate1 (new CallbackDelegate (CallbackOKStatic));
			PInvokeDelegate1 (c);
			PInvokeDelegate1 (CallbackOKStatic_Field);
			PInvokeDelegate1 (CallbackOKStatic_StaticField);
			PInvokeDelegate2 (null, CallbackOKStatic);
			PInvokeDelegate2 (new CallbackDelegate (CallbackOKStatic), null);
			PInvokeDelegate3 (CallbackOKStatic_Field, null, CallbackOKStatic_StaticField);
		}
		
		private void TestMethod_AnonymousCallbackOK1 ()
		{
			// These anonymous delegates will turn out non-static since they access a method variable in outer scope.
			int o = 7;
			CallbackDelegate c = delegate () { int a, b; try { a = o; } catch { b = o; } };
			
			CallbackOKStatic_Field = delegate () { int a, b; try { a = o; } catch { b = o; } };
			CallbackOKStatic_StaticField = delegate () { int a, b; try { a = o; } catch { b = o; } };
			PInvokeDelegate1 (delegate () { int a, b; try { a = o; } catch { b = o; } });
			PInvokeDelegate1 (new CallbackDelegate (delegate () { int a, b; try { a = o; } catch { b = o; } }));
			PInvokeDelegate1 (c);
			PInvokeDelegate1 (CallbackOKStatic_Field);
			PInvokeDelegate1 (CallbackOKStatic_StaticField);
			PInvokeDelegate2 (null, delegate () { int a, b; try { a = o; } catch { b = o; } });
			PInvokeDelegate2 (new CallbackDelegate (delegate () { int a, b; try { a = o; } catch { b = o; } }), null);
			PInvokeDelegate3 (CallbackOKStatic_Field, null, CallbackOKStatic_StaticField);
		}
		
		private void TestMethod_AnonymousCallbackOKStatic ()
		{
			// These anonymous delegates will turn out static given that they don't access any class/method variables in outer scope.
			CallbackDelegate c = delegate () { int a, b; try { a = 1; } catch { b = 2; } };
				
			CallbackOK1_Field = delegate () { int a, b; try { a = 1; } catch { b = 2; } };
			CallbackOK1_StaticField = delegate () { int a, b; try { a = 1; } catch { b = 2; } };
			
			PInvokeDelegate1 (delegate () { int a, b; try { a = 1; } catch { b = 2; } });
			PInvokeDelegate1 (new CallbackDelegate (delegate () { int a, b; try { a = 1; } catch { b = 2; } }));
			PInvokeDelegate1 (c);
			PInvokeDelegate1 (CallbackOK1_Field);
			PInvokeDelegate1 (CallbackOK1_StaticField);
			PInvokeDelegate2 (null, delegate () { int a, b; try { a = 1; } catch { b = 2; } });
			PInvokeDelegate2 (new CallbackDelegate (delegate () { int a, b; try { a = 1; } catch { b = 2; } }), null);
			PInvokeDelegate3 (CallbackOK1_Field, null, CallbackOK1_StaticField);
		}
		
		private void TestMethod_CallbackFailEmpty_a ()
		{
			PInvokeDelegate1 (CallbackFailEmpty);
		}
		
		private void TestMethod_AnonymousCallbackFailEmpty_a ()
		{
			PInvokeDelegate1 (delegate () {});
		}
		
		private void TestMethod_CallbackFailEmpty_b ()
		{
			PInvokeDelegate1 (new CallbackDelegate (CallbackFailEmpty));
		}
		
		private void TestMethod_AnonymousCallbackFailEmpty_b ()
		{
			PInvokeDelegate1 (new CallbackDelegate (delegate () {}));
		}
		
		private void TestMethod_CallbackFailEmpty_c ()
		{
			CallbackDelegate c = CallbackFailEmpty;
			
			PInvokeDelegate1 (c);
		}
		
		private void TestMethod_AnonymousCallbackFailEmpty_c ()
		{
			CallbackDelegate c = delegate () {};
			
			PInvokeDelegate1 (c);
		}
		
		private void TestMethod_CallbackFailEmpty_d ()
		{			
			CallbackFailEmptyStatic_Field = CallbackFailEmpty;
			
			PInvokeDelegate1 (CallbackFailEmptyStatic_Field);
		}
		
		private void TestMethod_AnonymousCallbackFailEmpty_d ()
		{			
			CallbackFailEmptyStatic_Field = delegate () {};
			
			PInvokeDelegate1 (CallbackFailEmptyStatic_Field);
		}
		
		private void TestMethod_CallbackFailEmpty_e ()
		{
			CallbackFailEmptyStatic_StaticField = CallbackFailEmpty;
			
			PInvokeDelegate1 (CallbackFailEmptyStatic_StaticField);
		}
		
		private void TestMethod_AnonymousCallbackFailEmpty_e ()
		{
			CallbackFailEmptyStatic_StaticField = delegate () {};
			
			PInvokeDelegate1 (CallbackFailEmptyStatic_StaticField);
		}
		
		private void TestMethod_CallbackFailEmpty_f ()
		{
			PInvokeDelegate2 (null, CallbackFailEmpty);
		}
		
		private void TestMethod_AnonymousCallbackFailEmpty_f ()
		{
			PInvokeDelegate2 (null, delegate () {});
		}
		
		private void TestMethod_CallbackOKEmpty_g ()
		{
			PInvokeDelegate2 (new CallbackDelegate (CallbackFailEmpty), null);
		}
		
		private void TestMethod_AnonymousCallbackOKEmpty_g ()
		{
			PInvokeDelegate2 (new CallbackDelegate (delegate () {}), null);
		}
		
		private void TestMethod_CallbackFailEmpty_h ()
		{			
			CallbackFailEmptyStatic_Field = CallbackFailEmpty;
			CallbackFailEmptyStatic_StaticField = CallbackFailEmpty;
			
			PInvokeDelegate3 (CallbackFailEmptyStatic_Field, null, CallbackFailEmptyStatic_StaticField);
		}
		
		private void TestMethod_AnonymousCallbackFailEmpty_h ()
		{			
			CallbackFailEmptyStatic_Field = delegate () {};
			CallbackFailEmptyStatic_StaticField = delegate () {};
			
			PInvokeDelegate3 (CallbackFailEmptyStatic_Field, null, CallbackFailEmptyStatic_StaticField);
		}
		
		class TestClass_InstanceFieldFail {
			private CallbackDelegate field;
			public void DoBadA ()
			{
				DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest.PInvokeDelegate1 (field);
			}
			public void Set ()
			{	// Note that we set the field (textually) after using the pinvoke above.
				field = DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest.CallbackFailEmptyStatic;
			}
		}
		
		class TestClass_StaticFieldFail {
			private static CallbackDelegate field;
			public void Set ()
			{
				// Reverse textual order from instance field test above.
				field = DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest.CallbackFailEmptyStatic;
			}
			public void DoBadA ()
			{
				DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest.PInvokeDelegate1 (field);
			}
		}
		
		class TestClass_AnonymousInstanceFieldFail {
			private CallbackDelegate field;
			public void DoBadA ()
			{
				DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest.PInvokeDelegate1 (field);
			}
			public void Set ()
			{	// Note that we set the field (textually) after using the pinvoke above.
				field = delegate () {};
			}
		}
		
		class TestClass_AnonymousStaticFieldFail {
			private static CallbackDelegate field;
			public void Set ()
			{
				// Reverse textual order from instance field test above.
				field = delegate () {};
			}
			public void DoBadA ()
			{
				DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest.PInvokeDelegate1 (field);
			}
		}
		
		private void TestMethod_CallbackFailEmptyStatic ()
		{
			PInvokeDelegate1 (CallbackFailEmptyStatic);
		}
		
		private void TestMethod_AnonymousCallbackFailEmptyStatic ()
		{
			PInvokeDelegate1 (delegate () { });
		}
		
		private void TestMethod_CallbackFailNoCatch ()
		{
			PInvokeDelegate1 (CallbackFailNoCatch);
		}
		
		private void TestMethod_AnonymousCallbackFailNoCatch ()
		{
			PInvokeDelegate1 (delegate () { try { } finally {} });
		}
		
		private void TestMethod_CallbackFailNoEmptyCatch ()
		{
			PInvokeDelegate1 (CallbackFailNoEmptyCatch);
		}
		
		private void TestMethod_AnonymousCallbackFailNoEmptyCatch ()
		{
			PInvokeDelegate1 (delegate () { try { } catch (Exception ex) { } });
		}
		
		private void TestMethod_CallbackFailNotEntireMethod1 ()
		{
			PInvokeDelegate1 (CallbackFailNotEntireMethod1);
		}
		
		private void TestMethod_AnonymousCallbackFailNotEntireMethod1 ()
		{
			PInvokeDelegate1 (delegate () { int i = 0; try { } catch { } });
		}
		
		private void TestMethod_CallbackFailNotEntireMethod2 ()
		{
			PInvokeDelegate1 (CallbackFailNotEntireMethod2);
		}
		
		private void TestMethod_AnonymousCallbackFailNotEntireMethod2 ()
		{
			PInvokeDelegate1 (delegate () { int i; try { } catch { } i = 0; } );
		}
		
		private void TestMethod_CallbackFailNoEmptyCatchEntireMethod ()
		{
			PInvokeDelegate1 (CallbackFailNoEmptyCatchEntireMethod);
		}
		
		private void TestMethod_AnonymousCallbackFailNoEmptyCatchEntireMethod ()
		{
			PInvokeDelegate1 (delegate () { try { int a = 1; try { int b = 2; } catch { int c = 3; } } catch (Exception ex) { int d = 4; } });
		}
		
		private void AssertTest (string name)
		{
			bool success = name.Contains ("OK");
			if (success) {
				AssertRuleSuccess<DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest> (name);
			} else {
				AssertRuleFailure<DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest> (name);
			}
		}
		
		private void AssertClass<T> ()
		{
			bool failed = false;
			RuleResult result;
			
			foreach (MethodDefinition method in DefinitionLoader.GetTypeDefinition <T> ().Methods) {
				result = ((TestRunner) Runner).CheckMethod (method);
				if (result == RuleResult.Failure)
					failed = true;
			}

			Assert.IsTrue (failed);
		}
		
		[Test]
		public void Test_OK1 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_OK2 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackOK1 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackOKStatic ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_a ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_b ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_c ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_d ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_e ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_f ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackOKEmpty_g ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_h ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_StaticFieldFail ()
		{
			AssertClass<TestClass_StaticFieldFail> ();
			AssertClass<TestClass_AnonymousStaticFieldFail> ();
		}
		
		[Test]
		public void Test_InstanceFieldFail ()
		{
			AssertClass<TestClass_InstanceFieldFail> ();
			AssertClass<TestClass_AnonymousInstanceFieldFail> ();
		}
		
		[Test]
		public void Test_CallbackFailEmptyStatic ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNoCatch ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNoEmptyCatch ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNotEntireMethod1 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNotEntireMethod2 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNoEmptyCatchEntireMethod ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "TestMethod_Anonymous"));
		}
		
	}
}
