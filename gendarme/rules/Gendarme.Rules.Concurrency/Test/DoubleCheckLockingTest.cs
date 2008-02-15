//
// Unit tests for DoubleCheckLockingRule
//
// Authors:
//	Aaron Tomb <atomb@soe.ucsc.edu>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Aaron Tomb
// Copyright (C) 2006-2008 Novell, Inc (http://www.novell.com)
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
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Concurrency;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class DoubleCheckLockingTest {
	
		public class Singleton {
		
			private static volatile Singleton instance;
			private static object syncRoot = new object ();

			private Singleton()
			{
			}

			public static Singleton SingleCheckBefore {
				get {
					if (instance == null) {
						lock (syncRoot) {
							instance = new Singleton();
						}
					}
					return instance;
				}
			}

			public static Singleton SingleCheckAfter {
				get {
					lock (syncRoot) {
						if (instance == null) {
							instance = new Singleton();
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
								instance = new Singleton();
						}
					}
					return instance;
				}
			}
		}
	
		private IMethodRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;
		private TypeDefinition type;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Concurrency.DoubleCheckLockingTest/Singleton"];
			rule = new DoubleCheckLockingRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string name)
		{
			string get_name = "get_" + name;
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == get_name)
					return method;
			}
			return null;
		}

		[Test]
		public void SingleCheckBefore ()
		{
			MethodDefinition method = GetTest ("SingleCheckBefore");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void SingleCheckAfter ()
		{
			MethodDefinition method = GetTest ("SingleCheckAfter");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void DoubleCheck ()
		{
			MethodDefinition method = GetTest ("DoubleCheck");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}
	}
}
