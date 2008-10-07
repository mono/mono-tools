//
// Unit tests for ConsiderConvertingMethodToPropertyRule
//
// Authors:
//	Adrian Tsai <adrian_tsai@hotmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) 2007 Adrian Tsai
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

using System.Collections;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class ConsiderConvertingMethodToPropertyTest : MethodRuleTestFixture<ConsiderConvertingMethodToPropertyRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.GeneratedCodeMethod);
			AssertRuleDoesNotApply <IEnumerable> ("GetEnumerator");
		}

		public class ShouldBeCaught {
			int Foo;
			bool Bar;

			int GetFoo () { return Foo; }
			bool IsBar () { return Bar; }

			bool HasFoo () { return (0 != Foo); }

			// note: won't be reported since it does not return void
			int SetFoo (int value) { return (Foo = value); }
			void SetBar (bool value) { Bar = value; }
		}

		[Test]
		public void TestShouldBeCaught ()
		{
			AssertRuleFailure<ShouldBeCaught> ("GetFoo");
			AssertRuleFailure<ShouldBeCaught> ("IsBar");
			AssertRuleFailure<ShouldBeCaught> ("HasFoo");
			AssertRuleSuccess<ShouldBeCaught> ("SetFoo");
			AssertRuleSuccess<ShouldBeCaught> ("SetBar");
		}

		public class ShouldBeIgnored {
			int getfoo;
			int GetFoo {
				get { return getfoo; }
				set { getfoo = value; }
			}
			bool HasFoo {
				get { return false; }
			}

			byte [] Baz;

			byte [] GetBaz () { return Baz; }

			byte [] HasBaz () { return null; }
		}

		[Test]
		public void TestShouldBeIgnored ()
		{
			AssertRuleDoesNotApply<ShouldBeIgnored> ("get_GetFoo");
			AssertRuleDoesNotApply<ShouldBeIgnored> ("GetBaz");
			AssertRuleDoesNotApply<ShouldBeIgnored> ("get_HasFoo");
			AssertRuleDoesNotApply<ShouldBeIgnored> ("HasBaz");
		}

		public class ShouldBeIgnoredMultipleValuesInSet {
			long value;

			public long GetMyValue ()
			{
				return value;
			}

			public void SetMyValue (int value, int factor)
			{
				this.value = (long)(value * factor);
			}
		}

		[Test]
		public void TestShouldBeIgnoredMultipleValuesInSet ()
		{
			AssertRuleFailure<ShouldBeIgnoredMultipleValuesInSet> ("GetMyValue");
			// ignored, too many parameters to be converted into a property
			AssertRuleSuccess<ShouldBeIgnoredMultipleValuesInSet> ("SetMyValue");
		}

		public class GetConstructor {
			public GetConstructor () { } // Should be ignored
		}

		[Test]
		public void TestGetConstructor ()
		{
			AssertRuleDoesNotApply<ShouldBeIgnored> (".ctor");
		}

		private void GetVoid ()
		{
		}

		private bool GetBool ()
		{
			return true;
		}

		internal class Base {
			public virtual bool GetBool ()
			{
				return true;
			}
		}

		internal class Inherit : Base {
			public override bool GetBool ()
			{
				return false;
			}
		}

		[Test]
		public void GetOnly ()
		{
			AssertRuleFailure<ConsiderConvertingMethodToPropertyTest> ("GetBool", 1);
			AssertRuleFailure<Base> ("GetBool", 1);
			// we can't blame Inherit for Base bad choice since it can be outside the
			// developer's control to change (e.g. in another assembly)
			AssertRuleDoesNotApply<Inherit> ("GetBool");
			// a Get* method that return void is badly named but it's not the issue at hand
			AssertRuleSuccess<ConsiderConvertingMethodToPropertyTest> ("GetVoid");
		}

		private void SetObject (object value)
		{
			// no getter so it should be ignored
			// otherwise by fixing this th euser would only trigger another rule
		}

		[Test]
		public void SetOnly ()
		{
			AssertRuleSuccess<ConsiderConvertingMethodToPropertyTest> ("SetObject");
		}
	}
}
