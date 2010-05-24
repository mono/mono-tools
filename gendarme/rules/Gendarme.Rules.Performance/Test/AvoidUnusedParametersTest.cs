//
// Unit Test for AvoidUnusedParameters Rule.
//
// Authors:
//      Néstor Salceda <nestor.salceda@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//      (C) 2007 Néstor Salceda
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Performance {

	public abstract class AbstractClass {
		public abstract void AbstractMethod (int x);
	}

	public class VirtualClass {
		public virtual void VirtualMethod (int x) 
		{
		}
	}

	public class OverrideClass : VirtualClass {
		public override void VirtualMethod (int x) 
		{
		}
	}
	
	[TestFixture]
	public class AvoidUnusedParametersTest : MethodRuleTestFixture<AvoidUnusedParametersRule> {

		[TestFixtureSetUp]
		public void SetUp ()
		{
			Runner.Engines.Subscribe ("Gendarme.Framework.Engines.SuppressMessageEngine");
		}
		
		public void PrintBannerUsingParameter (Version version) 
		{
			Console.WriteLine ("Welcome to the foo program {0}", version);
		}

		public void PrintBannerUsingAssembly (Version version)
		{
			Console.WriteLine ("Welcome to the foo program {0}", Assembly.GetExecutingAssembly ().GetName ().Version);
		}

		public void PrintBannerWithoutParameters () 
		{
			Console.WriteLine ("Welcome to the foo program {0}", Assembly.GetExecutingAssembly ().GetName ().Version);
		}

		[Test]
		public void PrintBanner ()
		{
			AssertRuleSuccess<AvoidUnusedParametersTest> ("PrintBannerUsingParameter");
			AssertRuleFailure<AvoidUnusedParametersTest> ("PrintBannerUsingAssembly", 1);
			AssertRuleDoesNotApply<AvoidUnusedParametersTest> ("PrintBannerWithoutParameters");
		}

		public void MethodWithUnusedParameters (IEnumerable enumerable, int x) 
		{
			Console.WriteLine ("Method with unused parameters");
		}

		[Test]
		public void MethodWithUnusedParametersTest ()
		{
			AssertRuleFailure<AvoidUnusedParametersTest> ("MethodWithUnusedParameters", 2);
		}

		public void MethodWith5UsedParameters (int x, IEnumerable enumerable, string foo, char c, float f) 
		{
			Console.WriteLine (f);
			Console.WriteLine (c);
			Console.WriteLine (foo);
			Console.WriteLine (enumerable);
			Console.WriteLine (x);
		}

		[Test]
		public void MethodWith5UsedParametersTest ()
		{
			AssertRuleSuccess<AvoidUnusedParametersTest> ("MethodWith5UsedParameters");
		}

		public static void StaticMethodWithUnusedParameters (int x, string foo) 
		{
		}

		[Test]
		public void StaticMethodWithUnusedParametersTest ()
		{
			AssertRuleFailure<AvoidUnusedParametersTest> ("StaticMethodWithUnusedParameters", 2);
		}

		public static void StaticMethodWithUsedParameters (int x, string foo) 
		{
			Console.WriteLine (x);
			Console.WriteLine (foo);
		}

		[Test]
		public void StaticMethodWithUsedParametersTest ()
		{
			AssertRuleSuccess<AvoidUnusedParametersTest> ("StaticMethodWithUsedParameters");
		}

		public static void StaticMethodWith5UsedParameters (int x, string foo, IEnumerable enumerable, char c, float f) 
		{
			Console.WriteLine (f);
			Console.WriteLine (c);
			Console.WriteLine (foo);
			Console.WriteLine (enumerable);
			Console.WriteLine (x);
		}

		[Test]
		public void StaticMethodWith5UsedParametersTest ()
		{
			AssertRuleSuccess<AvoidUnusedParametersTest> ("StaticMethodWith5UsedParameters");
		}

		public static void StaticMethodWith5UnusedParameters (int x, string foo, IEnumerable enumerable, char c, float f)
		{
		}

		[Test]
		public void StaticMethodWith5UnusedParametersTest ()
		{
			AssertRuleFailure<AvoidUnusedParametersTest> ("StaticMethodWith5UnusedParameters", 5);
		}
		
		public delegate void SimpleCallback (int x);
		public void SimpleCallbackImpl (int x) 
		{
		}

		public void SimpleCallbackImpl2 (int x) 
		{
		}

		[Test]
		public void DelegateMethodTest ()
		{
			SimpleCallback callback = new SimpleCallback (SimpleCallbackImpl);
			AssertRuleDoesNotApply<AvoidUnusedParametersTest> ("SimpleCallbackImpl");
		}

		[Test]
		public void DelegateMethodTestWithMultipleDelegates ()
		{
			SimpleCallback callback = new SimpleCallback (SimpleCallbackImpl);
			SimpleCallback callback2 = new SimpleCallback (SimpleCallbackImpl2);

			AssertRuleDoesNotApply<AvoidUnusedParametersTest> ("SimpleCallbackImpl");
			AssertRuleDoesNotApply<AvoidUnusedParametersTest> ("SimpleCallbackImpl2");
		}

		public void AnonymousMethodWithUnusedParameters ()
		{
			SimpleCallback callback = delegate (int x) {
				//Empty	
			};
		}

		[Test]
		public void AnonymousMethodTest ()
		{
			MethodDefinition method = null;
			// compiler generated code is compiler dependant, check for [g]mcs (inner type)
			TypeDefinition type = DefinitionLoader.GetTypeDefinition (typeof (AvoidUnusedParametersTest).Assembly, "AvoidUnusedParametersTest/<>c__CompilerGenerated0");
			if (type != null)
				method = DefinitionLoader .GetMethodDefinition (type, "<AnonymousMethodWithUnusedParameters>c__2", null);
			// otherwise try for csc (inside same class)
			if (method == null) {
				type = DefinitionLoader.GetTypeDefinition<AvoidUnusedParametersTest> ();
				foreach (MethodDefinition md in type.Methods) {
					if (md.Name.StartsWith ("<AnonymousMethodWithUnusedParameters>")) {
						method = md;
						break;
					}
				}
			}
			Assert.IsNotNull (method, "method not found!");
			AssertRuleDoesNotApply (method);
		}

		public delegate void SimpleEventHandler (int x);
		public event SimpleEventHandler SimpleEvent;
		public void OnSimpleEvent (int x) 
		{
		}

		[Test]
		public void EventTest ()
		{
			SimpleEvent += new SimpleEventHandler (OnSimpleEvent);
			AssertRuleDoesNotApply<AvoidUnusedParametersTest> ("OnSimpleEvent");
		} 

		public void EmptyMethod (int x) 
		{
		}

		[Test]
		public void EmptyMethodTest ()
		{
			AssertRuleFailure<AvoidUnusedParametersTest> ("EmptyMethod", 1);
		}

		public static bool operator == (AvoidUnusedParametersTest t1, AvoidUnusedParametersTest t2)
		{
			return t1.Equals (t2);
		}

		public static bool operator != (AvoidUnusedParametersTest t1, AvoidUnusedParametersTest t2)
		{
			return !t1.Equals (t2);
		}

		public struct StructureOk {

			public static bool operator == (StructureOk s1, StructureOk s2)
			{
				return Object.ReferenceEquals (s1, s2);
			}

			public static bool operator != (StructureOk s1, StructureOk s2)
			{
				return !Object.ReferenceEquals (s1, s2);
			}
		}

		public struct StructureBad {

			public static bool operator == (StructureBad s1, StructureBad s2)
			{
				return true;
			}

			public static bool operator != (StructureBad s1, StructureBad s2)
			{
				return false;
			}
		}

		[Test]
		public void OperatorsClass ()
		{
			AssertRuleSuccess<AvoidUnusedParametersTest> ("op_Equality");
			AssertRuleSuccess<AvoidUnusedParametersTest> ("op_Inequality");
		}

		[Test]
		public void OperatorsStructureOk ()
		{
			AssertRuleSuccess<AvoidUnusedParametersTest.StructureOk> ("op_Equality");
			AssertRuleSuccess<AvoidUnusedParametersTest.StructureOk> ("op_Inequality");
		}

		[Test]
		public void OperatorsStructureBad ()
		{
			AssertRuleFailure<AvoidUnusedParametersTest.StructureBad> ("op_Equality", 2);
			AssertRuleFailure<AvoidUnusedParametersTest.StructureBad> ("op_Inequality", 2);
		}

		[Test]
		public void OperatorsCecil ()
		{
			AssertRuleSuccess<OpCode> ("op_Equality");
			AssertRuleSuccess<OpCode> ("op_Inequality");
		}

		public void ButtonClick_EvenArgsUnused (object o, EventArgs e)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
		}

		public void ButtonClick_NoParameterUnused (object o, EventArgs e)
		{
			Console.WriteLine ("uho");
		}

		[Test]
		public void EventArgs ()
		{
			AssertRuleDoesNotApply<AvoidUnusedParametersTest> ("ButtonClick_EvenArgsUnused");
			AssertRuleDoesNotApply<AvoidUnusedParametersTest> ("ButtonClick_NoParameterUnused");
		}

		[Test]
		public void AbstractMethodTest () 
		{
			AssertRuleDoesNotApply<AbstractClass> ("AbstractMethod");
		}

		[Test]
		public void VirtualMethodTest () 
		{
			AssertRuleDoesNotApply<VirtualClass> ("VirtualMethod");
		}

		[Test]
		public void OverrideMethodTest () 
		{
			AssertRuleDoesNotApply<OverrideClass> ("VirtualMethod");
		}

		// using our own extern helps more in coverage than SimpleMethods.External would
		[DllImport ("libc.so")]
		private static extern double cos (double x);

		[Test]
		public void ExternMethodTest () 
		{
			AssertRuleDoesNotApply<AvoidUnusedParametersTest> ("cos");
		}

		[Conditional ("DO_NOT_DEFINE")]
		void WriteLine (string s)
		{
			// the C.WL will not be compiled since DO_NOT_DEFINE is undefined
			// which means parameter 's' will be unused by the method
			Console.WriteLine (s);
		}

		[Test]
		public void ConditionalCode ()
		{
			AssertRuleDoesNotApply<AvoidUnusedParametersTest> ("WriteLine");
		}

		public class FxCopTest {

			// CA1801
			public class ReviewUnusedParameters {
				static public void Fail (int count)
				{
				}

				// manually suppressed - no MessageId
				[SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters")]
				static public void ManuallySuppressed (int count)
				{
				}

				// automatically suppressed using VS2010
				[System.Diagnostics.CodeAnalysis.SuppressMessage ("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "count")]
				static public void AutomaticallySuppressed (int count)
				{
				}

				// automatically suppressed using VS2010 (see GlobalSupressions.cs)
				static public void GloballySuppressed (int count)
				{
				}
			}
		}

		[Test]
		public void CA1801 ()
		{
			AssertRuleFailure<FxCopTest.ReviewUnusedParameters> ("Fail", 1);
			AssertRuleDoesNotApply<FxCopTest.ReviewUnusedParameters> ("ManuallySuppressed");
			AssertRuleDoesNotApply<FxCopTest.ReviewUnusedParameters> ("AutomaticallySuppressed");
			AssertRuleDoesNotApply<FxCopTest.ReviewUnusedParameters> ("GloballySuppressed");
		}
	}
}
