// 
// Unit tests for PreferParamsArrayForVariableArgumentsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Runtime.InteropServices;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class PreferParamsArrayForVariableArgumentsTest : MethodRuleTestFixture<PreferParamsArrayForVariableArgumentsRule> {

		// special case, this return false for HasParameters
		private void ShowItems_NoParameter (__arglist)
		{
			ArgIterator args = new ArgIterator (__arglist);
			for (int i = 0; i < args.GetRemainingCount (); i++) {
// gmcs cannot compile __refvalue correctly - bnc 569539
#if !__MonoCS__
				Console.WriteLine (__refvalue (args.GetNextArg (), string));
#endif
			}
		}

		public void ShowItems_Bad (string header, __arglist)
		{
			Console.WriteLine (header);
			ArgIterator args = new ArgIterator (__arglist);
			for (int i = 0; i < args.GetRemainingCount (); i++) {
// gmcs cannot compile __refvalue correctly - bnc 569539
#if !__MonoCS__
				Console.WriteLine (__refvalue (args.GetNextArg (), string));
#endif
			}
		}

		[Test]
		public void ArgIterator ()
		{
			AssertRuleFailure<PreferParamsArrayForVariableArgumentsTest> ("ShowItems_NoParameter", 1);
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "private");

			AssertRuleFailure<PreferParamsArrayForVariableArgumentsTest> ("ShowItems_Bad", 1);
			Assert.AreEqual (Severity.Critical, Runner.Defects [0].Severity, "public");
		}

		[DllImport ("libc.dll")]
		static extern int printf (string format, __arglist);

		[Test]
		public void Interop ()
		{
			AssertRuleSuccess<PreferParamsArrayForVariableArgumentsTest> ("printf");
		}

		public void ShowItems_Params (string header, params string [] items)
		{
			Console.WriteLine (header);
			for (int i = 0; i < items.Length; i++) {
				Console.WriteLine (items [i]);
			}
		}

		[Test]
		public void Params ()
		{
			AssertRuleSuccess<PreferParamsArrayForVariableArgumentsTest> ("ShowItems_Params");
		}
	}
}

