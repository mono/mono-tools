//
// Unit tests for ParameterNamesShouldMatchOverridenMethodTest
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework.Helpers;
using Gendarme.Rules.Naming;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Naming {

	interface ISomeInterface {
		bool InterfaceMethod (int im);
	}

	interface ISomeInterface2 {
		bool InterfaceMethod2 (int im);
	}

	abstract public class SuperBaseClass {
		protected virtual void VirtualSuperIncorrect (int vsi1, bool vsi2)
		{
		}
		protected virtual void VirtualSuperIncorrect (int vsi1, int vsi2_)
		{
		}
	}

	abstract public class BaseClass : SuperBaseClass {
		protected virtual void VirtualCorrect (int vc1, int vc2)
		{
		}

		protected virtual void VirtualIncorrect (int vi1, int vi2)
		{
		}

		protected abstract void AbstractCorrect (int ac1, int ac2);

		protected abstract void AbstractIncorrect (int ai1, int ai2);

		protected virtual void NoOverwrite (int a, int b)
		{
		}
	}

	[TestFixture]
	public class ParameterNamesShouldMatchOverridenMethodTest : MethodRuleTestFixture<ParameterNamesShouldMatchOverriddenMethodRule> {

		class TestCase : BaseClass, ISomeInterface, ISomeInterface2, IEquatable<string> {
			protected override void VirtualCorrect (int vc1, int vc2)
			{
			}

			protected override void VirtualIncorrect (int vi1, int vi2a)
			{
			}

			protected override void VirtualSuperIncorrect (int vsi1, bool vsi2_)
			{
			}

			protected override void AbstractCorrect (int ac1, int ac2)
			{
				throw new NotImplementedException ();
			}

			protected override void AbstractIncorrect (int ai1, int ai2_)
			{
				throw new NotImplementedException ();
			}

			protected virtual void NoOverwrite (int a, int bb)
			{
			}

			public bool InterfaceMethod (int im_)
			{
				return false;
			}

			bool ISomeInterface2.InterfaceMethod2 (int im_)
			{
				return false;
			}

			void NoParameter ()
			{
			}

			public bool Equals (string s)
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void TestVirtual ()
		{
			AssertRuleSuccess<TestCase> ("VirtualCorrect");
			AssertRuleFailure<TestCase> ("VirtualIncorrect", 1);
			AssertRuleFailure<TestCase> ("VirtualSuperIncorrect", 1);
		}

		[Test]
		public void TestAbstract ()
		{
			AssertRuleSuccess<TestCase> ("AbstractCorrect");
			AssertRuleFailure<TestCase> ("AbstractIncorrect", 1);
		}

		[Test]
		public void TestNoOverwrite ()
		{
			AssertRuleSuccess<TestCase> ("NoOverwrite");
		}

		[Test]
		public void TestInterfaceMethod ()
		{
			AssertRuleFailure<TestCase> ("InterfaceMethod", 1);
			AssertRuleFailure<TestCase> ("Test.Rules.Naming.ISomeInterface2.InterfaceMethod2", 1);
		}

		[Test]
		public void TestDoesNotApply ()
		{
			AssertRuleDoesNotApply<TestCase> ("NoParameter");
		}

		[Test]
		public void GenericInterface ()
		{
			AssertRuleSuccess<OpCodeBitmask> ("Equals", new Type [] { typeof (OpCodeBitmask) });
			AssertRuleFailure<TestCase> ("Equals", 1);
		}
	}
}
