// 
// Unit tests for FinalizersShouldBeProtectedRule
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

using Mono.Cecil;

using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	internal class NoFinalizerClass {
	}

	internal class ProtectedFinalizerClass {
		~ProtectedFinalizerClass () { }
	}

	[TestFixture]
	public class FinalizersShouldBeProtectedTest : TypeRuleTestFixture<FinalizersShouldBeProtectedRule> {

		private TypeDefinition non_protected_finalizer_class;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (unit);

			non_protected_finalizer_class = new TypeDefinition (
				"Test.Rules.Design",
				"PublicFinalizerClass",
				TypeAttributes.Public,
				assembly.MainModule.TypeSystem.Object);

			assembly.MainModule.Types.Add (non_protected_finalizer_class);

			var finalizer = new MethodDefinition (
				"Finalize",
				MethodAttributes.Public | MethodAttributes.Virtual,
				assembly.MainModule.TypeSystem.Void);

			non_protected_finalizer_class.Methods.Add (finalizer);
		}

		[Test]
		public void TestNoFinalizerClass ()
		{
			AssertRuleDoesNotApply<NoFinalizerClass>();
		}

		[Test]
		public void TestProtectedFinalizerClass ()
		{
			AssertRuleSuccess<ProtectedFinalizerClass> ();
		}

		[Test]
		public void TestNonProtectedFinalizerDefinedClass ()
		{
			AssertRuleFailure (non_protected_finalizer_class, 1);
		}
	}
}
