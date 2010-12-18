//
// Unit Test for AvoidCodeDuplicatedInSiblingClasses Rule.
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 Néstor Salceda
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
using Gendarme.Rules.Smells;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Smells {

	public class BaseClassWithCodeDuplicated {
		protected IList list;
	}

	public class OverriderClassWithCodeDuplicated : BaseClassWithCodeDuplicated {
		public void CodeDuplicated () 
		{
			foreach (int i in list) {
				Console.WriteLine (i);
			}
			list.Add (1);
		}
	}

	public class OtherOverriderWithCodeDuplicated : BaseClassWithCodeDuplicated {
		public void OtherMethod ()
		{
			foreach (int i in list) {
				Console.WriteLine (i);
			}
			list.Remove (1);
		}
	}

	public class BaseClassWithoutCodeDuplicated {
		protected IList list;

		protected void PrintValuesInList () 
		{
			foreach (int i in list) {
				Console.WriteLine (i);
			}
		}
	}

	public class OverriderClassWithoutCodeDuplicated : BaseClassWithoutCodeDuplicated {
		public void SomeCode () 
		{
			PrintValuesInList ();
			list.Add (1);
		}
	}

	public class OtherOverriderWithoutCodeDuplicated : BaseClassWithoutCodeDuplicated {
		public void MoreCode ()
		{
			PrintValuesInList ();
			list.Remove (1);
		}
	}

	[TestFixture]
	public class AvoidCodeDuplicatedInSiblingClassesTest {
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			rule = new AvoidCodeDuplicatedInSiblingClassesRule ();
			runner = new TestRunner (rule);
		}

		[Test]
		public void BaseClassWithCodeDuplicatedTest () 
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.BaseClassWithCodeDuplicated");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "Test.Rules.Smells.BaseClassWithCodeDuplicated failure test");
			Assert.AreEqual (1, runner.Defects.Count, "Test.Rules.Smells.BaseClassWithCodeDuplicated defect count check");
		}

		[Test]
		public void BaseClassWithoutCodeDuplicatedTest ()
		{
			type = assembly.MainModule.GetType ("Test.Rules.Smells.BaseClassWithoutCodeDuplicated");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}
	}
}
