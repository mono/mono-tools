// 
// Test.Rules.Design.StronglyTypeICollectionMembersTest
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
	public class StronglyTypeICollectionMembersTest : TypeRuleTestFixture<StronglyTypeICollectionMembersRule> {

		class Case1 : ICollection {

			public void CopyTo (Array array, int index)
			{
			}

			public int Count
			{
				get { return 0; }
			}

			public bool IsSynchronized
			{
				get { return true; }
			}

			public object SyncRoot
			{
				get { return null; }
			}

			public IEnumerator GetEnumerator ()
			{
				return String.Empty.GetEnumerator ();
			}
		}

		class Case2 : Case1 {
			public void CopyTo (object [] array, int index)
			{
			}
		}

		class Case3 : Case2 {
			public void CopyTo (Exception [] array, int index)
			{
			}
		}

		class Case4 : Case3 {
		}

		class Case5 : ICollection {
			void ICollection.CopyTo (Array array, int index)
			{
			}

			int ICollection.Count
			{
				get { return 0; }
			}

			bool ICollection.IsSynchronized
			{
				get { return false; }
			}

			object ICollection.SyncRoot
			{
				get { return null; }
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return String.Empty.GetEnumerator ();
			}

			void CopyTo (Exception [] array, int index)
			{
			}
		}



		[Test]
		public void DoesNotApply ()
		{
			// TODO: Write tests that don't apply.
			// AssertRuleDoesNotApply<type> ();
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
