//
// Unit tests for UseStringEmptyRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using Gendarme.Rules.Performance;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Performance {

	[TestFixture]
	public class UseStringEmptyTest {
	
		public class TestCase {
		
			public const string public_const_field = "";
			
			private static string private_static_field = "";
			
			
			public string GetConstField ()
			{
				return public_const_field;
			}
			
			public string GetStaticField ()
			{
				return private_static_field;
			}
			
			public string Append (string user_value)
			{
				return user_value + "";
			}
			
			public string Enclose (string user_value)
			{
				return "" + user_value + "";
			}
						
			// nice way

			public string public_field = "";
			
			public string GetField ()
			{
				return public_field;
			}
			
			public string Prepend (string user_value)
			{
				return String.Empty + user_value;
			}
			
			public int NoStringWereHarmedInThisTestCase ()
			{
				return 42;
			}
		}


		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private ModuleDefinition module;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			module = assembly.MainModule;
			type = assembly.MainModule.Types["Test.Rules.Performance.UseStringEmptyTest/TestCase"];
			rule = new UseStringEmptyRule ();
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
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void Append ()
		{
			MethodDefinition method = GetTest ("Append");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void Enclose ()
		{
			MethodDefinition method = GetTest ("Enclose");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void Constructor ()
		{
			// the "public_field" field is set to "" in the (hidden) ctor
			MethodDefinition method = GetTest (".ctor");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void StaticConstructor ()
		{
			// the "private_static_field" field is set to "" in the (hidden) class ctor
			MethodDefinition method = GetTest (".cctor");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void GetField ()
		{
			MethodDefinition method = GetTest ("GetField");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void GetStaticField ()
		{
			MethodDefinition method = GetTest ("GetStaticField");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void Prepend ()
		{
			MethodDefinition method = GetTest ("Prepend");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}
		
		[Test]
		public void NoHarm ()
		{
			MethodDefinition method = GetTest ("NoStringWereHarmedInThisTestCase");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}
	}
}
