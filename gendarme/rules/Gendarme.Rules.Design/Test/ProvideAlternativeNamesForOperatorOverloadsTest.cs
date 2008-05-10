//
// Unit tests for ProvideAlternativeNamesForOperatorOverloadsRule
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
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Design;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Design {

	[TestFixture]
	public class ProvideAlternativeNamesForOperatorOverloadsTest {

		private ProvideAlternativeNamesForOperatorOverloadsRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new ProvideAlternativeNamesForOperatorOverloadsRule ();
			runner = new TestRunner (rule);
		}

		public TypeDefinition GetTest (string name)
		{
			foreach (TypeDefinition type in assembly.MainModule.Types ["Test.Rules.Design.ProvideAlternativeNamesForOperatorOverloadsTest"].NestedTypes) {
				if (type.Name == name)
					return type;
			}
			return null;
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

		class EverythingIsThere {

			public static EverythingIsThere operator + (EverythingIsThere a) { return null; }
			public static EverythingIsThere operator - (EverythingIsThere a) { return null; }
			public static EverythingIsThere operator ! (EverythingIsThere a) { return null; }
			public static EverythingIsThere operator ~ (EverythingIsThere a) { return null; }

			public static EverythingIsThere operator ++ (EverythingIsThere a) { return null; }
			public static EverythingIsThere operator -- (EverythingIsThere a) { return null; }
			public static bool operator true (EverythingIsThere a) { return false; }
			public static bool operator false (EverythingIsThere a) { return true; }

			public static EverythingIsThere operator + (EverythingIsThere a, EverythingIsThere b) { return null; }
			public static EverythingIsThere operator - (EverythingIsThere a, EverythingIsThere b) { return null; }
			public static EverythingIsThere operator * (EverythingIsThere a, EverythingIsThere b) { return null; }
			public static EverythingIsThere operator / (EverythingIsThere a, EverythingIsThere b) { return null; }
			public static EverythingIsThere operator % (EverythingIsThere a, EverythingIsThere b) { return null; }

			public static EverythingIsThere operator & (EverythingIsThere a, EverythingIsThere b) { return null; }
			public static EverythingIsThere operator | (EverythingIsThere a, EverythingIsThere b) { return null; }
			public static EverythingIsThere operator ^ (EverythingIsThere a, EverythingIsThere b) { return null; }

			public static EverythingIsThere operator << (EverythingIsThere a, int b) { return null; }
			public static EverythingIsThere operator >> (EverythingIsThere a, int b) { return null; }

			public static bool operator > (EverythingIsThere a, EverythingIsThere b) { return false; }
			public static bool operator >= (EverythingIsThere a, EverythingIsThere b) { return false; }
			public static bool operator < (EverythingIsThere a, EverythingIsThere b) { return false; }
			public static bool operator <= (EverythingIsThere a, EverythingIsThere b) { return false; }
			public static bool operator != (EverythingIsThere a, EverythingIsThere b) { return false; }
			public static bool operator == (EverythingIsThere a, EverythingIsThere b) { return false; } //for !=

			public EverythingIsThere Plus () { return null; }
			public EverythingIsThere Negate () { return null; }
			public EverythingIsThere LogicalNot () { return null; }
			public EverythingIsThere OnesComplement () { return null; }

			public EverythingIsThere Increment () { return null; }
			public EverythingIsThere Decrement () { return null; }
			public EverythingIsThere IsTrue () { return null; }
			public EverythingIsThere IsFalse () { return null; }

			public EverythingIsThere Add (EverythingIsThere other) { return null; }
			public EverythingIsThere Subtract (EverythingIsThere other) { return null; }
			public EverythingIsThere Multiply (EverythingIsThere other) { return null; }
			public EverythingIsThere Divide (EverythingIsThere other) { return null; }
			public EverythingIsThere Modulus (EverythingIsThere other) { return null; }

			public EverythingIsThere BitwiseAnd (EverythingIsThere other) { return null; }
			public EverythingIsThere BitwiseOr (EverythingIsThere other) { return null; }
			public EverythingIsThere ExclusiveOr (EverythingIsThere other) { return null; }

			public EverythingIsThere LeftShift (EverythingIsThere other) { return null; }
			public EverythingIsThere RightShift (EverythingIsThere other) { return null; }

			public int Compare (EverythingIsThere other) { return 0; }

			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}
		}

		[Test]
		public void TestEverythingIsThere ()
		{
			TypeDefinition type = GetTest ("EverythingIsThere");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		class MissingCompare {
			public static bool operator > (MissingCompare a, MissingCompare b) { return false; }
			public static bool operator >= (MissingCompare a, MissingCompare b) { return false; }
			public static bool operator < (MissingCompare a, MissingCompare b) { return false; }
			public static bool operator <= (MissingCompare a, MissingCompare b) { return false; }
			public static bool operator != (MissingCompare a, MissingCompare b) { return false; }
			public static bool operator == (MissingCompare a, MissingCompare b) { return false; } //for !=
		}

		[Test]
		public void TestMissingCompare ()
		{
			TypeDefinition type = GetTest ("MissingCompare");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (5, runner.Defects.Count, "Count");
		}

		class MissingUnary {
			public static MissingUnary operator + (MissingUnary a) { return null; }
			public static MissingUnary operator - (MissingUnary a) { return null; }
			public static MissingUnary operator ! (MissingUnary a) { return null; }
			public static MissingUnary operator ~ (MissingUnary a) { return null; }

			public static MissingUnary operator ++ (MissingUnary a) { return null; }
			public static MissingUnary operator -- (MissingUnary a) { return null; }
			public static bool operator true (MissingUnary a) { return false; }
			public static bool operator false (MissingUnary a) { return true; }
		}

		[Test]
		public void TestMissingUnary ()
		{
			TypeDefinition type = GetTest ("MissingUnary");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (8, runner.Defects.Count, "Count");
		}

		class MissingBinary {
			public static MissingBinary operator + (MissingBinary a, MissingBinary b) { return null; }
			public static MissingBinary operator - (MissingBinary a, MissingBinary b) { return null; }
			public static MissingBinary operator * (MissingBinary a, MissingBinary b) { return null; }
			public static MissingBinary operator / (MissingBinary a, MissingBinary b) { return null; }
			public static MissingBinary operator % (MissingBinary a, MissingBinary b) { return null; }

			public static MissingBinary operator & (MissingBinary a, MissingBinary b) { return null; }
			public static MissingBinary operator | (MissingBinary a, MissingBinary b) { return null; }
			public static MissingBinary operator ^ (MissingBinary a, MissingBinary b) { return null; }

			public static MissingBinary operator << (MissingBinary a, int b) { return null; }
			public static MissingBinary operator >> (MissingBinary a, int b) { return null; }
		}

		[Test]
		public void TestMissingBinary ()
		{
			TypeDefinition type = GetTest ("MissingBinary");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (10, runner.Defects.Count, "Count");
		}
	}
}
