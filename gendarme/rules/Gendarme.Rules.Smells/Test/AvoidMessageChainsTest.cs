//
// Unit Tests for AvoidMessageChains Rule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using Gendarme.Rules.Smells;
using Mono.Cecil;

using Test.Rules.Fixtures;
using Test.Rules.Definitions;

using NUnit.Framework;

namespace Test.Rules.Smells {

	class One {
		public Two ReturnTwo (int x) {return null;}
	}

	class Two {
		public Three ReturnThree (string x) {return null;}
	}

	class Three {
		public Four ReturnFour (object o, int f) {return null;}
	}
	class Four {
		public Five ReturnFive (float f) {return null;}
	}
	class Five {
		public Six ReturnSix () {return null;}
	}
	class Six {
		public One ReturnOne (char c) {return null;}
	}

	[TestFixture]
	public class AvoidMessageChainsTest : MethodRuleTestFixture<AvoidMessageChainsRule> {
		
		public void MethodWithoutChain ()
		{
			new One ().ReturnTwo (3).ReturnThree ("Avoid chaining me");
		}

		public void MethodWithArgumentsWithoutChain (string s)
		{
			Four four = new Two ().ReturnThree (s).ReturnFour (new object (), 3);

		}

		[Test]
		public void MethodWithoutChainTest ()
		{
			AssertRuleSuccess<AvoidMessageChainsTest> ("MethodWithoutChain");
			AssertRuleSuccess<AvoidMessageChainsTest> ("MethodWithArgumentsWithoutChain");
		}

		public void MethodWithChain ()
		{
			object obj = new object ();
			new One ().ReturnTwo (3).ReturnThree ("Ha ha! Chained").ReturnFour (obj, 5).ReturnFive (3).ReturnSix ().ReturnOne ('a');
		}

		[Test]
		public void MethodWithChainTest ()
		{
			AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithChain", 1);
		}

		public void MethodWithVariousChains ()
		{
			object obj = new object ();
			new One ().ReturnTwo (3).ReturnThree ("Ha ha! Chained").ReturnFour (obj, 5).ReturnFive (3).ReturnSix ().ReturnOne ('a');
			Two two= new Three ().ReturnFour (obj, 3).ReturnFive (4 + 4).ReturnSix ().ReturnOne ('2').ReturnTwo (8);
		}

		[Test]
		public void MethodWithVariousChainsTest ()
		{
			AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithVariousChains", 2);

		}

		public void MethodWithArgumentsChained (int x, float f)
		{
			new One ().ReturnTwo (x).ReturnThree ("Ha ha! Chained").ReturnFour (new object (), x).ReturnFive (f).ReturnSix ().ReturnOne ('a');
		}

		[Test]
		public void MethodWithArgumentsChainedTest ()
		{
			AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithArgumentsChained", 1);
		}

		[Test]
		[Ignore ("Still not working.")]
		public void CanonicalScenariosTest ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);

			//When a method is empty, it returns a ret instruction
			//I wonder if this means some compiler optimization or
			//anything else
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public void ChainWithTemporaryVariables ()
		{
			Version version = Assembly.GetExecutingAssembly ().GetName ().Version;
			int major = version.Major;
		}

		[Test]
		[Ignore ("Uncaught")]
		public void ChainWithTemporaryVariablesTest ()
		{
			AssertRuleFailure<AvoidMessageChainsTest> ("ChainWithTemporaryVariables", 1);
		}

		public void NoChainWithTemporaryVariables ()
		{
			int x;
			char c;
			Console.WriteLine ("More tests");
		}

		[Test]
		[Ignore ("Uncaught")]
		public void NoChainWithTemporaryVariablesTEst ()
		{
			AssertRuleSuccess<AvoidMessageChainsTest> ("NoChainWithTemporaryVariables");
		}
	}
}
