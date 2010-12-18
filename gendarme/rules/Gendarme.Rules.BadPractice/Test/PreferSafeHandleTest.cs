// 
// Unit tests for PreferSafeHandleRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2009 Jesse Jones
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
using System.Reflection;
using System.Runtime.InteropServices;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public sealed class PreferSafeHandleTest : TypeRuleTestFixture<PreferSafeHandleRule> {

		internal class Good1 {
			internal SafeHandle ptr;
		}

		internal class Bad1 {
			internal IntPtr ptr;
		}

		internal class Bad2 {
			internal UIntPtr ptr;
		}

		internal class Bad3 {
			~Bad3 ()
			{
			}
			
			internal UIntPtr ptr;
		}

		internal class Bad4 : IDisposable {
			public void Dispose ()
			{
			}
			
			internal UIntPtr ptr;
		}

		internal class Bad5 : IDisposable {
			~Bad5 ()
			{
			}
			
			public void Dispose ()
			{
			}
			
			internal UIntPtr ptr;
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		[Test]
		public void Cases ()
		{
			AssertRuleSuccess<Good1> ();
			
			AssertRuleFailure<Bad1> ();
			Assert.AreEqual (Confidence.Low, Runner.Defects [0].Confidence, "Bad1-Confidence-Low");

			AssertRuleFailure<Bad2> ();
			Assert.AreEqual (Confidence.Low, Runner.Defects [0].Confidence, "Bad2-Confidence-Low");

			AssertRuleFailure<Bad3> ();
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "Bad3-Confidence-Normal");

			AssertRuleFailure<Bad4> ();
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "Bad4-Confidence-Normal");

			AssertRuleFailure<Bad5> ();
			Assert.AreEqual (Confidence.High, Runner.Defects [0].Confidence, "Bad5-Confidence-High");
		}
	}
}
