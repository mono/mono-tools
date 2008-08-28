//
// Unit Test for UseCorrectCasingRule
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
using System.Runtime.CompilerServices;

using Gendarme.Framework;
using Gendarme.Rules.Naming;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

// no namespace
class Foo { }

namespace Test.IO { class Foo {} }
namespace Test.Fa { class Foo {} }
namespace Test.ASP { class Foo {} class Bar {} }
namespace Test.A { class Foo {} }
namespace Test.Rules.ROCKS { class Foo { } }

namespace Test.Rules.Naming {

	[TestFixture]
	public class UseCorrectCasingTypeTest : TypeRuleTestFixture<UseCorrectCasingRule> {

		[CompilerGenerated]
		public class GeneratedType {
		}

		public class CorrectCasing {
		}

		public class incorrectCasing {
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<GeneratedType> ();
		}

		[Test]
		public void NamespaceOfLength0 ()
		{
			AssertRuleSuccess<Foo> ();
		}

		[Test]
		public void NamespaceOfLength1 ()
		{
			AssertRuleFailure<Test.A.Foo> (1);
		}

		[Test]
		public void NamespaceOfLength2 ()
		{
			AssertRuleSuccess<Test.IO.Foo> ();
			AssertRuleFailure<Test.Fa.Foo> (1);
		}

		[Test]
		public void IncorrectNamespaceReportedOnce ()
		{
			AssertRuleFailure<Test.ASP.Foo> (1);
			AssertRuleSuccess<Test.ASP.Bar> ();
		}

		[Test]
		public void LongNamespace ()
		{
			AssertRuleSuccess<UseCorrectCasingTypeTest> ();
			AssertRuleFailure<Test.Rules.ROCKS.Foo> (1);
		}

		[Test]
		public void Types ()
		{
			AssertRuleSuccess<CorrectCasing> ();
			AssertRuleFailure<incorrectCasing> (1);
		}
	}

	public class CasingMethods {
		public void CorrectCasing (int foo, string bar) { }
		public void incorrectCasing (int foo, string bar) { }
		public void CorrectCasingWithTwoIncorrectParameters (int Bar, string Foo) { }
		public void incorrectCasingWithTwoIncorrectParameters (int Bar, string Foo) { }
	}

	public class MoreComplexCasing {
		static MoreComplexCasing () { } // .cctor, should be ignored
		public MoreComplexCasing () { } // .ctor, should be ignored
		public int GoodProperty { get { return 0; } set { } }
		public int badProperty { get { return 0; } set { } }
		public int get_AccessorLike () { return 0; } // should be catched
		public void set_AccessorLike (int value) { } // should be catched
		public event EventHandler GoodEvent
		{
			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
		public event EventHandler badEvent
		{
			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
		public static int operator + (MoreComplexCasing a, int b) { return 0; } // ignore!
	}

	public class PrivateEventCasing {
		private event EventHandler good_private_event;
	}

	[TestFixture]
	public class UseCorrectCasingTest {
		public class AnonymousMethod {
			private void MethodWithAnonymousMethod ()
			{
				string [] values = new string [] { "one", "two", "three" };
				if (Array.Exists (values, delegate (string myString) { return myString.Length == 3; }))
					Console.WriteLine ("Exists strings with length == 3");
			}
		}

		private UseCorrectCasingRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new UseCorrectCasingRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetMethod (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}


		[Test]
		public void TestCorrectCasedMethod ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CasingMethods"];
			MethodDefinition method = GetMethod ("CorrectCasing");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestIncorrectCasedMethod ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CasingMethods"];
			MethodDefinition method = GetMethod ("incorrectCasing");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestCorrectCasedMethodWithIncorrectCasedParameters ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CasingMethods"];
			MethodDefinition method = GetMethod ("CorrectCasingWithTwoIncorrectParameters");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (2, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestIncorrectCasedMethodWithIncorrectCasedParameters ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.CasingMethods"];
			MethodDefinition method = GetMethod ("incorrectCasingWithTwoIncorrectParameters");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (3, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestIgnoringCtor ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.MoreComplexCasing"];
			// .ctor and .cctor
			foreach (MethodDefinition method in type.Constructors) {
				Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult");
				Assert.AreEqual (0, runner.Defects.Count, "Count");
			}
		}

		[Test]
		public void TestGoodProperty ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.MoreComplexCasing"];
			MethodDefinition method = GetMethod ("get_GoodProperty");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (0, runner.Defects.Count, "Count1");
			method = GetMethod ("set_GoodProperty");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (0, runner.Defects.Count, "Count2");
		}

		[Test]
		public void TestBadProperty ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.MoreComplexCasing"];
			MethodDefinition method = GetMethod ("get_badProperty");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (1, runner.Defects.Count, "Count1");
			method = GetMethod ("set_badProperty");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (1, runner.Defects.Count, "Count2");
		}

		[Test]
		public void TestGoodEventHandler ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.MoreComplexCasing"];
			MethodDefinition method = GetMethod ("add_GoodEvent");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (0, runner.Defects.Count, "Count1");
			method = GetMethod ("remove_GoodEvent");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (0, runner.Defects.Count, "Count2");
		}

		[Test]
		public void TestBadEventHandler ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.MoreComplexCasing"];
			MethodDefinition method = GetMethod ("add_badEvent");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (1, runner.Defects.Count, "Count1");
			method = GetMethod ("remove_badEvent");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (1, runner.Defects.Count, "Count2");
		}

		[Test]
		public void TestGoodPrivateEvent ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.PrivateEventCasing"];
			MethodDefinition method = GetMethod ("add_good_private_event");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult1");
			method = GetMethod ("remove_good_private_event");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult2");
		}

		[Test]
		public void TestPropertyLikeMethods ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.MoreComplexCasing"];
			MethodDefinition method = GetMethod ("get_AccessorLike");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (1, runner.Defects.Count, "Count1");
			method = GetMethod ("set_AccessorLike");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (1, runner.Defects.Count, "Count2");
		}

		[Test]
		public void TestIgnoringOperator ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.MoreComplexCasing"];
			MethodDefinition method = GetMethod ("op_Addition");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestAnonymousMethod ()
		{
			// compiler generated code is compiler dependant, check for [g]mcs (inner type)
			type = GetTest ("AnonymousMethod/<>c__CompilerGenerated0");
			// otherwise try for csc (inside same class)
			if (type == null)
				type = GetTest ("AnonymousMethod");

			Assert.IsNotNull (type, "type not found");
			foreach (MethodDefinition method in type.Methods) {
				switch (method.Name) {
				case "MethodWithAnonymousMethod":
					// this isn't part of the test (but included with CSC)
					break;
				default:
					Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method), "RuleResult");
					Assert.AreEqual (0, runner.Defects.Count, "Count");
					break;
				}
			}
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Naming.UseCorrectCasingTest/" + name;
			return assembly.MainModule.Types [fullname];
		}
	}
}
