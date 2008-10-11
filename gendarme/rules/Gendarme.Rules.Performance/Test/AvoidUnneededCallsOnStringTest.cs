//
// Unit test for AvoidUnneededCallsOnStringRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

#pragma warning disable 649

	[TestFixture]
	public class AvoidUnneededCallsOnStringTest : MethodRuleTestFixture<AvoidUnneededCallsOnStringRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public class Item {

			private string field = "";
			private int nonStringField;
			private static int nonStringStaticField;

			public string ToStringOnLocalString ()
			{
				string a = String.Empty;
				return a.ToString ();
			}

			public string ToStringOnParameter (string param)
			{
				return param.ToString ();
			}

			public static string ToStringOnParameterStatic (string param)
			{
				return param.ToString ();
			}

			public string ToStringOnStaticField ()
			{
				return String.Empty.ToString ();
			}

			public string ToStringOnField ()
			{
				return field.ToString ();
			}

			public string ToStringOnMethodResult ()
			{
				return String.Empty.ToLower ().ToString ();
			}

			private int ReturnInt ()
			{
				return 0;
			}

			public void ValidToString (int param)
			{
				int local = 0;
				string var = local.ToString ();
				var = nonStringField.ToString ();
				var = nonStringStaticField.ToString ();
				var = param.ToString ();
				var = ReturnInt ().ToString ();
			}

			public string ThisToString ()
			{
				return this.ToString ();
			}

			// ToString(IFormatProvider)

			public string ToStringIFormatProviderField ()
			{
				return String.Empty.ToString (null);
			}

			private object value;

			public string ToStringBadParameterType (string format)
			{
				return ((int) value).ToString (format);
			}

			// Clone

			public object CloneField ()
			{
				return String.Empty.Clone ();
			}

			public object Clone ()
			{
				return null;
			}

			public object ThisClone ()
			{
				return this.Clone ();
			}

			// Substring

			public object SubstringZeroField ()
			{
				return String.Empty.Substring (0);
			}

			public object SubstringOneField ()
			{
				return String.Empty.Substring (1);
			}

			public object SubstringIntInt ()
			{
				return String.Empty.Substring (0, 0);
			}

			public string Substring (int n)
			{
				return String.Empty;
			}

			public void CallSubstring ()
			{
				Console.WriteLine (Substring (0));
			}
		}

		[Test]
		public void TestLocalString ()
		{
			AssertRuleFailure<Item> ("ToStringOnLocalString", 1);
		}

		[Test]
		public void TestParameter ()
		{
			AssertRuleFailure<Item> ("ToStringOnParameter", 1);
			AssertRuleFailure<Item> ("ToStringOnParameterStatic", 1);
		}

		[Test]
		public void TestStaticField ()
		{
			AssertRuleFailure<Item> ("ToStringOnStaticField", 1);
		}

		[Test]
		public void TestField ()
		{
			AssertRuleFailure<Item> ("ToStringOnField", 1);
		}

		[Test]
		public void TestMethodResult ()
		{
			AssertRuleFailure<Item> ("ToStringOnMethodResult", 1);
		}

		[Test]
		public void TestValidToString ()
		{
			AssertRuleSuccess<Item> ("ValidToString");
		}

		[Test]
		public void ToStringIFormatProvider ()
		{
			AssertRuleFailure<Item> ("ToStringIFormatProviderField", 1);
		}

		[Test]
		public void ToStringOk ()
		{
			AssertRuleSuccess<Item> ("ToStringBadParameterType");
			AssertRuleSuccess<Item> ("ThisToString");
		}

		[Test]
		public void Clone ()
		{
			AssertRuleFailure<Item> ("CloneField", 1);
		}

		[Test]
		public void CloneOk ()
		{
			AssertRuleSuccess<Item> ("ThisClone");
		}

		[Test]
		public void Substring ()
		{
			AssertRuleFailure<Item> ("SubstringZeroField", 1);
		}

		[Test]
		public void SubstringOk ()
		{
			AssertRuleSuccess<Item> ("SubstringOneField");
			AssertRuleSuccess<Item> ("SubstringIntInt");
			AssertRuleSuccess<Item> ("CallSubstring");
		}
	}
}
