//
// Unit tests for InstantiateArgumentExceptionsCorrectlyRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2008 Néstor Salceda
// Copyright (C) 2008,2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Rules.Exceptions;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Exceptions {

	[TestFixture]
	public class InstantiateArgumentExceptionCorrectlyTest : MethodRuleTestFixture<InstantiateArgumentExceptionCorrectlyRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no exception is instantiated
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
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
		public void SuccessOnArgumentExceptionWithOneArgumentTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("ArgumentExceptionWithOneArgument");
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

		public void ArgumentExceptionWithOtherConstructor (int x)
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
			Assert.AreEqual (Severity.Low, Runner.Defects [0].Severity, "Low");
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

		public int ArgumentExceptionProperty {
			get {
				// public ArgumentException ()
				// i.e. we don't care about this ctor (no parameter)
				throw new ArgumentException ("uho");
			}
			set {
				// public ArgumentException (string message, string paramName)
				// i.e. we CARE about this ctor second parameter (paramName)
				throw new ArgumentException ("ArgumentExceptionProperty", "uho");
			}
		}

		public int ArgumentNullExceptionProperty {
			get {
				// public ArgumentNullException (string paramName)
				// i.e. we CARE about this ctor single parameter (paramName)
				throw new ArgumentNullException ("uho");
			}
			set {
				// public ArgumentNullException (string paramName, string message)
				// i.e. we CARE about this ctor first parameter (paramName)
				throw new ArgumentNullException ("ArgumentNullExceptionProperty", "aha");
			}
		}

		[Test]
		public void PropertiesTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("get_ArgumentExceptionProperty");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("set_ArgumentExceptionProperty", 1);

			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("get_ArgumentNullExceptionProperty", 1);
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("set_ArgumentNullExceptionProperty");
		}

		public ArgumentException MethodReturningArgumentException_Empty (int value)
		{
			// public ArgumentException ()
			// i.e. we don't care about this ctor (no parameter)
			return new ArgumentException ();
		}

		public ArgumentException MethodReturningArgumentException_String (int value)
		{
			// public ArgumentException (string message)
			// i.e. we don't care about this ctor (no paramName)
			return new ArgumentException ("value");
		}

		public ArgumentException MethodReturningArgumentException_StringException (int value)
		{
			// public ArgumentException (string message, Exception innerException)
			// i.e. we don't care about this ctor (no paramName)
			return new ArgumentException ("heho", new Exception ());
		}

		public ArgumentException MethodReturningArgumentException_StringString_Good (int value)
		{
			// public ArgumentException (string message, string paramName)
			// i.e. we CARE about this ctor second parameter (paramName)
			return new ArgumentException ("heho", "value");
		}

		public ArgumentException MethodReturningArgumentException_StringString_Bad (int value)
		{
			// public ArgumentException (string message, string paramName)
			// i.e. we CARE about this ctor second parameter (paramName)
			return new ArgumentException ("value", "heho");
		}

		public ArgumentException MethodReturningArgumentException_StringStringException_Good (int value)
		{
			// public ArgumentException (string message, string paramName, Exception innerException)
			// i.e. we CARE about this ctor second parameter (paramName)
			return new ArgumentException ("heho", "value", new Exception ());
		}

		public ArgumentException MethodReturningArgumentException_StringStringException_Bad (int value)
		{
			// public ArgumentException (string message, string paramName, Exception innerException)
			// i.e. we CARE about this ctor second parameter (paramName)
			return new ArgumentException ("value", "heho", new Exception ());
		}

		[Test]
		public void MethodReturningArgumentException ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentException_Empty");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentException_String");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentException_StringException");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentException_StringString_Good");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentException_StringString_Bad");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentException_StringStringException_Good");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentException_StringStringException_Bad");
		}

		public ArgumentNullException MethodReturningArgumentNullException_Empty (int value)
		{
			// public ArgumentNullException ()
			// i.e. we don't care about this ctor (no parameter)
			return new ArgumentNullException ();
		}

		public ArgumentNullException MethodReturningArgumentNullException_StringOk (int value)
		{
			// public ArgumentNullException (string paramName)
			// i.e. we CARE about this ctor single parameter (paramName)
			return new ArgumentNullException ("value");
		}

		public ArgumentNullException MethodReturningArgumentNullException_StringBad (int value)
		{
			// public ArgumentNullException (string paramName)
			// i.e. we CARE about this ctor single parameter (paramName)
			return new ArgumentNullException ("heho");
		}

		public ArgumentNullException MethodReturningArgumentNullException_StringException (int value)
		{
			// public ArgumentNullException (string message, Exception innerException)
			// i.e. we don't care about this ctor (no paramName)
			return new ArgumentNullException ("heho", new Exception ());
		}

		public ArgumentNullException MethodReturningArgumentNullException_StringString_Good (int value)
		{
			// public ArgumentNullException (string paramName, string message)
			// i.e. we CARE about this ctor first parameter (paramName)
			return new ArgumentNullException ("value", "heho");
		}

		public ArgumentNullException MethodReturningArgumentNullException_StringString_Bad (int value)
		{
			// public ArgumentNullException (string paramName, string message)
			// i.e. we CARE about this ctor first parameter (paramName)
			return new ArgumentNullException ("heho", "value");
		}

		[Test]
		public void MethodReturningArgumentNullException ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentNullException_Empty");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentNullException_StringOk");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentNullException_StringBad", 1);
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentNullException_StringException");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentNullException_StringString_Good");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentNullException_StringString_Bad", 1);
		}

		public ArgumentOutOfRangeException MethodReturningArgumentOutOfRangeException_Empty (int value)
		{
			// public ArgumentOutOfRangeException ()
			// i.e. we don't care about this ctor (no parameter)
			return new ArgumentOutOfRangeException ();
		}

		public ArgumentOutOfRangeException MethodReturningArgumentOutOfRangeException_StringOk (int value)
		{
			// public ArgumentOutOfRangeException (string paramName)
			// i.e. we CARE about this ctor single parameter (paramName)
			return new ArgumentOutOfRangeException ("value");
		}

		public ArgumentOutOfRangeException MethodReturningArgumentOutOfRangeException_StringBad (int value)
		{
			// public ArgumentOutOfRangeException (string paramName)
			// i.e. we CARE about this ctor single parameter (paramName)
			return new ArgumentOutOfRangeException ("heho");
		}

		public ArgumentOutOfRangeException MethodReturningArgumentOutOfRangeException_StringException (int value)
		{
			// public ArgumentOutOfRangeException (string message, Exception innerException)
			// i.e. we don't care about this ctor (no paramName)
			return new ArgumentOutOfRangeException ("heho", new Exception ());
		}

		public ArgumentOutOfRangeException MethodReturningArgumentOutOfRangeException_StringString_Good (int value)
		{
			// public ArgumentOutOfRangeException (string paramName, string message)
			// i.e. we CARE about this ctor first parameter (paramName)
			return new ArgumentOutOfRangeException ("value", "heho");
		}

		public ArgumentOutOfRangeException MethodReturningArgumentOutOfRangeException_StringString_Bad (int value)
		{
			// public ArgumentOutOfRangeException (string paramName, string message)
			// i.e. we CARE about this ctor first parameter (paramName)
			return new ArgumentOutOfRangeException ("heho", "value");
		}

		[Test]
		public void MethodReturningArgumentOutOfRangeException ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentOutOfRangeException_Empty");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentOutOfRangeException_StringOk");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentOutOfRangeException_StringBad", 1);
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentOutOfRangeException_StringException");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentOutOfRangeException_StringString_Good");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningArgumentNullException_StringString_Bad", 1);
		}

		public DuplicateWaitObjectException MethodReturningDuplicateWaitObjectException_Empty (int value)
		{
			// public DuplicateWaitObjectException ()
			// i.e. we don't care about this ctor (no parameter)
			return new DuplicateWaitObjectException ();
		}

		public DuplicateWaitObjectException MethodReturningDuplicateWaitObjectException_StringOk (int value)
		{
			// public DuplicateWaitObjectException (string paramName)
			// i.e. we CARE about this ctor single parameter (paramName)
			return new DuplicateWaitObjectException ("value");
		}

		public DuplicateWaitObjectException MethodReturningDuplicateWaitObjectException_StringBad (int value)
		{
			// public DuplicateWaitObjectException (string paramName)
			// i.e. we CARE about this ctor single parameter (paramName)
			return new DuplicateWaitObjectException ("heho");
		}

		public DuplicateWaitObjectException MethodReturningDuplicateWaitObjectException_StringException (int value)
		{
			// public DuplicateWaitObjectException (string message, Exception innerException)
			// i.e. we don't care about this ctor (no paramName)
			return new DuplicateWaitObjectException ("heho", new Exception ());
		}

		public DuplicateWaitObjectException MethodReturningDuplicateWaitObjectException_StringString_Good (int value)
		{
			// public DuplicateWaitObjectException (string paramName, string message)
			// i.e. we CARE about this ctor first parameter (paramName)
			return new DuplicateWaitObjectException ("value", "heho");
		}

		public DuplicateWaitObjectException MethodReturningDuplicateWaitObjectException_StringString_Bad (int value)
		{
			// public DuplicateWaitObjectException (string paramName, string message)
			// i.e. we CARE about this ctor first parameter (paramName)
			return new DuplicateWaitObjectException ("heho", "value");
		}

		[Test]
		public void MethodReturningDuplicateWaitObjectException ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningDuplicateWaitObjectException_Empty");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningDuplicateWaitObjectException_StringOk");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningDuplicateWaitObjectException_StringBad", 1);
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningDuplicateWaitObjectException_StringException");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningDuplicateWaitObjectException_StringString_Good");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("MethodReturningDuplicateWaitObjectException_StringString_Bad", 1);
		}

		public void MessageLoadedFromLocal (int value)
		{
			string msg = "The parameter: {0} is out of range";
			// public ArgumentOutOfRangeException (string paramName, string message)
			throw new ArgumentOutOfRangeException ("value", msg);
		}

		[Test]
		public void SuccessOnMessageLoadedFromLocalTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("MessageLoadedFromLocal");
		}

		public void StringFormatForMessage (int value)
		{
			// public ArgumentException (string message, string paramName)
			throw new ArgumentException (String.Format ("value {0} is bad", value), "value");
		}

		[Test]
		public void ArgumentExceptionTest ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("StringFormatForMessage");
		}

		// adapted from NamespaceEngine.cs 
		// gmcs creates an inner type with fields and the exception can be thrown from there (without parameter)
		class CompilerGeneratedInnerIterator {
			private static IList<TypeDefinition> types;

			public static IEnumerable<TypeDefinition> TypesInside (string nameSpace)
			{
				if (nameSpace == null)
					throw new ArgumentNullException ("nameSpace");

				foreach (TypeDefinition type in types) {
					yield return type;
				}
			}
		}

		[Test]
		public void Yield ()
		{
			AssertRuleSuccess<CompilerGeneratedInnerIterator> ("TypesInside");
			TypeDefinition inner = (DefinitionLoader.GetTypeDefinition<CompilerGeneratedInnerIterator> ().NestedTypes [0] as TypeDefinition);
			foreach (MethodDefinition method in inner.Methods) {
				AssertRuleDoesNotApply (method);
			}
		}

		public void CallLocalizedThrow ()
		{
			throw new ArgumentNullException ("obj", "a localized string");
		}

		public int DoThis (object obj)
		{
			if (obj == null)
				CallLocalizedThrow ();
			return obj.GetHashCode ();
		}

		[Test]
		public void CheckThenDelegateThrow ()
		{
			// no exception throw (or created)
			AssertRuleDoesNotApply<InstantiateArgumentExceptionCorrectlyTest> ("DoThis");
			// no parameter to check against
			AssertRuleDoesNotApply<InstantiateArgumentExceptionCorrectlyTest> ("CallLocalizedThrow");
		}

		static public int GoodStaticMethod (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			return obj.GetHashCode ();
		}

		static public void GoodStaticMethod2 (object obj, string s)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");
			if (String.IsNullOrEmpty (s))
				throw new ArgumentNullException ("s");
			Console.WriteLine ("{0}: {1}", s, obj.GetHashCode ());
		}

		static public int BadStaticMethod (object obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("object");
			return obj.GetHashCode ();
		}

		static public void BadStaticMethod2 (object obj, string s)
		{
			if (obj == null)
				throw new ArgumentNullException ("object");
			if (String.IsNullOrEmpty (s))
				throw new ArgumentNullException ("string");
			Console.WriteLine ("{0}: {1}", s, obj.GetHashCode ());
		}

		static public void BadStaticMethod2half (object objectHash, string stringParameterName)
		{
			if (objectHash == null)
				throw new ArgumentNullException ("objectHash");
			if (String.IsNullOrEmpty (stringParameterName))
				throw new ArgumentNullException ("stringParameter");
			Console.WriteLine ("{0}: {1}", stringParameterName, objectHash.GetHashCode ());
		}

		[Test]
		public void StaticMethods ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("GoodStaticMethod");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("BadStaticMethod", 1);

			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("GoodStaticMethod2");
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("BadStaticMethod2", 2);
			AssertRuleFailure<InstantiateArgumentExceptionCorrectlyTest> ("BadStaticMethod2half", 1);
		}

		public object this [int index] {
			get {
				throw new ArgumentOutOfRangeException ("index");
			}
			set {
				throw new ArgumentOutOfRangeException ("index", "foo");
			}
		}

		[Test]
		public void Indexer ()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("get_Item");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest> ("set_Item");
		}

		public void ArgumentNullExceptionWithBranchInMessageSelectionAndIncorrectParameterName(int parameter)
		{
			throw new ArgumentNullException("asdf", parameter > 0 ? "little parameter " : "big parameter");
		}

		public void ArgumentOutOfRangeExceptionWithBranchInMessageSelectionAndIncorrectParameterName(int parameter)
		{
			throw new ArgumentOutOfRangeException("asdf", parameter > 0 ? "little parameter " : "big parameter");
		}

		public void DuplicateWaitObjectExceptionWithBranchInMessageSelectionAndIncorrectParameterName(int parameter)
		{
			throw new DuplicateWaitObjectException("asdf", parameter > 0 ? "little parameter " : "big parameter");
		}

		public void ArgumentNullExceptionWithBranchInMessageSelectionAndCorrectParameterName(int parameter)
		{
			throw new ArgumentNullException("parameter", parameter > 0 ? "little parameter " : "big parameter");
		}

		public void ArgumentOutOfRangeExceptionWithBranchInMessageSelectionAndCorrectParameterName(int parameter)
		{
			throw new ArgumentOutOfRangeException("parameter", parameter > 0 ? "little parameter " : "big parameter");
		}

		public void DuplicateWaitObjectExceptionWithBranchInMessageSelectionAndCorrectParameterName(int parameter)
		{
			throw new DuplicateWaitObjectException("parameter", parameter > 0 ? "little parameter " : "big parameter");
		}

		[Test]
		public void ArgumentExceptionsWithBranchInMessageSelectionDoesNotThrow()
		{
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest>("ArgumentNullExceptionWithBranchInMessageSelectionAndCorrectParameterName");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest>("ArgumentOutOfRangeExceptionWithBranchInMessageSelectionAndCorrectParameterName");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest>("DuplicateWaitObjectExceptionWithBranchInMessageSelectionAndCorrectParameterName");

			//Ideally the rules below would be AssertRuleFailure - but better to incorrectly pass than to throw an unhandled exception
			// If the handling code is changed a better resolution here may be possible
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest>("ArgumentNullExceptionWithBranchInMessageSelectionAndIncorrectParameterName");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest>("ArgumentOutOfRangeExceptionWithBranchInMessageSelectionAndIncorrectParameterName");
			AssertRuleSuccess<InstantiateArgumentExceptionCorrectlyTest>("DuplicateWaitObjectExceptionWithBranchInMessageSelectionAndIncorrectParameterName");
		}
	}
}
