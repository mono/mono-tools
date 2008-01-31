// 
// Unit tests for GetEntryAssemblyMayReturnNullRule
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
		private Runner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new GetEntryAssemblyMayReturnNullRule ();
			runner = new MinimalRunner ();
		}

		private TypeDefinition GetTest<T> ()
		{
			return assembly.MainModule.Types [typeof (T).FullName];
		}

		[Test]
		public void TestMethodNotCallingGetEntryAssembly ()
		{
			MessageCollection messages = rule.CheckMethod (GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("NoCalls", new Type [] { }), runner);
			Assert.IsNull (messages);
		}

		[Test]
		public void TestGetEntryAssemblyCallFromExecutable ()
		{
			assembly.EntryPoint = GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("Main", new Type [] { });
			assembly.Kind = AssemblyKind.Console;
			MessageCollection messages = rule.CheckMethod (GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("ThreeCalls", new Type [] { }), runner);
			Assert.IsNull (messages);
			assembly.EntryPoint = null;
			assembly.Kind = AssemblyKind.Dll;
		}

		[Test]
		public void TestMethodCallingGetEntryAssemblyOnce ()
		{
			MessageCollection messages = rule.CheckMethod (GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("OneCall", new Type [] { }), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestMethodCallingGetEntryAssemblyThreeTimes ()
		{
			MessageCollection messages = rule.CheckMethod (GetTest<ClassCallingGetEntryAssembly> ().Methods.GetMethod ("ThreeCalls", new Type [] { }), runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (3, messages.Count);
		}
	}
}
