//
// Unit tests for EnsureSymmetryForOverloadedOperatorsRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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

using Gendarme.Framework;
using Gendarme.Rules.Design;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Design {

	[TestFixture]
	public class EnsureSymmetryForOverloadedOperatorsTest {

		private EnsureSymmetryForOverloadedOperatorsRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Design.EnsureSymmetryForOverloadedOperatorsTest"];
			rule = new EnsureSymmetryForOverloadedOperatorsRule ();
			runner = new TestRunner (rule);
		}

		public TypeDefinition GetTest (string name)
		{
			foreach (TypeDefinition nType in type.NestedTypes) {
				if (nType.Name == name)
					return nType;
			}
			return null;
		}

		public TypeDefinition CreateType (string name, string [] methods, int parameterCount)
		{
			TypeDefinition testType = new TypeDefinition (name, "", TypeAttributes.Class, type.BaseType);
			testType.Module = assembly.MainModule;
			TypeDefinition returnType = new TypeDefinition ("Boolean", "System", TypeAttributes.Class, type.BaseType);
			foreach (string method in methods) {
				MethodDefinition mDef = new MethodDefinition (method, MethodAttributes.Static | MethodAttributes.SpecialName, returnType);
				for (int i = 0; i < parameterCount; i++)
					mDef.Parameters.Add (new ParameterDefinition (testType));
				testType.Methods.Add (mDef);
			}
			return testType;
		}

		class FalsePositive {
			public void DoStuff () { }
		}

		[Test]
		public void TestFalsePositive ()
		{
			TypeDefinition type = GetTest ("FalsePositive");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		class EverythingOK {
			public static EverythingOK operator + (EverythingOK a, EverythingOK b) { return null; }
			public static EverythingOK operator - (EverythingOK a, EverythingOK b) { return null; }
			public static EverythingOK operator * (EverythingOK a, EverythingOK b) { return null; }
			public static EverythingOK operator / (EverythingOK a, EverythingOK b) { return null; }
			public static EverythingOK operator % (EverythingOK a, EverythingOK b) { return null; }
			public static bool operator > (EverythingOK a, EverythingOK b) { return false; }
			public static bool operator >= (EverythingOK a, EverythingOK b) { return false; }
			public static bool operator < (EverythingOK a, EverythingOK b) { return false; }
			public static bool operator <= (EverythingOK a, EverythingOK b) { return false; }
			public static bool operator != (EverythingOK a, EverythingOK b) { return false; }
			public static bool operator == (EverythingOK a, EverythingOK b) { return false; }

			public static bool operator true (EverythingOK a) { return false; }
			public static bool operator false (EverythingOK a) { return true; }
		}

		[Test]
		public void TestEverythingOK ()
		{
			TypeDefinition type = GetTest ("EverythingOK");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		class Missing1 {
			public static Missing1 operator + (Missing1 a, Missing1 b) { return null; }
			public static Missing1 operator * (Missing1 a, Missing1 b) { return null; }
		}

		[Test]
		public void TestMissing1 ()
		{
			TypeDefinition type = GetTest ("Missing1");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (2, runner.Defects.Count, "Count");
		}

		class Missing2 {
			public static Missing2 operator - (Missing2 a, Missing2 b) { return null; }
			public static Missing2 operator / (Missing2 a, Missing2 b) { return null; }
		}

		[Test]
		public void TestMissing2 ()
		{
			TypeDefinition type = GetTest ("Missing2");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (3, runner.Defects.Count, "Count");
			// divide fires for multiply and modulus
		}

		[Test]
		public void TestModulus ()
		{
			TypeDefinition type = this.CreateType ("Modulus", new string [] { "op_Modulus" }, 2);
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestGreater ()
		{
			TypeDefinition type = this.CreateType ("Greater", new string [] { "op_GreaterThan", "op_GreaterThanOrEqual" }, 2);
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (2, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestLess ()
		{
			TypeDefinition type = this.CreateType ("Less", new string [] { "op_LessThan", "op_LessThanOrEqual" }, 2);
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (2, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestEquality ()
		{
			TypeDefinition type = this.CreateType ("Equality", new string [] { "op_Equality" }, 2);
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestInequality ()
		{
			TypeDefinition type = this.CreateType ("Inequality", new string [] { "op_Inequality" }, 2);
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestTrue ()
		{
			TypeDefinition type = this.CreateType ("True", new string [] { "op_True" }, 1);
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestFalse ()
		{
			TypeDefinition type = this.CreateType ("False", new string [] { "op_False" }, 1);
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
	}
}
