//
// Unit tests for ProvideTryParseAlternativeRule
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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


using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class ProvideTryParseAlternativeTest : TypeRuleTestFixture<ProvideTryParseAlternativeRule> {

		class ClassWithoutMethods {
		}
		
		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
		}

		interface IParseOnly {
			IParseOnly Parse (string s);
		}

		interface ITryParseToo {
			ITryParseToo Parse (string s);
			bool TryParse (string s, out ITryParseToo value);
		}

		[Test]
		public void Interfaces ()
		{
			AssertRuleFailure<IParseOnly> (1);
			AssertRuleSuccess<ITryParseToo> ();
		}

		class ClassParseOnly {
			ClassParseOnly Parse (string s)
			{
				return null;
			}
		}

		class ClassTryParseToo {
			ClassTryParseToo Parse (string s)
			{
				return null;
			}

			bool TryParse (string s, out ClassTryParseToo value)
			{
				value = null;
				return false;
			}
		}

		[Test]
		public void Classes ()
		{
			AssertRuleFailure<ClassParseOnly> (1);
			AssertRuleSuccess<ClassTryParseToo> ();
			AssertRuleSuccess<ClassWithoutMethods> ();
		}

		class StructParseOnly {
			StructParseOnly Parse (string s)
			{
				return null;
			}
		}

		class StructTryParseToo {
			StructTryParseToo Parse (string s)
			{
				return null;
			}

			bool TryParse (string s, out StructTryParseToo value)
			{
				value = null;
				return false;
			}
		}

		[Test]
		public void Structures ()
		{
			AssertRuleFailure<StructParseOnly> (1);
			AssertRuleSuccess<StructTryParseToo> ();
		}

		class BadTryParse {
			// bad candidate - does not return 'bool'
			static void TryParse (string s, out BadTryParse btp)
			{
				btp = new BadTryParse ();
			}

			// bad candidate - first parameter is not 'string'
			static bool TryParse (char c, out BadTryParse btp)
			{
				btp = new BadTryParse ();
				return true;
			}

			// bad candidate - last parameter is not 'out <type>'
			static bool TryParse (string s, BadTryParse btp)
			{
				btp = new BadTryParse ();
				return true;
			}

			static BadTryParse Parse (string s)
			{
				return new BadTryParse ();
			}
		}

		class BadParse {
			private bool AParse (string s)
			{
				return true;
			}

			private bool Parse ()
			{
				return true;
			}

			static bool TryParse (string s, out BadParse value)
			{
				value = null;
				return true;
			}
		}

		[Test]
		public void WrongSignature ()
		{
			// no valid TryParse was found - but the Parse is valid
			AssertRuleFailure<BadTryParse> (1);
			// no valid Parse - a TryParse without Parse is not a defect
			AssertRuleSuccess<BadParse> ();
		}
	}
}

