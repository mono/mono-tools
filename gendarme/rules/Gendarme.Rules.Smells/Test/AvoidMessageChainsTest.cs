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

	[TestFixture]
	public class AvoidMessageChainsTest : MethodRuleTestFixture<AvoidMessageChainsRule> {
		
		public void MethodWithoutChain ()
		{
			Console.WriteLine ("I love rock and roll");		
		}

		[Test]
		public void MethodWithoutChainTest ()
		{
			AssertRuleSuccess<AvoidMessageChainsTest> ("MethodWithoutChain");
		}

		public void MethodWithChain ()
		{
			//4 Consecutive calls
			Console.OutputEncoding.EncoderFallback.CreateFallbackBuffer ().GetNextChar ();
			Console.WriteLine ("Blah");
			new System.Collections.ArrayList ();
		}

		public void MethodWithVariousChains ()
		{
			int major = Assembly.GetExecutingAssembly ().GetName ().Version.Major;
			int minor = Assembly.GetExecutingAssembly ().GetName ().Version.Minor;
		}

		[Test]
		public void MethodWithChainTest ()
		{
			AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithChain", 1);
			AssertRuleFailure<AvoidMessageChainsTest> ("MethodWithVariousChains", 2);
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
		[Ignore ("Next case")]
		public void ChainWithTemporaryVariablesTest ()
		{
			AssertRuleFailure<AvoidMessageChainsTest> ("ChainWithTemporaryVariables", 1);
		}
	}
}
