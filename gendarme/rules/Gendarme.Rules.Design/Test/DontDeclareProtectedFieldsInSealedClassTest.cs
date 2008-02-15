// 
// Unit tests for DoNotDeclareProtectedFieldsInSealedTypeRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using Gendarme.Rules.Design;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Design
{
	[TestFixture]
	public class DoNotDeclareProtectedFieldsInSealedTypeTest {
		
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
		
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;
		
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new DoNotDeclareProtectedFieldsInSealedTypeRule ();
			runner = new TestRunner (rule);
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
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (2, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void unInheritableClassWithoutProtectedFieldsTest ()
		{
			type = GetTest ("UnInheritableClassWithoutProtectedFields");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void inheritableClassWithProtectedFieldsTest ()
		{
			type = GetTest ("InheritableClassWithProtectedFields");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
