//
// Unit tests for DontIgnoreMethodResultRule
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

using Gendarme.Framework;
using Gendarme.Rules.Performance;
using Mono.Cecil;
using NUnit.Framework;
using System.Globalization;
using System.Reflection;

namespace Test.Rules.Performance
{
	[TestFixture]
	public class DontIgnoreMethodResultTest
	{
		public class Item
		{
			public void Violations()
			{
				"violationOne".ToUpper(CultureInfo.InvariantCulture);
				string violationTwo = "MediuM ";
				violationTwo.ToLower(CultureInfo.InvariantCulture).Trim();
			}

			public static void CreateItem()
			{
				new Item();
			}

			public void NotAViolation()
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("test").Append("test");
				ReturnInt();
			}

			private int ReturnInt()
			{
				return 0;
			}
		}

		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private ModuleDefinition module;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			string unit = Assembly.GetExecutingAssembly().Location;
			assembly = AssemblyFactory.GetAssembly(unit);
			module = assembly.MainModule;
			type = module.Types["Test.Rules.Performance.DontIgnoreMethodResultTest/Item"];
			rule = new DontIgnoreMethodResultRule();
		}

		MethodDefinition GetTest(string name)
		{
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == name)
					return method;

			return null;
		}

		MessageCollection CheckMethod(MethodDefinition method)
		{
			return rule.CheckMethod(method, new MinimalRunner());
		}

		[Test]
		public void TestStringMethods()
		{
			MethodDefinition method = GetTest("Violations");
			Assert.IsNotNull(CheckMethod(method));
		}

		[Test]
		public void TestConstructor()
		{
			MethodDefinition method = GetTest("CreateItem");
			Assert.IsNotNull(CheckMethod(method));
		}

		[Test]
		public void TestStringBuilder()
		{
			MethodDefinition method = GetTest("NotAViolation");
			Assert.IsNull(CheckMethod(method));
		}




	}
}
