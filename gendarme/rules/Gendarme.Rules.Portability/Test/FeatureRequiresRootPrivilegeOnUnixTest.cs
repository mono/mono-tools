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
using System.Reflection;
using System.Diagnostics;
using System.Net.NetworkInformation;

using Gendarme.Framework;
using Gendarme.Rules.Portability;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Portability {

	class MyPing : Ping { } //MyPing..ctor calls Ping..ctor, this triggers the rule.
	class MyProcess : Process { } //Using MyProcess.PriorityClass calls Process.PriorityClass. The property is not virtual.

	[TestFixture]
	public class FeatureRequiresRootPrivilegeOnUnixTest {

		private FeatureRequiresRootPrivilegeOnUnixRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;
		private TypeDefinition type;

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

		public void SetPriorityNormalVariable ()
		{
			ProcessPriorityClass priority = ProcessPriorityClass.Normal;
			Process p = new Process ();
			p.PriorityClass = priority;
		}

		public void SetMyPriority ()
		{
			MyProcess p = new MyProcess ();
			p.PriorityClass = ProcessPriorityClass.AboveNormal;
		}

		public void CreatePing ()
		{
			new Ping ();
		}

		public void CreateObject ()
		{
			new object ();
		}

		public void UsePing (Ping ping)
		{
			// e.g. Ping could be supplied from another assembly
			// but Gendarme should still flag it's usage inside the analyzed assembly
			ping.Send ("127.0.0.1");
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Portability.FeatureRequiresRootPrivilegeOnUnixTest"];
			rule = new FeatureRequiresRootPrivilegeOnUnixRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name)
					return method;
			}
			return null;
		}

		[Test]
		public void TestSetPriority ()
		{
			MethodDefinition method = GetTest ("SetPriority");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestSetMyPriority ()
		{
			MethodDefinition method = GetTest ("SetMyPriority");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestSetPriorityNormal ()
		{
			MethodDefinition method = GetTest ("SetPriorityNormal"); //allowed value
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		[Ignore ("we warn if the value is set from a variable")]
		public void TestSetPriorityNormalVariable ()
		{
			MethodDefinition method = GetTest ("SetPriorityNormalVariable");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestMyPing ()
		{
			TypeDefinition type = assembly.MainModule.Types ["Test.Rules.Portability.MyPing"]; //this class extends Ping
			MethodDefinition method = type.Constructors [0]; //the constructor calls the base constructor
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestCreatePing ()
		{
			MethodDefinition method = GetTest ("CreatePing"); // calls new Ping ()
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestCreateObject ()
		{
			MethodDefinition method = GetTest ("CreateObject"); //calls new object()
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void TestUsePing ()
		{
			// use an already created Ping instance
			MethodDefinition method = GetTest ("UsePing");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}
	}
}
