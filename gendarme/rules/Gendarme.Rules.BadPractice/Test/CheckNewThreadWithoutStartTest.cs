//
// Unit tests for CheckNewThreadWithoutStartRule
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
using System.Threading;

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
	public class CheckNewThreadWithoutStartTest : MethodRuleTestFixture<CheckNewThreadWithoutStartRule> {

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
		private CheckNewThreadWithoutStartRule rule;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			type = assembly.MainModule.GetType ("Test.Rules.BadPractice.CheckNewThreadWithoutStartTest");
			rule = new CheckNewThreadWithoutStartRule ();
			runner = new TestRunner (rule);
		}

		public MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}

		private void ThreadStart () { }

		public void DirectStart ()
		{
			new Thread (ThreadStart).Start ();
		}

		[Test]
		public void TestDirectThrow ()
		{
			MethodDefinition method = GetTest ("DirectStart");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public void SimpleError ()
		{
			new Thread (ThreadStart);
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
			Thread a = new Thread (ThreadStart);
			int c = 0;
			for (int i = 0; i < 10; i++)
				c += i * 2;
			a.Start ();
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
			Thread a = new Thread (ThreadStart);
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
			Thread a = new Thread (ThreadStart);
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
			Thread a = new Thread (ThreadStart);
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

		public Thread TryCatch ()
		{
			Thread a = new Thread (ThreadStart);
			Thread b = null;
			Thread c = null;
			try {
				b = a;
			}
			catch (Exception) {
				c = b;
			}
			return c;
		}

		[Test]
		public void TestTryCatch ()
		{
			MethodDefinition method = GetTest ("TryCatch");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public Thread TryCatch2 ()
		{
			Thread a = new Thread (ThreadStart);
			Thread b = null;
			try {
				throw new Exception ();
			}
			catch (InvalidOperationException) {
				b = a;
			}
			catch (InvalidCastException) {
				return b;
			}
			return null;
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
			Thread a = new Thread (ThreadStart);
			Thread b = null;
			try {
				throw new Exception ();
			}
			catch (Exception) {
				b = a;
			}
			finally {
				b.Start ();
			}
		}

		[Test]
		public void TestTryCatchFinally ()
		{
			MethodDefinition method = GetTest ("TryCatchFinally");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public void Out (out Thread thread)
		{
			thread = new Thread (ThreadStart);
		}

		[Test]
		public void TestOut ()
		{
			MethodDefinition method = GetTest ("Out");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		public void Ref (ref Thread thread)
		{
			thread = new Thread (ThreadStart);
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
			Thread a = new Thread (ThreadStart);
			a.ToString ();
			a.Name = "Thread";
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
			Thread a = new Thread (ThreadStart);
			Console.WriteLine (a);
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
