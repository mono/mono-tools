//
// Unit tests for AvoidRepetitiveCallsToPropertiesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class AvoidRepetitiveCallsToPropertiesTest : MethodRuleTestFixture<AvoidRepetitiveCallsToPropertiesRule> {

		[Test]
		public void DoesNotApply ()
		{
			// method has no body
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// method does not call any other method
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		class TestCase {
			// very small this will get inlined
			public int Integer {
				get { return 0; }
			}

			public virtual string Message {
				get { return "coucou"; }
			}

			public string SingleCalls ()
			{
				return String.Format ("{0} {1}", Integer, Message);
			}

			public string Multiple ()
			{
				return String.Format ("{0} {1} {2} {3} {4} {5} {6} {7}",
					Integer, Message, Integer, Message,
					Integer, Message, Integer, Message);
			}

			// make it large enough (by calling GetHashCode) not to be inlined
			static Version Version {
				get { return new Version (1.GetHashCode (), 2.GetHashCode (), 3.GetHashCode (), 4.GetHashCode ()); }
			}

			static string ShowVersion ()
			{
				return String.Format ("{0}.{1}.{2}.{3}",
					Version, Version, Version, Version);
			}
		}

		[Test]
		public void SingleCall ()
		{
			AssertRuleSuccess<TestCase> ("SingleCalls");
		}

		[Test]
		public void MultipleCalls ()
		{
			// only Message (virtual) will trigger since Integer is small enough to be inlined
			AssertRuleFailure<TestCase> ("Multiple", 1);
			Assert.AreEqual (Severity.Medium, Runner.Defects [0].Severity, "Medium(2)");
		}

		[Test]
		public void StaticCalls ()
		{
			AssertRuleFailure<TestCase> ("ShowVersion", 1);
			Assert.AreEqual (Severity.Low, Runner.Defects [0].Severity, "Low(2)");
		}

		private string TwoDifferentInstances (TestCase a, TestCase b)
		{
			return String.Format ("{0} {1}", a.Message, b.Message);
		}

		private string SameInstance (TestCase a)
		{
			return String.Format ("{0} {1} {2}", a.Message, a.Message, a.Message);
		}

		[Test]
		public void MultipleInstances ()
		{
			// same property on two different instances
			AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("TwoDifferentInstances");

			// same property on the same instance
			AssertRuleFailure<AvoidRepetitiveCallsToPropertiesTest> ("SameInstance", 1);
			Assert.AreEqual (Severity.Medium, Runner.Defects [0].Severity, "Medium(3)");
		}

		private int Count (CollectionBase cb)
		{
			return cb.Count + cb.Count + cb.Count + cb.Count + cb.Count + cb.Count +
				cb.Count + cb.Count + cb.Count + cb.Count + cb.Count + cb.Count;
		}

		[Test]
		public void CollectionBaseCountAbuse ()
		{
			// we can't assume different runtime/version will have a constant code size
			// so we adjust the inline limit of the rule (above and below)
			MethodDefinition md = Helpers.DefinitionLoader.GetMethodDefinition<CollectionBase> ("get_Count");
			try {
				Rule.InlineLimit = md.Body.CodeSize + 1;
				AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("Count");

				Rule.InlineLimit = md.Body.CodeSize - 1;
				AssertRuleFailure<AvoidRepetitiveCallsToPropertiesTest> ("Count", 1);
				Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "High(6)");
			}
			finally {
				Rule.InlineLimit = 20;
			}
		}

		public virtual string this [int index] {
			get { return null; }
		}

		void SameIndex ()
		{
			Console.WriteLine ("{0} {1} {2}", this [0], this [0], this [0]);
		}

		void DifferentIndex ()
		{
			Console.WriteLine ("{0} {1} {2}", this [0], this [1], this [2]);
		}

		void CalculatedIndex ()
		{
			int n = 0;
			Console.WriteLine ("{0} {1} {2}", this [n++], this [n++], this [n++]);
		}

		[Test]
		public void Indexes ()
		{
			// note: the rule currently ignore indexed properties
			AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("SameIndex");
			AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("DifferentIndex");
			AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("CalculatedIndex");
		}

		// test case distilled from ParameterNamesShouldMatchOverriddenMethodRule
		bool SameReturnType (MethodReference a, MethodReference b)
		{
			return (a.ReturnType.FullName == b.ReturnType.FullName);
		}

		[Test]
		public void Chain ()
		{
			AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("SameReturnType");
		}

		void UsingDateTime ()
		{
			Console.WriteLine ("start {0} {1}", DateTime.Now, DateTime.UtcNow);
			// ...
			Console.WriteLine ("end {0} {1}", DateTime.Now, DateTime.UtcNow);
		}

		void UsingStopwatch ()
		{
			Stopwatch sw = Stopwatch.StartNew ();
			Console.WriteLine ("start {0}", sw.Elapsed);
			// ...
			Console.WriteLine ("end {0}", sw.Elapsed);
		}

		[Test]
		public void WellKnownUsages ()
		{
			AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("UsingDateTime");
			AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("UsingStopwatch");
		}

		internal static MethodDefinition Current {
			get { return null; }
		}

		internal static MethodDefinition Target {
			get { return null; }
		}

		static bool AreEquivalent (VariableReference source, VariableReference target)
		{
			IList<VariableDefinition> cv = Current.Body.Variables;
			IList<VariableDefinition> tv = Target.Body.Variables;
			return cv.Count > source.Index && tv.Count > target.Index ?
				cv [source.Index].VariableType.Equals (tv [target.Index].VariableType) : false;
		}

		static bool AreEquivalent2 (ParameterDefinition source, ParameterDefinition target)
		{
			if ((source == null) || (target == null))
				return false;

			int ss = source.Index;
			int ts = target.Index;
			if ((ss <= 0) || (ts <= 0))
				return false;

			IList<ParameterDefinition> cp = Current.Parameters;
			IList<ParameterDefinition> tp = Target.Parameters;
			return ((cp.Count > ss) && (tp.Count > ts)) ?
				cp [ss].ParameterType.Equals (tp [ts].ParameterType) : false;
		}

		[Test]
		public void Properties ()
		{
			AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("AreEquivalent");
			AssertRuleSuccess<AvoidRepetitiveCallsToPropertiesTest> ("AreEquivalent2");
		}
	}
}

