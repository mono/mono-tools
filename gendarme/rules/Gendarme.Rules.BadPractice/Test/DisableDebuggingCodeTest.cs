// 
// Unit tests for DisableDebuggingCodeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Diagnostics;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;

using Test.Rules.Helpers;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class DisableDebuggingCodeTest {

		// note: [Conditional] is usable on type from 2.0 onward but only if it inherit from Attribute

		[Conditional ("DEBUG")]
		public void ConditionalDebug ()
		{
			Console.WriteLine ("debug");
		}

		[Conditional ("TRACE")]
		public void ConditionalTrace ()
		{
			Console.WriteLine ("debug");
		}

		[Conditional ("OTHER")]
		[Conditional ("DEBUG")]
		public void ConditionalMultiple ()
		{
			Console.WriteLine ("debug");
		}

		[Conditional ("OTHER")]
		public void ConditionalOther ()
		{
			Console.WriteLine ("debug");
		}

		public void UsingTrace ()
		{
			Trace.WriteLine ("debug");
		}

		public void UsingDebug ()
		{
			Debug.WriteLine ("debug");
		}

		[Category ("DEBUG")] // wrong attribute
		public void UsingConsole ()
		{
			Console.WriteLine ("debug");
		}


		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new DisableDebuggingCodeRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string method)
		{
			return assembly.MainModule.Types ["Test.Rules.BadPractice.DisableDebuggingCodeTest"].GetMethod (method);
		}

		[Test]
		public void Conditional ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (GetTest ("ConditionalDebug")), "ConditionalDebug");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (GetTest ("ConditionalTrace")), "ConditionalTrace");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (GetTest ("ConditionalMultiple")), "ConditionalMultiple");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetTest ("ConditionalOther")), "ConditionalOther");
		}

		[Test]
		public void NonDebug ()
		{
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (GetTest ("UsingDebug")), "UsingDebug");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (GetTest ("UsingTrace")), "UsingTrace");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (GetTest ("UsingConsole")), "UsingConsole");
		}

		[Test]
		public void Initialize ()
		{
			rule.Active = false;
			runner.OnAssembly (assembly);
			Assert.IsFalse (rule.Active, "Default-Active-False");

			rule.Active = true;
			runner.OnAssembly (assembly);
			Assert.IsTrue (rule.Active, "Assembly-Active-True");

			runner.OnModule (assembly.MainModule);
			Assert.IsTrue (rule.Active, "Module-Active-True");
		}
	}
}
