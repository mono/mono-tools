//
// Unit test for ReviewUseOfModuloOneOnIntegersRule
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
	public class ReviewUseOfModuloOneOnIntegersTest : MethodRuleTestFixture<ReviewUseOfModuloOneOnIntegersRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		public int ModuloOne (int i)
		{
			return (i % 1);
			// LDC.I4.1 + REM
		}

		public int ModuloTwo (int i)
		{
			return (i % 2);
		}

		public int Modulo (int i, int m)
		{
			return (i % m);
		}

		public uint ModuloOneUnsigned (uint i)
		{
			return (i % 1);
			// LDC.I4.1 + REM
		}

		public uint ModuloTwoUnsigned (uint i)
		{
			return (i % 2);
		}

		public uint ModuloUnsigned (uint i, uint m)
		{
			return (i % m);
		}

		[Test]
		public void LocalVariables ()
		{
			AssertRuleFailure<ReviewUseOfModuloOneOnIntegersTest> ("ModuloOne", 1);
			AssertRuleSuccess<ReviewUseOfModuloOneOnIntegersTest> ("ModuloTwo");
			AssertRuleSuccess<ReviewUseOfModuloOneOnIntegersTest> ("Modulo");

			AssertRuleFailure<ReviewUseOfModuloOneOnIntegersTest> ("ModuloOneUnsigned", 1);
			AssertRuleSuccess<ReviewUseOfModuloOneOnIntegersTest> ("ModuloTwoUnsigned");
			AssertRuleSuccess<ReviewUseOfModuloOneOnIntegersTest> ("ModuloUnsigned");
		}

		private const long LongOne = 1;

		public long TwoModuloOneConst (long i)
		{
			return ((i % LongOne) % 1);
			// LDC.I4.1 + CONV.I8 + REM
		}

		[Test]
		public void ConstantAndVariable ()
		{
			AssertRuleFailure<ReviewUseOfModuloOneOnIntegersTest> ("TwoModuloOneConst", 2);
		}

		// the value is set inside the class constructor
		protected readonly ulong ULongOne = 1;

		public ulong ModuloOneReadOnly (ulong i)
		{
			return (i % ULongOne);
		}

		[Test]
		public void ReadOnly ()
		{
			AssertRuleSuccess<ReviewUseOfModuloOneOnIntegersTest> ("ModuloOneReadOnly");
		}

		public byte ModuloByte (byte i)
		{
			return (byte) (i % 1);
		}

		public sbyte ModuloSByte (sbyte i)
		{
			return (sbyte) (i % 1);
		}

		public double ModuloOneDouble (double d)
		{
			return (d % 1.0d);
			// LDC.R8 1 + REM
		}

		public float ModuloOneFloat (float f)
		{
			return (f % 1.0f);
			// LDC.R4 1 + REM
		}

		[Test]
		public void FloatingPoint ()
		{
			AssertRuleSuccess<ReviewUseOfModuloOneOnIntegersTest> ("ModuloOneDouble");
			AssertRuleSuccess<ReviewUseOfModuloOneOnIntegersTest> ("ModuloOneFloat");
		}

		public Decimal ModuloOneDecimal (decimal d)
		{
			return ((d % 1.0m) % 1m);
		}

		public Decimal ModuloOneDecimalConst (decimal d)
		{
			return (d % Decimal.One);
		}

		[Test]
		public void Decimals ()
		{
			// decimals needs a method call - i.e. it's not handled by Rem[_Un] IL instructions
			AssertRuleDoesNotApply<ReviewUseOfModuloOneOnIntegersTest> ("ModuloOneDecimal");
			AssertRuleDoesNotApply<ReviewUseOfModuloOneOnIntegersTest> ("ModuloOneDecimalConst");
		}
	}
}
