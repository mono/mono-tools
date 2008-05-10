//
// Unit tests for OverrideEqualsMethodRule
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
	public class OverrideEqualsMethodTest {

		private OverrideEqualsMethodRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new OverrideEqualsMethodRule ();
			runner = new TestRunner (rule);
		}

		public TypeDefinition GetTest (string name)
		{
			foreach (TypeDefinition type in assembly.MainModule.Types ["Test.Rules.Design.OverrideEqualsMethodTest"].NestedTypes) {
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

		class EverythingOK {
			public static bool operator == (EverythingOK a, EverythingOK b)
			{
				return true;
			}

			public static bool operator != (EverythingOK a, EverythingOK b)
			{
				return true;
			}

			public override bool Equals (object obj)
			{
				return base.Equals (obj);
			}
		}

		[Test]
		public void TestEverythingOK ()
		{
			TypeDefinition type = GetTest ("EverythingOK");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		class NoEquals {
			public static bool operator == (NoEquals a, NoEquals b)
			{
				return true;
			}

			public static bool operator != (NoEquals a, NoEquals b)
			{
				return true;
			}
		}

		[Test]
		public void TestNoEquals ()
		{
			TypeDefinition type = GetTest ("NoEquals");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
	}
}
