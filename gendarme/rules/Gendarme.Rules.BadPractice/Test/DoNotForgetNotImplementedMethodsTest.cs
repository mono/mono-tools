//
// Unit Test for DoNotForgetNotImplementedMethodsRule.
//
// Authors:
//      Cedric Vivier <cedricv@neonux.com>
//
//      (C) 2008 Cedric Vivier
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

using Gendarme.Framework;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.BadPractice {

	public class ImplementedOrNotMethods {

		public void Empty ()
		{
			// but empty methods are not considered
		}

		public void NotImplemented1 ()
		{
			throw new NotImplementedException ();
		}

		private string BuildLazyMessage (string s)
		{
			return string.Format ("I'm too lazy to implement {0}", s);
		}

		public void NotImplemented2 ()
		{
			throw new NotImplementedException (BuildLazyMessage ("this"));
		}

		public int Implemented1 ()
		{
			return 1;
		}

		public void Implemented2 ()
		{
			Console.WriteLine ("foo");
		}

		public void Implemented3 ()
		{
			throw new InvalidOperationException ();
		}

		public Exception Implemented4 ()
		{
			return new NotImplementedException ();
		}

		public void NotFullyImplemented (int x)
		{
			if (x < 0) {
				throw new NotImplementedException ("x < 0 has a different behavior which isn't implemented yet.");
			} else {
				Implemented2 ();
			}
		}
	}

	interface InterfaceMethods {
		void Method ();
	}

	abstract class AbstractMethods {
		abstract protected int Method ();
	}

	[TestFixture]
	public class DoNotForgetNotImplementedMethodsTest : MethodRuleTestFixture<DoNotForgetNotImplementedMethodsRule> {

		[Test]
		public void Empty ()
		{
			AssertRuleSuccess<ImplementedOrNotMethods> ("Empty");
		}

		[Test]
		public void NotImplementedMethodsTest ()
		{
			AssertRuleFailure<ImplementedOrNotMethods> ("NotImplemented1");
			AssertRuleFailure<ImplementedOrNotMethods> ("NotImplemented2");
		}

		//the rule does not currently report the case below
		//not sure it would be worth the performance penalty (all method body has to be navigated)
		[Test]
		public void NotFullyImplementedMethodsTest ()
		{
			AssertRuleSuccess<ImplementedOrNotMethods> ("NotFullyImplemented");
		}

		[Test]
		public void ImplementedMethods ()
		{
			AssertRuleSuccess<ImplementedOrNotMethods> ("Implemented1");
			AssertRuleSuccess<ImplementedOrNotMethods> ("Implemented2");
			AssertRuleSuccess<ImplementedOrNotMethods> ("Implemented3");
			AssertRuleSuccess<ImplementedOrNotMethods> ("Implemented4");
		}

		[Test]
		public void OutOfScopeMethods ()
		{
			AssertRuleDoesNotApply<InterfaceMethods> ("Method");
			AssertRuleDoesNotApply<AbstractMethods> ("Method");
		}
	}
}
