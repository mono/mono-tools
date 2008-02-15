// 
// Unit tests for UsingCloneWithoutImplementingICloneableRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.Collections;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Design;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Design
{
	[TestFixture]
	public class UsingCloneWithoutImplementingICloneableTest {
		public class UsingCloneAndImplementingICloneable: ICloneable
		{
			public virtual object Clone ()
			{
				return this.MemberwiseClone ();
			}
		}
		
		public class UsingCloneWithoutImplementingICloneable 
		{
			public object Clone ()
			{
				return this.MemberwiseClone ();
			}
		}
		
		public class NeitherUsingCloneNorImplementingICloneable
		{
			public object clone ()
			{
				return this;
			}
		}
		
		public class AnotherExampleOfNotUsingBoth
		{
			public int Clone ()
			{
				return 1;
			}
		}
		
		public class OneMoreExample
		{
			public object Clone (int i)
			{
				return this.MemberwiseClone ();
			}
		}

		public class CloningType {
			public CloningType Clone ()
			{
				return new CloningType ();
			}
		}

		// ArrayList implements ICloneable but it located in another assembly (mscorlib)
		public class MyArrayList : ArrayList {

			public override object Clone ()
			{
				return new MyArrayList ();
			}
		}

		public class SecondLevelClone : UsingCloneAndImplementingICloneable {

			// CS0108 on purpose
			public object Clone ()
			{
				return new SecondLevelClone ();
			}
		}

		public class SecondLevelCloneWithOverride : UsingCloneAndImplementingICloneable {

			public override object Clone ()
			{
				return new SecondLevelCloneWithOverride ();
			}
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
			rule = new UsingCloneWithoutImplementingICloneableRule ();
			runner = new TestRunner (rule);
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Design.UsingCloneWithoutImplementingICloneableTest/" + name;
			return assembly.MainModule.Types[fullname];
		}
			
		[Test]
		public void usingCloneAndImplementingICloneableTest ()
		{
			type = GetTest ("UsingCloneAndImplementingICloneable");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void usingCloneWithoutImplementingICloneableTest ()
		{
			type = GetTest ("UsingCloneWithoutImplementingICloneable");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void neitherUsingCloneNorImplementingICloneableTest ()
		{
			type = GetTest ("NeitherUsingCloneNorImplementingICloneable");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void anotherExampleOfNotUsingBothTest ()
		{
			type = GetTest ("AnotherExampleOfNotUsingBoth");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void OneMoreExampleTest ()
		{
			type = GetTest ("OneMoreExample");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void CloneReturnTypeNotObject ()
		{
			type = GetTest ("CloningType");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void InheritFromTypeOutsideAssembly ()
		{
			type = GetTest ("MyArrayList");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void InheritFromTypeImplementingICloneableButWithoutOverride ()
		{
			type = GetTest ("SecondLevelClone");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void InheritFromTypeImplementingICloneable ()
		{
			type = GetTest ("SecondLevelCloneWithOverride");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
