//
// Unit tests for DoNotLockOnThisOrTypesRule
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

using Gendarme.Rules.Concurrency;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class DoNotLockOnThisOrTypesTest : MethodRuleTestFixture<DoNotLockOnThisOrTypesRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL for p/invokes
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		static Dictionary<string, Type> cache = new Dictionary<string, Type> ();

		public bool LockThis (string s)
		{
			lock (this) {
				return cache.ContainsKey (s);
			}
		}

		[Test]
		public void This ()
		{
			AssertRuleFailure<DoNotLockOnThisOrTypesTest> ("LockThis");
		}

		public bool LockType (string s)
		{
			lock (typeof (DoNotLockOnThisOrTypesTest)) {
				return cache.ContainsKey (s);
			}
		}

		public bool LockTypes (string s)
		{
			lock (typeof (DoNotLockOnThisOrTypesTest)) {
				lock (s.GetType ()) {
					return cache.ContainsKey (s);
				}
			}
		}

		[Test]
		public void Type ()
		{
			AssertRuleFailure<DoNotLockOnThisOrTypesTest> ("LockType", 1);
			AssertRuleFailure<DoNotLockOnThisOrTypesTest> ("LockTypes", 2);
		}

		static object locker = new object ();

		public bool LockObject (string s)
		{
			lock (locker) {
				return cache.ContainsKey (s);
			}
		}

		public bool NoLock (string s)
		{
			return cache.ContainsKey (s);
		}

		[Test]
		public void Object ()
		{
			AssertRuleSuccess<DoNotLockOnThisOrTypesTest> ("LockObject");
			AssertRuleSuccess<DoNotLockOnThisOrTypesTest> ("NoLock");
		}
	}
}
