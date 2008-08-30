//
// Unit tests for ArrayFieldsShouldNotBeReadOnlyRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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
using Gendarme.Rules.Security;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Security {

	public class HasStaticPublicReadonlyArray {
		public static readonly string [] Array;
	}

	public class HasPublicReadonlyArray {
		public readonly string [] Array;
	}

	public class HasProtectedReadonlyArray {
		protected readonly string [] Array;
	}

	public class HasInternalReadonlyArray {
		internal readonly string [] Array;
	}

	public class HasPrivateReadonlyArray {
		private readonly string [] Array;
	}

	public class HasNoReadonlyArray {
		public readonly string NoArray;
	}

	public class HasPublicArray {
		public string [] Array;
	}

	public struct StructHasStaticPublicReadonlyArray {
		public static readonly string [] Array;
	}

	public struct StructHasPublicReadonlyArray {
		public readonly string [] Array;
	}

/* this does not compile 
	public struct StructHasProtectedReadonlyArray {
		protected readonly string [] Array;
	}
*/
	public struct StructHasInternalReadonlyArray {
		internal readonly string [] Array;
	}

	public struct StructHasPrivateReadonlyArray {
		private readonly string [] Array;
	}

	public struct StructHasNoReadonlyArray {
		public readonly string NoArray;
	}

	public struct StructHasPublicArray {
		public string [] Array;
	}

	[TestFixture]
	public class ArrayFieldsShouldNotBeReadOnlyTest : TypeRuleTestFixture<ArrayFieldsShouldNotBeReadOnlyRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
		}

		[Test]
		public void TestHasStaticPublicReadonlyArray ()
		{
			AssertRuleFailure<HasStaticPublicReadonlyArray> (1);
			AssertRuleFailure<StructHasStaticPublicReadonlyArray> (1);
		}

		[Test]
		public void TestHasPublicReadonlyArray ()
		{
			AssertRuleFailure<HasPublicReadonlyArray> (1);
			AssertRuleFailure<StructHasPublicReadonlyArray> (1);
		}

		[Test]
		public void TestHasProtectedReadonlyArray ()
		{
			AssertRuleFailure<HasProtectedReadonlyArray> (1);
		}

		[Test]
		public void TestHasInternalReadonlyArray ()
		{
			AssertRuleSuccess<HasInternalReadonlyArray> ();
			AssertRuleSuccess<StructHasInternalReadonlyArray> ();
		}

		[Test]
		public void TestHasPrivateReadonlyArray ()
		{
			AssertRuleSuccess<HasPrivateReadonlyArray> ();
			AssertRuleSuccess<StructHasPrivateReadonlyArray> ();
		}

		[Test]
		public void TestHasNoReadonlyArray ()
		{
			AssertRuleSuccess<HasNoReadonlyArray> ();
			AssertRuleSuccess<StructHasNoReadonlyArray> ();
		}

		[Test]
		public void TestHasPublicArray ()
		{
			AssertRuleSuccess<HasNoReadonlyArray> ();
			AssertRuleSuccess<StructHasNoReadonlyArray> ();
		}
	}
}
