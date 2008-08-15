//
// Unit tests for InstantiateArgumentExceptionsCorrectlyRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using System.Collections;
using Gendarme.Rules.Exceptions;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Exceptions {
	[TestFixture]
	public class InstantiateArgumentExceptionCorrectlyTest : MethodRuleTestFixture<InstantiateArgumentExceptionCorrectlyRule> {

		[Test]
		public void SkipOnBodylessMethods ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		[Test]
		public void SuccessOnEmptyMethods ()
		{
			AssertRuleSuccess (SimpleMethods.EmptyMethod);
		}

		public void ArgumentExceptionWithTwoParametersInGoodOrder (int parameter)
		{
			throw new ArgumentException ("Invalid parameter", "parameter");
		}

		[Test]
		public void SuccessOnArgumentExceptionWithTwoParametersInGoodOrderTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithTwoParametersInGoodOrder");
		}

		public void ArgumentExceptionWithTwoParametersInBadOrder (int parameter)
		{
			throw new ArgumentException ("parameter", "Invalid parameter");
		}

		[Test]
		public void SuccessOnArgumentExceptionWithTwoParametersInBadOrderTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithTwoParametersInBadOrder", 1);
		}

		public void ArgumentNullExceptionWithTwoParametersInGoodOrder (int parameter)
		{
			throw new ArgumentNullException ("parameter", "This parameter is null");
		}

		[Test]
		public void SuccessOnArgumentNullExceptionWithTwoParametersInGoodOrderTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentNullExceptionWithTwoParametersInGoodOrder");
		}

		public void ArgumentNullExceptionWithTwoParametersInBadOrder (int parameter)
		{
			throw new ArgumentNullException ("This parameter is null", "parameter");
		}

		[Test]
		public void FailOnArgumentNullExceptionWithTwoParametersInBadOrderTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentNullExceptionWithTwoParametersInBadOrder", 1);
		}

		public void ArgumentOutOfRangeExceptionWithTwoParametersInGoodOrder (int parameter)
		{
			throw new ArgumentOutOfRangeException ("parameter", "This parameter is out of order");
		}

		[Test]
		public void SuccessOnArgumentOutOfRangeExceptionWithTwoParametersInGoodOrderTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentOutOfRangeExceptionWithTwoParametersInGoodOrder");
		}	
		
		public void ArgumentOutOfRangeExceptionWithTwoParametersInBadOrder (int parameter)
		{
			throw new ArgumentOutOfRangeException ("This parameter is out of order", "parameter");
		}

		[Test]
		public void FailOnArgumentOutOfRangeExceptionWithTwoParametersInBadOrderTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentOutOfRangeExceptionWithTwoParametersInBadOrder", 1);
		}

		public void DuplicateWaitObjectExceptionWithTwoParametersInGoodOrder (int parameter)
		{
			throw new DuplicateWaitObjectException ("parameter", "A simple message");
		}

		[Test]
		public void SuccessOnDuplicateWaitObjectExceptionWithTwoParametersInGoodOrderTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("DuplicateWaitObjectExceptionWithTwoParametersInGoodOrder");
		}

		public void DuplicateWaitObjectExceptionWithTwoParametersInBadOrder (int parameter)
		{
			throw new DuplicateWaitObjectException ("A simple message", "parameter");
		}

		[Test]
		public void FailOnDuplicateWaitObjectExceptionWithTwoParametersInBadOrderTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("DuplicateWaitObjectExceptionWithTwoParametersInBadOrder", 1);
		}

		//

		public void ArgumentExceptionWithOneArgument (int parameter)
		{
			throw new ArgumentException ("parameter");
		}

		[Test]
		public void FailOnArgumentExceptionWithOneArgumentTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithOneArgument", 1);
		}

		public void ArgumentExceptionWithOneMessage (int parameter)
		{
			throw new ArgumentException ("Invalid parameter");
		}

		[Test]
		public void SuccessOnArgumentExceptionWithOneMessageTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithOneMessage");
		}

		public void ArgumentNullExceptionWithOneParameter (int parameter)
		{
			throw new ArgumentNullException ("parameter");
		}

		[Test]
		public void SuccessOnArgumentNullExceptionWithOneParameterTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentNullExceptionWithOneParameter");
		}

		public void ArgumentNullExceptionWithOneMessage (int parameter)
		{
			throw new ArgumentNullException ("This argument is null " + parameter);
		}

		[Test]
		public void FailOnArgumentNullExceptionWithOneMessageTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentNullExceptionWithOneMessage", 1);
		}

		public void ArgumentOutOfRangeExceptionWithOneParameter (int parameter)
		{
			throw new ArgumentOutOfRangeException ("parameter");
		}

		[Test]
		public void SuccessOnArgumentOutOfRangeExceptionWithOneParameterTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentOutOfRangeExceptionWithOneParameter");
		}

		public void ArgumentOutOfRangeExceptionWithOneMessage (int parameter)
		{
			throw new ArgumentOutOfRangeException ("The parameter is out of range");
		}

		[Test]
		public void FailOnArgumentOutOfRangeExceptionWithOneMessageTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentOutOfRangeExceptionWithOneMessage", 1);
		}

		public void DuplicateWaitObjectExceptionWithOneParameter (int parameter)
		{
			throw new DuplicateWaitObjectException ("parameter");
		}

		[Test]
		public void SuccessOnDuplicateWaitObjectExceptionWithOneParameterTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("DuplicateWaitObjectExceptionWithOneParameter");
		}

		public void DuplicateWaitObjectExceptionWithOneMessage (int parameter)
		{
			throw new DuplicateWaitObjectException ("There are a duplicate wait object.");
		}

		[Test]
		public void FailOnDuplicateWaitObjectExceptionWithOneMessageTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("DuplicateWaitObjectExceptionWithOneMessage", 1);
		}

		public void ArgumentExceptionWithOtherConstructor ()
		{
			throw new ArgumentException ("A sample message" , new Exception ("Other message"));
		}

		[Test]
		public void SuccessOnArgumentExceptionWithOtherConstructorTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithOtherConstructor");
		}

		public void ArgumentExceptionWithMessageAndConditionals (Array array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new IndexOutOfRangeException ("Index was outside the bounds of the array.");
			if (array.Rank > 1)
				throw new ArgumentException ("array is multidimensional");
			if ((array.Length > 0) && (index >= array.Length))
				throw new IndexOutOfRangeException ("Index was outside the bounds of the array.");
			if (index > array.Length)
				throw new IndexOutOfRangeException ("Index was outside the bounds of the array.");
		
			IEnumerator it = null;
			int i = index;
			while (it.MoveNext ()) {
				array.SetValue (it.Current, i++);
			}
		}

		[Test]
		public void SuccessOnArgumentExceptionWithMessageAndConditionalsTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithMessageAndConditionals");
		}

		public void MixedArgumentExceptionsAndConditionals (string name, int versionRequiredToExtract, int madeByInfo)
		{
			if (name == null)  {
				//failed here
				throw new System.ArgumentNullException("ZipEntry name");
			}

			if ( name.Length == 0 ) {
				throw new ArgumentException("ZipEntry name is empty");
			}

			if (versionRequiredToExtract != 0 && versionRequiredToExtract < 10) {
				throw new ArgumentOutOfRangeException("versionRequiredToExtract");
			}
		}
		
		[Test]
		public void FailOnMixedArgumentExceptionsAndConditionalsTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("MixedArgumentExceptionsAndConditionals", 1);
		}
		
		private string GetString (string message)
		{
			return message;
		}
	
		public void ArgumentExceptionsWithTranslatedMessage (int parameter)
		{
			throw new ArgumentException (GetString("Error"), "parameter");
		}
		
		[Test]
		public void SuccessOnArgumentExceptionsWithTranslatedMessageTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionsWithTranslatedMessage");
		}

		public void ArgumentExceptionWithTranslatedInverted (int parameter)
		{
			throw new ArgumentException ("parameter", GetString ("Error"));

		}

		[Test]
		public void FailOnArgumentExceptionWithTranslatedInvertedTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithTranslatedInverted", 1);
		}

		public void ArgumentExceptionWithTranslatedDescription (int parameter)
		{
			throw new ArgumentException (GetString ("Error"));
		}

		[Test]
		public void SuccessOnArgumentExceptionWithTranslatedDescriptionTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithTranslatedDescription");
		}

		public int WellNamedProperty {
			set {
				if (value == 0)
					throw new ArgumentException ("The parameter is zero.", "WellNamedProperty");
			}
		}

		[Test]
		public void SuccessOnWellNamedPropertyTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("set_WellNamedProperty");
		}

		public int BadNamedProperty {
			set {
				if (value == 0)
					throw new ArgumentException ("The parameter is zero.", "value");
			}
		}

		[Test]
		public void FailOnBadNamedPropertyTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("set_BadNamedProperty", 1);
		}

		public int WellNamedPropertyWithArgumentNullException {
			set {
				throw new ArgumentNullException ("WellNamedPropertyWithArgumentNullException");
			}
		}

		[Test]
		public void SuccessOnWellNamedPropertyWithArgumentNullExceptionTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("set_WellNamedPropertyWithArgumentNullException");
		}

		public int WellNamedPropertyWithArgumentExceptionAndOneParameter {
			set {
				throw new ArgumentException ("This is a sample description.");
			}
		}

		[Test]
		public void SuccessOnWellNamedPropertyWithArgumentExceptionAndOneParameterTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("set_WellNamedPropertyWithArgumentExceptionAndOneParameter");
		}

		public int BadNamedPropertyWithArgumentExceptionAndOneParameter {
			set {
				throw new ArgumentException ("BadNamedPropertyWithArgumentExceptionAndOneParameter");
			}
		}

		[Test]
		public void FailOnBadNamedPropertyWithArgumentExceptionAndOneParameterTest ()
		{
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("set_BadNamedPropertyWithArgumentExceptionAndOneParameter", 1);
		}

		public ArgumentException MethodReturningException (int value)
		{
			return new ArgumentException ("value");
		}

		[Test]
		public void SkipOnMethodReturningExceptionTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningException");
		}
	}
}
