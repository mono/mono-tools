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
		private Runner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new AvoidUnsealedUninheritedInternalClassesRule ();
			runner = new MinimalRunner ();
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
			MessageCollection messages = rule.CheckType (GetTest<Visible> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestUnsealedInner ()
		{
			MessageCollection messages = rule.CheckType (GetTest ("Outer/UnsealedInner"), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestSealedInner ()
		{
			MessageCollection messages = rule.CheckType (GetTest ("Outer/SealedInner"), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestAbstract ()
		{
			MessageCollection messages = rule.CheckType (GetTest<Abstract> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestConcrete ()
		{
			MessageCollection messages = rule.CheckType (GetTest<Concrete> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestSealed ()
		{
			MessageCollection messages = rule.CheckType (GetTest<Sealed> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestUnsealed ()
		{
			MessageCollection messages = rule.CheckType (GetTest<Unsealed> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}
	}
}
