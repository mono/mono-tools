//
// Unit Test for AvoidLargeClasses Rule.
//
// Authors:
//      Néstor Salceda <nestor.salceda@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//      (C) 2007 Néstor Salceda
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
using NUnit.Framework;
using Gendarme.Framework;
using Gendarme.Rules.Smells;
using Test.Rules.Helpers;

namespace Test.Rules.Smells {

	public enum LargeEnum {
		X, X1, X2, X3, X4, X5,
		X6, X7, X8, X9,
		Y, Y1, Y2, Y3, Y4, Y5,
		Y6, Y7, Y8, Y9, 
		Z, Z1, Z2, Z3, Z4, Z5,
		Z6, Z7, Z8, Z9, 
		W, W1, W2, W3, W4, W5,
		W6, W7, W8, W9
	}
	
	public class LargeClass {
		int x, x1, x2, x3;
		string foo, foo1, foo2, foo3;
		DateTime bar, bar1, bar2, bar3;
		float f, f1, f2, f3;
		char c, c1, c2, c3;
		short s, s1, s2, s3;
		string[] array;
	}

	public class ConstantClass {
		const int x = 0, x1 = 1, x2 = 2, x3 = 3;
		static readonly string foo, foo1, foo2, foo3;
		static readonly DateTime bar, bar1, bar2, bar3;
		float one, two, three, four;
		const char c = 'c', c1 = 'b', c2 = 'a', c3 = 'z';
		const short s = 2, s1 = 4, s2 = 6, s3 = 8;
		static readonly string[] array;
	}

	public class ClassWithPrefixedFieldsWithCamelCasing {
		int fooBar;
		int fooBaz;
	}

	public class ClassWithoutPrefixedFieldsWithMDashCasing {
		int m_member;
		int m_other;
	}

	public class ClassWithPrefixedFieldsWithMDashCasing {
		int m_foo_bar;
		int m_foo_baz;
	}

	public class ClassWithPrefixedFieldsWithDashCasing {
		int phone_number;
		int phone_area_code;
	}

	public class ClassWithSomeConstantsFields {
		public const string DefaultPath = "path";
		public const string DefaultConfig = "config";
	}

	public class ClassWithSomeReadOnlyFields {
		public readonly int DefaultInteger = 1;
		public readonly double DefaultDouble = 1.0d;
	}

	public class NoFieldClass {
	}

	public class NotLargeClass {
		int a;
	}

	class AutoImplementedPropertiesClass {
		public double DoubleProperty { get; set; }
		public string StringProperty { get; set; }
		public int Int32ReadOnlyProperty { get; private set; }
	}

	[TestFixture]
	public class AvoidLargeClassesTest {
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			rule = new AvoidLargeClassesRule ();
			runner = new TestRunner (rule);
		}

		[Test]
		public void LargeClassTest () 
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.LargeClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "Test.Rules.Smells.LargeClass failure test");
			// 'x', 'foo', 'bar', 'f', 'c' and 's' are reported as prefixes
			Assert.AreEqual (6, runner.Defects.Count, "Test.Rules.Smells.LargeClass defect count check");
		}

		[Test]
		public void NotLargeClassTest () 
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.NoFieldClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "Test.Rules.Smells.NoFieldClass does not apply test");

			type = assembly.MainModule.GetType ("Test.Rules.Smells.NotLargeClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "Test.Rules.Smells.NotLargeClass success test");
		}

		
		[Test]
		public void ConstantClassTest () 
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.ConstantClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void ClassWithPrefixedFieldsWithCamelCasingTest ()
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.ClassWithPrefixedFieldsWithCamelCasing");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "Test.Rules.Smells.ClassWithPrefixedFieldsWithCamelCasing failure test");
			Assert.AreEqual (1, runner.Defects.Count, "Test.Rules.Smells.ClassWithPrefixedFieldsWithCamelCasing defect count check");
		}
			
		[Test]
		public void ClassWithoutPrefixedFieldsWithMDashCasingTest () 
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.ClassWithoutPrefixedFieldsWithMDashCasing");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void ClassWithPrefixedFieldsWithDashCasingTest () 
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.ClassWithPrefixedFieldsWithDashCasing");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "Test.Rules.Smells.ClassWithPrefixedFieldsWithDashCasing failure test");
			Assert.AreEqual (1, runner.Defects.Count, "Test.Rules.Smells.ClassWithPrefixedFieldsWithDashCasing defect count check");
		}

		[Test]
		public void ClassWithPrefixedFieldsWithMDashCasingTest () 
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.ClassWithPrefixedFieldsWithMDashCasing");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "Test.Rules.Smells.ClassWithPrefixedFieldsWithMDashCasing failure test");
			Assert.AreEqual (1, runner.Defects.Count, "Test.Rules.Smells.ClassWithPrefixedFieldsWithMDashCasing defect count check");
		}

		[Test]
		public void ClassWithSomeConstantsFields ()
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.ClassWithSomeConstantsFields");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void ClassWithSomeReadOnlyFields ()
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.ClassWithSomeReadOnlyFields");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void ClassWithAutoImplementedProperties ()
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.AutoImplementedPropertiesClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void EnumsShouldNotBeCheckedTest ()
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.LargeEnum");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}
	}
}
