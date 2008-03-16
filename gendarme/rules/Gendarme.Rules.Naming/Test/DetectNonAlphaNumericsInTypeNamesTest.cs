// 
// Unit tests for DetectNonAlphaNumericsInTypeNamesRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
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
using System.Runtime.InteropServices;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Naming;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Naming {

	internal class InternalClassName_WithUnderscore {
	}

	public class PublicClassName_WithUnderscore {
	}

	internal interface InternalInterface_WithUnderscore {
	}

	public interface PublicInterface_WithUnderscore {
	}

	// from Mono.Cecil.Pdb
	[Guid ("809c652e-7396-11d2-9771-00a0c9b4d50c")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComVisible (true)]
	interface IMetaDataDispenser {
		void DefineScope_Placeholder ();
	}

	[TestFixture]
	public class DetectNonAlphaNumericsInTypeNamesTest {
			
		public class ClassContainingProperty {
			int i=0;
			public int Property {
				get {
					return i;
				}
				set {
					i = value;
				}
			}
		}
		
		public class ClassContainingEvent
		{
			public event EventHandler MyEvent
			{
				add { } 
				remove { }
			}
		}
	
		class ClassContainingConversionOperator 
		{ 
			private string FullName = "";
			public ClassContainingConversionOperator (string str) 
			{ 
				FullName = str; 
			} 
			public static implicit operator ClassContainingConversionOperator (string str) 
			{ 
				return new ClassContainingConversionOperator (str); 
			}
		}
			
		public class ClassContainingPrivateMethodWithUnderscore {
			private void my_method ()
			{
			}
		}
		
		public class ClassContainingPublicMethodWithUnderscore {
			public void methot_test ()
			{
			}
		}
			
		private class NestedPrivateClassName_WithUnderscore {
		}

		public class NestedPublicClassName_WithUnderscore {
		}

		private interface NestedPrivateInterface_WithUnderscore {
		}

		public interface NestedPublicInterface_WithUnderscore {
		}
			
		class DefaultPrivate_Class
		{
			void DefaultPrivate_Method ()
			{
			}
		}
		
		public class ClassWithoutUnderscore 
		{
			public void methodWithoutUnderscore ()
			{
			}
		}

		public class ClassWithDelegate {

			public bool MethodDefiningDelegate ()
			{
				byte[] array = null;
				return !Array.Exists (array, delegate (byte value) { return value.Equals (this); });
			}
		}

		
		private IRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new DetectNonAlphanumericInTypeNamesRule();
			runner = new TestRunner (rule);
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Naming.DetectNonAlphaNumericsInTypeNamesTest/" + name;
			return assembly.MainModule.Types[fullname];
		}
		
		[Test]
		public void propertyTest ()
		{
			type = GetTest ("ClassContainingProperty");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult." + method.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + method.Name);
			}
		}
			
		[Test]
		public void eventTest ()
		{
			type = GetTest ("ClassContainingEvent");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult." + method.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + method.Name);
			}
		}
			
		[Test]
		public void conversionOperatorTest ()
		{
			type = GetTest ("ClassContainingConversionOperator");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult." + method.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + method.Name);
			}
		}
		
		[Test]
		public void privateMethodWithUnderscoreTest ()
		{
			type = GetTest ("ClassContainingPrivateMethodWithUnderscore");
			Assert.AreEqual (1, type.Methods.Count, "Methods.Count");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult." + method.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + method.Name);
			}
		}
		
		[Test]
		public void publicMethodWithUnderscoreTest ()
		{
			type = GetTest ("ClassContainingPublicMethodWithUnderscore");
			Assert.AreEqual (1, type.Methods.Count, "Methods.Count");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
				Assert.AreEqual (1, runner.Defects.Count, "Count");
			}
		}
		
		[Test]
		public void nestedPrivateClassWithUnderscoreTest ()
		{
			type = GetTest ("NestedPrivateClassName_WithUnderscore");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void nestedPublicClassWithUnderscoreTest ()
		{
			type = GetTest ("NestedPublicClassName_WithUnderscore");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void nestedPrivateInterfaceWithUnderscoreTest ()
		{
			type = GetTest ("NestedPrivateInterface_WithUnderscore");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void nestedPublicInterfaceWithUnderscoreTest ()
		{
			type = GetTest ("NestedPublicInterface_WithUnderscore");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void defaultPrivateClassTest ()
		{
			type = GetTest ("DefaultPrivate_Class");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void defaultPrivateMethodTest ()
		{
			type = GetTest ("DefaultPrivate_Class");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult." + method.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + method.Name);
			}
		}
		
		[Test]
		public void classWithoutUnderscoreTest ()
		{
			type = GetTest ("ClassWithoutUnderscore");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void methodWithoutUnderscoreTest ()
		{
			type = GetTest ("ClassWithoutUnderscore");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult." + method.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + method.Name);
			}
		}

		[Test]
		public void classWithDelegate ()
		{
			type = GetTest ("ClassWithDelegate");
			foreach (MethodDefinition method in type.Methods) {
				RuleResult result = method.IsGeneratedCode () ? RuleResult.DoesNotApply : RuleResult.Success;
				Assert.AreEqual (result, runner.CheckMethod (method), "RuleResult." + method.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + method.Name);
			}
		}

		[Test]
		public void InternalClassWithUnderscoreTest ()
		{
			type = assembly.MainModule.Types["Test.Rules.Naming.InternalClassName_WithUnderscore"];
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void PublicClassWithUnderscoreTest ()
		{
			type = assembly.MainModule.Types["Test.Rules.Naming.PublicClassName_WithUnderscore"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void InternalInterfaceWithUnderscoreTest ()
		{
			type = assembly.MainModule.Types["Test.Rules.Naming.InternalInterface_WithUnderscore"];
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void PublicInterfaceWithUnderscoreTest ()
		{
			type = assembly.MainModule.Types["Test.Rules.Naming.PublicInterface_WithUnderscore"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void ComInterop ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.IMetaDataDispenser"];
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "Type/RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Type/Count");

			MethodDefinition method = type.GetMethod ("DefineScope_Placeholder");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "Method/RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Method/Count");
		}
	}
}
