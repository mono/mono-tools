//
// Unit test for AvoidConstructorsInStaticTypesRule
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
using System.Reflection;
using System.Text;
using Gendarme.Framework;
using Gendarme.Rules.Correctness;
using Mono.Cecil;
using NUnit.Framework;


namespace Test.Rules.Correctness
{


	[TestFixture]
	public class AvoidConstructorsInStaticTypesTest
	{
		public class CannotBeMadeStatic
		{
			public void Method()
			{

			}
		}

		public class CanBeMadeStatic
		{
			public static void Method()
			{
			}
		}

		public static class IsStatic
		{
			public static void Method()
			{
			}
		}

		public class IsMadeStatic
		{
			private IsMadeStatic()
			{
			}

			public static void Method()
			{
			}
		}

		public class EmptyClass 
		{
		}

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private MessageCollection messageCollection;

		[TestFixtureSetUp]
		public void FixtureSetUp()
		{
			string unit = Assembly.GetExecutingAssembly().Location;
			assembly = AssemblyFactory.GetAssembly(unit);
			rule = new AvoidConstructorsInStaticTypesRule();
			messageCollection = null;
		}


		[Test]
		public void TestClassHasNoPublicConstructors()
		{
			type = assembly.MainModule.Types["Test.Rules.Correctness.AvoidConstructorsInStaticTypesTest/IsMadeStatic"];
			messageCollection = rule.CheckType(type, new MinimalRunner());
			Assert.IsNull(messageCollection);
		}

		[Test]
		public void TestClassIsDeclaredStatic()
		{
			type = assembly.MainModule.Types["Test.Rules.Correctness.AvoidConstructorsInStaticTypesTest/IsStatic"];
			messageCollection = rule.CheckType(type, new MinimalRunner());
			Assert.IsNull(messageCollection);
		}


		[Test]
		public void TestClassCannotBeMadeStatic()
		{
			type = assembly.MainModule.Types["Test.Rules.Correctness.AvoidConstructorsInStaticTypesTest/CannotBeMadeStatic"];
			messageCollection = rule.CheckType(type, new MinimalRunner());
			Assert.IsNull(messageCollection);
		}

		[Test]
		public void TestClassCanBeMadeStatic()
		{
			type = assembly.MainModule.Types["Test.Rules.Correctness.AvoidConstructorsInStaticTypesTest/CanBeMadeStatic"];
			messageCollection = rule.CheckType(type, new MinimalRunner());
			Assert.IsNotNull(messageCollection);
		}

		[Test]
		public void TestEmptyClass ()
		{
			type = assembly.MainModule.Types["Test.Rules.Correctness.AvoidConstructorsInStaticTypesTest/EmptyClass"];
			messageCollection = rule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
	}
}
