//
// Unit test for DoNotCompareWithNaNRule
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

using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class DoNotCompareWithNaNTest : MethodRuleTestFixture<DoNotCompareWithNaNRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply<float> ();
			AssertRuleDoesNotApply<double> ();
		}

		public class SingleCases {

			public bool EqualityOperatorLeft (float a)
			{
				return (Single.NaN == a);
			}

			public bool EqualityOperatorRight (float a)
			{
				return (a == Single.NaN);
			}

			public bool Equality (float a, float b)
			{
				// note: ok for this rule (not for another one)
				return (a == b);
			}

			public bool InequalityOperatorLeft (float a)
			{
				return (Single.NaN != a);
			}

			public bool InequalityOperatorRight (float a)
			{
				return (a != Single.NaN);
			}

			public bool Inequality (float a, float b)
			{
				// note: ok for this rule (not for another one)
				return (a != b);
			}

			public bool NaNEquals (float a)
			{
				return Single.NaN.Equals (a);
			}

			public bool EqualsNaN (float a)
			{
				return a.Equals (Single.NaN);
			}

			public bool Equals (float a, float b)
			{
				// note: ok for this rule (not for another one)
				return (a.Equals (b) && b.Equals (a));
			}
		}

		public class DoubleCases {

			public bool EqualityOperatorLeft (double a)
			{
				return (Double.NaN == a);
			}

			public bool EqualityOperatorRight (double a)
			{
				return (a == Double.NaN);
			}

			public bool Equality (double a, double b)
			{
				// note: ok for this rule (not for another one)
				return (a == b);
			}

			public bool InequalityOperatorLeft (double a)
			{
				return (Double.NaN != a);
			}

			public bool InequalityOperatorRight (double a)
			{
				return (a != Double.NaN);
			}

			public bool Inequality (double a, double b)
			{
				// note: ok for this rule (not for another one)
				return (a != b);
			}

			public bool NaNEquals (double a)
			{
				return Double.NaN.Equals (a);
			}

			public bool EqualsNaN (double a)
			{
				return a.Equals (Double.NaN);
			}

			public bool Equals (double a, double b)
			{
				// note: ok for this rule (not for another one)
				return (a.Equals (b) && b.Equals (a));
			}
		}

		[Test]
		public void EqualityOperator ()
		{
			AssertRuleFailure<SingleCases> ("EqualityOperatorLeft", 1);
			AssertRuleFailure<SingleCases> ("EqualityOperatorRight", 1);
			// no LDC_R[4|8]
			AssertRuleDoesNotApply<SingleCases> ("Equality");

			AssertRuleFailure<DoubleCases> ("EqualityOperatorLeft", 1);
			AssertRuleFailure<DoubleCases> ("EqualityOperatorRight", 1);
			// no LDC_R[4|8]
			AssertRuleDoesNotApply<DoubleCases> ("Equality");
		}

		[Test]
		public void InequalityOperator ()
		{
			AssertRuleFailure<SingleCases> ("InequalityOperatorLeft", 1);
			AssertRuleFailure<SingleCases> ("InequalityOperatorRight", 1);
			// no LDC_R[4|8]
			AssertRuleDoesNotApply<SingleCases> ("Inequality");

			AssertRuleFailure<DoubleCases> ("InequalityOperatorLeft", 1);
			AssertRuleFailure<DoubleCases> ("InequalityOperatorRight", 1);
			// no LDC_R[4|8]
			AssertRuleDoesNotApply<DoubleCases> ("Inequality");
		}

		[Test]
		public void NaNEquals ()
		{
			AssertRuleFailure<SingleCases> ("NaNEquals", 1);
			AssertRuleFailure<DoubleCases> ("NaNEquals", 1);
		}

		[Test]
		public void EqualsNaN ()
		{
			AssertRuleFailure<SingleCases> ("EqualsNaN", 1);
			AssertRuleFailure<DoubleCases> ("EqualsNaN", 1);
		}

		[Test]
		public void Equals ()
		{
			AssertRuleDoesNotApply<SingleCases> ("Equals");
			AssertRuleDoesNotApply<DoubleCases> ("Equals");
		}
	}
}
