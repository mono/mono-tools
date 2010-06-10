//
// Unit tests for UseStringEmptyRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

	[TestFixture]
	public class UseStringEmptyTest : MethodRuleTestFixture<UseStringEmptyRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}
	
		public class TestCase {
		
			public const string public_const_field = "";
			
			private static string private_static_field = "";
			
			
			public string GetConstField ()
			{
				return public_const_field;
			}
			
			public string GetStaticField ()
			{
				return private_static_field;
			}
			
			public string Append (string user_value)
			{
				return user_value + "";
			}
			
			public string Enclose (string user_value)
			{
				return "" + user_value + "";
			}
						
			// nice way

			public string public_field = "";
			
			public string GetField ()
			{
				return public_field;
			}
			
			public string Prepend (string user_value)
			{
				return String.Empty + user_value;
			}
			
			public int NoStringWereHarmedInThisTestCase ()
			{
				return 42;
			}

			public void SmallSwitch (string s)
			{
				switch (s) {
				// case String.Empty: error CS0150: A constant value is expected
				case "":
					Console.WriteLine ("empty");
					break;
				default:
					Console.WriteLine (s);
					break;
				}
			}

			public void LargeSwitch (string s)
			{
				switch (s) {
				// case String.Empty: error CS0150: A constant value is expected
				case "":
					Console.WriteLine ("unknown");
					break;
				case "zero":
					Console.WriteLine ("0");
					break;
				case "one":
					Console.WriteLine ("1");
					break;
				case "two":
					Console.WriteLine ("2");
					break;
				case "three":
					Console.WriteLine ("3");
					break;
				case "four":
					Console.WriteLine ("4");
					break;
				default:
					Console.WriteLine ("large value");
					break;
				}
			}
		}

		[Test]
		public void GetConstField ()
		{
			AssertRuleFailure<TestCase> ("GetConstField", 1);
		}

		[Test]
		public void Append ()
		{
			AssertRuleFailure<TestCase> ("Append", 1);
		}

		[Test]
		public void Enclose ()
		{
			// this could be one (csc) or two ([g]mcs) defects depending on how this is compiled
			AssertRuleFailure<TestCase> ("Enclose");
		}

		[Test]
		public void Constructor ()
		{
			// the "public_field" field is set to "" in the (hidden) ctor
			AssertRuleFailure<TestCase> (".ctor", 1);
		}

		[Test]
		public void StaticConstructor ()
		{
			// the "private_static_field" field is set to "" in the (hidden) class ctor
			AssertRuleFailure<TestCase> (".cctor", 1);
		}

		[Test]
		public void GetField ()
		{
			AssertRuleDoesNotApply<TestCase> ("GetField");
		}

		[Test]
		public void GetStaticField ()
		{
			AssertRuleDoesNotApply<TestCase> ("GetStaticField");
		}

		[Test]
		public void Prepend ()
		{
			AssertRuleDoesNotApply<TestCase> ("Prepend");
		}
		
		[Test]
		public void NoHarm ()
		{
			AssertRuleDoesNotApply<TestCase> ("NoStringWereHarmedInThisTestCase");
		}

		[Test]
		[Ignore ("switch/case optimized into if/else by compilers")]
		public void Switch ()
		{
			// compilers optimize the switch into a bunch of if/else - however, syntax wise,
			// String.Empty cannot be used in a swicth/case. Undetectable IL wise
			AssertRuleSuccess<TestCase> ("SmallSwitch");
			AssertRuleSuccess<TestCase> ("LargeSwitch");
		}
	}
}
