//
// Unit tests for ReviewLockUsedOnlyForOperationsOnVariablesRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Cedric Vivier
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Threading;

using Gendarme.Rules.Concurrency;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class ReviewLockUsedOnlyForOperationsOnVariablesTest : MethodRuleTestFixture<ReviewLockUsedOnlyForOperationsOnVariablesRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("DoesNotApply1");
			AssertRuleDoesNotApply<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("DoesNotApply2");
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Success1");
			AssertRuleSuccess<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Success2");
			AssertRuleSuccess<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Success3");
			AssertRuleSuccess<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Success4");
			AssertRuleSuccess<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Success5");
			AssertRuleSuccess<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Success6");
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure0", 1);
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure1", 1);
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure2", 1);
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure3", 1);
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure4", 1);
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure5", 1);
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure5b", 2);
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure6", 1);
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure7", 1);
			AssertRuleFailure<ReviewLockUsedOnlyForOperationsOnVariablesTest> ("Failure8", 3);
		}


		private object locker = new object ();
		private object locker2 = new object ();

		public bool DoesNotApply1 ()
		{
			bool ret = true;
			x = 42;
			y = 33;
			return ret;
		}

		public void DoesNotApply2 ()
		{
			Monitor.Enter (locker);
			x = 0;
			//does not apply since we do not exit
		}

		public bool Success1 (string s)
		{
			lock (locker) {
				return cache.ContainsKey (s);
			}
		}

		public bool Success2 ()
		{
			object o = new object ();
			lock (o) {
				Type t = o.GetType ();
				lock (locker) {
					return cache.ContainsKey ("foo");
				}
			}
		}

		public bool Success3 ()
		{
			bool ret = false;
			object o = new object ();
			lock (o) {
				object o2 = new object ();
				lock (locker) {
					Console.WriteLine("foo");
					x++;
				}
				lock (locker) {
					ret = true;
					Console.WriteLine("foo");
				}
			}
			return ret;
		}

		public bool Success4 ()
		{
			bool ret = false;
			object o = new object ();
			lock (o) {
				Console.WriteLine (ret);
				lock (locker2) {
					if (!ret)
						ret = true;
				}
			}
			return ret;
		}

		bool ret;

		public void Success5 ()
		{
			lock (locker) {
				if (!ret)
					ret = true;
			}
			x = 0;
		}

		public void Success6 (string name)
		{
			lock (locker) {
				s = name.Substring(1);
			}
		}

		public void Failure0 ()
		{
			lock (locker) {
				x = 1;
				y = 2;
			}
		}

		public void Failure1 ()
		{
			lock (locker) {
				x++;
			}
		}

		public void Failure2 ()
		{
			lock (locker) {
				x = y;
			}
		}

		public void Failure3 ()
		{
			lock (locker) {
				lock (locker2) {
					Console.WriteLine (x);
				}
				lock (locker2) {
					x++;
				}
				lock (locker2) {
					Console.WriteLine (x);
				}
			}
		}

		public void Failure4 ()
		{
			lock (locker) {
				lock (locker2) {
					Console.WriteLine (x);
				}
				lock (locker2) {
					x--;
					y--;
				}
				lock (locker2) {
					Console.WriteLine (y);
				}
			}
		}

		public void Failure5 ()
		{
			object o = new object ();
			lock (locker) {
				lock (locker2) {
					Console.WriteLine ("foo");
					lock (o) {
						y--;
					}
				}
				lock (locker2) {
					Console.WriteLine ("foo");
				}
				y++;
			}
		}

		public void Failure5b ()
		{
			object o = new object ();
			lock (locker) {
				lock (locker2) {
					Console.WriteLine ("foo");
					lock (o) {
						y--;
					}
				}
				lock (locker2) {
					y++;
				}
				y++;
			}
		}

		public void Failure6 (int j)
		{
			lock (locker) {
				lock (locker2) {
					Console.WriteLine ("foo");
				}
				lock (locker2) {
					Console.WriteLine ("foo");
				}
			}
			lock (locker) {
				y += j;
			}
		}

		string s;

		public void Failure7 (string name)
		{
			lock (locker) {
				s = name;
			}
		}

		public void Failure8 (int i, int j)
		{
			bool ret = false;
			lock (locker) {
				lock (locker2) {
					ret = true;
				}
				lock (locker2) {
					object o = new object ();
					object o2 = new object ();
					lock (o) {
						Console.WriteLine ("foo");
						lock (o2) {
							x = j;
						}
					}
				}
				y += j;
				lock (locker2) {
					y += j;
				}
			}
		}

		Dictionary<string, int> cache;
		int x;
		int y;
	}

}

