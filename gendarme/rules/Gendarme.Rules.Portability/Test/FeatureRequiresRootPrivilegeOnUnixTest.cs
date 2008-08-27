//
// Unit tests for FeatureRequiresRootPrivilegeOnUnixRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2007 Andreas Noever
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
using System.Net.NetworkInformation;

using Gendarme.Rules.Portability;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Portability {

	class MyPing : Ping { } //MyPing..ctor calls Ping..ctor, this triggers the rule.
	class MyProcess : Process { } //Using MyProcess.PriorityClass calls Process.PriorityClass. The property is not virtual.

	class ProcessLookAlike {
		public ProcessPriorityClass PriorityClass { get; set; }
	}

	[TestFixture]
	public class FeatureRequiresRootPrivilegeOnUnixTest : MethodRuleTestFixture<FeatureRequiresRootPrivilegeOnUnixRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}

		public void SetPriority ()
		{
			Process p = new Process ();
			p.PriorityClass = ProcessPriorityClass.AboveNormal;
		}

		public void SetPriorityNormal ()
		{
			Process p = new Process ();
			p.PriorityClass = ProcessPriorityClass.Normal;
		}

		public void SetMyPriority ()
		{
			MyProcess p = new MyProcess ();
			p.PriorityClass = ProcessPriorityClass.AboveNormal;
		}

		public void SetLookAlikePriority ()
		{
			ProcessLookAlike p = new ProcessLookAlike ();
			p.PriorityClass = ProcessPriorityClass.AboveNormal;
		}

		[Test]
		public void TestSetPriority ()
		{
			AssertRuleFailure<FeatureRequiresRootPrivilegeOnUnixTest> ("SetPriority", 1);
			AssertRuleFailure<FeatureRequiresRootPrivilegeOnUnixTest> ("SetMyPriority", 1);
			AssertRuleSuccess<FeatureRequiresRootPrivilegeOnUnixTest> ("SetPriorityNormal");
			AssertRuleSuccess<FeatureRequiresRootPrivilegeOnUnixTest> ("SetLookAlikePriority");
		}

		public void SetPriorityUnknown (ProcessPriorityClass priority)
		{
			Process p = new Process ();
			p.PriorityClass = priority;
		}

		public void SetPriorityNormalVariable ()
		{
			ProcessPriorityClass priority = ProcessPriorityClass.Normal;
			Process p = new Process ();
			p.PriorityClass = priority;
		}

		public void SetPriorityAboveNormalVariable ()
		{
			ProcessPriorityClass priority = ProcessPriorityClass.AboveNormal;
			Process p = new Process ();
			p.PriorityClass = priority;
		}

		[Test]
		public void TestSetPriorityVariable ()
		{
			AssertRuleSuccess<FeatureRequiresRootPrivilegeOnUnixTest> ("SetPriorityNormalVariable");
			AssertRuleSuccess<FeatureRequiresRootPrivilegeOnUnixTest> ("SetPriorityUnknown");
		}

		[Test]
		[Ignore ("we don't track variables value")]
		public void TestSetPriorityAboveVariable ()
		{
			AssertRuleFailure<FeatureRequiresRootPrivilegeOnUnixTest> ("SetPriorityAboveNormalVariable", 1);
		}

		public void CreatePing ()
		{
			new Ping ();
		}

		public void UsePing (Ping ping)
		{
			// e.g. Ping could be supplied from another assembly
			// but Gendarme should still flag it's usage inside the analyzed assembly
			ping.Send ("127.0.0.1");
		}

		[Test]
		public void TestCreatePing ()
		{
			AssertRuleFailure<FeatureRequiresRootPrivilegeOnUnixTest> ("CreatePing", 1);
			AssertRuleFailure<FeatureRequiresRootPrivilegeOnUnixTest> ("UsePing", 1);
		}

		public void CreateObject ()
		{
			new object ();
		}

		[Test]
		public void TestCreate ()
		{
			AssertRuleSuccess<FeatureRequiresRootPrivilegeOnUnixTest> ("CreateObject");
		}
	}
}
