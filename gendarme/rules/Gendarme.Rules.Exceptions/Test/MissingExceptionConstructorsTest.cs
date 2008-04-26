//
// Unit tests for MissingExceptionConstructorsRule
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
using System.Runtime.Serialization;

using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Exceptions {

	[TestFixture]
	public class MissingExceptionConstructorsTest : TypeRuleTestFixture<MissingExceptionConstructorsRule> {

		[Test]
		public void NotAnException ()
		{
			AssertRuleDoesNotApply<int> ();
		}

		public class NoPublicCtorException : Exception {

			private NoPublicCtorException ()
			{
			}
		}

		public class StaticCtorException : Exception {

			static StaticCtorException ()
			{
			}

			private StaticCtorException (int error)
			{
			}
		}

		[Test]
		public void MissingAll ()
		{
			AssertRuleFailure<NoPublicCtorException> (4);
			AssertRuleFailure<StaticCtorException> (4);
		}

		public class EmptyCtorException : Exception {
			public EmptyCtorException ()
			{
			}
		}

		public class MessageCtorException : Exception {
			public MessageCtorException (string message)
			{
			}
		}

		public class MessageExtraCtorException : Exception {
			public MessageExtraCtorException (string message, string extra)
			{
			}
		}

		public class InnerCtorException : Exception {
			public InnerCtorException (string message, Exception inner)
			{
			}
		}

		public class InnerExtraCtorException : Exception {
			public InnerExtraCtorException (string message, string extra, Exception inner)
			{
			}
		}

		[Test]
		public void MissingSome ()
		{
			AssertRuleFailure<EmptyCtorException> (3);

			AssertRuleFailure<MessageCtorException> (3);
			AssertRuleFailure<MessageExtraCtorException> (3);

			AssertRuleFailure<InnerCtorException> (3);
			AssertRuleFailure<InnerExtraCtorException> (3);
		}

		public class EmptyMessageCtorException : Exception {
			public EmptyMessageCtorException ()
			{
			}

			public EmptyMessageCtorException (string message)
			{
			}
		}

		[Test]
		public void MissingHalf ()
		{
			AssertRuleFailure<EmptyMessageCtorException> (2);
		}

		public class MissinsEmptyCtorException : Exception {
			public MissinsEmptyCtorException (string message)
			{
			}

			public MissinsEmptyCtorException (string message, Exception inner)
			{
			}

			protected MissinsEmptyCtorException (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class MissinsStringCtorException : Exception {
			public MissinsStringCtorException ()
			{
			}

			public MissinsStringCtorException (string message, Exception inner)
			{
			}

			private MissinsStringCtorException (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class MissinsInnerCtorException : Exception {
			public MissinsInnerCtorException ()
			{
			}

			public MissinsInnerCtorException (string message)
			{
			}

			protected MissinsInnerCtorException (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class MissinsSerializationCtorException : Exception {
			public MissinsSerializationCtorException ()
			{
			}

			public MissinsSerializationCtorException (string message)
			{
			}

			public MissinsSerializationCtorException (string message, Exception inner)
			{
			}
		}

		[Test]
		public void MissingOne ()
		{
			AssertRuleFailure<MissinsEmptyCtorException> (1);
			AssertRuleFailure<MissinsStringCtorException> (1);
			AssertRuleFailure<MissinsInnerCtorException> (1);
			AssertRuleFailure<MissinsSerializationCtorException> (1);
		}

		public class CompleteException : Exception {

			public CompleteException ()
			{
			}

			public CompleteException (string message)
			{
			}

			public CompleteException (string message, Exception inner)
			{
			}

			protected CompleteException (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class CompleteExtraException : Exception {

			public CompleteExtraException ()
			{
			}

			public CompleteExtraException (string message, string extra)
			{
			}

			public CompleteExtraException (string message, string extra, Exception inner)
			{
			}

			private CompleteExtraException (SerializationInfo info, StreamingContext context)
			{
			}
		}

		[Test]
		public void Complete ()
		{
			AssertRuleSuccess<CompleteException> ();
			AssertRuleSuccess<CompleteExtraException> ();
		}
	}
}
