//
// Unit tests for AvoidLargeNumberOfLocalVariablesRule
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
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace System.Windows.Forms {
	public class Form {
		public void InitializeComponent ()
		{
		}
	}
}

namespace Test.Rules.Performance {

	[TestFixture]
	public class AvoidLargeNumberOfLocalVariablesTest : MethodRuleTestFixture<AvoidLargeNumberOfLocalVariablesRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply<System.Windows.Forms.Form> ("InitializeComponent");
		}

		private void SmallMethod (int a)
		{
			int x = a * a - a;
			Console.WriteLine (x * x);
		}

		[Test]
		public void Small ()
		{
			AssertRuleSuccess<AvoidLargeNumberOfLocalVariablesTest> ("SmallMethod");
			AssertRuleSuccess<AvoidLargeNumberOfLocalVariablesTest> ("WriteLine");
		}

		private void LargeMethod ()
		{
			byte b1 = 1, b2 = 2, b3 = 3, b4 = 4, b5 = 5, b6 = 6, b7 = 7, b8 = 8;
			sbyte sb1 = 1, sb2 = 2, sb3 = 3, sb4 = 4, sb5 = 5, sb6 = 6, sb7 = 7, sb8 = 8;
			short s1 = 1, s2 = 2, s3 = 3, s4 = 4, s5 = 5, s6 = 6, s7 = 7, s8 = 8;
			ushort us1 = 1, us2 = 2, us3 = 3, us4 = 4, us5 = 5, us6 = 6, us7 = 7, us8 = 8;
			int i1 = 1, i2 = 2, i3 = 3, i4 = 4, i5 = 5, i6 = 6, i7 = 7, i8 = 8;
			uint ui1 = 1, ui2 = 2, ui3 = 3, ui4 = 4, ui5 = 5, ui6 = 6, ui7 = 7, ui8 = 8;
			long l1 = 1, l2 = 2, l3 = 3, l4 = 4, l5 = 5, l6 = 6, l7 = 7, l8 = 8;
			ulong ul1 = 1, ul2 = 2, ul3 = 3, ul4 = 4, ul5 = 5, ul6 = 6, ul7 = 7, ul8 = 8;
			WriteLine (b1, sb1, s1, us1, i1, ui1, l1, ul1);
			WriteLine (b2, sb2, s2, us2, i2, ui2, l2, ul2);
			WriteLine (b3, sb3, s3, us3, i3, ui3, l3, ul3);
			WriteLine (b4, sb4, s4, us4, i4, ui4, l4, ul4);
			WriteLine (b5, sb5, s5, us5, i5, ui5, l5, ul5);
			WriteLine (b6, sb6, s6, us6, i6, ui6, l6, ul6);
			WriteLine (b7, sb7, s7, us7, i7, ui7, l7, ul7);
			WriteLine (b8, sb8, s8, us8, i8, ui8, l8, ul8);
		}

		// CSC creates an array 64 user variables + 1 compiler variable for the array (CWL)
		// so we need to move the CWL elsewhere for the test
		private void WriteLine (byte b, sbyte sb, short s, ushort us, int i, uint ui, long l, ulong ul)
		{
			Console.WriteLine ("{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}", b, sb, s, us, i, ui, l, ul);
		}

		[Test]
		public void Large ()
		{
			// 64 variables
			AssertRuleSuccess<AvoidLargeNumberOfLocalVariablesTest> ("LargeMethod");
		}

		private void TooLargeMethod ()
		{
			byte b1 = 1, b2 = 2, b3 = 3, b4 = 4, b5 = 5, b6 = 6, b7 = 7, b8 = 8;
			sbyte sb1 = 1, sb2 = 2, sb3 = 3, sb4 = 4, sb5 = 5, sb6 = 6, sb7 = 7, sb8 = 8;
			short s1 = 1, s2 = 2, s3 = 3, s4 = 4, s5 = 5, s6 = 6, s7 = 7, s8 = 8;
			ushort us1 = 1, us2 = 2, us3 = 3, us4 = 4, us5 = 5, us6 = 6, us7 = 7, us8 = 8;
			int i1 = 1, i2 = 2, i3 = 3, i4 = 4, i5 = 5, i6 = 6, i7 = 7, i8 = 8;
			uint ui1 = 1, ui2 = 2, ui3 = 3, ui4 = 4, ui5 = 5, ui6 = 6, ui7 = 7, ui8 = 8;
			long l1 = 1, l2 = 2, l3 = 3, l4 = 4, l5 = 5, l6 = 6, l7 = 7, l8 = 8;
			ulong ul1 = 1, ul2 = 2, ul3 = 3, ul4 = 4, ul5 = 5, ul6 = 6, ul7 = 7, ul8 = 8;
			string template = "{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}";
			Console.WriteLine (template, b1, sb1, s1, us1, i1, ui1, l1, ul1);
			Console.WriteLine (template, b2, sb2, s2, us2, i2, ui2, l2, ul2);
			Console.WriteLine (template, b3, sb3, s3, us3, i3, ui3, l3, ul3);
			Console.WriteLine (template, b4, sb4, s4, us4, i4, ui4, l4, ul4);
			Console.WriteLine (template, b5, sb5, s5, us5, i5, ui5, l5, ul5);
			Console.WriteLine (template, b6, sb6, s6, us6, i6, ui6, l6, ul6);
			Console.WriteLine (template, b7, sb7, s7, us7, i7, ui7, l7, ul7);
			Console.WriteLine (template, b8, sb8, s8, us8, i8, ui8, l8, ul8);
		}

		[Test]
		public void TooLarge ()
		{
			// 65 variables ([g]mcs) and 66 variables (one temp for CSC)
			AssertRuleFailure<AvoidLargeNumberOfLocalVariablesTest> ("TooLargeMethod", 1);
		}

		[Test]
		public void SmallMaximum ()
		{
			int max = Rule.MaximumVariables;
			try {
				Rule.MaximumVariables = 1;
				AssertRuleSuccess<AvoidLargeNumberOfLocalVariablesTest> ("SmallMethod");
				AssertRuleFailure<AvoidLargeNumberOfLocalVariablesTest> ("LargeMethod", 1);
			}
			finally {
				Rule.MaximumVariables = max;
			}
		}
	}
}
