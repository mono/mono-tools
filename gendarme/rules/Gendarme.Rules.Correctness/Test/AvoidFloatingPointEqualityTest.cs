//
// Unit test for AvoidFloatingPointEqualityRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
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

#pragma warning disable 168, 219

	[TestFixture]
	public class AvoidFloatingPointEqualityTest : MethodRuleTestFixture<AvoidFloatingPointEqualityRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply<float> ();
			AssertRuleDoesNotApply<double> ();
		}

		public class Double {
			public double field = 0.0d;
			public static double staticField = 0.0d;

			public double Property {
				get { return field; }
			}

			public static double StaticMethod ()
			{
				return 0.0d;
			}

			public void Fields ()
			{
				bool a = field == staticField;
				bool b = staticField == field;
			}

			public bool MethodResult ()
			{
				bool a = Property == StaticMethod ();
				bool b = StaticMethod () == Property;
				return a && b;
			}

			public void Parameters (double f, double g)
			{
				bool a = f == g;
			}

			public bool StaticParameters (double f, double g)
			{
				return f == g;
			}

			public void Local ()
			{
				double d1 = 0.0d;
				double d2 = 0.0d;
				if (d1 == d2) {
					int i = 1;
					Console.WriteLine (d1 == (double)i);
				} else if (d2 == d1) {
					uint i = 1;
					Console.WriteLine ((double)i == d2);
				}
			}

			public void LocalAbove4 ()
			{
				string s1 = String.Empty;
				string s2 = String.Empty;
				string s3 = String.Empty;
				string s4 = String.Empty;
				double d1 = 0.0d;
				double d2 = 0.0d;
				if (d1 == d2) {
					Console.WriteLine (d1 == ((double)1));
				} else if (d2 == d1) {
					Console.WriteLine (((double)0) == d2);
				}
			}

			public void ComparisonAfterArithmeticOperation ()
			{
				double d1 = 0.0d;
				double d2 = 0.0d;
				bool a = d1 * 1 == d2 * 1;
			}

			public void LegalComparisons ()
			{
				double d = 0.0d;
				bool a;
				bool b;

				//a = float.PositiveInfinity == f;
				b = d == double.PositiveInfinity;

				//a = float.NegativeInfinity == f;
				b = d == double.NegativeInfinity;
			}

			public void EqualsCall ()
			{
				double d1 = 0.0d;
				double d2 = 0.0d;
				if (d1.Equals (d2)) {
					d1.Equals (1.0);
				} else {
					(1.0d).Equals (d2);
				}
			}

			public bool CompareWithArray ()
			{
				double [] values = new double [] { 0.0d, 1.0d };
				return values [0] == values [1];
			}
		}

		public class Float {

			public float field = 0f;
			public static float staticField = 0f;

			public float Property {
				get { return field; }
			}

			public static float StaticMethod ()
			{
				return 0f;
			}

			public void Fields ()
			{
				bool a = field == staticField;
				bool b = staticField == field;
			}

			public bool MethodResult ()
			{
				bool a = Property == StaticMethod();
				bool b = StaticMethod() == Property;
				return a && b;
			}

			public void Parameters (float f, float g)
			{
				bool a = f == g;
			}

			public bool StaticParameters (float f, float g)
			{
				return f == g;
			}

			public void Local ()
			{
				float f1 = 0f;
				float f2 = 0f;
				if (f1 == f2) {
					int i = 1;
					Console.WriteLine (f1 == (float) i);
				} else if (f2 == f1) {
					uint i = 1;
					Console.WriteLine ((float) i == f2);
				}
			}

			public void LocalAbove4 ()
			{
				string s1 = String.Empty;
				string s2 = String.Empty;
				string s3 = String.Empty;
				string s4 = String.Empty;
				float f1 = 0f;
				float f2 = 0f;
				if (f1 == f2) {
					Console.WriteLine (f1 == ((float) 1));
				} else if (f2 == f1) {
					Console.WriteLine (((float) 0) == f2);
				}
			}

			public void ComparisonAfterArithmeticOperation ()
			{
				float f1 = 0f;
				float f2 = 0f;
				bool a = f1 * 1 == f2 * 1;
			}

			public void LegalComparisons ()
			{
				float f = 0f;
				bool a;
				bool b;
				
				//a = float.PositiveInfinity == f;
				b = f == float.PositiveInfinity;
				
				//a = float.NegativeInfinity == f;
				b = f == float.NegativeInfinity;
			}

			public void EqualsCall ()
			{
				float f1 = 0f;
				float f2 = 0f;
				bool a = f1.Equals(f2);
			}

			public bool CompareWithArray ()
			{
				float [] values = new float [] { 0.0f, 1.0f };
				return values [0] == values [1];
			}
		}

		public class Legal {

			object o = null;

			public void IntegerComparisons ()
			{
				int i = 0;
				int j = 1;
				bool a = i == j;
			}

			public bool NoComparison ()
			{
				return false;
			}

			public bool Property {
				get {
					if (o != null)
						return ((object)o.ToString () == this);
					else
						return false;
				}
			}
		}

		[Test]
		public void TestFields ()
		{
			AssertRuleFailure<Float> ("Fields", 2);
			AssertRuleFailure<Double> ("Fields", 2);
		}

		[Test]
		public void TestMethodResult ()
		{
			AssertRuleFailure<Float> ("MethodResult", 2);
			AssertRuleFailure<Double> ("MethodResult", 2);
		}
		
		[Test]
		public void TestParameters ()
		{
			AssertRuleFailure<Float> ("Parameters", 1);
			AssertRuleFailure<Double> ("Parameters", 1);
		}

		[Test]
		public void TestStaticParameters ()
		{
			AssertRuleFailure<Float> ("StaticParameters", 1);
			AssertRuleFailure<Double> ("StaticParameters", 1);
		}

		[Test]
		public void TestLocal ()
		{
			AssertRuleFailure<Float> ("Local");
			AssertRuleFailure<Double> ("Local");
		}

		[Test]
		public void TestLocalAbove4 ()
		{
			AssertRuleFailure<Float> ("LocalAbove4");
			AssertRuleFailure<Double> ("LocalAbove4");
		}

		[Test]
		public void TestComparisonAfterArithmeticOperation ()
		{
			AssertRuleFailure<Float> ("ComparisonAfterArithmeticOperation", 1);
			AssertRuleFailure<Double> ("ComparisonAfterArithmeticOperation", 1);
		}

		[Test]
		public void TestLegalComparisons ()
		{
			AssertRuleSuccess<Float> ("LegalComparisons");
			AssertRuleSuccess<Double> ("LegalComparisons");
		}

		[Test]
		public void TestLegalIntegerComparisons ()
		{
			AssertRuleSuccess<Legal> ("IntegerComparisons");
		}

		[Test]
		public void TestEqualsCall ()
		{
			AssertRuleFailure<Float> ("EqualsCall", 1);
			AssertRuleFailure<Double> ("EqualsCall", 3);
		}
		
		[Test]
		public void TestNoFloatComparison()
		{
			AssertRuleDoesNotApply<Legal> ("NoComparison");
		}

		[Test]
		public void TestCompareWithArray ()
		{
			AssertRuleFailure<Float> ("CompareWithArray", 1);
			AssertRuleFailure<Double> ("CompareWithArray", 1);
		}

		[Test]
		public void TestLegalStuff ()
		{
			AssertRuleSuccess<Legal> ("get_Property");
		}
	}
}
