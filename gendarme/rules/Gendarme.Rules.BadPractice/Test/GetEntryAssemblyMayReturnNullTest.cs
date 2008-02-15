// 
// Unit tests for GetEntryAssemblyMayReturnNullRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) Daniel Abramov
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;

namespace Test.Rules.BadPractice {

	internal class ClassCallingGetEntryAssembly {

		public static void Main () // fake main
		{
		}

		public void OneCall ()
		{
			object o = System.Reflection.Assembly.GetEntryAssembly ();
		}

		public void ThreeCalls ()
		{
			string s = System.Reflection.Assembly.GetEntryAssembly ().ToString ();
			int x = 2 + 2;
			x = x.CompareTo (1);
			object o = System.Reflection.Assembly.GetEntryAssembly ();
			System.Reflection.Assembly.GetEntryAssembly ();
		}

		public void NoCalls ()
		{
			int x = 42;
			int y = x * 42;
			x = x * y.CompareTo (42);
		}
	}

	[TestFixture]
	public class GetEntryAssemblyMayReturnNullTest {

		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new GetEntryAssemblyMayReturnNullRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.Types [typeof (T).FullName];
		}

		[Test]
		public void TestMethodNotCallingGetEntryAssembly ()
		{
			MethodDefinition method = GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("NoCalls", new Type [] { });
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestGetEntryAssemblyCallFromExecutable ()
		{
			try {
				assembly.EntryPoint = GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("Main", new Type [] { });
				assembly.Kind = AssemblyKind.Console;
				MethodDefinition method = GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("ThreeCalls", new Type [] { });
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult");
				Assert.AreEqual (0, runner.Defects.Count, "Count");
			}
			finally {
				assembly.EntryPoint = null;
				assembly.Kind = AssemblyKind.Dll;
			}
		}

		[Test]
		public void TestMethodCallingGetEntryAssemblyOnce ()
		{
			MethodDefinition method = GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("OneCall", new Type [] { });
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestMethodCallingGetEntryAssemblyThreeTimes ()
		{
			MethodDefinition method = GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("ThreeCalls", new Type [] { });
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (3, runner.Defects.Count, "Count");
		}
	}
}
