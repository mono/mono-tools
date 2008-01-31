// 
// Unit tests for AvoidUnsealedConcreteAttributesRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) Daniel Abramov
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

	internal class NotAttribute {
	}

	internal class AnAttribute : Attribute {
	}

	internal sealed class SealedAttributeInheritsAnAttribute : AnAttribute {
	}

	internal sealed class SealedAttribute : Attribute {
	}

	internal abstract class AbstractAttribute : Attribute {
	}

	[TestFixture]
	public class AvoidUnsealedConcreteAttributesTest {

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private Runner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new AvoidUnsealedConcreteAttributesRule ();
			runner = new MinimalRunner ();
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.Types [typeof (T).FullName];
		}

		[Test]
		public void TestAbstractAttribute ()
		{
			MessageCollection messages = rule.CheckType (GetTest<AbstractAttribute> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestAnAttribute ()
		{
			MessageCollection messages = rule.CheckType (GetTest<AnAttribute> (), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestNotAttribute ()
		{
			MessageCollection messages = rule.CheckType (GetTest<NotAttribute> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestSealedAttribute ()
		{
			MessageCollection messages = rule.CheckType (GetTest<SealedAttribute> (), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestSealedAttributeInheritsAnAttribute ()
		{
			MessageCollection messages = rule.CheckType (GetTest<SealedAttributeInheritsAnAttribute> (), runner);
			Assert.IsNull (messages);
		}
	}
}
