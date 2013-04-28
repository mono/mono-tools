//
// Unit tests for AvoidUnusedPrivateFieldsRule
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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class AvoidUnusedPrivateFieldsTest : TypeRuleTestFixture<AvoidUnusedPrivateFieldsRule> {

		[Test]
		public void Simple ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);	// no fields
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleSuccess (SimpleTypes.Structure);	// a few public fields
		}

		class ClassUnusedStaticPrivateField {
			private static int x;

			[DllImport ("libc.so")]
			private static extern void strncpy (StringBuilder dest, string src, uint n);
		}

		[Test]
		public void Class ()
		{
			AssertRuleFailure<ClassUnusedStaticPrivateField> (1);
		}

		class ClassUnusedPrivateField {
			private int x;
			public int X {
				get { return 0; }
				set { ; }
			}
		}

		class ClassPrivateField {
			private int x;
			public int X {
				get { return x; }
				set { x = value; }
			}
		}

		[Test]
		public void StaticFields ()
		{
			AssertRuleFailure<ClassUnusedPrivateField> (1);
			AssertRuleSuccess<ClassPrivateField> ();
		}

		class ClassConstUnused {
			private const string DefaultRulesFile = "rules.xml";
			private const int PdbHiddenLine = 0xFEEFEE;
		}

		class ClassConstString {
			private const string DefaultRulesFile = "rules.xml";

			public void Show ()
			{
				Console.WriteLine (DefaultRulesFile);
			}
		}

		class ClassConstInt {
			private const int PdbHiddenLine = 0xFEEFEE;

			public void Show ()
			{
				Console.WriteLine (PdbHiddenLine);
			}
		}

		class ClassConstSmallInt {
			// small enough to be changed into Ldc_I4_S
			const int DefaultAmountOfElements = 13;

			public void Show ()
			{
				Console.WriteLine (DefaultAmountOfElements);
			}
		}

		class ClassConstSmallerInt {
			// small enough to be changed into Ldc_I4_7
			const int AssignationRatio = 7;

			public void Show ()
			{
				Console.WriteLine (AssignationRatio);
			}
		}

		[Test]
		public void ConstantFields ()
		{
			// constant (literals) are ignored since their value is copied
			// into IL (i.e. the field itself is not used)
			AssertRuleSuccess<ClassConstUnused> ();
			AssertRuleSuccess<ClassConstString> ();
			AssertRuleSuccess<ClassConstInt> ();
			AssertRuleSuccess<ClassConstSmallInt> ();
			AssertRuleSuccess<ClassConstSmallerInt> ();
		}

		class GenericUnused<T> {
			IList<T> list;
		}

		class GenericUsed<T> {
			IList<T> list;

			public void Show ()
			{
				foreach (T t in list)
					Console.WriteLine (t);
			}
		}

		[Test]
		public void GenericsFields ()
		{
			AssertRuleFailure<GenericUnused<int>> (1);
			AssertRuleSuccess<GenericUsed<int>> ();
		}

		class FieldsUsedInNested {
			private bool field;

			private static string staticField;

			class Nested {
				public void Foo (FieldsUsedInNested parent)
				{
					FieldsUsedInNested.staticField = "bar";
					parent.field = true;
				}
			}
		}

		[Test]
		public void FieldsUsedInNestedType ()
		{
			AssertRuleSuccess<FieldsUsedInNested> ();
		}
		
		class CompilerGenerated {
			public string Name { get; set; }
		}
		
		[Test]
		public void ClassWithCompilerGeneratedFields ()
		{
			AssertRuleSuccess<CompilerGenerated> ();
		}
		
		class CompilerGeneratedAndUnused {
			private int number;
			public string Name { get; set; }
		}
		
		[Test]
		public void ClassWithCompilerGeneratedFieldsAndUnusedPrivate ()
		{
			AssertRuleFailure<CompilerGeneratedAndUnused> (1);
		}
	}
}
