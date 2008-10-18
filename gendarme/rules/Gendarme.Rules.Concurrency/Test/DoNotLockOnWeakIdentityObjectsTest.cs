//
// Unit tests for DoNotLockOnWeakIdentityObjectsRule
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
using System.Reflection;
using System.Threading;

using Gendarme.Rules.Concurrency;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class DoNotLockOnWeakIdentityObjectsTest : MethodRuleTestFixture<DoNotLockOnWeakIdentityObjectsRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL for p/invokes
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no calls[virt]
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		static Dictionary<string, Type> cache = new Dictionary<string, Type> ();

		static object locker = new object ();

		public bool LockObject (string s)
		{
			lock (locker) {
				return cache.ContainsKey (s);
			}
		}

		[Test]
		public void Object ()
		{
			AssertRuleSuccess<DoNotLockOnWeakIdentityObjectsTest> ("LockObject");
		}

		public bool LockOnLocalString ()
		{
			// sealed type
			string s = "lock";
			lock (s) {
				return cache.ContainsKey (s);
			}
		}

		// sealed type
		private static ExecutionEngineException static_eee;
		// sealed type
		private StackOverflowException so;

		public bool LocksOnExceptions ()
		{
			lock (static_eee) {
				if (cache.ContainsKey ("a"))
					return true;
			}
			lock (so) {
				return cache.ContainsKey ("b");
			}
		}

		// sealed type
		private Thread thread;

		public bool LockOnThread (string s)
		{
			lock (Thread.CurrentThread) {
				return cache.ContainsKey (s);
			}
		}

		[Test]
		public void WeakIdentity_Sealed ()
		{
			AssertRuleFailure<DoNotLockOnWeakIdentityObjectsTest> ("LockOnLocalString", 1);
			AssertRuleFailure<DoNotLockOnWeakIdentityObjectsTest> ("LocksOnExceptions", 2);
			AssertRuleFailure<DoNotLockOnWeakIdentityObjectsTest> ("LockOnThread", 1);
		}

		// abstract type
		private MarshalByRefObject instance_field = null;

		public bool LockOnMarshalByRefObject (string s)
		{
			lock (instance_field) {
				return cache.ContainsKey (s);
			}
		}

		public bool LockOnAppDomain (string s)
		{
			// AppDomain inherits from MBRO
			lock (AppDomain.CurrentDomain) {
				return cache.ContainsKey (s);
			}
		}

		// non-sealed type
		private OutOfMemoryException oom;

		public class MyOutOfMemoryException : OutOfMemoryException {
		}

		private MyOutOfMemoryException myOom;

		public bool LocksOnOutOfMemory ()
		{
			lock (oom) {
				if (cache.ContainsKey ("a"))
					return true;
			}
			lock (myOom) {
				return cache.ContainsKey ("b");
			}
		}

		// abstract type
		private MemberInfo mi;
		// non-sealed type
		private ParameterInfo pi;

		public bool LockOnReflectionObjects (string s)
		{
			lock (mi) {
				lock (pi) {
					return cache.ContainsKey (s);
				}
			}
		}

		[Test]
		public void WeakIdentity ()
		{
			AssertRuleFailure<DoNotLockOnWeakIdentityObjectsTest> ("LockOnMarshalByRefObject", 1);
			AssertRuleFailure<DoNotLockOnWeakIdentityObjectsTest> ("LockOnAppDomain", 1);
			AssertRuleFailure<DoNotLockOnWeakIdentityObjectsTest> ("LocksOnOutOfMemory", 2);
			AssertRuleFailure<DoNotLockOnWeakIdentityObjectsTest> ("LockOnReflectionObjects", 2);
		}
	}
}
