//
// Unit Test for DontDestroyStackTraceTest Rule
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
using System.Collections;
using System.Reflection;
using Gendarme.Framework;
using Gendarme.Rules.Exceptions;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Exceptions {

	[TestFixture]	
	public class DontDestroyStackTraceTest {
	
		private IMethodRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		
		// Test setup
		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Exceptions.DontDestroyStackTraceTest"];
			rule = new DontDestroyStackTrace ();
			runner = new TestRunner (rule);
		}

		// Test infrastructure
		private MethodDefinition GetMethodToTest (string name)
		{
			return type.Methods.GetMethod (name, new Type [0]);
		}

		// Individual test cases
		[Test]
		public void TestThrowOriginalEx ()
		{
			MethodDefinition method = GetMethodToTest ("ThrowOriginalEx");
			// Should result in 1 warning message
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestThrowOriginalExWithJunk ()
		{
			MethodDefinition method = GetMethodToTest ("ThrowOriginalExWithJunk");
			// Should result in 1 warning message
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestRethrowOriginalEx ()
		{
			MethodDefinition method = GetMethodToTest ("RethrowOriginalEx");
			// Should result in 0 warning messages
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestThrowOriginalExAndRethrowWithJunk ()
		{
			MethodDefinition method = GetMethodToTest ("ThrowOriginalExAndRethrowWithJunk");
			// Should result in one warning message
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestRethrowOriginalExAndThrowWithJunk ()
		{
			MethodDefinition method = GetMethodToTest ("RethrowOriginalExAndThrowWithJunk");
			// Should result in one warning message
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		// Functions whose IL is used by the test cases for the DontDestroyStackTrace rule

		public void ThrowOriginalEx ()
		{
			try  {
				Int32.Parse("Broken!");
			}
			catch (Exception ex) {
				// Throw exception immediately.
				// This should trip the DontDestroyStackTrace rule.
				throw ex;
			}
		}

		public void ThrowOriginalExWithJunk ()
		{
			try {
				Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				int j = 0;
				for (int k=0; k<10; k++) {
					// throw some junk into the catch block, to ensure that
					j += 10;
					Console.WriteLine (j);
				}

				// This should trip the DontDestroyStackTrace rule, because we're
				// throwing the original exception.
				throw ex;
			}
		}

		public void RethrowOriginalEx ()
		{
			try {
				Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				// avoid compiler warning
				Assert.IsNotNull (ex);
				// This should NOT trip the DontDestroyStackTrace rule, because we're
				// rethrowing the original exception.
				throw;
			}
		}

		public void ThrowOriginalExAndRethrowWithJunk ()
		{
			int i = 0;
			try {
				i = Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				int j = 0;
				for (int k=0; k<10; k++) {
					// throw some junk into the catch block, to ensure that
					j += 10;
					Console.WriteLine (j);
					if ((i % 1234) > 56) {
						// This should trip the DontDestroyStackTrace rule, because we're
						// throwing the original exception.
						throw ex;
					}
				}

				// More junk - just to ensure that alternate paths through
				// this catch block end up at a throw and a rethrow
				throw;
			}
		}

		public void RethrowOriginalExAndThrowWithJunk ()
		{
			int i = 0;
			try {
				i = Int32.Parse ("Broken!");
			}
			catch (Exception ex) {
				int j = 0;
				for (int k=0; k<10; k++) {
					// throw some junk into the catch block, to ensure that
					j += 10;
					Console.WriteLine (j);
					if ((i % 1234) > 56) {
						// More junk - just to ensure that alternate paths through
						// this catch block end up at a throw and a rethrow
						throw;
					}
				}

				// This should trip the DontDestroyStackTrace rule, because we're
				// throwing the original exception.
				throw ex;
			}
		}
	}
}
