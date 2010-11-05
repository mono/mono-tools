// 
// Unit tests for MarkAssemblyWithAssemblyVersionRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using System.Diagnostics.CodeAnalysis;
using System.IO;

using Mono.Cecil;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Design {

	[TestFixture]
	public class MarkAssemblyWithAssemblyVersionTest : AssemblyRuleTestFixture<MarkAssemblyWithAssemblyVersionRule> {

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			Runner.Engines.Subscribe ("Gendarme.Framework.Engines.SuppressMessageEngine");
		}

		static AssemblyDefinition CreateAssembly (string name, ModuleKind kind)
		{
			return AssemblyDefinition.CreateAssembly (
				new AssemblyNameDefinition (name, new Version (0, 0)),
				name, kind);
		}

		[Test]
		public void Good ()
		{
			AssemblyDefinition assembly = CreateAssembly ("GoodVersion", ModuleKind.Dll);
			assembly.Name.Version = new Version (1, 2, 3, 4);
			AssertRuleSuccess (assembly);
		}

		[Test]
		public void Bad ()
		{
			AssemblyDefinition assembly = CreateAssembly ("BadVersion", ModuleKind.Dll);
			assembly.Name.Version = new Version ();
			AssertRuleFailure (assembly, 1);
		}

		[Test]
		public void FxCop_ManuallySuppressed ()
		{
			AssemblyDefinition assembly = CreateAssembly ("SuppressedVersion", ModuleKind.Dll);
			TypeDefinition type = DefinitionLoader.GetTypeDefinition<SuppressMessageAttribute> ();

			MethodDefinition ctor = DefinitionLoader.GetMethodDefinition (type, ".ctor",
				new Type [] { typeof (string), typeof (string) });
			CustomAttribute ca = new CustomAttribute (assembly.MainModule.Import (ctor));
			ca.ConstructorArguments.Add (
				new CustomAttributeArgument (
					assembly.MainModule.TypeSystem.String, "Microsoft.Design"));
			ca.ConstructorArguments.Add (
				new CustomAttributeArgument (
					assembly.MainModule.TypeSystem.String, "CA1016:MarkAssembliesWithAssemblyVersion"));
			assembly.CustomAttributes.Add (ca);

			var stream = new MemoryStream ();
			assembly.Write (stream);

			stream.Position = 0;

			assembly = AssemblyDefinition.ReadAssembly (stream);

			AssertRuleDoesNotApply (assembly);
		}

		[Test]
		public void FxCop_GloballySuppressed ()
		{
			AssemblyDefinition assembly = DefinitionLoader.GetAssemblyDefinition (this.GetType ());
			// see GlobalSuppressions.cs
			AssertRuleDoesNotApply (assembly);
		}
	}
}
