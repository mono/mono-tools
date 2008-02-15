//
// Unit Test for UseCorrectPrefixRule
//
// Authors:
//      Abramov Daniel <ex@vingrad.ru>
//
//  (C) 2007 Abramov Daniel
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
using System.Collections;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Naming;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Naming {

	public class CorrectClass {
	}

	public class AnotherCorrectClass {
	}

	public class CIncorrectClass {
	}

	public interface ICorrectInterface {
	}

	public interface IncorrectInterface {
	}

	public interface AnotherIncorrectInterface {
	}

	public class CLSAbbreviation { // ok
	}

	public interface ICLSAbbreviation { // ok too
	}

	[TestFixture]
	public class UseCorrectPrefixTest {

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new UseCorrectPrefixRule ();
			runner = new TestRunner (rule);
		}

		[Test]
		public void TestCorrectClass ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CorrectClass"];
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestAnotherCorrectClass ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.AnotherCorrectClass"];
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestIncorrectClass ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CIncorrectClass"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestCorrectInterface ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.ICorrectInterface"];
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestIncorrectInterface ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.IncorrectInterface"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestAnotherIncorrectInterface ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.AnotherIncorrectInterface"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestClassAbbreviation ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CLSAbbreviation"];
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestInterfaceAbbreviation ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.ICLSAbbreviation"];
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
