// 
// Unit tests for DontDeclareProtectedFieldsInSealedClassRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//
// Copyright (c) <2007> Nidhi Rawal
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
using Gendarme.Rules.Design;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Design
{
	[TestFixture]
	public class DontDeclareProtectedFieldsInSealedClassTest {
		
		public sealed class UnInheritableClassWithProtectedField
		{
			protected int i;
			protected double d;
		}
		
		public sealed class UnInheritableClassWithoutProtectedFields
		{
			public string s;
			private float f;
		}
		
		public class InheritableClassWithProtectedFields
		{
			protected double d;
			protected int j;
		}
		
		private ITypeRule typeRule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		MessageCollection messageCollection;
		
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			typeRule = new DontDeclareProtectedFieldsInSealedClassRule();
			messageCollection = null;
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Design.DontDeclareProtectedFieldsInSealedClassTest/" + name;
			return assembly.MainModule.Types[fullname];
		}
		
		[Test]
		public void unInheritableClassWithProtectedFieldTest ()
		{
			type = GetTest ("UnInheritableClassWithProtectedField");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (2, messageCollection.Count);
		}
		
		[Test]
		public void unInheritableClassWithoutProtectedFieldsTest ()
		{
			type = GetTest ("UnInheritableClassWithoutProtectedFields");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void inheritableClassWithProtectedFieldsTest ()
		{
			type = GetTest ("InheritableClassWithProtectedFields");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
	}
}