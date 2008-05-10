// 
// Unit tests for ImplementEqualsAndGetHashCodeInPairRule
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
using System.Collections;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Design;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Design {
	
	[TestFixture]
	public class ImplementEqualsAndGetHashCodeInPairTest {

		public class ImplementsEqualsButNotGetHashCode {
			public override bool Equals (Object obj)
			{
				return this == obj;
			}
		}
			
		public class ImplementsGetHashCodeButNotEquals {
			public override int GetHashCode ()
			{
				return 2;
			}
		}
		
		public class ImplementsNoneOfThem {
			public void test ()
			{
			}
		}
		
		public class ImplementsBothOfThem {
			public override int GetHashCode ()
			{
				return 2;
			}
			public new bool Equals (Object obj)
			{
				return this == obj;
			}
		}

		public class ImplementsEqualsUsesObjectGetHashCode {
			public override bool Equals (Object obj)
			{
				return this == obj;
			}
			public static void Main (string [] args)
			{
				int j = 0;
				ImplementsEqualsUsesObjectGetHashCode i = new ImplementsEqualsUsesObjectGetHashCode ();
				j = i.GetHashCode ();
			}
		}

		public class ImplementsEqualsReuseBaseGetHashCode {
			public override bool Equals (Object obj)
			{
				return this == obj;
			}
			public override int  GetHashCode()
			{
 				 return base.GetHashCode();
			}
			public static void Main (string [] args)
			{
				int j = 0;
				ImplementsEqualsUsesObjectGetHashCode i = new ImplementsEqualsUsesObjectGetHashCode ();
				j = i.GetHashCode ();
			}
		}
		
		public class ImplementsGetHashCodeUsesObjectEquals {
			public override int GetHashCode ()
			{
				return 1;
			}
			public static void Main (string [] args)
			{
				ImplementsGetHashCodeUsesObjectEquals i = new ImplementsGetHashCodeUsesObjectEquals ();
				ImplementsGetHashCodeUsesObjectEquals i1 = new ImplementsGetHashCodeUsesObjectEquals ();
				i.Equals (i1);
			}
		}
			
		public class ImplementingEqualsWithTwoArgs {
			public bool Equals (Object obj1, Object obj2)
			{
				return obj1 == obj2;
			}
		}
		
		public class ImplementingGetHashCodeWithOneArg {
			public int GetHashCode (int j)
			{
				return j*2;
			}
		}
		
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new ImplementEqualsAndGetHashCodeInPairRule ();
			runner = new TestRunner (rule);
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Design.ImplementEqualsAndGetHashCodeInPairTest/" + name;
			return assembly.MainModule.Types[fullname];
		}
		
		[Test]
		public void EqualsButNotGetHashCodeTest ()
		{ 
			TypeDefinition type = GetTest ("ImplementsEqualsButNotGetHashCode");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void GetHashCodeButNotEqualsTest ()
		{
			TypeDefinition type = GetTest ("ImplementsGetHashCodeButNotEquals");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void NoneOfThemTest ()
		{
			TypeDefinition type = GetTest ("ImplementsNoneOfThem");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void BothOfThemTest ()
		{
			TypeDefinition type = GetTest ("ImplementsBothOfThem");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void ImplementsEqualsUsesObjectGetHashCodeTest ()
		{
			TypeDefinition type = GetTest ("ImplementsEqualsUsesObjectGetHashCode");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void ImplementsEqualsReuseBaseGetHashCodeTest ()
		{
			TypeDefinition type = GetTest ("ImplementsEqualsReuseBaseGetHashCode");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void ImplementsGetHashCodeUsesObjectEqualsTest ()
		{
			TypeDefinition type = GetTest ("ImplementsGetHashCodeUsesObjectEquals");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void EqualsWithTwoArgsTest ()
		{
			TypeDefinition type = GetTest ("ImplementingEqualsWithTwoArgs");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void GetHashCodeWithOneArgTest ()
		{
			TypeDefinition type = GetTest ("ImplementingGetHashCodeWithOneArg");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
