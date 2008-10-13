//
// Unit tests for RemoveUnusedLocalVariablesRule
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
using System.Collections.Generic;

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class RemoveUnusedLocalVariablesTest : MethodRuleTestFixture<RemoveUnusedLocalVariablesRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.GeneratedCodeMethod);
		}

		[Test]
		public void Empty ()
		{
			AssertRuleSuccess (SimpleMethods.EmptyMethod);
		}

		private bool ReturnConstant ()
		{
			// compiler create this local variable
			return true;
		}

		private bool ReturnLocal_Ok ()
		{
			bool a = false;
			return a;
		}

		[Test]
		public void Return ()
		{
			AssertRuleSuccess<RemoveUnusedLocalVariablesTest> ("ReturnConstant");
			AssertRuleSuccess<RemoveUnusedLocalVariablesTest> ("ReturnLocal_Ok");
		}

		private bool ReturnLocal_Unused ()
		{
			bool a = false;
			bool b; // unused but CSC removed it from IL while [g]mcs keeps it
			return a;
		}

		private bool ReturnLocal_StoreOnly ()
		{
			bool a = false;
			bool b = false; // stored only - so it's, in fact, unused
					// again CSC removes it from IL while [g]mcs keeps it
			return a;
		}

		[Test]
		[Ignore ("depends if the compiler removes the unused variables or not from IL")]
		public void CompilerDependent ()
		{
			AssertRuleFailure<RemoveUnusedLocalVariablesTest> ("ReturnLocal_StoreOnly");
			AssertRuleFailure<RemoveUnusedLocalVariablesTest> ("ReturnLocal_Unused");
		}

		// test case provided by Richard Birkby
		internal sealed class FalsePositive6 {

			public void Run ()
			{
				GetType ();
				foreach (string s in AddValues ()) {
					Console.WriteLine (s);
				}
			}

			public static IEnumerable<string> AddValues ()
			{
				string newValue = null;

				foreach (string value in new string [] { "1", "2", "3" }) {
					decimal firstValue;
					decimal secondValue;
					if (Decimal.TryParse (newValue, out firstValue) &&
					Decimal.TryParse (value, out secondValue)) {
						newValue = (firstValue + secondValue).ToString ();
					} else {
						newValue = null;
					}
				}

				yield return newValue;
			}
		}

		[Test]
		public void Out ()
		{
			AssertRuleSuccess<FalsePositive6> ("AddValues");
		}
	}
}
