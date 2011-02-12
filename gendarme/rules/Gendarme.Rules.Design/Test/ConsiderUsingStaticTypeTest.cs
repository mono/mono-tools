//
// Unit tests for ConsiderUsingStaticTypeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2011 Novell, Inc (http://www.novell.com)
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
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Design {

	[TestFixture]
	public class ConsiderUsingStaticTypeTest : TypeRuleTestFixture<ConsiderUsingStaticTypeRule> {

		[Test]
		public void SkipOnNonClassesTest ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		static class StaticClass {

			static void Show ()
			{
				Console.WriteLine ("hello");
			}
		}

		[Test]
		public void SuccessOnStaticClassTest ()
		{
			TypeDefinition type = DefinitionLoader.GetTypeDefinition (typeof (ConsiderUsingStaticTypeTest).Assembly, "Test.Rules.Design.ConsiderUsingStaticTypeTest/StaticClass");
			AssertRuleSuccess (type);
		}

		class CouldBeStaticClass {

			static void Show ()
			{
				Console.WriteLine ("hello");
			}
		}

		[Test]
		public void FailOnCouldBeStaticClassTest ()
		{
			AssertRuleFailure<CouldBeStaticClass> (1);
		}

		public class EmptyClass {
			// this creates a public ctor
		}

		[Test]
		public void EmptyClassHasDefaultPublicInstanceCtor ()
		{
			AssertRuleFailure<EmptyClass> ();
		}

		//You cannot do this class static
		//This is the same testcase that EventArgs inheritance
		public class InheritAddingStatic : EmptyClass {
			static private int x;

			static public void Show ()
			{
				Console.WriteLine (x);
			}
		}

		public class InheritAddingInstance : EmptyClass {

			string Message = "Hello";

			public void Display ()
			{
				Console.WriteLine (Message);
			}
		}
		
		[Test]
		public void SkipOnInheritanceTest ()
		{
			AssertRuleDoesNotApply<InheritAddingStatic> ();
			AssertRuleDoesNotApply<InheritAddingInstance> ();
		}

		public class ClassWithOnlyFields {
			int x;
			char c;
		}

		[Test]
		public void SuccessOnClassWithOnlyFieldsTest ()
		{
			AssertRuleSuccess<ClassWithOnlyFields> ();
		}

		public class ClassWithOnlyMethods {
			public void MakeStuff ()
			{
			}
		}

		[Test]
		public void SuccessOnClassWithOnlyMethodsTest ()
		{
			AssertRuleSuccess<ClassWithOnlyMethods> ();
		}

		public class ClassWithNonDefaultConstructor {
			public ClassWithNonDefaultConstructor (int x)
			{
			}

			static void Show ()
			{
				Console.WriteLine ("hello");
			}
		}

		[Test]
		public void NonDefaultConstructor ()
		{
			AssertRuleSuccess<ClassWithNonDefaultConstructor> ();
		}
	}
}
