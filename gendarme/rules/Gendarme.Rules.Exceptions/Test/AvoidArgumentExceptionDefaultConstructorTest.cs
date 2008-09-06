//
// Unit tests for AvoidArgumentExceptionDefaultConstructorRule
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

using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Exceptions {

	[TestFixture]
	public class AvoidArgumentExceptionDefaultConstructorTest : MethodRuleTestFixture<AvoidArgumentExceptionDefaultConstructorRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no newobj (so no new *Exception possible)
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public void Argument_Single (string s)
		{
			if (String.IsNullOrEmpty (s))
				throw new ArgumentException ();
		}

		public void ArgumentNull_Single (string s)
		{
			if (s == null)
				throw new ArgumentNullException ();
		}

		public void ArgumentOutOfRange_Single ()
		{
			throw new ArgumentOutOfRangeException ();
		}

		public void DuplicateWaitObject_Single ()
		{
			throw new DuplicateWaitObjectException ();
		}

		public void Multiple (string s)
		{
			if (s == null)
				throw new ArgumentNullException ();
			if (s.Length == 0)
				throw new ArgumentException ();
			if (s.Length > 32)
				throw new ArgumentOutOfRangeException ();
			throw new DuplicateWaitObjectException ();
		}

		[Test]
		public void NoArgument ()
		{
			AssertRuleFailure<AvoidArgumentExceptionDefaultConstructorTest> ("Argument_Single", 1);
			AssertRuleFailure<AvoidArgumentExceptionDefaultConstructorTest> ("ArgumentNull_Single", 1);
			AssertRuleFailure<AvoidArgumentExceptionDefaultConstructorTest> ("ArgumentOutOfRange_Single", 1);
			AssertRuleFailure<AvoidArgumentExceptionDefaultConstructorTest> ("DuplicateWaitObject_Single", 1);

			AssertRuleFailure<AvoidArgumentExceptionDefaultConstructorTest> ("Multiple", 4);
		}

		public void ThrowArgumentException (int n)
		{
			switch (n) {
			case 1:
				throw new ArgumentException ("message");
			case 2:
				throw new ArgumentException ("message", new ArgumentNullException ("n"));
			case 3:
				throw new ArgumentException ("message", "n");
			case 4:
				throw new ArgumentException ("message", "n", new ArgumentOutOfRangeException ("n"));
			}
		}

		[Test]
		public void Arguments ()
		{
			AssertRuleSuccess<AvoidArgumentExceptionDefaultConstructorTest> ("ThrowArgumentException");
		}

		public class MyOwnException : ArgumentException {

			public MyOwnException ()
			{
			}

			public MyOwnException (string paramName)
				: base (paramName)
			{
			}

			static void Throw ()
			{
				throw new MyOwnException ();
			}

			static void ThrowForParam (string s)
			{
				throw new MyOwnException (s);
			}
		}

		[Test]
		public void Derived ()
		{
			AssertRuleFailure<MyOwnException> ("Throw", 1);
			AssertRuleSuccess<MyOwnException> ("ThrowForParam");
		}
	}
}
