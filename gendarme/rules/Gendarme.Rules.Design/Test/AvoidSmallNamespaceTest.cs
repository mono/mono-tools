//
// Unit tests for AvoidSmallNamespaceRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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

using Mono.Cecil;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class AvoidSmallNamespaceTest : AssemblyRuleTestFixture<AvoidSmallNamespaceRule> {

		AssemblyDefinition assembly;

		private MethodDefinition Add (string namespaceName, string typeName, string methodName)
		{
			TypeDefinition type = new TypeDefinition (namespaceName, typeName, TypeAttributes.Class | TypeAttributes.Public, assembly.MainModule.TypeSystem.Object);
			MethodDefinition method = new MethodDefinition (methodName, MethodAttributes.Static | MethodAttributes.Private, assembly.MainModule.TypeSystem.Void);
			type.Methods.Add (method);
			assembly.MainModule.Types.Add (type);
			return method;
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			assembly = AssemblyDefinition.CreateAssembly (
				new AssemblyNameDefinition ("Assembly", new Version (1, 0)),
				"Module",
				ModuleKind.Console);
		}

		[SetUp]
		public void SetUp ()
		{
			// mess up logic so engines will re-run each time on the same assembly instance
			// (normally this can't happen as AssemblyDefinition are immutable as far as Gendarme is concerned)
			Runner.Assemblies.Clear ();
			Rule.Initialize (Runner);
			assembly.MainModule.Types.Clear ();
		}

		[Test]
		public void SingleNamespace ()
		{
			Add ("Namespace", "Type", "Method");
			AssertRuleSuccess (assembly);
		}

		[Test]
		public void GlobalNamespace ()
		{
			Add ("Namespace", "Type", "Method");
			Add (String.Empty, "Type", "Method");
			// we don't ignore the global namespace if it contains visible types
			AssertRuleFailure (assembly, 2);
		}

		[Test]
		public void SpecializationNamespaces ()
		{
			Add ("Namespace", "Type", "Method");
			Add ("Namespace.Design", "Type", "Method");
			Add ("Namespace.Interop", "Type", "Method");
			Add ("Namespace.Permissions", "Type", "Method");
			// Namespace is too small, but others won't be reported
			AssertRuleFailure (assembly, 1);
		}

		[Test]
		public void MultipleNamespacesNotEnoughTypes ()
		{
			Add ("Namespace", "Type", "Method");
			Add ("Namespace.Second", "Uho", "Failure");
			AssertRuleFailure (assembly, 2);
		}

		[Test]
		public void FakeEntryPoint ()
		{
			try {
				assembly.EntryPoint = Add ("Main", "Main", "Main");
				AssertRuleSuccess (assembly);
			}
			finally {
				assembly.EntryPoint = null;
			}
		}

		[Test]
		public void Zero ()
		{
			int minimum = Rule.Minimum;
			try {
				Rule.Minimum = 0;
				AssertRuleSuccess (assembly);
			}
			finally {
				Rule.Minimum = minimum;
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void Minimum ()
		{
			Rule.Minimum = Int32.MinValue;
		}
	}
}
