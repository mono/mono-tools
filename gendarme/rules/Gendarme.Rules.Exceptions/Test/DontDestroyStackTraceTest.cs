//
// Unit Test for DontDestroyStackTraceTest Rule
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
using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Exceptions {

	[TestFixture]
	public class DoNotDestroyStackTraceTest : MethodRuleTestFixture<DoNotDestroyStackTraceRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}
	
		public void ThrowOriginalEx ()
		{
			try  {
				Int32.Parse("Broken!");
			}
			catch (Exception ex) {
				// Throw exception immediately.
				// This should trip the DontDestroyStackTrace rule.
				throw ex;
			}
		}

		[Test]
		public void TestThrowOriginalEx ()
		{
			AssertRuleFailure<DoNotDestroyStackTraceTest> ("ThrowOriginalEx", 1);
		}

		public void ThrowOriginalExWithJunk ()
		{
			try {
				Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				int j = 0;
				for (int k=0; k<10; k++) {
					// throw some junk into the catch block, to ensure that
					j += 10;
					Console.WriteLine (j);
				}

				// This should trip the DontDestroyStackTrace rule, because we're
				// throwing the original exception.
				throw ex;
			}
		}

		[Test]
		public void TestThrowOriginalExWithJunk ()
		{
			AssertRuleFailure<DoNotDestroyStackTraceTest> ("ThrowOriginalExWithJunk", 1);
		}

		public void RethrowOriginalEx ()
		{
			try {
				Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				// avoid compiler warning
				Assert.IsNotNull (ex);
				// This should NOT trip the DontDestroyStackTrace rule, because we're
				// rethrowing the original exception.
				throw;
			}
		}

		[Test]
		public void TestRethrowOriginalEx ()
		{
			// no throw instruction is present in this method (its a rethrow)
			AssertRuleDoesNotApply<DoNotDestroyStackTraceTest> ("RethrowOriginalEx");
		}

		public void ThrowOriginalExAndRethrowWithJunk ()
		{
			int i = 0;
			try {
				i = Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				int j = 0;
				for (int k=0; k<10; k++) {
					// throw some junk into the catch block, to ensure that
					j += 10;
					Console.WriteLine (j);
					if ((i % 1234) > 56) {
						// This should trip DontDestroyStackTraceRule, because we're
						// throwing the original exception.
						throw ex;
					}
				}

				// More junk - just to ensure that alternate paths through
				// this catch block end up at a throw and a rethrow
				throw;
			}
		}

		[Test]
		public void TestThrowOriginalExAndRethrowWithJunk ()
		{
			AssertRuleFailure<DoNotDestroyStackTraceTest> ("ThrowOriginalExAndRethrowWithJunk", 1);
		}

		public void RethrowOriginalExAndThrowWithJunk ()
		{
			int i = 0;
			try {
				i = Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				int j = 0;
				for (int k=0; k<10; k++) {
					// throw some junk into the catch block, to ensure that
					j += 10;
					Console.WriteLine (j);
					if ((i % 1234) > 56) {
						// More junk - just to ensure that alternate paths through
						// this catch block end up at a throw and a rethrow
						throw;
					}
				}

				// This should trip the DontDestroyStackTrace rule, because we're
				// throwing the original exception.
				throw ex;
			}
		}

		[Test]
		public void TestRethrowOriginalExAndThrowWithJunk ()
		{
			AssertRuleFailure<DoNotDestroyStackTraceTest> ("RethrowOriginalExAndThrowWithJunk", 1);
		}

		public void ThrowNewExceptionUsingSameOldLocal ()
		{
			try {
				Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				// we deliberately choose to create a new exception
				ex = new InvalidOperationException ("uho");
				throw ex;
			}
		}

		public void ThrowNewExceptionUsingSameOldLocal_WithParameter ()
		{
			try {
				Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				// we deliberately choose to create a new exception
				ex = new InvalidOperationException ("uho", ex);
				throw ex;
			}
		}

		[Test]
		public void TestThrowNewExceptionUsingSameOldLocal ()
		{
			AssertRuleSuccess<DoNotDestroyStackTraceTest> ("ThrowNewExceptionUsingSameOldLocal");
			AssertRuleSuccess<DoNotDestroyStackTraceTest> ("ThrowNewExceptionUsingSameOldLocal_WithParameter");
		}

		// test case from bnc #668925
		// https://github.com/Iristyle/mono-tools/commit/5516987609de6fdd40f24b91281e95a3b1457ea7
		// CSC (at least for VS2008) put the ExceptionHandler.HandlerEnd past the last instruction (which cecil dislike)

		public void ThrowCatchThrowNew ()
		{
			try {
				throw new NotImplementedException ();
			}
			catch (Exception) {
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void TestThrowEatThrow ()
		{
			AssertRuleSuccess<DoNotDestroyStackTraceTest> ("ThrowCatchThrowNew");
		}
	}
}
