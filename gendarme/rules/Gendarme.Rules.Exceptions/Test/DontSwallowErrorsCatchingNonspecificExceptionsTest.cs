//
// Unit Test for DontSwallowErrorsCatchingNonspecificExceptions Rule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 Néstor Salceda
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
using System.Diagnostics;
using System.IO;

using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Exceptions {
	
	[TestFixture]
	public class DoNotSwallowErrorsCatchingNonSpecificExceptionsTest : MethodRuleTestFixture<DoNotSwallowErrorsCatchingNonSpecificExceptionsRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no exception handler
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}
		
		public void SwallowErrorsCatchingExceptionEmptyCatchBlock ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
			}
		}

		[Test]
		public void SwallowErrorsCatchingExceptionsEmptyCatchBlockTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("SwallowErrorsCatchingExceptionEmptyCatchBlock", 1);
		}

		public void SwallowErrorsCatchingExceptionNoEmptyCatchBlock ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		[Test]
		public void SwallowErrorsCatchingExceptionsNoEmptyCatchBlockTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("SwallowErrorsCatchingExceptionNoEmptyCatchBlock", 1);
		}

		public void SwallowErrorsCatchingSystemExceptionEmptyCatchBlock ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (SystemException exception) {
			}
		}

		[Test]
		public void SwallowErrorsCatchingSystemExceptionEmptyCatchBlockTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("SwallowErrorsCatchingSystemExceptionEmptyCatchBlock", 1);
		}

		public void SwallowErrorsCatchingSystemExceptionNoEmptyCatchBlock ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (SystemException exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		[Test]
		public void SwallowErrorsCatchingSystemExceptionNoEmptyCatchBlockTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("SwallowErrorsCatchingSystemExceptionNoEmptyCatchBlock", 1);
		}

		public void SwallowErrorsCatchingTypeExceptionEmptyCatchBlock ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception) {
			}
		}

		[Test]
		public void SwallowErrorsCatchingTypeExceptionEmptyCatchBlockTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("SwallowErrorsCatchingTypeExceptionEmptyCatchBlock", 1);
		}

		public void SwallowErrorsCatchingTypeExceptionNoEmptyCatchBlock ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception) {
				Console.WriteLine ("Has happened an exception.");
			}
		}
		
		[Test]
		public void SwallowErrorsCatchingTypeExceptionNoEmptyCatchBlockTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("SwallowErrorsCatchingTypeExceptionNoEmptyCatchBlock", 1);
		}

		public void SwallowErrorsCatchingAllEmptyCatchBlock ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch {
			}
		}

		[Test]
		public void SwallowErrorsCatchingAllEmptyCatchBlockTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("SwallowErrorsCatchingAllEmptyCatchBlock", 1);
		}

		public void SwallowErrorsCatchingAllNoEmptyCatchBlock ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch {
				Console.WriteLine ("Has happened an exception.");
			}
		}

		[Test]
		public void SwallowErrorsCatchingAllNoEmptyCatchBlockTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("SwallowErrorsCatchingAllNoEmptyCatchBlock", 1);
		}

		public void NotSwallowRethrowingGeneralException ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
				throw;
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		[Test]
		public void NotSwallowRethrowingGeneralExceptionTest ()
		{
			AssertRuleSuccess<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("NotSwallowRethrowingGeneralException");
		}
		
		public void NotSwallowRethrowingException ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
				throw exception;
				Console.WriteLine (exception.Message);
				Console.WriteLine (exception);
			}
		}

		[Test]
		public void NotSwallowRethrowingExceptionTest ()
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("NotSwallowRethrowingException", 1);
		}

		public void NotSwallowCatchingSpecificException ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (FileNotFoundException exception) {
			}
		}

		[Test]
		public void NotSwallowCatchingSpecificExceptionTest () 
		{
			AssertRuleSuccess<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("NotSwallowCatchingSpecificException");
		}

		public void NotSwallowThrowingANewException ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
				throw new SystemException ("Message");
			}
		}

		[Test]
		public void NotSwallowThrowingANewExceptionTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("NotSwallowThrowingANewException", 1);
		}

		public void NotSwallowCatchingAllThrowingANewException ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch {
				throw new Exception ("Message");
			}
		}

		[Test]
		public void NotSwallowCatchingAllThrowingANewExceptionTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("NotSwallowCatchingAllThrowingANewException", 1);
		}

		public void NotSwallowCatchingTypeExceptionThrowingANewException ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception) {
				throw new Exception ("Message");
			}
		}

		[Test]
		public void NotSwallowCatchingTypeExceptionThrowingANewExceptionTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("NotSwallowCatchingTypeExceptionThrowingANewException", 1);
		}

		public void NotSwallowCatchingSystemExceptionThrowingANewException ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (System.Exception exception) {
				throw new Exception ("Message");
			}
		}

		[Test]
		public void NotSwallowCatchingSystemExceptionThrowingANewExceptionTest () 
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("NotSwallowCatchingSystemExceptionThrowingANewException", 1);
		}

		public void SkipUsingGoto ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
			retry:
				if (exception == null)
					throw new Exception (exception.ToString ());

				Console.WriteLine ("Skipped");
				exception = exception.InnerException;
				goto retry;
			}
		}

		[Test]
		public void Goto ()
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("SkipUsingGoto", 1);
		}

		[Conditional ("DONTDEFINE")]
		public void ConditionWrite (object obj)
		{
			Console.WriteLine (obj);
		}

		public void ExceptionWithConditional ()
		{
			try {
				File.Open ("foo.txt", FileMode.Open);
			}
			catch (Exception exception) {
				ConditionWrite (exception);
				throw new Exception ("uho", exception);
			}
		}

		[Test]
		public void Conditional ()
		{
			AssertRuleFailure<DoNotSwallowErrorsCatchingNonSpecificExceptionsTest> ("ExceptionWithConditional", 1);
		}
	}
}
