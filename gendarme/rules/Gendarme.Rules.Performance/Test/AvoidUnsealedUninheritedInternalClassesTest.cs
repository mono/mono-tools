// 
// Unit tests for AvoidUnsealedUninheritedInternalClassesRule
//
// Authors:
//	Scott Peterson <lunchtimemama@gmail.com>
//
// Copyright (C) 2008 Scott Peterson
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Performance;

using NUnit.Framework;

namespace Test.Rules.Performance {

	public class Visible {
	}

	public class Outer {
		internal class UnsealedInner {
		}

		internal sealed class SealedInner {
		}
	}

	internal abstract class Abstract {
	}

	internal class Concrete : Abstract {
	}

	internal sealed class Sealed {
	}

	internal class Unsealed {
	}

	[TestFixture]
	public class AvoidUnsealedUninheritedInternalClassesTest {

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new AvoidUnsealedUninheritedInternalClassesRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.Types [typeof (T).FullName];
		}

		// note: the generic version doesn't work with inner types since Cecil/IL and 
		// reflection do not use the same naming convention
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Performance." + name;
			return assembly.MainModule.Types [fullname];
		}

		[Test]
		public void TestVisable ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckType (GetTest<Visible> ()));
		}

		[Test]
		public void TestUnsealedInner ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (GetTest ("Outer/UnsealedInner")));
			Assert.AreEqual (1, runner.Defects.Count);
		}

		[Test]
		public void TestSealedInner ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckType (GetTest ("Outer/SealedInner")));
		}

		[Test]
		public void TestAbstract ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckType (GetTest<Abstract> ()));
		}

		[Test]
		public void TestConcrete ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (GetTest<Concrete> ()));
			Assert.AreEqual (1, runner.Defects.Count);
		}

		[Test]
		public void TestSealed ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckType (GetTest<Sealed> ()));
		}

		[Test]
		public void TestUnsealed ()
		{
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (GetTest<Unsealed> ()));
			Assert.AreEqual (1, runner.Defects.Count);
		}
	}
}
