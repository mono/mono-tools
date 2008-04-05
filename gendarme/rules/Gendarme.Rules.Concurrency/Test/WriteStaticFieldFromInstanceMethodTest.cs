//
// Unit tests for WriteStaticFieldFromInstanceMethodRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006-2008 Novell, Inc (http://www.novell.com)
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
using Gendarme.Rules.Concurrency;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class WriteStaticFieldFromInstanceMethodTest {

		public class TestCase {

			public const string public_const_field = "";

			private static string private_static_field = String.Empty;
			public static int private_public_field = 1;

			static TestCase ()
			{
				private_static_field = "static";
			}

			public TestCase ()
			{
				private_static_field = "instance";
			}


			public string GetConstField ()
			{
				return public_const_field;
			}

			public string GetStaticField ()
			{
				return private_static_field;
			}

			public void SetStaticField (string user_value)
			{
				private_static_field = user_value;
			}

			public string Append (string user_value)
			{
				return private_static_field + user_value;
			}

			public string AppendChange (string user_value)
			{
				private_static_field += user_value;
				return private_static_field;
			}

			public int MultipleChanges ()
			{
				private_public_field = 10 + private_public_field;
				private_public_field = private_public_field + 12;
				return private_public_field;
			}
		}

		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types["Test.Rules.Concurrency.WriteStaticFieldFromInstanceMethodTest/TestCase"];
			rule = new WriteStaticFieldFromInstanceMethodRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition md in type.Methods) {
				if (md.Name == name)
					return md;
			}
			foreach (MethodDefinition md in type.Constructors) {
				if (md.Name == name)
					return md;
			}
			return null;
		}

		[Test]
		public void GetConstField ()
		{
			MethodDefinition method = GetTest ("GetConstField");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void GetStaticField ()
		{
			MethodDefinition method = GetTest ("GetStaticField");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void SetStaticField ()
		{
			MethodDefinition method = GetTest ("SetStaticField");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void Append ()
		{
			MethodDefinition method = GetTest ("Append");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void AppendChange ()
		{
			MethodDefinition method = GetTest ("AppendChange");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void MultipleChanges ()
		{
			MethodDefinition method = GetTest ("MultipleChanges");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
			Assert.AreEqual (2, runner.Defects.Count, "Count");
		}

		[Test]
		public void StaticConstructor ()
		{
			MethodDefinition method = GetTest (".cctor");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckMethod (method));
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void Constructor ()
		{
			MethodDefinition method = GetTest (".ctor");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
	}
}
