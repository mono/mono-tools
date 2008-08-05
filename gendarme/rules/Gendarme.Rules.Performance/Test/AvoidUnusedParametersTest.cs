//
// Unit Test for AvoidUnusedParameters Rule.
//
// Authors:
//      Néstor Salceda <nestor.salceda@gmail.com>
//
//      (C) 2007 Néstor Salceda
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
using System.Reflection;
using System.Runtime.InteropServices;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Rules.Performance;

using NUnit.Framework;
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
	public class AvoidUnusedParametersTest {
		
		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private MethodDefinition method;
		private TestRunner runner;

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

		public void MethodWithUnusedParameters (IEnumerable enumerable, int x) 
		{
			Console.WriteLine ("Method with unused parameters");
		}

		public void MethodWith5UsedParameters (int x, IEnumerable enumerable, string foo, char c, float f) 
		{
			Console.WriteLine (f);
			Console.WriteLine (c);
			Console.WriteLine (foo);
			Console.WriteLine (enumerable);
			Console.WriteLine (x);
		}

		public static void StaticMethodWithUnusedParameters (int x, string foo) 
		{
		}

		public static void StaticMethodWithUsedParameters (int x, string foo) 
		{
			Console.WriteLine (x);
			Console.WriteLine (foo);
		}

		public static void StaticMethodWith5UsedParameters (int x, string foo, IEnumerable enumerable, char c, float f) 
		{
			Console.WriteLine (f);
			Console.WriteLine (c);
			Console.WriteLine (foo);
			Console.WriteLine (enumerable);
			Console.WriteLine (x);
		}

		public static void StaticMethodWith5UnusedParameters (int x, string foo, IEnumerable enumerable, char c, float f)
		{
		}
		
		[DllImport ("libc.so")]
		private static extern double cos (double x);

		public delegate void SimpleCallback (int x);
		public void SimpleCallbackImpl (int x) 
		{
		}

		public void SimpleCallbackImpl2 (int x) 
		{
		}

		public void AnonymousMethodWithUnusedParameters ()
		{
			SimpleCallback callback = delegate (int x) {
				//Empty	
			};
		}

		public delegate void SimpleEventHandler (int x);
		public event SimpleEventHandler SimpleEvent;
		public void OnSimpleEvent (int x) 
		{
		}

		public void EmptyMethod (int x) 
		{
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

		public void ButtonClick_EvenArgsUnused (object o, EventArgs e)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
		}

		public void ButtonClick_NoParameterUnused (object o, EventArgs e)
		{
			Console.WriteLine ("uho");
		}

		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new AvoidUnusedParametersRule ();
			runner = new TestRunner (rule);
		}

		[Test]
		public void PrintBannerUsingParameterTest () 
		{
			method = GetMethodForTest ("PrintBannerUsingParameter");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void PrintBannerUsingAssemblyTest ()
		{
			method = GetMethodForTest ("PrintBannerUsingAssembly");
			Assert.AreEqual (RuleResult.Failure ,runner.CheckMethod (method));
			Assert.AreEqual (1, runner.Defects.Count);
		}
 
		[Test]
		public void PrintBannerWithoutParametersTest () 
		{
			method = GetMethodForTest ("PrintBannerWithoutParameters");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void MethodWithUnusedParametersTest () 
		{
			method = GetMethodForTest ("MethodWithUnusedParameters");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
			Assert.AreEqual (2, runner.Defects.Count);
		}
		
		[Test]
		public void AbstractMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Performance.AbstractClass", "AbstractMethod");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void VirtualMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Performance.VirtualClass", "VirtualMethod");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void OverrideMethodTest () 
		{
			method = GetMethodForTestFrom ("Test.Rules.Performance.OverrideClass", "VirtualMethod");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}
		
		[Test]
		public void ExternMethodTest () 
		{
			method = GetMethodForTest ("cos");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void DelegateMethodTest () 
		{
			SimpleCallback callback = new SimpleCallback (SimpleCallbackImpl);
			method = GetMethodForTest ("SimpleCallbackImpl");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void DelegateMethodTestWithMultipleDelegates () 
		{
			SimpleCallback callback = new SimpleCallback (SimpleCallbackImpl);
			SimpleCallback callback2 = new SimpleCallback (SimpleCallbackImpl2);

			method = GetMethodForTest ("SimpleCallbackImpl");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));

			method = GetMethodForTest ("SimpleCallbackImpl2");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void AnonymousMethodTest ()
		{
			// compiler generated code is compiler dependant, check for [g]mcs (inner type)
			method = GetMethodForTestFrom ("Test.Rules.Performance.AvoidUnusedParametersTest/<>c__CompilerGenerated0", "<AnonymousMethodWithUnusedParameters>c__2");
			// otherwise try for csc (inside same class)
			if (method == null) {
				TypeDefinition type = assembly.MainModule.Types ["Test.Rules.Performance.AvoidUnusedParametersTest"];
				foreach (MethodDefinition md in type.Methods) {
					if (md.Name.StartsWith ("<AnonymousMethodWithUnusedParameters>")) {
						method = md;
						break;
					}
				}
			}
			Assert.IsNotNull (method, "method not found!");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void EventTest () 
		{
			SimpleEvent += new SimpleEventHandler (OnSimpleEvent);
			method = GetMethodForTest ("OnSimpleEvent");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		} 

		[Test]
		public void MethodWith5UsedParametersTest () 
		{
			method = GetMethodForTest ("MethodWith5UsedParameters");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void StaticMethodWithUnusedParametersTest () 
		{
			method = GetMethodForTest ("StaticMethodWithUnusedParameters");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
			Assert.AreEqual (2, runner.Defects.Count);
		}

		[Test]
		public void StaticMethodWithUsedParametersTest () 
		{
			method = GetMethodForTest ("StaticMethodWithUsedParameters");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void StaticMethodWith5UsedParametersTest () 
		{
			method = GetMethodForTest ("StaticMethodWith5UsedParameters");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}
		
		[Test]
		public void StaticMethodWith5UnusedParametersTest () 
		{
			method = GetMethodForTest ("StaticMethodWith5UnusedParameters");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
			Assert.AreEqual (5, runner.Defects.Count);
		}

		[Test]
		public void EmptyMethodTest ()
		{
			method = GetMethodForTest ("EmptyMethod");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
			Assert.AreEqual (1, runner.Defects.Count);
		}

		[Test]
		public void OperatorsClass ()
		{
			method = GetMethodForTest ("op_Equality");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "op_Equality");

			method = GetMethodForTest ("op_Inequality");
			Assert.AreEqual (RuleResult.Success, rule.CheckMethod (method), "op_Inequality");
		}

		[Test]
		public void OperatorsStructureOk ()
		{
			method = GetMethodForTestFrom ("Test.Rules.Performance.AvoidUnusedParametersTest/StructureOk", "op_Equality");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "op_Equality");

			method = GetMethodForTestFrom ("Test.Rules.Performance.AvoidUnusedParametersTest/StructureOk", "op_Inequality");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "op_Inequality");
		}

		[Test]
		public void OperatorsStructureBad ()
		{
			method = GetMethodForTestFrom ("Test.Rules.Performance.AvoidUnusedParametersTest/StructureBad", "op_Equality");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "op_Equality");
			Assert.AreEqual (2, runner.Defects.Count);

			method = GetMethodForTestFrom ("Test.Rules.Performance.AvoidUnusedParametersTest/StructureBad", "op_Inequality");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "op_Inequality");
			Assert.AreEqual (2, runner.Defects.Count);
		}

		[Test]
		public void OperatorsCecil ()
		{
			string cecil = typeof (Mono.Cecil.Cil.OpCode).Assembly.Location;
			AssemblyDefinition ad = AssemblyFactory.GetAssembly (cecil);

			method = GetMethodFromAssembly (ad, "Mono.Cecil.Cil.OpCode", "op_Equality");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "op_Equality");

			method = GetMethodFromAssembly (ad, "Mono.Cecil.Cil.OpCode", "op_Inequality");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "op_Inequality");
		}

		[Test]
		public void EventArgs ()
		{
			method = GetMethodForTestFrom ("Test.Rules.Performance.AvoidUnusedParametersTest", "ButtonClick_EvenArgsUnused");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "ButtonClick_EvenArgsUnused/Result");
			Assert.AreEqual (0, runner.Defects.Count, "ButtonClick_EvenArgsUnused/Count");

			method = GetMethodForTestFrom ("Test.Rules.Performance.AvoidUnusedParametersTest", "ButtonClick_NoParameterUnused");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "ButtonClick_NoParameterUnused/Result");
			Assert.AreEqual (0, runner.Defects.Count, "ButtonClick_NoParameterUnused/Count");
		}

		private MethodDefinition GetMethodForTest (string methodName) 
		{
			return GetMethodFromAssembly (assembly, "Test.Rules.Performance.AvoidUnusedParametersTest", methodName);
		}

		private MethodDefinition GetMethodForTestFrom (string fullTypeName, string methodName) 
		{
			return GetMethodFromAssembly (assembly, fullTypeName, methodName);
		}

		private MethodDefinition GetMethodFromAssembly (AssemblyDefinition ad, string fullTypeName, string methodName)
		{
			TypeDefinition type = ad.MainModule.Types[fullTypeName];
			if (type != null) {
				foreach (MethodDefinition method in type.Methods) {
					if (method.Name == methodName)
						return method;
				}
			}
			return null;
		}
	}
}
