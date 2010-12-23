// 
// Unit tests for StaticConstructorsShouldBePrivateRule
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

using Gendarme.Rules.Security;
using Gendarme.Framework.Rocks;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Security {

	internal class NoStaticCtorDefinedClass {
	}

	internal class PrivateStaticCtorDefinedClass {
		static PrivateStaticCtorDefinedClass ()
		{
		}
	}

	internal class NonPrivateStaticCtorDefinedClass {
		// will be modified using Cecil
		static NonPrivateStaticCtorDefinedClass ()
		{
		}
	}

	[TestFixture]
	public class StaticConstructorsShouldBePrivateTest : TypeRuleTestFixture<StaticConstructorsShouldBePrivateRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
		}

		[Test]
		public void TestNoStaticCtorDefinedClass ()
		{
			AssertRuleSuccess<NoStaticCtorDefinedClass> ();
		}

		[Test]
		public void TestPrivateStaticCtorDefinedClass ()
		{
			AssertRuleSuccess<PrivateStaticCtorDefinedClass> ();
		}

		[Test]
		public void TestNonPrivateStaticCtorDefinedClass ()
		{
			TypeDefinition inspectedType = DefinitionLoader.GetTypeDefinition<NonPrivateStaticCtorDefinedClass> ();
			MethodDefinition static_ctor = null;
			foreach (MethodDefinition ctor in inspectedType.Methods) {
				if (ctor.IsConstructor && ctor.IsStatic) {
					static_ctor = ctor;
					break;
				}
			}

			static_ctor.IsPublic = true; // change it from private to public
			AssertRuleFailure (inspectedType, 1);
		}
	}
}
