//
// Unit Test for UsePreferredTermsRule
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
using Test.Rules.Helpers;

namespace Test.Rules.Naming {
	
	public class SomeComPlusStuff { // one obsolete term ('ComPlus')
	}
	
	public class SomeComPlusAndIndicesStuff { // two obsolete terms ('ComPlus' and 'Indices')
	}
	
	public class TermsMethodsAndProperties {

		public void SignOn () { } // incorrect
		public void SignIn () { } // correct
		
		public bool Writeable { get { return true; } } // incorrect
		public bool Writable { get { return true; } } // correct
	}
	
	[TestFixture]
	public class UsePreferredTermsTest {
		private UsePreferredTermsRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;
	
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new UsePreferredTermsRule ();
			runner = new TestRunner (rule);
		}
		
      		private MethodDefinition GetPropertyGetter (string name)
		{
			string get_name = "get_" + name;
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == get_name)
					return method;
			}
			return null;
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
		public void TestOneObsoleteTerm ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.SomeComPlusStuff"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void TestTwoObsoleteTerms ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.SomeComPlusAndIndicesStuff"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (2, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void TestCorrectMethodsAndProperties ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.TermsMethodsAndProperties"];
			MethodDefinition method = GetMethod ("SignIn");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (0, runner.Defects.Count, "Count1");
			method = GetMethod ("get_Writable");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (0, runner.Defects.Count, "Count2");
		}
		
		[Test]
		public void TestIncorrectMethodsAndProperties ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Naming.TermsMethodsAndProperties"];
			MethodDefinition method = GetMethod ("SignOn");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult1");
			Assert.AreEqual (1, runner.Defects.Count, "Count1");
			method = GetMethod ("get_Writeable");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult2");
			Assert.AreEqual (1, runner.Defects.Count, "Count2");
		}		
	}
}
