//
// Unit tests for NativeFieldsShouldNotBeVisibleRule
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

	public class HasPublicNativeField {
		public IntPtr Native;
	}

	public class HasProtectedNativeField {
		protected IntPtr Native;
	}

	public class HasInternalNativeField {
		internal IntPtr Native;
	}

	public class HasPublicReadonlyNativeField {
		public readonly IntPtr Native;
	}

	public class HasPublicNativeFieldArray {
		public IntPtr [] Native;
	}

	public class HasPublicReadonlyNativeFieldArray {
		public IntPtr [] Native;
	}

	public class HasPublicNativeFieldArrayArray {
		public IntPtr [] [] Native;
	}

	public class HasPublicNonNativeField {
		public object Field;
	}

	public class HasPrivateNativeField {
		private IntPtr Native;
	}

	[TestFixture]
	public class NativeFieldsShouldNotBeVisibleTest : TypeRuleTestFixture<NativeFieldsShouldNotBeVisibleRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
		}

		[Test]
		public void TestHasPublicNativeField ()
		{
			AssertRuleFailure<HasPublicNativeField> (1);
		}

		[Test]
		public void TestHasProtectedNativeField ()
		{
			AssertRuleFailure<HasProtectedNativeField> (1);
		}

		[Test]
		public void TestHasInternalNativeField ()
		{
			AssertRuleSuccess<HasInternalNativeField> ();
		}

		[Test]
		public void TestHasPublicReadonlyNativeField ()
		{
			AssertRuleSuccess<HasPublicReadonlyNativeField> ();
		}

		[Test]
		public void TestHasPublicNativeFieldArray ()
		{
			AssertRuleFailure<HasPublicNativeFieldArray> (1);
		}

		[Test]
		public void TestHasPublicReadonlyNativeFieldArray ()
		{
			AssertRuleFailure<HasPublicReadonlyNativeFieldArray> (1);
		}

		[Test]
		public void TestHasPublicNativeFieldArrayArray ()
		{
			AssertRuleFailure<HasPublicNativeFieldArrayArray> (1);
		}

		[Test]
		public void TestHasPublicNonNativeField ()
		{
			AssertRuleSuccess<HasPublicNonNativeField> ();
		}

		[Test]
		public void TestHasPrivateNativeField ()
		{
			AssertRuleSuccess<HasPrivateNativeField> ();
		}
	}
}
