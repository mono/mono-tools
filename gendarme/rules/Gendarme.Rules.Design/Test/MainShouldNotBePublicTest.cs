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

		static AssemblyDefinition CreateAssembly (string name, ModuleKind kind)
		{
			return AssemblyDefinition.CreateAssembly (
				new AssemblyNameDefinition (name, new Version (0, 0)), name, ModuleKind.Console);
		}

		static AssemblyDefinition CreateTestAssembly (string name, TypeAttributes classAttributes, MethodAttributes mainAttributes)
		{
			var assembly = CreateAssembly (name, ModuleKind.Console);

			var testClass = new TypeDefinition ("", "MainClass", classAttributes, assembly.MainModule.TypeSystem.Object);
			assembly.MainModule.Types.Add (testClass);

			var mainMethod = new MethodDefinition ("Main", mainAttributes, assembly.MainModule.TypeSystem.Void);
			testClass.Methods.Add (mainMethod);

			assembly.EntryPoint = mainMethod;

			return assembly;
		}

		private void GenerateRequiredAssemblies ()
		{
			// public class, private method
			goodAssembly = CreateTestAssembly (
				"GoodAssembly",
				TypeAttributes.Class | TypeAttributes.Public,
				MethodAttributes.Static | MethodAttributes.Private);

			// internal class, public method
			anotherGoodAssembly = CreateTestAssembly (
				"AnotherGoodAssembly",
				TypeAttributes.Class | TypeAttributes.NotPublic,
				MethodAttributes.Static | MethodAttributes.Public);

			// public class, public method
			badAssembly = CreateTestAssembly (
				"BadAssembly",
				TypeAttributes.Class | TypeAttributes.Public,
				MethodAttributes.Static | MethodAttributes.Public);

			// has a reference to Micrisoft.VisualBasic assembly (i.e. likely compiled VB.NET)
			vbBadAssembly = CreateTestAssembly (
				"BadAssembly",
				TypeAttributes.Class | TypeAttributes.Public,
				MethodAttributes.Static | MethodAttributes.Public);
			vbBadAssembly.MainModule.AssemblyReferences.Add (new AssemblyNameReference ("Microsoft.VisualBasic", new Version (1, 0, 0, 0)));

			// no entry point
			noEntryPointAssembly = CreateAssembly ("NoEntryPointAssembly", ModuleKind.Dll);
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
