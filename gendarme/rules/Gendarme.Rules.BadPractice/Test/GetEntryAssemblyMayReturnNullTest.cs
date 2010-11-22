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
using System.Linq;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

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
	public class GetEntryAssemblyMayReturnNullTest : MethodRuleTestFixture<GetEntryAssemblyMayReturnNullRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no NEWOBJ
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void TestMethodNotCallingGetEntryAssembly ()
		{
			AssertRuleSuccess<ClassCallingGetEntryAssembly> ("NoCalls");
		}

		private TypeDefinition GetTest<T> (AssemblyDefinition assembly)
		{
			return assembly.MainModule.GetType (typeof (T).FullName);
		}

		[Test]
		public void TestGetEntryAssemblyCallFromExecutable ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (unit);
			try {
				assembly.EntryPoint = GetTest<ClassCallingGetEntryAssembly> (assembly).Methods.FirstOrDefault (m => m.Name == "Main");
				assembly.MainModule.Kind = ModuleKind.Console;
				MethodDefinition method = GetTest<ClassCallingGetEntryAssembly> (assembly).Methods.FirstOrDefault (m => m.Name == "ThreeCalls");
				Assert.AreEqual (RuleResult.DoesNotApply, (Runner as TestRunner).CheckMethod (method), "RuleResult");
				Assert.AreEqual (0, Runner.Defects.Count, "Count");
			}
			finally {
				assembly.EntryPoint = null;
				assembly.MainModule.Kind = ModuleKind.Dll;
			}
		}

		[Test]
		public void TestMethodCallingGetEntryAssemblyOnce ()
		{
			AssertRuleFailure<ClassCallingGetEntryAssembly> ("OneCall", 1);
		}

		[Test]
		public void TestMethodCallingGetEntryAssemblyThreeTimes ()
		{
			AssertRuleFailure<ClassCallingGetEntryAssembly> ("ThreeCalls", 3);
		}
	}
}
