//
// Unit tests for NewLineLiteralRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
using Gendarme.Rules.Portability;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Portability {

	[TestFixture]
	public class NewLineTest {

		private IMethodRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;
		private TypeDefinition type;

		public string GetNewLineLiteral_13 ()
		{
			return "Hello\nMono";
		}

		public string GetNewLineLiteral_10 ()
		{
			return "\rHello Mono";
		}

		public string GetNewLineLiteral ()
		{
			return "Hello Mono\r\n";
		}

		public string GetNewLine ()
		{
			return String.Concat ("Hello Mono", Environment.NewLine);
		}

		public string GetNull ()
		{
			return null;
		}

		public string GetEmpty ()
		{
			return "";
		}

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types ["Test.Rules.Portability.NewLineTest"];
			rule = new NewLineLiteralRule ();
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
		public void HasNewLineLiteral_13 ()
		{
			MethodDefinition method = GetTest ("GetNewLineLiteral_13");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void HasNewLineLiteral_10 ()
		{
			MethodDefinition method = GetTest ("GetNewLineLiteral_10");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void HasNewLineLiteral ()
		{
			MethodDefinition method = GetTest ("GetNewLineLiteral");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void HasNewLine ()
		{
			MethodDefinition method = GetTest ("GetNewLine");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void HasNull ()
		{
			MethodDefinition method = GetTest ("GetNull");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void HasEmpty ()
		{
			MethodDefinition method = GetTest ("GetEmpty");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}
	}
}
