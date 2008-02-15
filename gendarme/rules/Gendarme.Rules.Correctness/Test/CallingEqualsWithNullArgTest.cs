// 
// Unit tests for CallingEqualsWithNullArgRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
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
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Correctness;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Correctness
{
	[TestFixture]
	public class CallingEqualsWithNullArgTest {
		
		public class CallToEqualsWithNullArg
		{
			public static void Main (string [] args)
			{
				CallToEqualsWithNullArg c = new CallToEqualsWithNullArg ();
				c.Equals (null);
			}
		}
		
		public class CallingEqualsWithNonNullArg 
		{
			public static void Main (string [] args)
			{
				CallingEqualsWithNonNullArg c = new CallingEqualsWithNonNullArg ();
				CallingEqualsWithNonNullArg c1 = new CallingEqualsWithNonNullArg ();
				c.Equals (c1);
			}
		}
		
		public class CallingEqualsOnEnum
		{
			enum Days { Saturday, Sunday, Monday, Tuesday, Wednesday, Thursday, Friday };
			
			public bool Equals (Enum e)
			{
				if (e == null)
					return false;
				else
					return e.GetType () == typeof (Days);
			}
			
			public void PassingArgNullInEquals ()
			{
				Type e = typeof (Days);
				e.Equals (null);
			}
			
			public void NotPassingNullArgInEquals ()
			{
				Type e = typeof (Days);
				Type e1 = typeof (Days);
				e.Equals (e1);
			}
		}
		
		public struct structure 
		{
			public bool Equals (structure s)
			{
				return s.GetType () == typeof (structure);
			}
		}
		
		public class CallingEqualsOnStruct
		{			
			public void PassingNullArgument ()
			{
				structure s = new structure ();
				s.Equals (null);
			}
			
			public void PassingNonNullArg ()
			{
				structure s = new structure ();
				structure s1 = new structure ();
				s.Equals (s1);
			}
		}
		
		public class CallingEqualsOnArray
		{
			int [] a = new int [] {1, 2, 3};
			
			public bool Equals (int [] b)
			{
				if (b == null)
					return false;
				else
					return a.Length == b.Length;
			}
			
			public void PassingNullArg ()
			{
				int [] b = new int [] {1, 2, 3};
				b.Equals (null);
			}
			
			public void PassingNonNullArg ()
			{
				int [] b = new int [] {1, 2, 3};
				b.Equals (a);
			}
		}
		
		private IMethodRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;
		private TypeDefinition type;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new CallingEqualsWithNullArgRule ();
			runner = new TestRunner (rule);
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Correctness.CallingEqualsWithNullArgTest/" + name;
			return assembly.MainModule.Types[fullname];
		}
	
		[Test]
		public void callToEqualsWithNullArgTest ()
		{
			type = GetTest ("CallToEqualsWithNullArg");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult." + method.Name);
				Assert.AreEqual (1, runner.Defects.Count, "Count." + method.Name);
			}
		}
		
		[Test]
		public void callingEqualsWithNonNullArgTest ()
		{
			type = GetTest ("CallingEqualsWithNonNullArg");
			foreach (MethodDefinition method in type.Methods) {
				Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult." + method.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + method.Name);
			}
		}
		
		[Test]
		public void enumPassingArgNullInEqualsTest ()
		{
			type = GetTest ("CallingEqualsOnEnum");
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == "PassingArgNullInEquals") {
					Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
					Assert.AreEqual (1, runner.Defects.Count, "Count");
				}
			}
		}
		
		[Test]
		public void enumNotPassingArgNullInEqualsTest ()
		{
			type = GetTest ("CallingEqualsOnEnum");
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == "NotPassingNullArgInEquals") {
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
					Assert.AreEqual (0, runner.Defects.Count, "Count");
				}
			}
		}
		
		[Test]
		public void passingNullArgumentThanStructTest ()
		{
			type = GetTest ("CallingEqualsOnStruct");
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == "PassingNullArgument") {
					Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
					Assert.AreEqual (1, runner.Defects.Count, "Count");
				}
			}
		}
				
		[Test]
		public void passingNonNullStructArgTest ()
		{
			type = GetTest ("CallingEqualsOnStruct");
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == "PassingNonNullArg") {
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
					Assert.AreEqual (0, runner.Defects.Count, "Count");
				}
			}
		}
		
		[Test]
		public void passingNullArgumentThanArrayTest ()
		{
			type = GetTest ("CallingEqualsOnArray");
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == "PassingNullArg") {
					Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
					Assert.AreEqual (1, runner.Defects.Count, "Count");
				}
			}
		}
		
		[Test]
		public void passingNonNullArrayArgumentTest ()
		{
			type = GetTest ("CallingEqualsOnArray");
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == "PassingNonNullArg") {
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
					Assert.AreEqual (0, runner.Defects.Count, "Count");
				}
			}
		}
	}
}
