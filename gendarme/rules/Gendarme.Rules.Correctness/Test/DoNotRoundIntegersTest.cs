//
// Unit test for DoNotRoundIntegersRule
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

using Gendarme.Framework;
using Gendarme.Rules.Correctness;

using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class DoNotRoundIntegersTest : MethodRuleTestFixture<DoNotRoundIntegersRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		// [decimal|double] Ceiling ([decimal|double])

		public decimal CeilingIntegerDecimal (int x)
		{
			return Math.Ceiling ((decimal) x);
		}

		public double CeilingIntegerDouble (int x)
		{
			return Math.Ceiling ((double) x);
		}

		public decimal CeilingFloatDecimal (float f)
		{
			return Math.Ceiling ((decimal) f);
		}

		public double CeilingFloatDouble (float f)
		{
			return Math.Ceiling (f);
		}

		public decimal CeilingDoubleDecimal (double d)
		{
			return Math.Ceiling ((decimal) d);
		}

		public double CeilingDoubleDouble (double d)
		{
			return Math.Ceiling ((double) d);
		}

		[Test]
		public void Ceiling ()
		{
			Assert.AreEqual (1d, Math.Ceiling ((double) 1), "Ceiling(double)");
			Assert.AreEqual (1m, Math.Ceiling ((decimal) 1), "Ceiling(decimal)");

			AssertRuleFailure<DoNotRoundIntegersTest> ("CeilingIntegerDecimal", 1);
			AssertRuleFailure<DoNotRoundIntegersTest> ("CeilingIntegerDouble", 1);

			AssertRuleSuccess<DoNotRoundIntegersTest> ("CeilingFloatDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("CeilingFloatDouble");

			AssertRuleSuccess<DoNotRoundIntegersTest> ("CeilingDoubleDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("CeilingDoubleDouble");
		}

		// [decimal|double] Floor ([decimal|double])

		private int x = 1;
		private double d = 3.14d;
		private float f = 42f;

		public decimal FloorIntegerDecimal ()
		{
			return Math.Floor ((decimal) x);
		}

		public double FloorIntegerDouble ()
		{
			return Math.Floor ((double) x);
		}

		public decimal FloorFloatDecimal ()
		{
			return Math.Floor ((decimal) f);
		}

		public double FloorFloatDouble ()
		{
			return Math.Floor (f);
		}

		public decimal FloorDoubleDecimal ()
		{
			return Math.Floor ((decimal) d);
		}

		public double FloorDoubleDouble ()
		{
			return Math.Floor ((double) d);
		}

		[Test]
		public void Floor ()
		{
			Assert.AreEqual (1d, Math.Floor ((double) 1), "Floor(double)");
			Assert.AreEqual (1m, Math.Floor ((decimal) 1), "Floor(decimal)");

			AssertRuleFailure<DoNotRoundIntegersTest> ("FloorIntegerDecimal", 1);
			AssertRuleFailure<DoNotRoundIntegersTest> ("FloorIntegerDouble", 1);

			AssertRuleSuccess<DoNotRoundIntegersTest> ("FloorFloatDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("FloorFloatDouble");

			AssertRuleSuccess<DoNotRoundIntegersTest> ("FloorDoubleDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("FloorDoubleDouble");
		}

		// [decimal | double] Round ([decimal | double] [,Int32] [,MidpointRounding])

		static int sx = 1;
		static float sf = 42f;
		static double sd = 3.14d;

		public static decimal RoundIntegerDecimal ()
		{
			return Math.Round ((decimal) sx, 2);
		}

		public static double RoundIntegerDouble ()
		{
			return Math.Round ((double) sx, MidpointRounding.AwayFromZero);
		}

		public static decimal RoundFloatDecimal ()
		{
			return Math.Round ((decimal) sf, 1, MidpointRounding.ToEven);
		}

		public static double RoundFloatDouble ()
		{
			return Math.Round (sf, 0);
		}

		public static decimal RoundDoubleDecimal ()
		{
			return Math.Round ((decimal) sd, 2, MidpointRounding.AwayFromZero);
		}

		public static double RoundDoubleDouble ()
		{
			return Math.Round ((double) sd, 1, MidpointRounding.ToEven);
		}

		[Test]
		public void Round ()
		{
			Assert.AreEqual (1d, Math.Round ((double) 1), "Round(double)");
			Assert.AreEqual (1m, Math.Round ((decimal) 1), "Round(decimal)");

			AssertRuleFailure<DoNotRoundIntegersTest> ("RoundIntegerDecimal", 1);
			AssertRuleFailure<DoNotRoundIntegersTest> ("RoundIntegerDouble", 1);

			AssertRuleSuccess<DoNotRoundIntegersTest> ("RoundFloatDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("RoundFloatDouble");

			AssertRuleSuccess<DoNotRoundIntegersTest> ("RoundDoubleDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("RoundDoubleDouble");
		}

		// [decimal|double] Truncate ([decimal|double])

		public decimal TruncateIntegerDecimal ()
		{
			int x = 1;
			return Math.Truncate ((decimal) x);
		}

		public double TruncateIntegerDouble ()
		{
			int x = 1;
			return Math.Truncate ((double) x);
		}

		public decimal TruncateFloatDecimal ()
		{
			float f = 42f;
			return Math.Truncate ((decimal) f);
		}

		public double TruncateFloatDouble ()
		{
			float f = 42f;
			return Math.Truncate (f);
		}

		public decimal TruncateDoubleDecimal ()
		{
			double d = 3.14d;
			return Math.Truncate ((decimal) d);
		}

		public double TruncateDoubleDouble ()
		{
			double d = 3.14d;
			return Math.Truncate ((double) d);
		}

		[Test]
		public void Truncate ()
		{
			Assert.AreEqual (1d, Math.Truncate ((double) 1), "Truncate(double)");
			Assert.AreEqual (1m, Math.Truncate ((decimal) 1), "Truncate(decimal)");

			AssertRuleFailure<DoNotRoundIntegersTest> ("TruncateIntegerDecimal", 1);
			AssertRuleFailure<DoNotRoundIntegersTest> ("TruncateIntegerDouble", 1);

			AssertRuleSuccess<DoNotRoundIntegersTest> ("TruncateFloatDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("TruncateFloatDouble");

			AssertRuleSuccess<DoNotRoundIntegersTest> ("TruncateDoubleDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("TruncateDoubleDouble");
		}

		public double UnsignedLong (ulong l)
		{
			return Math.Ceiling ((double) l);
		}

		public double ChainAllByte (byte small)
		{
			double a = Math.Truncate (Math.Round (Math.Floor (Math.Ceiling ((double) small))));
			double b = Math.Round (Math.Floor (Math.Ceiling (Math.Truncate ((double) small))));
			double c = Math.Floor (Math.Ceiling (Math.Truncate (Math.Round ((double) small))));
			double d = Math.Ceiling (Math.Truncate (Math.Round (Math.Floor ((double) small))));
			return a + b + c + d;
		}

		public double ChainFloatingPoint (string s)
		{
			return Math.Floor (Double.Parse (s));
		}

		public decimal ChainDecimal (string s)
		{
			return Math.Round (Decimal.Parse (s), 2);
		}

		public decimal ChainChainDecimal (string s)
		{
			return Math.Ceiling (ChainDecimal (s));
		}

		[Test]
		public void Others ()
		{
			Assert.AreEqual (1.0, UnsignedLong (1), "UnsignedLong");
			AssertRuleFailure<DoNotRoundIntegersTest> ("UnsignedLong", 1);

			Assert.AreEqual (4.0, ChainAllByte (1), "ChainAllByte");
			AssertRuleFailure<DoNotRoundIntegersTest> ("ChainAllByte", 4);

			Assert.AreEqual (3.0, ChainFloatingPoint ((3.14).ToString ()), "ChainFloatingPoint");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("ChainFloatingPoint");

			Assert.AreEqual (3.14m, ChainDecimal ((3.1415).ToString ()), "ChainDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("ChainDecimal");

			Assert.AreEqual (4m, ChainChainDecimal ((3.1415).ToString ()), "ChainChainDecimal");
			AssertRuleSuccess<DoNotRoundIntegersTest> ("ChainChainDecimal");
		}

		// test case provided by Richard Birkby
		internal sealed class FalsePositive8 {

			public decimal Run ()
			{
				GetType ();
				int x = 5;
				return Math.Round (x * 0.5M);
			}
		}

		[Test]
		public void DecimalResult ()
		{
			AssertRuleSuccess<FalsePositive8> ("Run");
		}
	}
}
