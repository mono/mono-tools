//
// Unit test for AvoidUnneededCallsOnStringRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
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
using System.Text;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Performance;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Performance {

	[TestFixture]
	public class AvoidUnneededCallsOnStringTest {

		public class Item {

			private string field = "";
			private int nonStringField;
			private static int nonStringStaticField;

			public string ToStringOnLocalString ()
			{
				string a = String.Empty;
				return a.ToString ();
			}

			public string ToStringOnParameter (string param)
			{
				return param.ToString ();
			}

			public static string ToStringOnParameterStatic (string param)
			{
				return param.ToString ();
			}

			public string ToStringOnStaticField ()
			{
				return String.Empty.ToString ();
			}

			public string ToStringOnField ()
			{
				return field.ToString ();
			}

			public string ToStringOnMethodResult ()
			{
				return String.Empty.ToLower ().ToString ();
			}

			private int ReturnInt ()
			{
				return 0;
			}

			public void ValidToString (int param)
			{
				int local = 0;
				string var = local.ToString ();
				var = nonStringField.ToString ();
				var = nonStringStaticField.ToString ();
				var = param.ToString ();
				var = ReturnInt ().ToString ();
			}

			public string ThisToString ()
			{
				return this.ToString ();
			}

			// ToString(IFormatProvider)

			public string ToStringIFormatProviderField ()
			{
				return String.Empty.ToString (null);
			}

			private object value;

			public string ToStringBadParameterType (string format)
			{
				return ((int) value).ToString (format);
			}

			// Clone

			public object CloneField ()
			{
				return String.Empty.Clone ();
			}

			public object Clone ()
			{
				return null;
			}

			public object ThisClone ()
			{
				return this.Clone ();
			}

			// Substring

			public object SubstringZeroField ()
			{
				return String.Empty.Substring (0);
			}

			public object SubstringOneField ()
			{
				return String.Empty.Substring (1);
			}

			public object SubstringIntInt ()
			{
				return String.Empty.Substring (0, 0);
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
			type = assembly.MainModule.Types ["Test.Rules.Performance.AvoidUnneededCallsOnStringTest/Item"];
			rule = new AvoidUnneededCallsOnStringRule ();
			runner = new TestRunner (rule);
		}

		MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == name)
					return method;

			return null;
		}

		[Test]
		public void TestLocalString ()
		{
			MethodDefinition method = GetTest ("ToStringOnLocalString");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestParameter ()
		{
			MethodDefinition method = GetTest ("ToStringOnParameter");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Instance");

			method = GetTest ("ToStringOnParameterStatic");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Static");
		}

		[Test]
		public void TestStaticField ()
		{
			MethodDefinition method = GetTest ("ToStringOnStaticField");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestField ()
		{
			MethodDefinition method = GetTest ("ToStringOnField");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestMethodResult ()
		{
			MethodDefinition method = GetTest ("ToStringOnMethodResult");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method));
		}

		[Test]
		public void TestValidToString ()
		{
			MethodDefinition method = GetTest ("ValidToString");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method));
		}

		[Test]
		public void ToStringIFormatProvider ()
		{
			MethodDefinition method = GetTest ("ToStringIFormatProviderField");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Field");
		}

		[Test]
		public void ToStringOk ()
		{
			MethodDefinition method = GetTest ("ToStringBadParameterType");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "BadParameterType");

			method = GetTest ("ThisToString");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "ThisToString");
		}

		[Test]
		public void Clone ()
		{
			MethodDefinition method = GetTest ("CloneField");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Field");
		}

		[Test]
		public void CloneOk ()
		{
			MethodDefinition method = GetTest ("ThisClone");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "ThisClone");
		}

		[Test]
		public void Substring ()
		{
			MethodDefinition method = GetTest ("SubstringZeroField");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Field");
		}

		[Test]
		public void SubstringOk ()
		{
			MethodDefinition method = GetTest ("SubstringOneField");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "One");

			method = GetTest ("SubstringIntInt");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "IntInt");
		}
	}
}
