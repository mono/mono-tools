//
// Unit tests for MathMinMaxCandidateTest
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Performance {

	[TestFixture]
	public class MathMinMaxCandidateTest : MethodRuleTestFixture <MathMinMaxCandidateRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no body
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no greater or lesser than
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		// Decimal is not a primitive type

		Decimal DecimalMinWithMax (Decimal d)
		{
			return (d < Decimal.MaxValue) ? d : Decimal.MaxValue;
		}

		static Decimal DecimalMaxWithMinusOne (Decimal d)
		{
			return (d >= Decimal.MinusOne) ? d : Decimal.MinusOne;
		}

		[Test]
		public void DecimalNotInlined ()
		{
			AssertRuleDoesNotApply<MathMinMaxCandidateTest> ("DecimalMinWithMax");
			AssertRuleDoesNotApply<MathMinMaxCandidateTest> ("DecimalMaxWithMinusOne");
		}

		byte a, b;

		int ManualIntMinMethod (int a, int b)
		{
			return (a > b) ? b : a;
		}

		ulong ManualULongMaxMethod (ulong a, ulong b)
		{
			return (a > b) ? a : b;
		}

		long ManualLongMinMethod (long a, long b)
		{
			return (a >= b) ? b : a;
		}

		uint ManualUIntMaxMethod (uint a, uint b)
		{
			return (a >= b) ? a : b;
		}

		[Test]
		public void MathMinMaxOverloaded ()
		{
			AssertRuleFailure<MathMinMaxCandidateTest> ("ManualIntMinMethod");
			AssertRuleFailure<MathMinMaxCandidateTest> ("ManualUIntMaxMethod");

			AssertRuleFailure<MathMinMaxCandidateTest> ("ManualLongMinMethod");
			AssertRuleFailure<MathMinMaxCandidateTest> ("ManualULongMaxMethod");

			AssertRuleFailure<MathMinMaxCandidateTest> ("UsingCustomMinWithByteFields");
		}

		private float f;
		float MinWithValue {
			get { return f; }
			set { 
				if (f < value)
					f = value;
			}
		}

		private double d;
		double MaxWithValue {
			get { return d; }
			set { 
				if (d >= value)
					d = value;
			}
		}

		[Test]
		public void Half ()
		{
			AssertRuleSuccess<MathMinMaxCandidateTest> ("set_MinWithValue");
			AssertRuleSuccess<MathMinMaxCandidateTest> ("set_MaxWithValue");
		}

		// removes compiler warning (and create test case)
		void SetFields (byte a, byte b)
		{
			this.a = a;
			this.b = b;
			selection_start = Math.Min (a, b);
			index = Math.Max (b, a);
			large_change = Math.Min (b, a);
			maximum = Math.Max (a, b);
		}

		[Test]
		public void UsingMathMinMax ()
		{
			AssertRuleDoesNotApply<MathMinMaxCandidateTest> ("SetFields");
		}

		void UsingCustomMinWithByteFields ()
		{
			byte c = 0;
			if (a > b) {
				c = b;
			} else {
				c = a;
			}
			Console.WriteLine (c);
		}

		private int selection_start;
		private int index;
		private void DoMoreStuffInsideCondition ()
		{
			if (index >= selection_start) {
				Start = selection_start;
				End = index;
			} else {
				Start = index;
				End = selection_start;
			}
		}

		// from mcs/class/System/System.Net/CookieContainer.cs
		public int Capacity {
			get { return End; }
			set { 
				if (value < 0 || (value < End && End != Int32.MaxValue))
					throw new ArgumentOutOfRangeException ("value");

				if (value < Start)
					Start = value;

				End = value;							
			}
		}

		[Test]
		public void LookAlike ()
		{
			AssertRuleSuccess<MathMinMaxCandidateTest> ("DoMoreStuffInsideCondition");
			AssertRuleSuccess<MathMinMaxCandidateTest> ("set_Capacity");
		}

		int ManualMixedMin (char a, int b)
		{
			return (a >= b) ? a : b;
		}

		char ManualCharMax (char a, char b)
		{
			return (a >= b) ? a : b;
		}

		[Test]
		public void NonOverloadedTypes ()
		{
			AssertRuleSuccess<MathMinMaxCandidateTest> ("ManualMixedMin");
			AssertRuleSuccess<MathMinMaxCandidateTest> ("ManualCharMax");
		}

		// from mcs/class/Managed.Windows.Forms/System.Windows.Forms.Theming/Default/CheckBoxPainter.cs
		private int Clamp (int value, int lower, int upper)
		{
			if (value < lower)
				return lower;
			else if (value > upper)
				return upper;
			else
				return value;
		}

		[Test]
		public void ClampValue ()
		{
			AssertRuleFailure<MathMinMaxCandidateTest> ("Clamp");
		}

		// from mcs/class/System/System.Text.RegularExpressions/compiler.cs
		private int Start;
		private int End;
		public int Length {
			get { return Start < End ? End - Start : Start - End; }
		}

		[Test]
		public void OperationsAfterBranch_BothSize ()
		{
			AssertRuleSuccess<MathMinMaxCandidateTest> ("get_Length");
		}

		// from mcs/class/Managed.Windows.Forms/System.Windows.Forms/ScrollBar.cs
		private int large_change;
		private int maximum;
		public int LargeChange {
			get {
				if (large_change > maximum)
					return (maximum + 1);
				else
					return large_change;
			}
		}

		[Test]
		public void OperationsAfterBranch_OneSize ()
		{
			AssertRuleSuccess<MathMinMaxCandidateTest> ("get_LargeChange");
		}

		void MinOut (short a, short b, out short c)
		{
			c = (a > b) ? b : a;
		}

		void MaxRef (ref ushort a, ushort b)
		{
			a = (a > b) ? a : b;
		}

		void Out (out int a)
		{
			a = (this.a > b) ? Start : End;
		}

		void Ref (ref int b)
		{
			b = (a < b) ? End : Start;
		}

		[Test]
		public void SpecialParameters ()
		{
			AssertRuleFailure<MathMinMaxCandidateTest> ("MinOut");
			AssertRuleFailure<MathMinMaxCandidateTest> ("MaxRef");

			AssertRuleSuccess<MathMinMaxCandidateTest> ("Out");
			AssertRuleSuccess<MathMinMaxCandidateTest> ("Ref");
		}

		sbyte MinArray (sbyte [] array)
		{
			// small index (specialized opcodes)
			return (array [0] <= array [1]) ? array [0] : array [1];
		}

		void MaxArray (ulong [] array)
		{
			// large index (general opcodes)
			array [12] = (array [10] >= array [11]) ? array [10] : array [11];
		}

		uint Array (uint [] array)
		{
			return (array [0] >= array [1]) ? array [2] : array [3];
		}

		[Test]
		[Ignore ("Rule does not consider arrays - need tests to see if the JIT does better than manual inlining")]
		public void Arrays ()
		{
			AssertRuleSuccess<MathMinMaxCandidateTest> ("Array");

			AssertRuleFailure<MathMinMaxCandidateTest> ("MinArray");
			AssertRuleFailure<MathMinMaxCandidateTest> ("MaxArray");
		}

		static double Minimum (double d, float f)
		{
			return (d < f) ? d : f;
		}

		static double Maximum (double d, float f)
		{
			return (d >= f) ? f : d;
		}

		[Test]
		public void Static ()
		{
			AssertRuleFailure<MathMinMaxCandidateTest> ("Minimum");
			AssertRuleFailure<MathMinMaxCandidateTest> ("Maximum");
		}
	}
}
