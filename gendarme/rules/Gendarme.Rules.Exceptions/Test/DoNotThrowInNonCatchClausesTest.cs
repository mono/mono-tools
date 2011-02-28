//
// Unit Tests for DoNotThrowInNonCatchClausesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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
using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Exceptions {

	[TestFixture]
	public class DoNotThrowInNonCatchClausesTest : MethodRuleTestFixture<DoNotThrowInNonCatchClausesRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		void ThrowInTry ()
		{
			try {
				throw new NotImplementedException ("no luck");
			}
			finally {
				Console.WriteLine ();
			}
		}

		void ThrowInCatch ()
		{
			try {
				Console.WriteLine ();
			}
			catch (Exception e) {
				throw new NotImplementedException ("no luck", e);
			}
		}

		// copied from DontDestroyStackTraceTest since CSC compiles the HandlerEnd as past the method offset
		void ThrowCatchThrowNew ()
		{
			try {
				throw new NotImplementedException ();
			}
			catch (Exception) {
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<DoNotThrowInNonCatchClausesTest> ("ThrowInTry");
			AssertRuleSuccess<DoNotThrowInNonCatchClausesTest> ("ThrowInCatch");
			AssertRuleSuccess<DoNotThrowInNonCatchClausesTest> ("ThrowCatchThrowNew");
		}

		void RethrowInCatch ()
		{
			try {
				Console.WriteLine ();
			}
			catch (Exception) {
				throw; // rethrow in IL which is seen only in catch clauses
			}
		}

		[Test]
		public void Rethrow ()
		{
			AssertRuleDoesNotApply<DoNotThrowInNonCatchClausesTest> ("RethrowInCatch");
		}

		void ThrowInFinally ()
		{
			try {
				Console.WriteLine ();
			}
			finally {
				throw new NotImplementedException ("no luck");
			}
		}

		void ThrowInFinallyToo ()
		{
			try {
				throw new NotImplementedException ("no luck");
			}
			catch (Exception e) {
				throw new NotImplementedException ("no more luck", e);
			}
			finally {
				if (GetType ().IsSealed)
					throw new NotImplementedException ("never any luck");
				else
					throw new NotSupportedException ("stop playing cards");
			}
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure<DoNotThrowInNonCatchClausesTest> ("ThrowInFinally", 1);
			AssertRuleFailure<DoNotThrowInNonCatchClausesTest> ("ThrowInFinallyToo", 2);
		}
	}
}
