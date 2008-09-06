//
// Unit tests for AvoidThrowingBasicExceptionsRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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

using Gendarme.Framework;
using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Exceptions {
	[TestFixture]
	public class AvoidThrowingBasicExceptionsTest : MethodRuleTestFixture<AvoidThrowingBasicExceptionsRule> {
	
		class ExceptionThrower {
			public void CreateException ()
			{
				Exception ex = new Exception ("upyachka");
			}
			
			public void CreateApplicationException ()
			{
				ApplicationException ex = new ApplicationException ("krevedko");
			}
			
			public void CreateSystemException ()
			{
				SystemException ex = new SystemException ("^_^");
			}
						
			public void ThrowException ()
			{
				throw new Exception ("yarrr!");
			}

			public void ThrowApplicationException ()
			{
				throw new ApplicationException ("s.r.u.");
			}

			public void ThrowSystemException ()
			{
				throw new SystemException ("zhazha");
			}
			
			public void ThrowSpecificExceptions ()
			{
				Random r = new Random ();
				// to avoid compiler optimizations that remove everything after throw
				if (r.NextDouble () > 0.5) throw new NotImplementedException ();
				if (r.NextDouble () > 0.5) throw new OverflowException ("too much upyachka!");
				if (r.NextDouble () > 0.5) throw new ExecutionEngineException ();
			}
			
			public void ThrowTwoBasicExceptions ()
			{
				Random r = new Random ();
				// to avoid compiler optimizations that remove everything after throw
				if (r.NextDouble () > 0.5) throw new NotImplementedException ();
				// bad #1:
				if (r.NextDouble () > 0.5) throw new Exception ("upyachka");
				if (r.NextDouble () > 0.5) throw new OverflowException ("YARR!!11");
				// bad #2:
				if (r.NextDouble () > 0.5) throw new ApplicationException ("this is parta!!11oneoneone");
				if (r.NextDouble () > 0.5) throw new ExecutionEngineException ();
			}
		}
		
		
		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no newobj (so no new *Exception possible)
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void CreateBasicExceptionsFails ()
		{
			AssertRuleFailure<ExceptionThrower> ("CreateException");
			AssertRuleFailure<ExceptionThrower> ("CreateApplicationException");
			AssertRuleFailure<ExceptionThrower> ("CreateSystemException");
		}
		
		[Test]
		public void ThrowBasicExceptionsFails ()
		{
			AssertRuleFailure<ExceptionThrower> ("ThrowException");
			AssertRuleFailure<ExceptionThrower> ("ThrowApplicationException");
			AssertRuleFailure<ExceptionThrower> ("ThrowSystemException");
		}
		
		[Test]
		public void ThrowSpecificExceptionsSucceeds ()
		{
			AssertRuleSuccess<ExceptionThrower> ("ThrowSpecificExceptions");
		}
		
		[Test]
		public void GotCountRight ()
		{
			AssertRuleFailure<ExceptionThrower> ("ThrowTwoBasicExceptions", 2);
		}
	}
}
