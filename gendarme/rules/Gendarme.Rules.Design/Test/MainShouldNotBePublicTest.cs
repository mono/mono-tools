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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class MainShouldNotBePublicTest : AssemblyRuleTestFixture<MainShouldNotBePublicRule> {

		private AssemblyDefinition goodAssembly;
		private AssemblyDefinition anotherGoodAssembly;
		private AssemblyDefinition badAssembly;
		private AssemblyDefinition vbBadAssembly;
		private AssemblyDefinition noEntryPointAssembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
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

			// has a reference to Micrisoft.VisualBasic assembly (i.e. likely compiled VB.NET)
			vbBadAssembly = AssemblyFactory.DefineAssembly ("BadAssembly", AssemblyKind.Console);
			vbBadAssembly.MainModule.Types.Add (badMainClass.Clone ());
			vbBadAssembly.EntryPoint = badMain;
			vbBadAssembly.MainModule.AssemblyReferences.Add (new AssemblyNameReference ("Microsoft.VisualBasic", "neutral", new Version (1, 0, 0, 0)));

			// no entry point
			noEntryPointAssembly = AssemblyFactory.DefineAssembly ("NoEntryPointAssembly", AssemblyKind.Dll);
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess (goodAssembly);
			AssertRuleSuccess (anotherGoodAssembly);
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure (badAssembly, 1);
			AssertRuleFailure (vbBadAssembly, 1);
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (noEntryPointAssembly);
		}
	}
}
