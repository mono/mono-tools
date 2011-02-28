//
// Unit Tests for AvoidNullCheckWithAsOperatorRule
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
using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class AvoidNullCheckWithAsOperatorTest : MethodRuleTestFixture<AvoidNullCheckWithAsOperatorRule> {

		string AsString_Bad1a (object obj)
		{
			return obj == null ? null : obj as string;
		}

		string AsString_Bad1b (object obj)
		{
			if (obj != null)
				return obj as string;
			return null;
		}

		string AsString_Bad2a (object obj)
		{
			return obj != null ? obj as string : null;
		}

		string AsString_Bad2b (object obj)
		{
			if (obj == null)
				return null;
			return obj as string;
		}

		string AsString_Good (object obj)
		{
			return obj as string;
		}

		string AsString_Good2 (string message, object obj)
		{
			if (message == null)
				throw new ArgumentNullException ("message");
			Console.WriteLine (message);
			return obj as string;
		}

		string AsString_Good3 (string message, object obj)
		{
			if (message == null)
				return null;
			Console.WriteLine (message);
			return obj as string;
		}

		void LocalsBad1a ()
		{
			object o = null;
			string a = o == null ? null : o as string;
			Console.WriteLine (a);
		}

		void LocalsBad2a ()
		{
			object o = null;
			string a = o != null ? o as string : null;
			Console.WriteLine (a);
		}

		// extracted from: moon/class/System.Windows/Mono.Xaml/XamlPropertySetter.cs
		// no 'as' has been harmed by this test
		object ConvertValue (object value)
		{
			if (value == null)
				return null;

			if (value is Type || value is SimpleMethods)
				return value;

			return value.ToString ();
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			// no ldnull instruction
			AssertRuleDoesNotApply<AvoidNullCheckWithAsOperatorTest> ("AsString_Good");
			AssertRuleDoesNotApply<AvoidNullCheckWithAsOperatorTest> ("AsString_Good2");
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<AvoidNullCheckWithAsOperatorTest> ("AsString_Good3");
			AssertRuleSuccess<AvoidNullCheckWithAsOperatorTest> ("ConvertValue");
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure<AvoidNullCheckWithAsOperatorTest> ("AsString_Bad1a");
			AssertRuleFailure<AvoidNullCheckWithAsOperatorTest> ("AsString_Bad1b");
			AssertRuleFailure<AvoidNullCheckWithAsOperatorTest> ("AsString_Bad2a");
			AssertRuleFailure<AvoidNullCheckWithAsOperatorTest> ("AsString_Bad2b");

			AssertRuleFailure<AvoidNullCheckWithAsOperatorTest> ("LocalsBad1a");
			AssertRuleFailure<AvoidNullCheckWithAsOperatorTest> ("LocalsBad2a");
		}
	}
}
