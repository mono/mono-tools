//
// Unit tests for CheckNewExceptionWithoutThrowingRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.BadPractice;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class CheckNewExceptionWithoutThrowingTest : MethodRuleTestFixture<CheckNewExceptionWithoutThrowingRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no NEWOBJ
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private CheckNewExceptionWithoutThrowingRule rule;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			type = assembly.MainModule.GetType ("Test.Rules.BadPractice.CheckNewExceptionWithoutThrowingTest");
			rule = new CheckNewExceptionWithoutThrowingRule ();
			runner = new TestRunner (rule);
		}

		public MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			Assert.Fail ("Method {0} not found!", name);
			return null;
		}

		public void DirectThrow ()
		{
			throw new Exception ();
		}

		[Test]
		public void TestDirectThrow ()
		{
			MethodDefinition method = GetTest ("DirectThrow");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public void SimpleError ()
		{
			new Exception ();
		}

		[Test]
		public void TestSimpleError ()
		{
			MethodDefinition method = GetTest ("SimpleError");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		public void LocalVariable ()
		{
			Exception a = new Exception ();
			int c = 0;
			for (int i = 0; i < 10; i++)
				c += i * 2;
			throw a;
		}

		[Test]
		public void TestLocalVariable ()
		{
			MethodDefinition method = GetTest ("LocalVariable");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public object Return ()
		{
			Exception a = new Exception ();
			object b = a;
			return b;
		}

		[Test]
		public void TestReturn ()
		{
			MethodDefinition method = GetTest ("Return");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public object ReturnOther ()
		{
			Exception a = new Exception ();
			object b = a;
			b = new object ();
			return b;
		}

		[Test]
		public void TestReturnOther ()
		{
			MethodDefinition method = GetTest ("ReturnOther");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		public object Branch ()
		{
			Exception a = new Exception ();
			if (new Random ().Next () == 0) {
				a = null;
			}
			return a;
		}

		[Test]
		public void TestBranch ()
		{
			MethodDefinition method = GetTest ("Branch");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public void TryCatch ()
		{
			Exception a = new Exception ();
			Exception b = null;
			Exception c = null;
			try {
				b = a;
			}
			catch (Exception) {
				c = b;
			}
			throw c;
		}

		[Test]
		public void TestTryCatch ()
		{
			MethodDefinition method = GetTest ("TryCatch");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public void TryCatch2 ()
		{
			Exception a = new Exception ();
			Exception b = null;
			try {
				throw new Exception ();
			}
			catch (InvalidOperationException) {
				b = a;
			}
			catch (InvalidCastException) {
				throw b;
			}
		}

		[Test]
		public void TestTryCatch2 ()
		{
			MethodDefinition method = GetTest ("TryCatch2");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}


		public void TryCatchFinally ()
		{
			Exception a = new Exception ();
			Exception b = null;
			try {
				throw new Exception ();
			}
			catch (Exception) {
				b = a;
			}
			finally {
				throw b;
			}
		}

		[Test]
		public void TestTryCatchFinally ()
		{
			MethodDefinition method = GetTest ("TryCatchFinally");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public void Out (out Exception ex)
		{
			ex = new Exception ();
		}

		[Test]
		public void TestOut ()
		{
			MethodDefinition method = GetTest ("Out");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public void Ref (ref Exception ex)
		{
			ex = new Exception ();
		}

		[Test]
		public void TestRef ()
		{
			MethodDefinition method = GetTest ("Ref");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}


		public void Call ()
		{
			Exception e = new Exception ();
			e.ToString ();
		}

		[Test]
		public void TestCall ()
		{
			MethodDefinition method = GetTest ("Call");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		public void Call2 ()
		{
			Exception e = new Exception ();
			Console.WriteLine (e);
		}

		[Test]
		public void TestCall2 ()
		{
			MethodDefinition method = GetTest ("Call2");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
