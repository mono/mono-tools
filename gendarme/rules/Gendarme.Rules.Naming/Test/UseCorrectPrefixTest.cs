//
// Unit Test for UseCorrectPrefixRule
//
// Authors:
//      Abramov Daniel <ex@vingrad.ru>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2007 Daniel Abramov
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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

using Gendarme.Rules.Naming;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Naming {

	public class C {
	}

	public class CorrectClass {
	}

	public class AnotherCorrectClass {
	}

	public class CIncorrectClass {
	}

	public class INcorrectClass {
	}

	public class ILRange {
	}

	public class InMemoryDoohicky
	{
	}

	public interface I {
	}

	public interface ICorrectInterface {
	}

	public interface IncorrectInterface {
	}

	public interface AnotherIncorrectInterface {
	}

	public class CLSAbbreviation { // ok
	}

	public interface ICLSAbbreviation { // ok too
	}

	public class GoodSingleGenericType<T, V, K> {
	}

	public class GoodPrefixGenericType<TPrefix> {
	}

	public class BadCapsGenericType<a, b> {
	}

	public class BadPrefixGenericType<Prefix> {
	}

	[TestFixture]
	public class UseCorrectPrefixTypeTest : TypeRuleTestFixture<UseCorrectPrefixRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.GeneratedType);
		}

		[Test]
		public void Types ()
		{
			AssertRuleSuccess<C> ();
			AssertRuleSuccess<CorrectClass> ();
			AssertRuleSuccess<AnotherCorrectClass> ();
			AssertRuleFailure<CIncorrectClass> (1);
			AssertRuleFailure<INcorrectClass> (1);
			AssertRuleSuccess<ILRange> ();
			AssertRuleSuccess<InMemoryDoohicky> ();
		}

		[Test]
		public void Interfaces ()
		{
			AssertRuleFailure<I> (1);
			AssertRuleSuccess<ICorrectInterface> ();
			AssertRuleFailure<IncorrectInterface> (1);
			AssertRuleFailure<AnotherIncorrectInterface> (1);
		}

		[Test]
		public void Abbreviations ()
		{
			AssertRuleSuccess<CLSAbbreviation> ();
			AssertRuleSuccess<ICLSAbbreviation> ();
		}

		[Test]
		public void GenericParameters ()
		{
			AssertRuleSuccess<GoodSingleGenericType<int, int, int>> ();
			AssertRuleSuccess<GoodPrefixGenericType<int>> ();
			AssertRuleFailure<BadCapsGenericType<int, int>> (2);
			AssertRuleFailure<BadPrefixGenericType<int>> (1);
		}
	}
}
