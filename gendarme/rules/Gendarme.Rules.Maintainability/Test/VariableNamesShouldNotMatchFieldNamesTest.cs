//
// Unit Tests for VariableNamesShouldNotMatchFieldNamesRule
//
// Authors:
//	N Lum <nol888@gmail.com>
//
// Copyright (C) 2010 N Lum
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Gendarme.Rules.Maintainability;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Mono.Cecil;
using Test.Rules.Helpers;

namespace Test.Rules.Maintainability {

	[TestFixture]
	public class VariableNamesShouldNotMatchFieldNamesTest : TypeRuleTestFixture<VariableNamesShouldNotMatchFieldNamesRule> {

		private class GoodClass {
			int Value;
			void DoSomething(int OtherValue)
			{
				int VeryOtherValue = 0;
				VeryOtherValue = OtherValue++;
			}
		}

		private class BadClass {
			int Value;
			void DoSomething(int Value)
			{
			}
		}

		// We ignore this, as we need .pdb access to resolve local variable names.
		private class BadIgnore {
			int Value = 0, Value1 = 1;
			void DoSomething(int Value)
			{
				int Value1;
				Value1 = Value + this.Value;
			}
		}

		private class DoesNotApplyClass1 {
			int Value;
		}

		private class DoesNotApplyClass2 {
			void DoSomething()
			{
			}
		}

		private TypeDefinition DoesNotApplyClassSpecial;

		[TestFixtureSetUp]
		public void SetUp()
		{
			// A type always has a method (implicit constructor) so we have to hack our own
			// methodless class.
			DoesNotApplyClassSpecial = DefinitionLoader.GetTypeDefinition<DoesNotApplyClass1> ();
			DoesNotApplyClassSpecial.Methods.Clear ();
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodClass> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadClass> (1);
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (DoesNotApplyClassSpecial);
			AssertRuleDoesNotApply<DoesNotApplyClass2> ();
		}

		[Test]
		public void Ignores ()
		{
			AssemblyDefinition assembly = DefinitionLoader.GetAssemblyDefinition<BadIgnore> ();
			int expected = assembly.MainModule.HasSymbols ? 2 : 1;
			AssertRuleFailure<BadIgnore> (expected);
		}
	}
}
