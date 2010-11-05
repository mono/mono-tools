//
// Unit Test for UsePluralNameInEnumFlags Rule.
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
using Gendarme.Rules.Naming;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Naming {
	
	[Flags]
	public enum AppDomainManagerInitializationOptions {
		None,
		RegisterWithHost
	}
	
	[Flags]
	public enum ConsoleModifier {
		Alt,
		Control,
		Shift
	}

	[TestFixture]
	public class UsePluralNameInEnumFlagsTest {
		
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;
	
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			rule = new UsePluralNameInEnumFlagsRule ();
			runner = new TestRunner (rule);
		}
		
		[Test]
		public void TestFlagsHasPluralName () 
		{
			type = assembly.MainModule.GetType ("Test.Rules.Naming.AppDomainManagerInitializationOptions");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void TestFlagsHasSingularName () 
		{
			type = assembly.MainModule.GetType ("Test.Rules.Naming.ConsoleModifier");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
	}		
}
