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
		
		private ITypeRule typeRule;
		private AssemblyDefinition assembly;
		private Runner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			typeRule = new ImplementEqualsAndGetHashCodeInPairRule ();
			runner = new MinimalRunner ();
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
			MessageCollection messageCollection = typeRule.CheckType (type, runner);
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (messageCollection.Count, 1);
		}
		
		[Test]
		public void GetHashCodeButNotEqualsTest ()
		{
			TypeDefinition type = GetTest ("ImplementsGetHashCodeButNotEquals");
			MessageCollection messageCollection = typeRule.CheckType (type, runner);
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (messageCollection.Count, 1);
		}
		
		[Test]
		public void NoneOfThemTest ()
		{
			TypeDefinition type = GetTest ("ImplementsNoneOfThem");
			Assert.IsNull (typeRule.CheckType (type, runner));
		}
		
		[Test]
		public void BothOfThemTest ()
		{
			TypeDefinition type = GetTest ("ImplementsBothOfThem");
			Assert.IsNull (typeRule.CheckType (type, runner));
		}
		
		[Test]
		public void ImplementsEqualsUsesObjectGetHashCodeTest ()
		{
			TypeDefinition type = GetTest ("ImplementsEqualsUsesObjectGetHashCode");
			MessageCollection messageCollection = typeRule.CheckType (type, runner);
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (messageCollection.Count, 1);
		}
		
		[Test]
		public void ImplementsEqualsReuseBaseGetHashCodeTest ()
		{
			TypeDefinition type = GetTest ("ImplementsEqualsReuseBaseGetHashCode");
			Assert.IsNull (typeRule.CheckType (type, runner));
		}

		[Test]
		public void ImplementsGetHashCodeUsesObjectEqualsTest ()
		{
			TypeDefinition type = GetTest ("ImplementsGetHashCodeUsesObjectEquals");
			MessageCollection messageCollection = typeRule.CheckType (type, runner);
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (messageCollection.Count, 1);
		}
		
		[Test]
		public void EqualsWithTwoArgsTest ()
		{
			TypeDefinition type = GetTest ("ImplementingEqualsWithTwoArgs");
			Assert.IsNull (typeRule.CheckType (type, runner));
		}
		
		[Test]
		public void GetHashCodeWithOneArgTest ()
		{
			TypeDefinition type = GetTest ("ImplementingGetHashCodeWithOneArg");
			Assert.IsNull (typeRule.CheckType (type, runner));
		}
	}
}
