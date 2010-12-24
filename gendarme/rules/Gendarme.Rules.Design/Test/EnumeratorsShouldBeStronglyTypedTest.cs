// 
// Test.Rules.Design.EnumeratorsShouldBeStronglyTypedTest
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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

using System;
using System.Collections;
using System.Reflection;

using Mono.Cecil;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace Test.Rules.Design {


	[TestFixture]
	public class EnumeratorsShouldBeStronglyTypedTest : TypeRuleTestFixture<EnumeratorsShouldBeStronglyTypedRule> {


		class Case1 : IEnumerator {
			object IEnumerator.Current
			{
				get { return null; }
			}

			public bool MoveNext ()
			{
				return true;
			}

			public void Reset ()
			{
			}
		}

		class Case2 : Case1 {
		}

		class Case3 : Case2 {
			public int Current
			{
				get { return 0; }
			}
		}

		class Case4 : Case3 {
		}

		class Case5 : IEnumerator {
			object IEnumerator.Current
			{
				get { return null; }
			}

			bool IEnumerator.MoveNext ()
			{
				return true;
			}

			void IEnumerator.Reset ()
			{
			}

			int Current
			{
				get { return 0; }
			}
		}

		class DerivedFromCollectionBase : CollectionBase {
		}

		class DerivedFromDictionaryBase : DictionaryBase {
		}

		class DerivedFromReadOnlyCollectionBase : ReadOnlyCollectionBase {
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<DerivedFromCollectionBase> ();
			AssertRuleDoesNotApply<DerivedFromDictionaryBase> ();
			AssertRuleDoesNotApply<DerivedFromReadOnlyCollectionBase> ();
			AssertRuleDoesNotApply (SimpleTypes.Class);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<Case3> ();
			AssertRuleSuccess<Case4> ();
			AssertRuleSuccess<Case5> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<Case1> ();
			AssertRuleFailure<Case2> ();
		}
	}
}
