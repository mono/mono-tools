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

using Gendarme.Rules.BadPractice;

using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class DisableDebuggingCodeTest : MethodRuleTestFixture<DisableDebuggingCodeRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no NEWOBJ
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

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

		[Test]
		public void CommonCheck ()
		{
			AssertRuleSuccess<DisableDebuggingCodeTest> ("ConditionalTrace");
			AssertRuleFailure<DisableDebuggingCodeTest> ("ConditionalOther", 1);
		}

		[Test]
		[Conditional ("DEBUG")]
		public void DebugCheck ()
		{
			AssertRuleSuccess<DisableDebuggingCodeTest> ("ConditionalDebug");
			AssertRuleSuccess<DisableDebuggingCodeTest> ("ConditionalMultiple");
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

		[Test]
		public void NonDebug ()
		{
#if DEBUG
 			AssertRuleSuccess<DisableDebuggingCodeTest> ("UsingDebug"); 
#else
			AssertRuleDoesNotApply<DisableDebuggingCodeTest> ("UsingDebug");	// method has no body in release
#endif
			AssertRuleSuccess<DisableDebuggingCodeTest> ("UsingTrace");
			AssertRuleFailure<DisableDebuggingCodeTest> ("UsingConsole", 1);
		}

		[Test]
		public void Initialize ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (unit);

			Rule.Active = false;
			(Runner as TestRunner).OnAssembly (assembly);
			Assert.IsFalse (Rule.Active, "Default-Active-False");

			Rule.Active = true;
			(Runner as TestRunner).OnAssembly (assembly);
			Assert.IsTrue (Rule.Active, "Assembly-Active-True");

			(Runner as TestRunner).OnModule (assembly.MainModule);
			Assert.IsTrue (Rule.Active, "Module-Active-True");
		}
	}
}
