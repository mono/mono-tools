#define CONTRACTS_FULL
#define SOME_DEFINE

//
// Unit test for AvoidMethodsWithSideEffectsInConditionalCodeRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// 	(C) 2009 Jesse Jones
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

using Gendarme.Framework;
using Gendarme.Rules.Correctness;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace System.Diagnostics.Contracts {
	[Serializable]
	[AttributeUsage (AttributeTargets.Method | AttributeTargets.Delegate, AllowMultiple = false)]
	public sealed class PureAttribute : Attribute {
	}
		
	public static class Contract {
		public static bool Foo (bool predicate)
		{
			return predicate;
		}
	}
}

namespace Test.Rules.Correctness {
	[TestFixture]
	public class AvoidMethodsWithSideEffectsInConditionalCodeTest : MethodRuleTestFixture<AvoidMethodsWithSideEffectsInConditionalCodeRule> {
	
		internal sealed class TestCases {
			// Anything can be used with non-conditionally compiled methods. 
			public void Good1 (bool data)
			{
				NonConditionalCall (PureIdentity (data));
				NonConditionalCall (NonPureIdentity (data));
			}
			
			// PureAttribute methods can be used with conditional code.
			public void Good2 (bool data)
			{
				ConditionalCall (PureIdentity (data));
			}
			
			// Getters can be used with conditional code.
			public void Good3 (ActivationContext context)
			{
				ConditionalCall (context.Form);
			}
			
			// Operators can be used with conditional code.
			public void Good4 (DateTime x, TimeSpan y)
			{
				ConditionalCall (x + y);
			}
			
			// All Contract methods can be used with conditional code.
			public void Good5 (bool data)
			{
				ConditionalCall (Contract.Foo (data));
			}
			
			// All System.String methods can be used with conditional code.
			public void Good6 (string data)
			{
				ConditionalCall (data.GetHashCode ());
			}
			
			// Predicate<T> can be used with conditional code.
			public void Good7 (Predicate<int> predicate, int x)
			{
				ConditionalCall (predicate (x));
			}
			
			// PureAttribute delegates can be used with conditional code.
			public void Good8 (PureDelegate d, bool data)
			{
				ConditionalCall (d (data));
			}
			
			// Dictionary`2::ContainsKey can be used with conditional code.
			public void Good9 (System.Collections.Generic.Dictionary<string, bool> d, string name)
			{
				ConditionalCall (d.ContainsKey (name));
			}
			
			// Non-pure methods cannot be used with conditional code.
			public void Bad1 (bool data)
			{
				ConditionalCall (NonPureIdentity (data));
			}
			
			// Method calls outside the assembly being tested resolve properly.
			public void Bad2 (bool data)
			{
				Trace.Assert (NonPureIdentity (data) != null);
			}
			
			// Non-pure delegates cannot be used with conditional code.
			public void Bad3 (NonPureDelegate d, bool data)
			{
				ConditionalCall (d (data));
			}
			
			// Dictionary`2::Remove cannot be used with conditional code.
			public void Bad4 (System.Collections.Generic.Dictionary<string, bool> d, string name)
			{
				ConditionalCall (d.Remove (name));
			}
			
			// Make sure contracts code is reported at the correct confidence.
			public void High1 (bool data)
			{
				ConditionalCall (NonPureIdentity (data));
			}
			
			public void High2 (bool data)
			{
				ContractsCall (NonPureIdentity (data));
			}
			
			public void Low (bool data)
			{
				OtherCall (NonPureIdentity (data));
			}
			
			[Conditional ("DEBUG")]
			public void ConditionalCall (object data)
			{
			}
			
			[Conditional ("CONTRACTS_FULL")]
			public void ContractsCall (object data)
			{
			}
			
			[Conditional ("SOME_DEFINE")]
			public void OtherCall (object data)
			{
			}
			
			public void NonConditionalCall (object data)
			{
			}
			
			[Pure]
			public delegate bool PureDelegate (bool data);
			
			public delegate bool NonPureDelegate (bool data);
			
			[Pure]
			private object PureIdentity (object data)
			{
				return data;
			}
			
			private object NonPureIdentity (object data)
			{
				return data;
			}
		}
		
		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}
		
		[Test]
		public void Cases ()
		{
			AssertRuleSuccess<TestCases> ("Good1");
			AssertRuleSuccess<TestCases> ("Good2");
			AssertRuleSuccess<TestCases> ("Good3");
			AssertRuleSuccess<TestCases> ("Good4");
			AssertRuleSuccess<TestCases> ("Good5");
			AssertRuleSuccess<TestCases> ("Good6");
			AssertRuleSuccess<TestCases> ("Good7");
			AssertRuleSuccess<TestCases> ("Good8");
			AssertRuleSuccess<TestCases> ("Good9");
			
			AssertRuleFailure<TestCases> ("Bad1");
			AssertRuleFailure<TestCases> ("Bad2");
			AssertRuleFailure<TestCases> ("Bad3");
			AssertRuleFailure<TestCases> ("Bad4");
		}
		
		[Test]
		public void Confidences ()
		{
			AssertRuleFailure<TestCases> ("High1");
			Assert.AreEqual (Confidence.High, Runner.Defects [0].Confidence, "High1-Confidence-High");
			
			AssertRuleFailure<TestCases> ("High2");
			Assert.AreEqual (Confidence.High, Runner.Defects [0].Confidence, "High2-Confidence-High");
			
			AssertRuleFailure<TestCases> ("Low");
			Assert.AreEqual (Confidence.Low, Runner.Defects [0].Confidence, "Low-Confidence-Low");
		}
	}
}
