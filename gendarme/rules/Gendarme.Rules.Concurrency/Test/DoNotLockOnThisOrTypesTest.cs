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

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class DoNotLockOnThisOrTypesTest : MethodRuleTestFixture<DoNotLockOnThisOrTypesRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL for p/invokes
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no calls[virt]
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
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

		static public bool StaticLockType (string s)
		{
			lock (typeof (DoNotLockOnThisOrTypesTest)) {
				return cache.ContainsKey (s);
			}
		}

		static public bool StaticLockTypes (string s)
		{
			lock (typeof (DoNotLockOnThisOrTypesTest)) {
				lock (s.GetType ()) {
					return cache.ContainsKey (s);
				}
			}
		}

		static bool TryEnter (object obj)
		{
			lock (obj) {
				Console.WriteLine ();
			}
			return true;
		}

		[Test]
		public void StaticType ()
		{
			AssertRuleFailure<DoNotLockOnThisOrTypesTest> ("StaticLockType", 1);
			AssertRuleFailure<DoNotLockOnThisOrTypesTest> ("StaticLockTypes", 2);
			AssertRuleSuccess<DoNotLockOnThisOrTypesTest> ("TryEnter");
		}

		object instance_locker = new object ();
		static object static_locker = new object ();

		public bool LockInstanceObject (string s)
		{
			lock (instance_locker) {
				return cache.ContainsKey (s);
			}
		}

		public bool LockStaticObject (string s)
		{
			lock (static_locker) {
				return cache.ContainsKey (s);
			}
		}

		public bool NoLock (string s)
		{
			return cache.ContainsKey (s);
		}

		[Test]
		public void Instance ()
		{
			AssertRuleSuccess<DoNotLockOnThisOrTypesTest> ("LockInstanceObject");
			AssertRuleSuccess<DoNotLockOnThisOrTypesTest> ("LockStaticObject");
			AssertRuleSuccess<DoNotLockOnThisOrTypesTest> ("NoLock");
		}

		static public bool StaticLockStaticObject (string s)
		{
			lock (static_locker) {
				return cache.ContainsKey (s);
			}
		}

		static public bool StaticNoLock (string s)
		{
			return cache.ContainsKey (s);
		}

		[Test]
		public void Static ()
		{
			AssertRuleSuccess<DoNotLockOnThisOrTypesTest> ("StaticLockStaticObject");
			AssertRuleSuccess<DoNotLockOnThisOrTypesTest> ("StaticNoLock");
		}

		abstract class Base {
			protected object locker = new object ();

			public object Locker {
				get { return locker; }
			}
		}

		class Concrete : Base {

			void LockField (string s)
			{
				try {
					lock (base.locker) {
						Console.WriteLine (s);
					}
				}
				catch {
				}
			}

			void LockProperty (string s)
			{
				try {
					lock (base.Locker) {
						Console.WriteLine (s);
					}
				}
				catch {
				}
			}
		}

		[Test]
		public void CallingBase ()
		{
			AssertRuleSuccess<Concrete> ("LockField");
			AssertRuleSuccess<Concrete> ("LockProperty");
		}
	}
}
