//
// Unit tests for AvoidTypeInterfaceInconsistencyRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Gendarme.Rules.Naming;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Naming {

	public interface IFoo {
	}

	// misnamed interface
	public interface JFoo {
	}

	// not implementing IFoo
	public class Foo : JFoo {
	}

	public interface IBar {
	}

	public class Bar : IBar {
	}

	// misnamed interface
	public interface Box {
	}

	// misnamed type
	public class IBox : Box {
	}

	[TestFixture]
	public class AvoidTypeInterfaceInconsistencyTest : TypeRuleTestFixture<AvoidTypeInterfaceInconsistencyRule> {

		[Test]
		public void DoesNotApply ()
		{
			// types
			AssertRuleDoesNotApply<Bar> ();
			AssertRuleDoesNotApply<Foo> ();
			AssertRuleDoesNotApply<IBox> ();
			// misnamed
			AssertRuleDoesNotApply<JFoo> ();
			AssertRuleDoesNotApply<Box> ();
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<IBar> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<IFoo> (1);
		}
	}
}
