//
// Unit tests for NonConstantStaticFieldsShouldNotBeVisibleRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
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

using Gendarme.Rules.Concurrency;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Concurrency {

	public class HasPublicConst {
		public const int CONST = 0;
	}

	public class HasPublicNonConstantStaticField {
		public static int Field;
	}

	public class HasProtectedNonConstantStaticField {
		protected static int Field;
	}

	public class HasInternalNonConstantStaticField {
		internal static int Field;
	}

	public class HasPublicConstantStaticField {
		public static readonly int Field;
	}

	public class HasPrivateNonConstantStaticField {
		private static int Field;
	}

	public class HasPublicNonConstantField {
		public int Field;
	}

	[TestFixture]
	public class NonConstantStaticFieldsShouldNotBeVisibleTest : TypeRuleTestFixture<NonConstantStaticFieldsShouldNotBeVisibleRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		[Test]
		public void TestHasPublicConst ()
		{
			AssertRuleSuccess<HasPublicConst> ();
		}

		[Test]
		public void TestHasPublicNonConstantStaticField ()
		{
			AssertRuleFailure<HasPublicNonConstantStaticField> (1);
		}

		[Test]
		public void TestHasProtectedNonConstantStaticField ()
		{
			AssertRuleFailure<HasProtectedNonConstantStaticField> (1);
		}

		[Test]
		public void TestHasInternalNonConstantStaticField ()
		{
			AssertRuleSuccess<HasInternalNonConstantStaticField> ();
		}

		[Test]
		public void TestHasPublicConstantStaticField ()
		{
			AssertRuleSuccess<HasPublicConstantStaticField> ();
		}

		[Test]
		public void TestHasPrivateNonConstantStaticField ()
		{
			AssertRuleSuccess<HasPrivateNonConstantStaticField> ();
		}

		[Test]
		public void TestHasPublicNonConstantField ()
		{
			AssertRuleSuccess<HasPublicNonConstantField> ();
		}
	}
}
