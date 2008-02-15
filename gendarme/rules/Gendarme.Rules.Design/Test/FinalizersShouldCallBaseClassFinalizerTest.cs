// 
// Unit tests for FinalizersShouldBeProtectedRule
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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Design;

using NUnit.Framework;

namespace Test.Rules.Design {

	internal class NoFinalizerClass {
	}

	internal class ProtectedFinalizerClass {
		~ProtectedFinalizerClass () { }
	}

	[TestFixture]
	public class FinalizersShouldBeProtectedTest {

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new FinalizersShouldBeProtectedRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.Types [typeof (T).FullName];
		}

		[Test]
		public void TestNoFinalizerClass ()
		{
			TypeDefinition type = GetTest<NoFinalizerClass> ();
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestProtectedFinalizerClass ()
		{
			TypeDefinition type = GetTest<ProtectedFinalizerClass> ();
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNonProtectedFinalizerDefinedClass ()
		{
			TypeDefinition typeDefinition = GetTest<FinalizerCallingBaseFinalizerClass> ().Clone () as TypeDefinition;
			MethodDefinition finalizer = typeDefinition.GetMethod (MethodSignatures.Finalize);
			// make it non-protected (e.g. public)
			finalizer.IsPublic = true;

			Assert.AreEqual (RuleResult.Failure, runner.CheckType (typeDefinition), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
	}
}
