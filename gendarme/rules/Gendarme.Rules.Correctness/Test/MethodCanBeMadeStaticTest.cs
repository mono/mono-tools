//
// Unit tests for MethodCanBeMadeStatic rule
//
// Authors:
//	Jb Evain <jbevain@gmail.com>
//
// Copyright (C) 2007 Jb Evain
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
using Gendarme.Rules.Correctness;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class MethodCanBeMadeStaticTest {

		public class Item {

			public int Foo ()
			{
				return 42;
			}

			public int _bar;

			public int Bar ()
			{
				return _bar = 42;
			}

			public static int Baz ()
			{
				return 42;
			}

			public virtual void Gazonk ()
			{
			}

			public void OnItemBang (object sender, EventArgs ea)
			{
			}
		}

		private IMethodRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;
		private TypeDefinition type;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Correctness.MethodCanBeMadeStaticTest/Item"];
			rule = new MethodCanBeMadeStaticRule ();
			runner = new TestRunner (rule);
		}

		MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == name)
					return method;

			return null;
		}

		[Test]
		public void TestGoodCandidate ()
		{
			MethodDefinition method = GetTest ("Foo");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNotGoodCandidate ()
		{
			MethodDefinition method = GetTest ("Bar");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (0, runner.Defects.Count, "Count1");
			method = GetTest ("Baz");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (0, runner.Defects.Count, "Count2");
			method = GetTest ("Gazonk");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult3");
			Assert.AreEqual (0, runner.Defects.Count, "Count3");
			method = GetTest ("OnItemBang");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult4");
			Assert.AreEqual (0, runner.Defects.Count, "Count4");
		}
	}
}
