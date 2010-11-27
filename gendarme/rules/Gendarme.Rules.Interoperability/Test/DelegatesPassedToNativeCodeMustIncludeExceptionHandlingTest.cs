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
	public class DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest
	{

		private DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		
		public delegate void CallbackDelegate ();
		public delegate byte CallbackDelegateByte ();
		public delegate void CallbackDelegateOut (out int a);
		public delegate System.DateTime CallbackDelegateNonVoidStruct ();

		[DllImport ("library.dll")]
		static extern void PInvokeNormal ();
		
		[DllImport ("library.dll")]
		static extern void PInvokeDelegate1 (CallbackDelegate d);
		
		[DllImport ("library.dll")]
		static extern void PInvokeDelegate2 (object obj, CallbackDelegate d);
		
		[DllImport ("library.dll")]
		static extern void PInvokeDelegate3 (CallbackDelegate d1, object obj, CallbackDelegate d3);
		
		[DllImport ("library.dll")]
		static extern void PInvokeDelegate4 (CallbackDelegateByte d);
		
		[DllImport ("library.dll")]
		static extern void PInvokeDelegate5 (CallbackDelegateOut d);

		[DllImport ("library.dll")]
		static extern void PInvokeDelegate6 (CallbackDelegateNonVoidStruct d);
		
#region Callback methods
		private CallbackDelegate CallbackOK1_Field;
		private CallbackDelegate CallbackOK1_StaticField;
		private void CallbackOK1 ()
		{
			int a, b;
			try {
				a = 1;
				Console.WriteLine (a);
			}
			catch {
				b = 2;
			}
		}
		
		private byte CallbackOK_NonVoid ()
		{
			try {
				Console.WriteLine ("try");
				return 1;
			} catch {
				return 2;
			}
		}
		
		private CallbackDelegate CallbackOKStatic_Field;
		private CallbackDelegate CallbackOKStatic_StaticField;
		private static void CallbackOKStatic ()
		{
			int a, b;
			try {
				a = 1;
				Console.WriteLine (a);
			}
			catch {
				b = 2;
			}
		}
		
		private void CallbackOKEmpty ()
		{
			// Nothing is done here, so this method is ok.
		}
		
		private CallbackDelegate CallbackFailEmpty_Field;
		private CallbackDelegate CallbackFailEmpty_StaticField;
		private void CallbackFailEmpty ()
		{
			Console.WriteLine ();
		}

		private byte CallbackFailEmpty_NonVoid ()
		{
			Console.WriteLine ();
			return 1;
		}
	
		private CallbackDelegate CallbackFailEmptyStatic_Field;
		private CallbackDelegate CallbackFailEmptyStatic_StaticField;
		private static void CallbackFailEmptyStatic ()
		{
			Console.WriteLine ();
		}
		
		private void CallbackFailNoCatch ()
		{
			try {
				Console.WriteLine ();
			} finally {
			}
		}

		private void CallbackFailNoEmptyCatch ()
		{
			try {
				Console.WriteLine ();
			} catch (StackOverflowException ex) {
			}
		}

		private void CallbackFailNotEntireMethod1 ()
		{
			Console.WriteLine ();
			try {
			} catch {
			}
		}
		
		private void CallbackFailNotEntireMethod2 ()
		{
			try { 
			} catch {
			}
			Console.WriteLine ();
		}
		
		private void CallbackFailNoEmptyCatchEntireMethod ()
		{
			try {
				Console.WriteLine ();
				try {
					Console.WriteLine ();
				} catch {
					Console.WriteLine ();
				}
			} catch (StackOverflowException ex) {
				Console.WriteLine ();
			}
		}
		#endregion
		
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			type = assembly.MainModule.GetType ("Test.Rules.Interoperability.DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest");
		}

		private void CheckMethod_OK1 ()
		{
			CallbackDelegate c = CallbackFailEmpty;
			// Create a bad delegate but not pass it to a pinvoke.
			PInvokeNormal ();
		}

		private void CheckMethod_AnonymousOK1 ()
		{
			CallbackDelegate c = delegate () {};
			// Create a bad delegate but not pass it to a pinvoke.
			PInvokeNormal ();
		}
		
		private void CheckMethod_OK2 ()
		{
			CallbackDelegate a = CallbackFailEmpty;
			CallbackDelegate b = CallbackOK1;
			// Create a bad delegate and a good delegate, pass the good delegate.
			PInvokeDelegate1 (b);
		}
		
		private void CheckMethod_AnonymousOK2 ()
		{
			CallbackDelegate a = delegate () { };
			CallbackDelegate b = delegate () { 
				try {
					Console.WriteLine ("try");
				} 
				catch {
				} 
			};
			// Create a bad delegate and a good delegate, pass the good delegate.
			PInvokeDelegate1 (b);
		}
		
		private void CheckMethod_CallbackOK1 ()
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
		
		private void CheckMethod_CallbackOKStatic ()
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
		
		private void CheckMethod_AnonymousCallbackOK1 ()
		{
			// These anonymous delegates will turn out non-static since they access a method variable in outer scope.
			int o = 7;
			CallbackDelegate c = delegate () {
				int a, b; 
				try { 
					a = o;
					Console.WriteLine (a);
				} 
				catch { 
				}
			};
			
			CallbackOKStatic_Field = delegate () {
				int a, b;
				try {
					a = o;
					Console.WriteLine (a);
				}
				catch {
				}
			};

			CallbackOKStatic_StaticField = delegate () {
				int a, b;
				try {
					a = o;
					Console.WriteLine (a);
				}
				catch {
				}
			};

			PInvokeDelegate1 (delegate () {
								int a, b; 
				try { 
					a = o;
					Console.WriteLine (a);
				} 
				catch { 
				}
			});

			PInvokeDelegate1 (new CallbackDelegate (delegate () {
								int a, b; 
				try { 
					a = o;
					Console.WriteLine (a);
				} 
				catch { 
				}
			}));
			PInvokeDelegate1 (c);
			PInvokeDelegate1 (CallbackOKStatic_Field);
			PInvokeDelegate1 (CallbackOKStatic_StaticField);
			PInvokeDelegate2 (null, delegate () {
				int a, b; 
				try { 
					a = o;
					Console.WriteLine (a);
				} 
				catch { 
				}
			});
			PInvokeDelegate2 (new CallbackDelegate (delegate () {
								int a, b; 
				try { 
					a = o;
					Console.WriteLine (a);
				} 
				catch { 
				}
			}), null);
			PInvokeDelegate3 (CallbackOKStatic_Field, null, CallbackOKStatic_StaticField);
		}
		
		private void CheckMethod_AnonymousCallbackOKStatic ()
		{
			// These anonymous delegates will turn out static given that they don't access any class/method variables in outer scope.
			CallbackDelegate c = delegate () {
				int a, b;
				try {
					a = 1;
					Console.WriteLine (a);
				}
				catch { 
					b = 2;
				}
			};
				
			CallbackOK1_Field = delegate () {
				int a, b;
				try {
					a = 1;
					Console.WriteLine (a);
				}
				catch {
					b = 2;
				}
			};

			CallbackOK1_StaticField = delegate () {
				int a, b;
				try {
					a = 1;
					Console.WriteLine (a);
				}
				catch {
					b = 2;
				}
			};
			
			PInvokeDelegate1 (delegate () {
				int a, b;
				try {
					a = 1;
					Console.WriteLine (a);
				}
				catch { 
					b = 2;
				}
			});

			PInvokeDelegate1 (new CallbackDelegate (delegate () {
				int a, b;
				try {
					a = 1;
					Console.WriteLine (a);
				}
				catch { 
					b = 2;
				}
			}));

			PInvokeDelegate1 (c);
			PInvokeDelegate1 (CallbackOK1_Field);
			PInvokeDelegate1 (CallbackOK1_StaticField);
			PInvokeDelegate2 (null, delegate () {
				int a, b;
				try {
					a = 1;
					Console.WriteLine (a);
				}
				catch { 
					b = 2;
				}
			});

			PInvokeDelegate2 (new CallbackDelegate (delegate () {
				int a, b;
				try {
					a = 1;
					Console.WriteLine (a);
				}
				catch { 
					b = 2;
				}
			}), null);

			PInvokeDelegate3 (CallbackOK1_Field, null, CallbackOK1_StaticField);
		}
		
		private void CheckMethod_CallbackFailEmpty_a ()
		{
			PInvokeDelegate1 (CallbackFailEmpty);
		}
		
		private void CheckMethod_AnonymousCallbackFailEmpty_a ()
		{
			PInvokeDelegate1 (delegate () { Console.WriteLine (); });
		}
		
		private void CheckMethod_CallbackFailEmpty_b ()
		{
			PInvokeDelegate1 (new CallbackDelegate (CallbackFailEmpty));
		}
		
		private void CheckMethod_AnonymousCallbackFailEmpty_b ()
		{
			PInvokeDelegate1 (new CallbackDelegate (delegate () { Console.WriteLine (); }));
		}
		
		private void CheckMethod_CallbackFailEmpty_c ()
		{
			CallbackDelegate c = CallbackFailEmpty;
			
			PInvokeDelegate1 (c);
		}
		
		private void CheckMethod_AnonymousCallbackFailEmpty_c ()
		{
			CallbackDelegate c = delegate () { Console.WriteLine (); };
			
			PInvokeDelegate1 (c);
		}
		
		private void CheckMethod_CallbackFailEmpty_d ()
		{			
			CallbackFailEmptyStatic_Field = CallbackFailEmpty;
			
			PInvokeDelegate1 (CallbackFailEmptyStatic_Field);
		}
		
		private void CheckMethod_AnonymousCallbackFailEmpty_d ()
		{			
			CallbackFailEmptyStatic_Field = delegate () { Console.WriteLine (); };
			
			PInvokeDelegate1 (CallbackFailEmptyStatic_Field);
		}
		
		private void CheckMethod_CallbackFailEmpty_e ()
		{
			CallbackFailEmptyStatic_StaticField = CallbackFailEmpty;
			
			PInvokeDelegate1 (CallbackFailEmptyStatic_StaticField);
		}
		
		private void CheckMethod_AnonymousCallbackFailEmpty_e ()
		{
			CallbackFailEmptyStatic_StaticField = delegate () { Console.WriteLine (); };
			
			PInvokeDelegate1 (CallbackFailEmptyStatic_StaticField);
		}
		
		private void CheckMethod_CallbackFailEmpty_f ()
		{
			PInvokeDelegate2 (null, CallbackFailEmpty);
		}
		
		private void CheckMethod_AnonymousCallbackFailEmpty_f ()
		{
			PInvokeDelegate2 (null, delegate () { Console.WriteLine (); });
		}
		
		private void CheckMethod_CallbackOKEmpty_g ()
		{
			PInvokeDelegate2 (new CallbackDelegate (CallbackFailEmpty), null);
		}
		
		private void CheckMethod_AnonymousCallbackOKEmpty_g ()
		{
			PInvokeDelegate2 (new CallbackDelegate (delegate () { Console.WriteLine (); }), null);
		}
		
		private void CheckMethod_CallbackFailEmpty_h ()
		{			
			CallbackFailEmptyStatic_Field = CallbackFailEmpty;
			CallbackFailEmptyStatic_StaticField = CallbackFailEmpty;
			
			PInvokeDelegate3 (CallbackFailEmptyStatic_Field, null, CallbackFailEmptyStatic_StaticField);
		}
		
		private void CheckMethod_AnonymousCallbackFailEmpty_h ()
		{			
			CallbackFailEmptyStatic_Field = delegate () { Console.WriteLine (); };
			CallbackFailEmptyStatic_StaticField = delegate () { Console.WriteLine (); };
			
			PInvokeDelegate3 (CallbackFailEmptyStatic_Field, null, CallbackFailEmptyStatic_StaticField);
		}
		
		class CheckClass_InstanceFieldFail {
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
		
		class CheckClass_StaticFieldFail {
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
		
		class CheckClass_AnonymousInstanceFieldFail {
			private CallbackDelegate field;
			public void DoBadA ()
			{
				DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest.PInvokeDelegate1 (field);
			}
			public void Set ()
			{	// Note that we set the field (textually) after using the pinvoke above.
				field = delegate () { Console.WriteLine (); };
			}
		}
		
		class CheckClass_AnonymousStaticFieldFail {
			private static CallbackDelegate field;
			public void Set ()
			{
				// Reverse textual order from instance field test above.
				field = delegate () { Console.WriteLine (); };
			}
			public void DoBadA ()
			{
				DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest.PInvokeDelegate1 (field);
			}
		}
		
		private void CheckMethod_CallbackFailEmptyStatic ()
		{
			PInvokeDelegate1 (CallbackFailEmptyStatic);
		}
		
		private void CheckMethod_AnonymousCallbackFailEmptyStatic ()
		{
			PInvokeDelegate1 (delegate () { Console.WriteLine (); });
		}
		
		private void CheckMethod_CallbackFailNoCatch ()
		{
			PInvokeDelegate1 (CallbackFailNoCatch);
		}
		
		private void CheckMethod_AnonymousCallbackFailNoCatch ()
		{
			PInvokeDelegate1 (delegate () { try { Console.WriteLine (); } finally {} });
		}
		
		private void CheckMethod_CallbackFailNoEmptyCatch ()
		{
			PInvokeDelegate1 (CallbackFailNoEmptyCatch);
		}
		
		private void CheckMethod_AnonymousCallbackFailNoEmptyCatch ()
		{
			PInvokeDelegate1 (delegate () { try { Console.WriteLine (); } catch (StackOverflowException ex) { } });
		}
		
		private void CheckMethod_CallbackFailNotEntireMethod1 ()
		{
			PInvokeDelegate1 (CallbackFailNotEntireMethod1);
		}
		
		private void CheckMethod_AnonymousCallbackFailNotEntireMethod1 ()
		{
			PInvokeDelegate1 (delegate () { Console.WriteLine (); try { } catch { } });
		}
		
		private void CheckMethod_CallbackFailNotEntireMethod2 ()
		{
			PInvokeDelegate1 (CallbackFailNotEntireMethod2);
		}
		
		private void CheckMethod_AnonymousCallbackFailNotEntireMethod2 ()
		{
			PInvokeDelegate1 (delegate () { try { } catch { } Console.WriteLine (); } );
		}
		
		private void CheckMethod_CallbackFailNoEmptyCatchEntireMethod ()
		{
			PInvokeDelegate1 (CallbackFailNoEmptyCatchEntireMethod);
		}
		
		private void CheckMethod_AnonymousCallbackFailNoEmptyCatchEntireMethod ()
		{
			PInvokeDelegate1 (delegate () { try { Console.WriteLine (); try { Console.WriteLine (); } catch { Console.WriteLine (); } } catch (Exception ex) { Console.WriteLine (); } });
		}
				
		private void CheckMethod_NonVoidOK ()
		{
			PInvokeDelegate4 (CallbackOK_NonVoid);
		}
		
		private void CheckMethod_NonVoidFail ()
		{
			PInvokeDelegate4 (CallbackFailEmpty_NonVoid);
		}
		
		private void CheckMethod_TwoFailures_a ()
		{
			PInvokeDelegate1 (delegate () { Console.WriteLine (); });
			PInvokeDelegate1 (delegate () { Console.WriteLine (); });
		}
			
		private void CheckMethod_TwoFailures_b ()
		{
			PInvokeDelegate3 (delegate () { Console.WriteLine (); }, null, delegate () { Console.WriteLine (); });
		}
		
		private void CheckMethod_OKOutParams ()
		{
			PInvokeDelegate5 (
				delegate (out int a)
				{
					a = 0;
				}
			);
		}
		
		private void CheckMethod_FailOutParams ()
		{
			PInvokeDelegate5 (
				delegate (out int a)
				{
					Console.WriteLine ();
					a = 2;
				}
			);
		}
		
		private void CheckMethod_OKNonVoidStruct ()
		{
			PInvokeDelegate6 (
				delegate () 
				{
					return new DateTime ();
				}
			);
		}
		
		private void CheckMethod_OKNonVoidStruct2 ()
		{
			PInvokeDelegate6 (
				delegate () 
				{
					return DateTime.MinValue;
				}
			);
		}
		
		private void AssertTest (string name)
		{
			AssertTest (name, 1);
		}
		
		private void AssertTest (string name, int expectedCount)
		{
			TestRunner runner;
			MethodDefinition method;
			RuleResult result;
			RuleResult expected =  name.Contains ("OK") ? RuleResult.Success : RuleResult.Failure;

			if (expected == RuleResult.Success)
				expectedCount = 0;
			
			// Since the rule only reports errors once for each method, and these tests reuse methods,
			// we need a new test runner for each test.
			
			runner = new TestRunner (new DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule ());
			runner.Rules.Clear ();
			method = DefinitionLoader.GetMethodDefinition <DelegatesPassedToNativeCodeMustIncludeExceptionHandlingTest> (name);
			result = runner.CheckMethod (method);
			
			Assert.AreEqual (expected, result);
			Assert.AreEqual (expectedCount, runner.Defects.Count, "DefectCount");
		}
		
		private void AssertClass<T> ()
		{
			TestRunner runner;
			bool failed = false;
			RuleResult result;
			
			// 
			// We assert that exactly 1 error is raised among all the methods in the class
			// 
			
			runner = new TestRunner (new DelegatesPassedToNativeCodeMustIncludeExceptionHandlingRule ());
			
			foreach (MethodDefinition method in DefinitionLoader.GetTypeDefinition <T> ().Methods) {
				result = runner.CheckMethod (method);
				if (result == RuleResult.Failure) {
					Assert.IsFalse (failed);
					Assert.AreEqual (1, runner.Defects.Count);
					failed = true;
				}
			}

			Assert.IsTrue (failed);
		}
		
		[Test]
		public void Test_NonVoidStruct ()
		{
			AssertTest ("CheckMethod_OKNonVoidStruct");
			AssertTest ("CheckMethod_OKNonVoidStruct2");
		}
		
		[Test]
		public void Test_OutParams ()
		{
			AssertTest ("CheckMethod_OKOutParams");
			AssertTest ("CheckMethod_FailOutParams", 1);
		}
		
		[Test]
		public void Test_TwoFailures ()
		{
			AssertTest ("CheckMethod_TwoFailures_a", 2);
			AssertTest ("CheckMethod_TwoFailures_b", 2);
		}
		
		[Test]
		public void Test_NonVoid ()
		{
			AssertTest ("CheckMethod_NonVoidOK");
			AssertTest ("CheckMethod_NonVoidFail");
		}
		
		[Test]
		public void Test_OK1 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_OK2 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackOK1 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackOKStatic ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_a ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_b ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_c ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_d ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_e ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_f ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackOKEmpty_g ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailEmpty_h ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"), 2);
		}
		
		[Test]
		public void Test_StaticFieldFail ()
		{
			AssertClass<CheckClass_StaticFieldFail> ();
			AssertClass<CheckClass_AnonymousStaticFieldFail> ();
		}
		
		[Test]
		public void Test_InstanceFieldFail ()
		{
			AssertClass<CheckClass_InstanceFieldFail> ();
			AssertClass<CheckClass_AnonymousInstanceFieldFail> ();
		}
		
		[Test]
		public void Test_CallbackFailEmptyStatic ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNoCatch ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNoEmptyCatch ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNotEntireMethod1 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNotEntireMethod2 ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
		[Test]
		public void Test_CallbackFailNoEmptyCatchEntireMethod ()
		{
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_"));
			AssertTest (MethodInfo.GetCurrentMethod ().Name.Replace ("Test_", "CheckMethod_Anonymous"));
		}
		
	}
}
