//
// Unit Test for AvoidSpeculativeGenerality Rule.
//
// Authors:
//      Néstor Salceda <nestor.salceda@gmail.com>
//
//      (C) 2007 Néstor Salceda
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
using System.Collections.Generic;

using Mono.Cecil;
using NUnit.Framework;
using Gendarme.Framework;
using Gendarme.Rules.Smells;
using Test.Rules.Helpers;

namespace Test.Rules.Smells {
	
	//
	public abstract class AbstractClass {
		public abstract void MakeStuff ();
	}

	public class OverriderClass : AbstractClass {
		public override void MakeStuff () 
		{
		
		}
	}

	//
	public abstract class OtherAbstractClass {
		public abstract void MakeStuff ();
	}

	public class OtherOverriderClass : OtherAbstractClass {
		public override void MakeStuff () 
		{
		}
	}

	public class YetAnotherOverriderClass : OtherAbstractClass {
		public override void MakeStuff () 
		{
		}
	}

	//

	public class ClassWithUnusedParameter {
		public void Foo (int x) 
		{
		}
	}

	public class ClassWithFourUnusedParameters {
		public void Foo (int x) 
		{
		}

		public void Bar (int x, char f) 
		{
		}

		public void Baz (float f) 
		{
		}

	}
	
	//
	public class UnnecessaryDelegatedClass {
		public void WriteLine (string message) 
		{
			Console.WriteLine (message);
		}

		public DateTime GetDateTime ()
		{
			return DateTime.Now;
		}
	}

	//
	public class NotUnnecessaryDelegatedClass {
		public void WriteLog (string message)
		{
			Console.WriteLine ("Starting Logging...");
			Console.WriteLine ("[LOGGING FOR] {0}", message);
		}

		public void PrintBanner ()
		{
			Console.WriteLine ("This is a simple banner");
			Console.WriteLine ("For the incredible Foo-Bar program");
			Console.WriteLine ("Send your suggestions to foo@domain.com");
		}

		public DateTime GetDateTime ()
		{
			return DateTime.Now;
		}
	}

	[TestFixture]
	public class AvoidSpeculativeGeneralityTest {
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new AvoidSpeculativeGeneralityRule ();
			runner = new TestRunner (rule);
		}

		[Test]
		public void AbstractClassesWithoutResponsabilityTest () 
		{
			type = assembly.MainModule.Types["Test.Rules.Smells.AbstractClass"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
			Assert.AreEqual (1, runner.Defects.Count);
		}

		[Test]
		public void AbstractClassesWithResponsabilityTest ()
		{
			type = assembly.MainModule.Types["Test.Rules.Smells.OtherAbstractClass"];
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void ClassWithUnusedParameterTest () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Smells.ClassWithUnusedParameter"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
			Assert.AreEqual (1, runner.Defects.Count);
		}
		
		[Test]
		public void ClassWithFourUnusedParametersTest () 
		{
			type = assembly.MainModule.Types ["Test.Rules.Smells.ClassWithFourUnusedParameters"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
			Assert.AreEqual (4, runner.Defects.Count);
		}

		[Test]
		public void ClassWithUnnecessaryDelegationTest ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Smells.UnnecessaryDelegatedClass"];
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
			Assert.AreEqual (1, runner.Defects.Count);
		}

		[Test]
		public void ClassWithoutUnnecessaryDelegationTest ()
		{
			type = assembly.MainModule.Types ["Test.Rules.Smells.NotUnnecessaryDelegatedClass"];
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}
	}
}
