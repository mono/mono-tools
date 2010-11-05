//
// Unit tests for DoubleCheckLockingRule
//
// Authors:
//	Aaron Tomb <atomb@soe.ucsc.edu>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Aaron Tomb
// Copyright (C) 2006-2008, 2010 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Gendarme.Rules.Concurrency;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class DoubleCheckLockingTest : MethodRuleTestFixture<DoubleCheckLockingRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL for p/invokes
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no calls[virt]
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}
	
		public class Singleton {
		
			private static volatile Singleton instance;
			private static object syncRoot = new object ();

			private Singleton ()
			{
			}

			public static Singleton SingleCheckBefore {
				get {
					if (instance == null) {
						lock (syncRoot) {
							instance = new Singleton ();
						}
					}
					return instance;
				}
			}

			public static Singleton MultipleChecksBefore {
				get {
					if (instance == null) {
						// useless but not dangerous
						if (instance == null) {
							lock (syncRoot) {
								instance = new Singleton ();
							}
						}
					}
					return instance;
				}
			}

			public static Singleton SingleCheckAfter {
				get {
					lock (syncRoot) {
						if (instance == null) {
							instance = new Singleton ();
						}
					}
					return instance;
				}
			}

			public static Singleton MultipleChecksAfter {
				get {
					lock (syncRoot) {
						if (instance == null) {
							// useless but not dangerous
							if (instance == null) {
								instance = new Singleton ();
							}
						}
					}
					return instance;
				}
			}

			public static Singleton DoubleCheck {
				get {
					if (instance == null) {
						lock (syncRoot) {
							if (instance == null) 
								instance = new Singleton ();
						}
					}
					return instance;
				}
			}
		}
	
		[Test]
		public void CheckBefore ()
		{
			AssertRuleSuccess<Singleton> ("get_SingleCheckBefore");
			AssertRuleSuccess<Singleton> ("get_MultipleChecksBefore");
		}

		[Test]
		public void CheckAfter ()
		{
			AssertRuleSuccess<Singleton> ("get_SingleCheckAfter");
			AssertRuleSuccess<Singleton> ("get_MultipleChecksAfter");
		}

		[Test]
		public void DoubleCheck ()
		{
			MethodDefinition md = DefinitionLoader.GetMethodDefinition<Singleton> ("get_DoubleCheck");
			// even if the rule applies only to < 2.0 it still works (for unit testsing) until 4.0
			if (md.DeclaringType.Module.Runtime < TargetRuntime.Net_4_0)
				AssertRuleFailure (md);
			else
				Assert.Ignore ("Rule applies for < 2.0 and works only for < 4.0");
		}
	}
}
