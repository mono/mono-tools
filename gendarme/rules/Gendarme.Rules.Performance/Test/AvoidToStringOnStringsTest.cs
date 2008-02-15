//
// Unit test for AvoidToStringOnStringsRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//
// Copyright (C) 2007 Lukasz Knop
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
using System.Text;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Performance;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Performance
{
	[TestFixture]
	public class AvoidToStringOnStringsTest
	{

		public class Item
		{
			private string field = "";
			private int nonStringField;
			private static int nonStringStaticField;

			public string ToStringOnLocalString()
			{
				string a = String.Empty;
				return a.ToString();
			}

			public string ToStringOnParameter(string param)
			{
				return param.ToString();
			}

			public string ToStringOnStaticField()
			{
				return String.Empty.ToString();
			}

			public string ToStringOnField()
			{
				return field.ToString();
			}

			public string ToStringOnMethodResult()
			{
				return String.Empty.ToLower().ToString();
			}

			private int ReturnInt()
			{
				return 0;
			}

			public void ValidToString(int param)
			{
				int local = 0;

				string var = local.ToString();
				var = nonStringField.ToString();
				var = nonStringStaticField.ToString();
				var = param.ToString();
				var = ReturnInt().ToString();
			}

			
		}

		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private ModuleDefinition module;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			string unit = Assembly.GetExecutingAssembly().Location;
			assembly = AssemblyFactory.GetAssembly(unit);
			module = assembly.MainModule;
			type = module.Types["Test.Rules.Performance.AvoidToStringOnStringsTest/Item"];
			rule = new AvoidToStringOnStringsRule();
			runner = new TestRunner (rule);
		}

		MethodDefinition GetTest(string name)
		{
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == name)
					return method;

			return null;
		}

		[Test]
		public void TestLocalString()
		{
			MethodDefinition method = GetTest("ToStringOnLocalString");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod(method));
		}

		[Test]
		public void TestParameter()
		{
			MethodDefinition method = GetTest("ToStringOnParameter");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod(method));
		}

		[Test]
		public void TestStaticField()
		{
			MethodDefinition method = GetTest("ToStringOnStaticField");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod(method));
		}

		[Test]
		public void TestField()
		{
			MethodDefinition method = GetTest("ToStringOnField");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod(method));
		}

		[Test]
		public void TestMethodResult()
		{
			MethodDefinition method = GetTest("ToStringOnMethodResult");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod(method));
		}

		[Test]
		public void TestValidToString()
		{
			MethodDefinition method = GetTest("ValidToString");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod(method));
		}



	}
}


