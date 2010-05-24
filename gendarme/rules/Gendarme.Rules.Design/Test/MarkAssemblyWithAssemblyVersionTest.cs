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

		[Test]
		public void Good ()
		{
			AssemblyDefinition assembly = AssemblyFactory.DefineAssembly ("GoodVersion", AssemblyKind.Dll);
			assembly.Name.Version = new Version (1, 2, 3, 4);
			AssertRuleSuccess (assembly);
		}

		[Test]
		public void Bad ()
		{
			AssemblyDefinition assembly = AssemblyFactory.DefineAssembly ("BadVersion", AssemblyKind.Dll);
			assembly.Name.Version = new Version ();
			AssertRuleFailure (assembly, 1);
		}

		[Test]
		public void FxCop_ManuallySuppressed ()
		{
			AssemblyDefinition assembly = AssemblyFactory.DefineAssembly ("SuppressedVersion", AssemblyKind.Dll);
			TypeDefinition type = DefinitionLoader.GetTypeDefinition<SuppressMessageAttribute> ();
			assembly.MainModule.TypeReferences.Add (type.Clone ());

			MethodDefinition ctor = DefinitionLoader.GetMethodDefinition (type, ".ctor",
				new Type [] { typeof (string), typeof (string) });
			CustomAttribute ca = new CustomAttribute (ctor);
			ca.ConstructorParameters.Add ("Microsoft.Design");
			ca.ConstructorParameters.Add ("CA1016:MarkAssembliesWithAssemblyVersion");
			assembly.CustomAttributes.Add (ca);

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
