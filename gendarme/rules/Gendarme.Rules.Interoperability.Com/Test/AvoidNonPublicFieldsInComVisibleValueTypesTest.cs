//
// Unit Tests for AvoidNonpublicFieldsInComVisibleValueTypesRule
//
// Authors:
//	N Lum <nol888@gmail.com>
//
// Copyright (C) 2010 N Lum
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

using System.Runtime.InteropServices;

using Gendarme.Rules.Interoperability.Com;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Interoperability.Com {

	/** Pass */
	[ComVisible (true)]
	public struct ComVisibleGood {
		public int SomeValue;
	}

	/** Do not apply */
	[ComVisible (true)]
	public struct ComVisibleTypeNoFields {
	}

	[ComVisible (false)]
	public struct NonComVisibleType {
		public int SomeValue;
	}

	public struct NoDefinedComVisible {
		public int SomeValue;
	}

	[ComVisible (true)]
	public class ComVisibleRefType {
		public int SomeValue;
	}

	/** Fail */
	[ComVisible (true)]
	public struct ComVisibleBad {
		internal int SomeValue;
	}

	[TestFixture]
	public class AvoidNonPublicFieldsInComVisibleValueTypesTest : TypeRuleTestFixture<AvoidNonPublicFieldsInComVisibleValueTypesRule> {

		[Test]
		public void DoesNotApply ()
		{
			// Reference type.
			AssertRuleDoesNotApply<ComVisibleRefType> ();

			// Type without ComVisible.
			AssertRuleDoesNotApply<NoDefinedComVisible> ();

			// Type with no fields.
			AssertRuleDoesNotApply<ComVisibleTypeNoFields> ();

			// Type with ComVisible = false.
			AssertRuleDoesNotApply<NonComVisibleType> ();
		}

		[Test]
		public void Good ()
		{
			// Type with explicit ComVisible and only public fields.
			AssertRuleSuccess<ComVisibleGood> ();
		}

		[Test]
		public void Bad ()
		{
			// Type with explicit ComVisible and an internal field.
			AssertRuleFailure<ComVisibleBad> (1);
		}
	}
}

