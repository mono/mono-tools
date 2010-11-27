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
using System.Reflection;

using Gendarme.Rules.Naming;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

// no namespace
class Foo { }

namespace Test.IO { class Foo {} }
namespace Test.Fa { class Foo {} }
namespace Test.ASP { class Foo {} class Bar {} }
namespace Test.A { class Foo {} }
namespace Test.Rules.ROCKS { class Foo { } }
namespace Test.aSP { class Zoo { } class Yar { } }

namespace Test.Rules.Naming {

	[TestFixture]
	public class UseCorrectCasingAssemblyTest : AssemblyRuleTestFixture<UseCorrectCasingRule> {

		[Test]
		public void Namespaces ()
		{
			// 1. Test.A
			// 2. Test.Fa
			// 3. Test.ASP
			// 4. Test.Rules.ROCKS
			// 5. Test.aSP
			string unit = Assembly.GetExecutingAssembly ().Location;
			AssertRuleFailure (AssemblyDefinition.ReadAssembly (unit), 5);
		}
	}

	[TestFixture]
	public class UseCorrectCasingTypeTest : TypeRuleTestFixture<UseCorrectCasingRule> {

		public class CorrectCasing {
		}

		public class incorrectCasing {
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.GeneratedType);
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

		public void IncorrectParameter (byte B) { }
		public void x () { }
		public void _X (short _S) { }
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
	public class UseCorrectCasingTest : MethodRuleTestFixture<UseCorrectCasingRule> {

		[Test]
		public void TestCorrectCasedMethod ()
		{
			AssertRuleSuccess<CasingMethods> ("CorrectCasing");
		}

		[Test]
		public void TestIncorrectCasedMethod ()
		{
			AssertRuleFailure<CasingMethods> ("incorrectCasing", 1);
		}

		[Test]
		public void TestCorrectCasedMethodWithIncorrectCasedParameters ()
		{
			AssertRuleFailure<CasingMethods> ("CorrectCasingWithTwoIncorrectParameters", 2);
		}

		[Test]
		public void TestIncorrectCasedMethodWithIncorrectCasedParameters ()
		{
			AssertRuleFailure<CasingMethods> ("incorrectCasingWithTwoIncorrectParameters", 3);
		}

		[Test]
		public void MoreCoverage ()
		{
			// parameter 'B' should be lower case to be CamelCase
			AssertRuleFailure<CasingMethods> ("IncorrectParameter", 1);
			// method name should be uppercase to be PascalCase
			AssertRuleFailure<CasingMethods> ("x", 1);
			// starts with an underscore for name and parameter, fails both Pascal and Camel checks
			AssertRuleFailure<CasingMethods> ("_X", 2);
		}

		[Test]
		public void TestIgnoringCtor ()
		{
			AssertRuleDoesNotApply<MoreComplexCasing> (".cctor");
			AssertRuleDoesNotApply<MoreComplexCasing> (".ctor");
		}

		[Test]
		public void TestGoodProperty ()
		{
			AssertRuleSuccess<MoreComplexCasing> ("get_GoodProperty");
			AssertRuleSuccess<MoreComplexCasing> ("set_GoodProperty");
		}

		[Test]
		public void TestBadProperty ()
		{
			AssertRuleFailure<MoreComplexCasing> ("get_badProperty", 1);
			AssertRuleFailure<MoreComplexCasing> ("set_badProperty", 1);
		}

		[Test]
		public void TestGoodEventHandler ()
		{
			AssertRuleSuccess<MoreComplexCasing> ("add_GoodEvent");
			AssertRuleSuccess<MoreComplexCasing> ("remove_GoodEvent");
		}

		[Test]
		public void TestBadEventHandler ()
		{
			AssertRuleFailure<MoreComplexCasing> ("add_badEvent", 1);
			AssertRuleFailure<MoreComplexCasing> ("remove_badEvent", 1);
		}

		[Test]
		public void TestGoodPrivateEvent ()
		{
			AssertRuleDoesNotApply<PrivateEventCasing> ("add_good_private_event");
			AssertRuleDoesNotApply<PrivateEventCasing> ("remove_good_private_event");
		}

		[Test]
		public void TestPropertyLikeMethods ()
		{
			AssertRuleFailure<MoreComplexCasing> ("get_AccessorLike", 1);
			AssertRuleFailure<MoreComplexCasing> ("set_AccessorLike", 1);
		}

		[Test]
		public void TestIgnoringOperator ()
		{
			AssertRuleSuccess<MoreComplexCasing> ("op_Addition");
		}

		public class AnonymousMethod {
			private void MethodWithAnonymousMethod ()
			{
				string [] values = new string [] { "one", "two", "three" };
				if (Array.Exists (values, delegate (string myString) { return myString.Length == 3; }))
					Console.WriteLine ("Exists strings with length == 3");
			}
		}

		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
		}

		[Test]
		public void TestAnonymousMethod ()
		{
			// compiler generated code is compiler dependant, check for [g]mcs (inner type)
			TypeDefinition type = assembly.MainModule.GetType ("Test.Rules.Naming.UseCorrectCasingTest/AnonymousMethod/<>c__CompilerGenerated0");
			// otherwise try for csc (inside same class)
			if (type == null)
				type = assembly.MainModule.GetType ("Test.Rules.Naming.UseCorrectCasingTest/AnonymousMethod");

			Assert.IsNotNull (type, "type not found");
			foreach (MethodDefinition method in type.Methods) {
				switch (method.Name) {
				case "MethodWithAnonymousMethod":
					// this isn't part of the test (but included with CSC)
					break;
				default:
					AssertRuleDoesNotApply (method);
					break;
				}
			}
		}
	}
}
