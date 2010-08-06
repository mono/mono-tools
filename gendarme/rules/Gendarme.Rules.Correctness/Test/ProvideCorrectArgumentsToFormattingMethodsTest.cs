//
// Unit tests for ProvideCorrectArgumentsToFormattingMethodsRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//	Antoine Vandecreme  <avandecreme@sopragroup.com>
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
using System.Globalization;
using Gendarme.Rules.Correctness;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;
using Tests.Rules.Correctness;

namespace Test.Rules.Correctness {
	[TestFixture]
	public class ProvideCorrectArgumentsToFormattingMethodsTest : MethodRuleTestFixture<ProvideCorrectArgumentsToFormattingMethodsRule> {
		[Test]
		public void SkipOnBodylessMethodsTest ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		[Test]
		public void SkipOnEmptyMethodTest ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}
		
		class FormattingCases {
			public void MethodWithBadFormatting (object value)
			{
				String.Format ("The value {0} isn't valid");
			}
		
			public void MethodWithGoodFormatting (object value)
			{
				String.Format ("The value {0} isn't valid", value);
			}

			public void MethodWithGoodFormattingAndThreeParams (object value1, object value2, object value3)
			{
				String.Format ("{0} {1} {2}", value1, value2, value3);
			}

			public void MethodWithGoodFormattingAndFiveParams (object value1, object value2, object value3, object value4, object value5)
			{
				String.Format ("{0} {1} {2} {3} {4}", value1, value2, value3, value4, value5);
			}

			public void MethodWithGoodFormattingAndSomeCalls (object value1, object value2)
			{
				String.Format ("{0} {1}", value1.ToString (), value2.ToString ());
			}

			public void MethodWithGoodFormattingAndDateTimes (DateTime dateTime)
			{
				String.Format ("'{0:yyyy-MM-dd HH:mm:ss}'", dateTime);
			}

			public void MethodWithGoodFormattingAndRepeatedIndexes (object value)
			{
				String.Format ("{0} - {0}", value);
			}

			public void MethodWithSpecialCharacters (object value)
			{
				String.Format ("The {2} '{0}' is not valid in the locked list for this section.  The following {3} can be locked: '{1}'", value, value, value, value);
			}

			public void MethodWithGoodFormattingButWithMultipleBrackets (int height, int width)
			{
				String.Format ("{{Width={0}, Height={1}}}", width, height);
			}

			public void MethodWithGoodFormattingLoadingFromLocal ()
			{
				string message = "The error {0} is not valid";
				string val = "Foo";
				String.Format (message, val);
			}

			public void MethodWithBadFormattingLoadingFromLocal ()
			{
				string message = "The error {0} is not valid";
				string val = "Foo";
				String.Format (message);
			}

			public void MethodWithoutParameters ()
			{
				String.Format ("I forget include parameters.");
			}

			public void MethodCallingEnumFormat (Type type, object value)
			{
				Enum.Format (type, value, "G");
			}

			public void MethodWithGoodFormattingAndArrayParameter ()
			{
				object [] values = { "value1", "value2", "value3" };
				object [] notValues = { "notValue1", "notValue2" };
				String.Format ("3 values : {0} {1} {2}", values);
			}

			public void MethodWithBadFormattingAndArrayParameter ()
			{
				object [] notValues = { "notValue1", "notValue2" };
				object [] values = { "value1", "value2", "value3" };
				String.Format ("3 values : {0} {1} {2}", notValues);
			}

			private string GetGoodFormat ()
			{
				return "{0} - {1}";
			}
			public void MethodCallingMethodToGetGoodFormat ()
			{
				String.Format (GetGoodFormat (), "value1", "value2");
			}

			public void MethodWithGoodFormattingAndIFormatProvider ()
			{
				String.Format (new CultureInfo ("en-US"), "Format : {0}", "value");
			}

			public void MethodWithBadFormattingAndIFormatProvider ()
			{
				String.Format (new CultureInfo ("en-US"), "Format : {0} {1}", "value1");
			}

			public void MethodWithGoodFormattingAndStringResource ()
			{
				String.Format (Resource.OneParameter, "parameter1");
				String.Format (Resource.TwoParameter, "parameter1", "parameter2");
			}

			public void MethodWithBadFormattingAndStringResource ()
			{
				String.Format (Resource.OneParameter, "parameter1", "parameter2");
				String.Format (Resource.TwoParameter, "parameter1");
			}
		}

		[Test]
		public void FailOnMethodWithBadFormattingTest ()
		{
			AssertRuleFailure<FormattingCases> ("MethodWithBadFormatting", 1);
		}

		[Test]
		public void SuccessOnMethodWithGoodFormattingTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormatting");
		}
		
		[Test]
		public void SuccessOnMethodWithGoodFormattingAndThreeParamsTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingAndThreeParams");
		}
		
		[Test]
		public void SuccessOnMethodWithGoodFormattingAndFiveParamsTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingAndFiveParams");
		}

		[Test]
		public void SuccessOnMethodWithGoodFormattingAndSomeCallsTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingAndSomeCalls");
		}

		[Test]
		public void SuccessOnMethodWithGoodFormattingAndDateTimesTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingAndDateTimes");
		}

		[Test]
		public void SuccessOnMethodWithGoodFormattingAndRepeatedIndexesTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingAndRepeatedIndexes");
		}

		[Test]
		public void SuccessOnMethodWithSpecialCharactersTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithSpecialCharacters");
		}

		[Test]
		public void SuccessOnMethodWithGoodFormattingButWithMultipleBracketsTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingButWithMultipleBrackets");
		}

		[Test]
		public void SuccessOnMethodWithGoodFormattingLoadingFromLocalTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingLoadingFromLocal");
		}

		[Test]
		public void FailOnMethodWithBadFormattingLoadingFromLocalTest ()
		{
			AssertRuleFailure<FormattingCases> ("MethodWithBadFormattingLoadingFromLocal", 1);
		}

		[Test]
		public void FailOnMethodWithoutParametersTest ()
		{
			AssertRuleFailure<FormattingCases> ("MethodWithoutParameters", 1);
		}

		[Test]
		public void SkipOnMethodCallingEnumFormatTest ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodCallingEnumFormat");
		}

		[Test]
		public void SuccessOnMethodWithGoodFormattingAndArrayParameter ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingAndArrayParameter");
		}

		[Test]
		public void FailOnMethodWithBadFormattingAndArrayParameter ()
		{
			AssertRuleFailure<FormattingCases> ("MethodWithBadFormattingAndArrayParameter");
		}

		[Test]
		public void SuccessOnMethodCallingMethodToGetGoodFormat ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodCallingMethodToGetGoodFormat");
		}

		[Test]
		public void SuccessOnMethodWithGoodFormattingAndIFormatProvider ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingAndIFormatProvider");
		}

		[Test]
		public void FailOnMethodWithBadFormattingAndIFormatProvider ()
		{
			AssertRuleFailure<FormattingCases> ("MethodWithBadFormattingAndIFormatProvider");
		}

		[Test]
		public void SuccessOnMethodWithGoodFormattingAndStringResource ()
		{
			AssertRuleSuccess<FormattingCases> ("MethodWithGoodFormattingAndStringResource");
		}

		[Test]
		public void FailOnMethodWithBadFormattingAndStringResource ()
		{
			AssertRuleFailure<FormattingCases> ("MethodWithBadFormattingAndStringResource", 2);
		}

		string InstanceLocalize (string s)
		{
			return s;
		}

		void ParseUsingInstanceMethod ()
		{
			try {
				throw new NotSupportedException ();
			}
			catch (Exception e) {
				throw new FormatException (String.Format (InstanceLocalize ("{0} {1}"), e.Source, e), e);
			}
		}

		static string StaticLocalize (string s)
		{
			return s;
		}

		void ParseUsingStaticMethod ()
		{
			try {
				throw new NotSupportedException ();
			}
			catch (Exception e) {
				throw new FormatException (String.Format (StaticLocalize ("{0} {1}"), e.Source, e), e);
			}
		}

		[Test]
		public void NoConstantString ()
		{
			AssertRuleSuccess<ProvideCorrectArgumentsToFormattingMethodsTest> ("ParseUsingInstanceMethod");
			AssertRuleSuccess<ProvideCorrectArgumentsToFormattingMethodsTest> ("ParseUsingStaticMethod");
		}

		string GetFormat (string a)
		{
			return String.Format ("{{0", a);
		}

		string End (string a)
		{
			return String.Format ("{", a);
		}

		string ConditionalIfString (bool value)
		{
			return String.Format ("{0} {1}", value ? "true" : "false", "uho");
		}

		string ConditionalIfCalls (bool value)
		{
			return String.Format ("{0} {1}", value ? ConditionalIfString (value) : ConditionalIfString (!value), "uho");
		}

		[Test]
		public void SpecialCase ()
		{
			AssertRuleFailure<ProvideCorrectArgumentsToFormattingMethodsTest> ("GetFormat", 1);
			AssertRuleFailure<ProvideCorrectArgumentsToFormattingMethodsTest> ("End", 1);

			AssertRuleSuccess<ProvideCorrectArgumentsToFormattingMethodsTest> ("ConditionalIfString");
			AssertRuleSuccess<ProvideCorrectArgumentsToFormattingMethodsTest> ("ConditionalIfCalls");
		}
	}
}
