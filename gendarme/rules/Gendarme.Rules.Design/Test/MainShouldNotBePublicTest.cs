// 
// Unit tests for MainShouldNotBePublicRule
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
using Gendarme.Rules.Design;

using NUnit.Framework;

namespace Test.Rules.Design {

	[TestFixture]
	public class MainShouldNotBePublicTest {

		private IAssemblyRule rule;
		private Runner runner;

		private AssemblyDefinition goodAssembly;
		private AssemblyDefinition anotherGoodAssembly;
		private AssemblyDefinition badAssembly;
		private AssemblyDefinition noEntryPointAssembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			rule = new MainShouldNotBePublicRule ();
			runner = new MinimalRunner ();
			GenerateRequiredAssemblies ();
		}

		private void GenerateRequiredAssemblies ()
		{
			// public class, private method
			goodAssembly = AssemblyFactory.DefineAssembly ("GoodAssembly", AssemblyKind.Console);
			TypeDefinition goodMainClass = new TypeDefinition ("MainClass", "", TypeAttributes.Class | TypeAttributes.Public, goodAssembly.MainModule.TypeReferences ["System.Object"]);
			MethodDefinition goodMain = new MethodDefinition ("Main", MethodAttributes.Static | MethodAttributes.Private, goodAssembly.MainModule.TypeReferences ["System.Void"]);
			goodMainClass.Methods.Add (goodMain);
			goodAssembly.MainModule.Types.Add (goodMainClass);
			goodAssembly.EntryPoint = goodMain;

			// internal class, public method
			anotherGoodAssembly = AssemblyFactory.DefineAssembly ("AnotherGoodAssembly", AssemblyKind.Console);
			TypeDefinition anotherGoodMainClass = new TypeDefinition ("MainClass", "", TypeAttributes.Class | TypeAttributes.NotPublic, anotherGoodAssembly.MainModule.TypeReferences ["System.Object"]);
			MethodDefinition anotherGoodMain = new MethodDefinition ("Main", MethodAttributes.Static | MethodAttributes.Public, anotherGoodAssembly.MainModule.TypeReferences ["System.Void"]);
			anotherGoodMainClass.Methods.Add (anotherGoodMain);
			anotherGoodAssembly.MainModule.Types.Add (anotherGoodMainClass);
			anotherGoodAssembly.EntryPoint = anotherGoodMain;

			// public class, public method
			badAssembly = AssemblyFactory.DefineAssembly ("BadAssembly", AssemblyKind.Console);
			TypeDefinition badMainClass = new TypeDefinition ("MainClass", "", TypeAttributes.Class | TypeAttributes.Public, goodAssembly.MainModule.TypeReferences ["System.Object"]);
			MethodDefinition badMain = new MethodDefinition ("Main", MethodAttributes.Static | MethodAttributes.Public, goodAssembly.MainModule.TypeReferences ["System.Void"]);
			badMainClass.Methods.Add (badMain);
			badAssembly.MainModule.Types.Add (badMainClass);
			badAssembly.EntryPoint = badMain;

			// no entry point
			noEntryPointAssembly = AssemblyFactory.DefineAssembly ("NoEntryPointAssembly", AssemblyKind.Dll);
		}

		[Test]
		public void TestAnotherGoodAssembly ()
		{
			Assert.IsNull (rule.CheckAssembly (anotherGoodAssembly, runner));
		}

		[Test]
		public void TestGoodAssembly ()
		{
			Assert.IsNull (rule.CheckAssembly (goodAssembly, runner));
		}

		[Test]
		public void TestBadAssembly ()
		{
			MessageCollection messages = rule.CheckAssembly (badAssembly, runner);
			Assert.IsNotNull (messages);
			Assert.AreEqual (1, messages.Count);
		}

		[Test]
		public void TestNoEntryPointAssembly ()
		{
			Assert.IsNull (rule.CheckAssembly (noEntryPointAssembly, runner));
		}
	}
}
