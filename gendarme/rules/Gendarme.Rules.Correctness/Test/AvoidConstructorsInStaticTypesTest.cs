//
// Unit test for AvoidConstructorsInStaticTypesRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
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
using System.Reflection;

using Mono.Cecil;
using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class AvoidConstructorsInStaticTypesTest : TypeRuleTestFixture<AvoidConstructorsInStaticTypesRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		public class CannotBeMadeStatic_Method {

			public void Method()
			{
			}
		}

		public class CannotBeMadeStatic_Field {

			private int field = 42;
		}

		public class CannotBeMadeStatic_Ctor {

			CannotBeMadeStatic_Ctor (int a, int b)
			{
			}
		}

		[Test]
		public void TestClassCannotBeMadeStatic ()
		{
			AssertRuleSuccess<CannotBeMadeStatic_Method> ();
			AssertRuleSuccess<CannotBeMadeStatic_Field> ();
			AssertRuleSuccess<CannotBeMadeStatic_Ctor> ();
		}

		public class CouldBeStatic {

			public static void Method ()
			{
			}
		}

		[Test]
		public void TestClassCanBeMadeStatic ()
		{
			AssertRuleFailure<CouldBeStatic> (1);
		}

		public class IsMadeStatic {

			private IsMadeStatic ()
			{
			}

			public static void Method ()
			{
			}
		}

		[Test]
		public void TestClassHasNoPublicConstructors ()
		{
			AssertRuleSuccess<IsMadeStatic> ();
		}

		public class EmptyClass {
			// this creates a public ctor
		}

		public class InheritClass : EmptyClass {
			private int x;

			public void Show ()
			{
				Console.WriteLine (x);
			}
		}

		public class InheritAddingOnlyStatic : InheritClass {

			static string Message = "Hello";

			static public void Display ()
			{
				Console.WriteLine (Message);
			}
		}

		[Test]
		public void Inheritance ()
		{
			AssertRuleFailure<EmptyClass> (1); // default, visible, ctor
			AssertRuleSuccess<InheritClass> ();
			AssertRuleSuccess<InheritAddingOnlyStatic> ();
		}

		static class StaticClass {

			static void Show ()
			{
				Console.WriteLine ("hello");
			}
		}

		[Test]
		public void StaticType ()
		{
			// the nice generic-based test syntax won't work on static types :(
			Assembly a = typeof (AvoidConstructorsInStaticTypesTest).Assembly;
			TypeDefinition type = DefinitionLoader.GetTypeDefinition (a, "Test.Rules.Correctness.AvoidConstructorsInStaticTypesTest/StaticClass");
			AssertRuleDoesNotApply (type);
		}
	}
}
