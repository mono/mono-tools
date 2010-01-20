//
// Unit Tests for AvoidMethodWithUnusedGenericTypeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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
using System.Collections.Generic;

using Gendarme.Rules.Design.Generic;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design.Generic {

	[TestFixture]
	public class AvoidMethodWithUnusedGenericTypeTest : MethodRuleTestFixture<AvoidMethodWithUnusedGenericTypeRule> {

		public void NoGenericParameter ()
		{
		}

		public class MyList : List<int> {
			public void NoGenericParameter ()
			{
			}
		}

		public class Tuple<T, K, V> {
			public void NoGenericParameter ()
			{
			}
		}

		[Test]
		public void DoesNotApply ()
		{
			// inside a type that does NOT use generics
			AssertRuleDoesNotApply<AvoidMethodWithUnusedGenericTypeTest> ("NoGenericParameter");
			// inside a type that use generics
			AssertRuleDoesNotApply<MyList> ("NoGenericParameter");
			AssertRuleDoesNotApply<Tuple<int, int, int>> ("NoGenericParameter");
		}

		public class BadCases {
			public void Single<T> ()
			{
			}

			public void Double<T, K> ()
			{
			}

			public void Triple<T, K, V> ()
			{
			}

			public void Partial<T, K> (T value)
			{
			}
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadCases> ("Single", 1);
			AssertRuleFailure<BadCases> ("Double", 2);
			AssertRuleFailure<BadCases> ("Triple", 3);

			AssertRuleFailure<BadCases> ("Partial", 1);
		}

		public class GoodCases {
			public void Single<T> (T value)
			{
			}

			public void Double<T, K> (T key, K value)
			{
			}

			public void Triple<T, K, V> (T a, K b, V c)
			{
			}

			public void Duplicate<T, K> (T key, K min, K max)
			{
			}

			public void SingleArray<T> (T [] values)
			{
			}
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodCases> ("Single");
			AssertRuleSuccess<GoodCases> ("Double");
			AssertRuleSuccess<GoodCases> ("Triple");

			AssertRuleSuccess<GoodCases> ("Duplicate");

			AssertRuleSuccess<GoodCases> ("SingleArray");
		}

		// from CommonRocks
		public static void AddRangeIfNew<T> (ICollection<T> self, IEnumerable<T> items)
		{
			foreach (T item in items) {
				if (!self.Contains (item))
					self.Add (item);
			}
		}

		[Test]
		public void Indirect ()
		{
			AssertRuleSuccess<AvoidMethodWithUnusedGenericTypeTest> ("AddRangeIfNew");
		}

		public T Parse<T> (string s)
		{
			return default (T);
		}

		[Test]
		public void ReturnValue ()
		{
			AssertRuleFailure<AvoidMethodWithUnusedGenericTypeTest> ("Parse", 1);
		}
	}
}
