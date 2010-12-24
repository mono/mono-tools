// 
// Test.Rules.Design.ListsAreStronglyTypedTest
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
	public class ListsAreStronglyTypedTest : TypeRuleTestFixture<ListsAreStronglyTypedRule> {

		class ImplicitlyImplementsIList : IList {
			#region Long interface implementation
			public int Add (object value)
			{
				throw new NotImplementedException ();
			}

			public void Clear ()
			{
				throw new NotImplementedException ();
			}

			public bool Contains (object value)
			{
				throw new NotImplementedException ();
			}

			public int IndexOf (object value)
			{
				throw new NotImplementedException ();
			}

			public void Insert (int index, object value)
			{
				throw new NotImplementedException ();
			}

			public bool IsFixedSize
			{
				get { throw new NotImplementedException (); }
			}

			public bool IsReadOnly
			{
				get { throw new NotImplementedException (); }
			}

			public void Remove (object value)
			{
				throw new NotImplementedException ();
			}

			public void RemoveAt (int index)
			{
				throw new NotImplementedException ();
			}

			public object this [int index]
			{
				get
				{
					throw new NotImplementedException ();
				}
				set
				{
					throw new NotImplementedException ();
				}
			}

			public void CopyTo (Array array, int index)
			{
				throw new NotImplementedException ();
			}

			public int Count
			{
				get { throw new NotImplementedException (); }
			}

			public bool IsSynchronized
			{
				get { throw new NotImplementedException (); }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException (); }
			}

			public IEnumerator GetEnumerator ()
			{
				throw new NotImplementedException ();
			}
			#endregion
		}

		class ExplicitlyImplementsIList : IList {
			#region Long interface implementation
			int IList.Add (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.Clear ()
			{
				throw new NotImplementedException ();
			}

			bool IList.Contains (object value)
			{
				throw new NotImplementedException ();
			}

			int IList.IndexOf (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.Insert (int index, object value)
			{
				throw new NotImplementedException ();
			}

			bool IList.IsFixedSize
			{
				get { throw new NotImplementedException (); }
			}

			bool IList.IsReadOnly
			{
				get { throw new NotImplementedException (); }
			}

			void IList.Remove (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.RemoveAt (int index)
			{
				throw new NotImplementedException ();
			}

			object IList.this [int index]
			{
				get
				{
					throw new NotImplementedException ();
				}
				set
				{
					throw new NotImplementedException ();
				}
			}

			void ICollection.CopyTo (Array array, int index)
			{
				throw new NotImplementedException ();
			}

			int ICollection.Count
			{
				get { throw new NotImplementedException (); }
			}

			bool ICollection.IsSynchronized
			{
				get { throw new NotImplementedException (); }
			}

			object ICollection.SyncRoot
			{
				get { throw new NotImplementedException (); }
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				throw new NotImplementedException ();
			}
			#endregion
		}

		class Case1 : ImplicitlyImplementsIList {
		}

		class Case2 : ExplicitlyImplementsIList {
		}

		class Case3 : Case2 {
			public int Add (Exception value)
			{
				throw new NotImplementedException ();
			}

			public bool Contains (Exception value)
			{
				throw new NotImplementedException ();
			}

			public int IndexOf (Exception value)
			{
				throw new NotImplementedException ();
			}

			public void Insert (int index, Exception value)
			{
				throw new NotImplementedException ();
			}

			public void Remove (Exception value)
			{
				throw new NotImplementedException ();
			}

			public Exception this [int index]
			{
				get
				{
					throw new NotImplementedException ();
				}
				set
				{
					throw new NotImplementedException ();
				}
			}
		}


		class Case4 : Case2 {
			public void Insert (int index, Exception value)
			{
				throw new NotImplementedException ();
			}

			public void Remove (Exception value)
			{
				throw new NotImplementedException ();
			}

			public Exception this [int index]
			{
				get
				{
					throw new NotImplementedException ();
				}
				set
				{
					throw new NotImplementedException ();
				}
			}
		}

		class Case5 : IList {
			#region Long interface implementation
			int IList.Add (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.Clear ()
			{
				throw new NotImplementedException ();
			}

			bool IList.Contains (object value)
			{
				throw new NotImplementedException ();
			}

			int IList.IndexOf (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.Insert (int index, object value)
			{
				throw new NotImplementedException ();
			}

			bool IList.IsFixedSize
			{
				get { throw new NotImplementedException (); }
			}

			bool IList.IsReadOnly
			{
				get { throw new NotImplementedException (); }
			}

			void IList.Remove (object value)
			{
				throw new NotImplementedException ();
			}

			void IList.RemoveAt (int index)
			{
				throw new NotImplementedException ();
			}

			object IList.this [int index]
			{
				get
				{
					throw new NotImplementedException ();
				}
				set
				{
					throw new NotImplementedException ();
				}
			}

			void ICollection.CopyTo (Array array, int index)
			{
				throw new NotImplementedException ();
			}
			#endregion

			int ICollection.Count
			{
				get { throw new NotImplementedException (); }
			}

			bool ICollection.IsSynchronized
			{
				get { throw new NotImplementedException (); }
			}

			object ICollection.SyncRoot
			{
				get { throw new NotImplementedException (); }
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				throw new NotImplementedException ();
			}

			public int Add (Exception value)
			{
				throw new NotImplementedException ();
			}

			public bool Contains (Exception value)
			{
				throw new NotImplementedException ();
			}

			public int IndexOf (Exception value)
			{
				throw new NotImplementedException ();
			}

			public void Insert (int index, Exception value)
			{
				throw new NotImplementedException ();
			}

			public void Remove (Exception value)
			{
				throw new NotImplementedException ();
			}

			public Exception this [int index]
			{
				get
				{
					throw new NotImplementedException ();
				}
				set
				{
					throw new NotImplementedException ();
				}
			}
		}


		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<Case3> ();
			AssertRuleSuccess<Case5> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ImplicitlyImplementsIList> (6);
			AssertRuleFailure<ExplicitlyImplementsIList> (6);
			AssertRuleFailure<Case1> (6);
			AssertRuleFailure<Case2> (6);
			AssertRuleFailure<Case4> (3);
		}
	}
}
