//
// Unit tests for NonConstantStaticFieldsShouldNotBeVisibleRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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

	public class HasPublicConst {
		public const int CONST = 0;
	}

	public class HasPublicNonConstantStaticField {
		public static int Field;
	}

	public class HasProtectedNonConstantStaticField {
		protected static int Field;
	}

	public class HasInternalNonConstantStaticField {
		internal static int Field;
	}

	public class HasPublicConstantStaticField {
		public static readonly int Field;
	}

	public class HasPrivateNonConstantStaticField {
		private static int Field;
	}

	public class HasPublicNonConstantField {
		public int Field;
	}

	[TestFixture]
	public class NonConstantStaticFieldsShouldNotBeVisibleTest {

		private NonConstantStaticFieldsShouldNotBeVisibleRule rule;
		private AssemblyDefinition assembly;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new NonConstantStaticFieldsShouldNotBeVisibleRule ();
		}

		public TypeDefinition GetTest (string name)
		{
			return assembly.MainModule.Types [name];
		}

		[Test]
		public void TestHasPublicConst ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Concurrency.HasPublicConst");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPublicNonConstantStaticField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Concurrency.HasPublicNonConstantStaticField");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasProtectedNonConstantStaticField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Concurrency.HasProtectedNonConstantStaticField");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasInternalNonConstantStaticField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Concurrency.HasInternalNonConstantStaticField");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPublicConstantStaticField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Concurrency.HasPublicConstantStaticField");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPrivateNonConstantStaticField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Concurrency.HasPrivateNonConstantStaticField");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPublicNonConstantField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Concurrency.HasPublicNonConstantField");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}
	}
}
