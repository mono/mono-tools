//
// Unit Test for ConsiderUsingStopwatchRule.
//
// Authors:
//      Cedric Vivier <cedricv@neonux.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//      (C) 2008 Cedric Vivier
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
using System.Diagnostics;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Maintainability;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Maintainability {

	#pragma warning disable 219
	public class TestClass {
		public void ProcessingTime1 ()
		{
			DateTime start = DateTime.Now;
			//stuff
			TimeSpan end = DateTime.Now - start;
		}

		public void ProcessingTime2 ()
		{
			DateTime start = DateTime.Now;
			//stuff
			DateTime end = DateTime.Now;
			TimeSpan duration = end - start;
		}

		public void ProcessingTime3 ()
		{
			DateTime start = DateTime.Now;
			Console.WriteLine("started at {0}", start);
			//stuff
			DateTime end = DateTime.Now;
			Console.WriteLine("ended at {0}", end);
			Console.WriteLine("duration : {0}", end - start);
		}

		public TimeSpan ProcessingTime4 ()
		{
			DateTime start = DateTime.Now;
			//stuff
			return DateTime.Now - start;
		}

		public void DateDiff1 ()
		{
			DateTime baseTime = new DateTime(2000, 12, 1);
			//stuff
			TimeSpan diff = DateTime.Now - baseTime;
		}

		public void DateDiff2 ()
		{
			DateTime baseTime = new DateTime(2000, 12, 1);
			//stuff
			DateTime now = DateTime.Now;
			TimeSpan diff = now - baseTime;
		}

		public void DateDiff3 ()
		{
			TimeSpan diff = DateTime.Now - new DateTime(2000, 12, 1);
		}

		public TimeSpan DateDiff4 ()
		{
			return new DateTime(2008, 1, 1) - new DateTime(2000, 12, 1);
		}

		public TimeSpan DateDiff5 (DateTime origin)
		{
			return DateTime.Now - origin;
		}

		public TimeSpan Mixed1 (DateTime origin)
		{
			DateTime start = DateTime.Now;
			//stuff
			TimeSpan end = DateTime.Now - start;
			return DateTime.Now - origin;
		}

		public TimeSpan Double ()
		{
			DateTime start = DateTime.Now;
			//stuff
			TimeSpan end = DateTime.Now - start;
			start = DateTime.Now;
			//stuff
			return DateTime.Now - start;
		}

		public TimeSpan Triple ()
		{
			DateTime start = DateTime.Now;
			//stuff
			TimeSpan end = DateTime.Now - start;

			start = DateTime.Now;
			//stuff
			end = DateTime.Now - start;

			start = DateTime.Now;
			//stuff
			return DateTime.Now - start;
		}

		public void ManyLocals ()
		{
			DateTime baseTime = DateTime.Now;
			DateTime dummy1 = new DateTime(2001, 12, 1);
			DateTime dummy2 = new DateTime(2002, 12, 1);
			DateTime dummy3 = new DateTime(2003, 12, 1);
			DateTime dummy4 = new DateTime(2004, 12, 1);
			DateTime now = DateTime.Now;
			TimeSpan diff = now - baseTime;
		}

		public TimeSpan Return ()
		{
			DateTime start = DateTime.Now;
			//stuff
			return DateTime.Now - start;
		}

		public TimeSpan Ref (ref DateTime start)
		{
			start = DateTime.Now;
			//stuff
			return DateTime.Now - start;
		}

		public TimeSpan Out (out DateTime end)
		{
			DateTime start = DateTime.Now;
			//stuff
			end = DateTime.Now;
			return end - start;
		}

		public TimeSpan UsingStopwatch ()
		{
			Stopwatch watch = new Stopwatch ();
			// stuff
			return watch.Elapsed;
		}

		private object boxed;

		private bool Box (object a, object b)
		{
			return ((DateTime) a - (DateTime) b < (TimeSpan) boxed);
		}
	}
	#pragma warning restore 169


	[TestFixture]
	public class ConsiderUsingStopwatchTest : MethodRuleTestFixture<ConsiderUsingStopwatchRule> {

		[Test]
		public void Initialize ()
		{
			// ensure that the rule does not apply for types defined in 1.x assemblies
			TypeDefinition violator = DefinitionLoader.GetTypeDefinition<ConsiderUsingStopwatchTest> ();
			TargetRuntime realRuntime = violator.Module.Runtime;
			try {

				// fake assembly runtime version and do the check
				violator.Module.Runtime = TargetRuntime.Net_1_1;
				Rule.Active = true;
				Rule.Initialize (Runner);
				Assert.IsFalse (Rule.Active, "Active");
			}
			catch {
				// rollback
				violator.Module.Runtime = realRuntime;
				Rule.Active = true;
			}
		}

		[Test]
		public void NotApplicable ()
		{
			// no body
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no call[virt] instruction
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<TestClass> ("DateDiff1");
			AssertRuleSuccess<TestClass> ("DateDiff2");
			AssertRuleSuccess<TestClass> ("DateDiff3");
			AssertRuleSuccess<TestClass> ("DateDiff4");
			AssertRuleSuccess<TestClass> ("DateDiff5");
			AssertRuleSuccess<TestClass> ("UsingStopwatch");
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure<TestClass> ("ProcessingTime1");
			AssertRuleFailure<TestClass> ("ProcessingTime2");
			AssertRuleFailure<TestClass> ("ProcessingTime3");
			AssertRuleFailure<TestClass> ("ProcessingTime4");
		}

		[Test]
		public void Mixed ()
		{
			AssertRuleFailure<TestClass> ("Mixed1", 1);
		}

		[Test]
		public void Double ()
		{
			AssertRuleFailure<TestClass> ("Double", 2);
		}

		[Test]
		public void Triple ()
		{
			AssertRuleFailure<TestClass> ("Triple", 3);
		}

		[Test]
		public void ManyLocals ()
		{
			AssertRuleFailure<TestClass> ("ManyLocals", 1);
		}

		[Test]
		public void SpecialCases ()
		{
			AssertRuleFailure<TestClass> ("Return", 1);
			AssertRuleFailure<TestClass> ("Ref", 1);
			AssertRuleFailure<TestClass> ("Out", 1);

			// boxed substraction without the use of Now
			AssertRuleSuccess<TestClass> ("Box");
		}
	}
}

