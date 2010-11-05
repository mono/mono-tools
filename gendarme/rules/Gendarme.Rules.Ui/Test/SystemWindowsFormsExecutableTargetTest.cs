//
// Unit tests for SystemWindowsFormsExecutableTargetRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006,2008 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Reflection;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.UI;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Ui {

	[TestFixture]
	public class SystemWindowsFormsExecutableTargetTest {

		private IAssemblyRule rule;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			rule = new SystemWindowsFormsExecutableTargetRule ();
			runner = new TestRunner (rule);
		}

		[Test]
		public void Library ()
		{
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (Assembly.GetExecutingAssembly ().Location);
			// this (unit test) assembly is a library (dll) and has no entry point
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckAssembly (assembly));
		}

		[Test]
		public void ConsoleExe ()
		{
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (new MemoryStream (ExecutableTargetTest.conexe_exe));
			// this assembly is a executable (exe) but doesn't refer to SWF
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckAssembly (assembly));
		}

		[Test]
		public void WinExe ()
		{
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (new MemoryStream (ExecutableTargetTest.swf_winexe_exe));
			// this assembly is a executable (exe), refer to SWF and is compiled with /winexe
			Assert.AreEqual (RuleResult.Success, runner.CheckAssembly (assembly));
		}

		[Test]
		public void SwfExe ()
		{
			AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (new MemoryStream (ExecutableTargetTest.swfexe_exe));
			// this assembly is a executable (exe) and refer to SWF but isn't compiled with /winexe
			Assert.AreEqual (RuleResult.Failure, runner.CheckAssembly (assembly));
		}
	}
}
