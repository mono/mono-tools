//
// Unit tests for MethodCanBeMadeStatic rule
//
// Authors:
//	Jb Evain <jbevain@gmail.com>
//
// Copyright (C) 2007 Jb Evain
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

using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Correctness;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class MethodCanBeMadeStaticTest {

		public class Item {

			public int Foo ()
			{
				return 42;
			}

			public int _bar;

			public int Bar ()
			{
				return _bar = 42;
			}

			public static int Baz ()
			{
				return 42;
			}

			public virtual void Gazonk ()
			{
			}
		}

		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private ModuleDefinition module;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			module = assembly.MainModule;
			type = module.Types ["Test.Rules.Correctness.MethodCanBeMadeStaticTest/Item"];
			rule = new MethodCanBeMadeStaticRule ();
		}

		MethodDefinition GetTest (string name)
		{
			foreach (MethodDefinition method in type.Methods)
				if (method.Name == name)
					return method;

			return null;
		}

		MessageCollection CheckMethod (MethodDefinition method)
		{
			return rule.CheckMethod (method, new MinimalRunner ());
		}

		[Test]
		public void TestGoodCandidate ()
		{
			MethodDefinition method = GetTest ("Foo");
			Assert.IsNotNull (CheckMethod(method));
		}

		[Test]
		public void TestNotGoodCandidate ()
		{
			MethodDefinition method = GetTest ("Bar");
			Assert.IsNull (CheckMethod (method));
			method = GetTest ("Baz");
			Assert.IsNull (CheckMethod (method));
			method = GetTest ("Gazonk");
			Assert.IsNull (CheckMethod (method));
		}
	}
}
