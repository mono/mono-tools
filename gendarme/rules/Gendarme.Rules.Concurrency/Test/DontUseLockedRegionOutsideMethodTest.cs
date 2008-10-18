//
// Unit tests for DontUseLockedRegionOutsideMethodTest
//
// Authors:
//	Andres G. Aragoneses <aaragoneses@novell.com>
//
// Copyright (C) 2008 Andres G. Aragoneses
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

using Gendarme.Rules.Concurrency;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class DoNotUseLockedRegionOutsideMethodTest : MethodRuleTestFixture<DoNotUseLockedRegionOutsideMethodRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL for p/invokes
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no calls[virt]
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public class Monitors {

			private Monitors ()
			{
			}

			public static void WithLockStatement () {
				lock ( new object () )
				{
					// do something...
					WithLockStatement ();
				}
			}

			public static void WithoutConcurrency () {
				// do something...
				WithoutConcurrency ();
			}

			public static void WithoutThreadExit () {
				// do something...
				WithoutThreadExit ();
				
				System.Threading.Monitor.Enter ( new object () );
				
				// do something...
				WithoutThreadExit ();
			}

			public static void TwoEnterOneExit ()
			{
				lock (new object ()) {
					System.Threading.Monitor.Enter (new object ());
				}
			}
		}
	
		[Test]
		public void Check ()
		{
			AssertRuleSuccess<Monitors> ("WithLockStatement");
			AssertRuleSuccess<Monitors> ("WithoutConcurrency");
			AssertRuleFailure<Monitors> ("WithoutThreadExit");
			AssertRuleFailure<Monitors> ("TwoEnterOneExit");
		}
	}
}
