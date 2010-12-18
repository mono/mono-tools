// 
// Unit tests for OnlyUseDisposeForIDisposableTypesRule
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
	public sealed class OnlyUseDisposeForIDisposableTypesTest : TypeRuleTestFixture<OnlyUseDisposeForIDisposableTypesRule> {

		public class Good1 {
			public void Reset ()
			{
			}
		}

		public class Good2 : IDisposable {
			public virtual void Dispose ()
			{
			}
		}

		public class Good3 : Good2 {
			public override void Dispose ()
			{
			}
		}

		public class Bad1 {
			public void Dispose ()
			{
			}
		}

		public class Bad2 {
			public void Dispose (int x)
			{
			}
		}

		public interface Bad3 {
			void Dispose ();
		}

		internal class Bad4 {
			public void Dispose ()
			{
			}
		}

		internal class Bad5 {
			public void Close ()
			{
				Dispose (0);
			}

			private void Dispose (int x)
			{
			}
		}

		internal class Bad6 {
			public void Dispose ()
			{
				Dispose (0);
			}

			private void Dispose (int x)
			{
			}
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
		}

		[Test]
		public void Cases ()
		{
			AssertRuleSuccess<Good1> ();
			AssertRuleSuccess<Good2> ();
			AssertRuleSuccess<Good3> ();
			
			AssertRuleFailure<Bad1> ();
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "Bad1-Severity-High");

			AssertRuleFailure<Bad2> ();
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "Bad2-Severity-High");

			AssertRuleFailure<Bad3> ();
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "Bad3-Severity-High");

			// Bad4 has internal visibility so we consider it less severe.
			AssertRuleFailure<Bad4> ();
			Assert.AreEqual (Severity.Medium, Runner.Defects [0].Severity, "Bad4-Severity-Medium");

			// Private Dispose.
			AssertRuleFailure<Bad5> ();
			Assert.AreEqual (Severity.Low, Runner.Defects [0].Severity, "Bad5-Severity-Low");

			AssertRuleFailure<Bad6> (2);
		}
	}
}
