// 
// Unit tests for DoNotThrowReservedExceptionRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Exceptions {

	[TestFixture]
	public class DoNotThrowReservedExceptionTest : MethodRuleTestFixture<DoNotThrowReservedExceptionRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		public Exception CreateExceptionBad ()
		{
			return new ExecutionEngineException ("uho");
		}

		public void ThrowOutOfRangeExceptionBad ()
		{
			throw new IndexOutOfRangeException ();
		}

		public void Throw (object o)
		{
			if (o == null)
				throw new NullReferenceException ("null", new OutOfMemoryException ());
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<DoNotThrowReservedExceptionTest> ("CreateExceptionBad", 1);
			AssertRuleFailure<DoNotThrowReservedExceptionTest> ("ThrowOutOfRangeExceptionBad", 1);
			AssertRuleFailure<DoNotThrowReservedExceptionTest> ("Throw", 2);
		}

		public Exception CreateExceptionGood ()
		{
			// another rule deals with "basic" exceptions
			return new Exception ();
		}

		public void ThrowOutOfRangeExceptionGood ()
		{
			throw new ArgumentOutOfRangeException ();
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<DoNotThrowReservedExceptionTest> ("CreateExceptionGood");
			AssertRuleSuccess<DoNotThrowReservedExceptionTest> ("ThrowOutOfRangeExceptionGood");
		}
	}
}
