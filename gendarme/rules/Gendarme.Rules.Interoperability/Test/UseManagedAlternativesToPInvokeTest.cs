//
// Unit tests for UseManagedAlternativesToPInvokeRule
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Daniel Abramov
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
using System.Reflection;
using System.Runtime.InteropServices;

using Gendarme.Framework;
using Gendarme.Rules.Interoperability;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Interoperability {

	[TestFixture]
	public class UseManagedAlternativesToPInvokeTest {

		private UseManagedAlternativesToPInvokeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[DllImport ("kernel32")]
		static extern void Sleep (uint time); // bad because we have Thread.Sleep ()

		[DllImport ("user32.dll")]
		static extern Boolean MessageBeep (UInt32 beepType); // ok

		[DllImport ("kernel32.dll")]
		static extern Boolean FindFirstFile (); // wrong definition, but this occurs frequently in real life :|

		public void EmptyMethod () { } // FIXME: replace with PerfectMethods.EmptyMethod when porting to new tests model


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			type = assembly.MainModule.GetType ("Test.Rules.Interoperability.UseManagedAlternativesToPInvokeTest");
			rule = new UseManagedAlternativesToPInvokeRule ();
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
		public void TestEmptyMethod ()
		{
			MethodDefinition method = GetTest ("EmptyMethod");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
		}

		[Test]
		public void TestBadMethod ()
		{
			MethodDefinition method = GetTest ("Sleep");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestBadMethodMultipleSolutions ()
		{
			MethodDefinition method = GetTest ("FindFirstFile");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestOkMethod ()
		{
			MethodDefinition method = GetTest ("MessageBeep");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}
	}
}
