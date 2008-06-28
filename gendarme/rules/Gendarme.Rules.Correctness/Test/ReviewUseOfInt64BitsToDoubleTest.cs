//
// Unit test for ReviewUseOfInt64BitsToDoubleRule
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
	public class ReviewUseOfInt64BitsToDoubleTest : MethodRuleTestFixture<ReviewUseOfInt64BitsToDoubleRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		public double ParametersBad (int x, int y)
		{
			return BitConverter.Int64BitsToDouble (x) + BitConverter.Int64BitsToDouble (y);
		}

		static short StaticShort = 16;

		public double StaticFieldBad ()
		{
			return BitConverter.Int64BitsToDouble (StaticShort);
		}

		byte InstanceByte;
		double InstanceDouble;

		public double InstanceFieldBad ()
		{
			return InstanceDouble + BitConverter.Int64BitsToDouble (InstanceByte);
		}

		public double LocalVariableBad ()
		{
			int x = 42;
			return BitConverter.Int64BitsToDouble (x);
		}

		public double CallBad (double d)
		{
			return BitConverter.Int64BitsToDouble (d.GetHashCode ());
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ReviewUseOfInt64BitsToDoubleTest> ("ParametersBad", 2);
			AssertRuleFailure<ReviewUseOfInt64BitsToDoubleTest> ("StaticFieldBad", 1);
			AssertRuleFailure<ReviewUseOfInt64BitsToDoubleTest> ("InstanceFieldBad", 1);
			AssertRuleFailure<ReviewUseOfInt64BitsToDoubleTest> ("LocalVariableBad", 1);
			AssertRuleFailure<ReviewUseOfInt64BitsToDoubleTest> ("CallBad", 1);
		}

		public double ParametersGood (long x, long y)
		{
			return BitConverter.Int64BitsToDouble (x) + BitConverter.Int64BitsToDouble (y);
		}

		static ulong StaticULong = 16;

		public double StaticFieldGood ()
		{
			return BitConverter.Int64BitsToDouble ((long)StaticULong);
		}

		long InstanceLong;

		public double InstanceFieldGood ()
		{
			return InstanceDouble + BitConverter.Int64BitsToDouble (InstanceLong);
		}

		public double LocalVariableGood ()
		{
			long x = 42;
			return BitConverter.Int64BitsToDouble (x);
		}

		public double CallGood (long x)
		{
			return BitConverter.Int64BitsToDouble (x);
		}

		[Test]
		public void Ok ()
		{
			AssertRuleSuccess<ReviewUseOfInt64BitsToDoubleTest> ("ParametersGood");
			AssertRuleSuccess<ReviewUseOfInt64BitsToDoubleTest> ("StaticFieldGood");
			AssertRuleSuccess<ReviewUseOfInt64BitsToDoubleTest> ("InstanceFieldGood");
			AssertRuleSuccess<ReviewUseOfInt64BitsToDoubleTest> ("LocalVariableGood");
			AssertRuleSuccess<ReviewUseOfInt64BitsToDoubleTest> ("CallGood");
		}
	}
}
