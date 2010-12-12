//
// Unit Tests for ComVisibleTypesShouldBeCreatableRule
//
// Authors:
//	Nicholas Rioux
//
// Copyright (C) 2010 Nicholas Rioux
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
using Test.Rules.Definitions;

namespace Test.Rules.Interoperability.Com {

	/** pass */
	[ComVisible (true)]
	public class ComVisibleCtorsGood {
		public ComVisibleCtorsGood ()
		{
		}
		public ComVisibleCtorsGood (int param)
		{
		}
	}

	[ComVisible (true)]
	public class PrivateCtor {
		private PrivateCtor (int param)
		{
		}
	}

	/** don't apply */
	[ComVisible (true)]
	public struct ComVisibleValueType {
		public ComVisibleValueType (int param)
		{
		}
	}

	[ComVisible (false)]
	public class NotComVisible {
		public NotComVisible(int param)
		{
		}
	}

	/** fail */
	[ComVisible (true)]
	public class ParameterizedCtorBad {
		public ParameterizedCtorBad (int param)
		{
		}
	}

	[TestFixture]
	public class ComVisibleTypesShouldBeCreatableTest : TypeRuleTestFixture<ComVisibleTypesShouldBeCreatableRule> {

		[Test]
		public void DoesNotApply ()
		{
			// Value type with a parameterized constructor.
			AssertRuleDoesNotApply<ComVisibleValueType> ();

			// Type with ComVisible set to false.
			AssertRuleDoesNotApply<NotComVisible> ();

			// ComVisible must be explicit.
			AssertRuleDoesNotApply (SimpleTypes.Class);

			// Rule only applies to classes.
			AssertRuleDoesNotApply (SimpleTypes.Structure);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		[Test]
		public void Good ()
		{
			// Type with explicit ComVisible, a default constructor, and a parameterized constructor.
			AssertRuleSuccess<ComVisibleCtorsGood> ();

			// Type with explicit ComVisible but no public constructors.
			AssertRuleSuccess<PrivateCtor> ();
		}

		[Test]
		public void Bad ()
		{
			// Type with explicit ComVisible and a public parameterized constructor.
			AssertRuleFailure<ParameterizedCtorBad> (1);
		}
	}
}

