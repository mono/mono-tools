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
using Test.Rules.Definitions;
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

		public Exception Implemented5 (object o)
		{
			if (o == null)
				throw new ArgumentNullException ("o");
			return new NotImplementedException ();
		}

		public void NotFullyImplemented_Split (int x)
		{
			if (x < 0) {
				throw new NotImplementedException ("x < 0 has a different behavior which isn't implemented yet.");
			} else {
				Implemented2 ();
			}
		}

		public void NotFullyImplemented_Check (object o)
		{
			if (o == null)
				throw new ArgumentNullException ("o");

			throw new NotImplementedException ("only basic checks are done");
		}

		public void NotFullyImplemented_Long (int x)
		{
			switch (x) {
			case 0:
				break;
			case 1:
				Implemented1 ();
				break;
			case 2:
				Implemented2 ();
				break;
			case 3:
				Implemented3 ();
				break;
			case 4:
				Implemented4 ();
				break;
			default:
				throw new NotImplementedException ("only basic checks are done");
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
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no NEWOBJ
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void NotImplementedMethodsTest ()
		{
			AssertRuleFailure<ImplementedOrNotMethods> ("NotImplemented1");
			AssertRuleFailure<ImplementedOrNotMethods> ("NotImplemented2");
		}

		[Test]
		public void NotFullyImplementedMethodsTest ()
		{
			// there's a branch but the code is too small for a NotImplementedException not to mean something
			AssertRuleFailure<ImplementedOrNotMethods> ("NotFullyImplemented_Split");
			AssertRuleFailure<ImplementedOrNotMethods> ("NotFullyImplemented_Check");
			// in this last case the code is big enough to be judged, partially, useful
			AssertRuleSuccess<ImplementedOrNotMethods> ("NotFullyImplemented_Long");
		}

		[Test]
		public void ImplementedMethods ()
		{
			// no NEWOBJ
			AssertRuleDoesNotApply<ImplementedOrNotMethods> ("Implemented1");
			// no NEWOBJ
			AssertRuleDoesNotApply<ImplementedOrNotMethods> ("Implemented2");
			AssertRuleSuccess<ImplementedOrNotMethods> ("Implemented3");
			// no THROW
			AssertRuleDoesNotApply<ImplementedOrNotMethods> ("Implemented4");
			AssertRuleSuccess<ImplementedOrNotMethods> ("Implemented5");
		}

		[Test]
		public void OutOfScopeMethods ()
		{
			AssertRuleDoesNotApply<InterfaceMethods> ("Method");
			AssertRuleDoesNotApply<AbstractMethods> ("Method");
		}
	}
}
