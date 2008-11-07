//
// Unit tests for AvoidRedundancyInTypeNameRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2008 Cedric Vivier
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

using System;

using Gendarme.Framework;
using Gendarme.Rules.Naming;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;


class OutsideNamespaceType
{
}

namespace FooX {
	namespace Bar {
		class Barometer {
		}
		class BarClass : Class {
		}
		class BarContext {
		}

		interface IBarInterface {
		}

		interface IBarContext {
		}
	}

	class Class {
	}

	interface IInterface {
	}
}

namespace Baz {
	class BazClass : FooX.Class {
	}

	class BazSuperClass : BazClass {
	}

	interface ISomeInterface {
	}

	interface IBazInterface {
	}

	enum BazVersion {
	}

	enum BazExistingVersion {
	}

	enum ExistingVersion {
	}
}

namespace Test.Rules.Naming {

	[TestFixture]
	public class AvoidRedundancyInTypeNameTest : TypeRuleTestFixture<AvoidRedundancyInTypeNameRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<OutsideNamespaceType> ();
			AssertRuleDoesNotApply (SimpleTypes.GeneratedType);
		}

		[Test]
		public void Failure1 ()
		{
			AssertRuleFailure<FooX.Bar.BarContext> (1);
			Assert.IsTrue (-1 != Runner.Defects [0].Text.IndexOf ("'Context'"), "BarContext");
		}

		[Test]
		public void Failure2 ()
		{
			AssertRuleFailure<Baz.IBazInterface> (1);
			Assert.IsTrue (-1 != Runner.Defects [0].Text.IndexOf ("'IInterface'"), "IBazInterface");
		}

		[Test]
		public void Failure3 ()
		{
			AssertRuleFailure<Baz.BazVersion> ();
			Assert.IsTrue (-1 != Runner.Defects [0].Text.IndexOf ("'Version'"), "BazVersion");
		}

		[Test]
		public void Failure4 ()
		{
			AssertRuleFailure<FooX.Bar.IBarContext> ();
			Assert.IsTrue (-1 != Runner.Defects [0].Text.IndexOf ("'IContext'"), "IBarContext");
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<FooX.Class> ();
			AssertRuleSuccess<FooX.Bar.Barometer> ();//'ometer' not a good suggestion
			AssertRuleSuccess<FooX.Bar.BarClass> (); //ambiguity with parent namespace's type
			AssertRuleSuccess<Baz.BazClass> (); //ambiguity with base type
			AssertRuleSuccess<Baz.BazSuperClass> (); //base class follow prefix pattern already
			AssertRuleSuccess<Baz.ISomeInterface> ();
			AssertRuleSuccess<Baz.BazExistingVersion> ();//ExistingVersion already exists
			AssertRuleSuccess<FooX.Bar.IBarInterface> (); //ambiguity with parent namespace's IInterface
		}
	}
}
