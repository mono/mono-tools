//
// Unit tests for ConsiderConvertingMethodToPropertyRule
//
// Authors:
//	Adrian Tsai <adrian_tsai@hotmail.com>
//
// Copyright (c) 2007 Adrian Tsai
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Design;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Design {

	[TestFixture]
	public class ConsiderConvertingMethodToPropertyTest {

		public class ShouldBeCaught {
			int Foo;
			bool Bar;

			int GetFoo () { return Foo; }
			bool IsBar () { return Bar; }

			int SetFoo (int value) { return (Foo = value); }
			void SetBar (bool value) { Bar = value; }
		}

		public class ShouldBeIgnored {
			int getfoo;
			int GetFoo
			{
				get { return getfoo; }
				set { getfoo = value; }
			}

			byte [] Baz;

			byte [] GetBaz () { return Baz; }
		}

		public class ShouldBeIgnoredMultipleValuesInSet {
			long value;

			public long GetMyValue ()
			{
				return value;
			}

			public void SetMyValue (int value, int factor)
			{
				this.value = (long)(value * factor);
			}
		}

		public class GetConstructor {
			public GetConstructor () { } // Should be ignored
		}


		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new ConsiderConvertingMethodToPropertyRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Design.ConsiderConvertingMethodToPropertyTest/" + name;
			return assembly.MainModule.Types [fullname];
		}

		[Test]
		public void TestShouldBeCaught ()
		{
			TypeDefinition type = GetTest ("ShouldBeCaught");
			foreach (MethodDefinition md in type.Methods) {
				Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (md), "RuleResult." + md.Name);
				Assert.AreEqual (1, runner.Defects.Count, "Count." + md.Name);
			}
		}

		[Test]
		public void TestShouldBeIgnored ()
		{
			TypeDefinition type = GetTest ("ShouldBeIgnored");
			foreach (MethodDefinition md in type.Methods) {
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (md), "RuleResult." + md.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + md.Name);
			}
		}

		[Test]
		public void TestShouldBeIgnoredMultipleValuesInSet ()
		{
			TypeDefinition type = GetTest ("ShouldBeIgnoredMultipleValuesInSet");
			foreach (MethodDefinition md in type.Methods) {
				switch (md.Name) {
				case "GetMyValue":
					Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (md), "RuleResult1");
					Assert.AreEqual (1, runner.Defects.Count, "Count1");
					break;
				case "SetMyValue":
					Assert.AreEqual (RuleResult.Success, runner.CheckMethod (md), "RuleResult2");
					Assert.AreEqual (0, runner.Defects.Count, "Count2");
					break;
				}
			}
		}

		[Test]
		public void TestGetConstructor ()
		{
			TypeDefinition type = GetTest ("GetConstructor");
			foreach (MethodDefinition md in type.Constructors) {
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (md), "RuleResult." + md.Name);
				Assert.AreEqual (0, runner.Defects.Count, "Count." + md.Name);
			}
		}
	}
}
