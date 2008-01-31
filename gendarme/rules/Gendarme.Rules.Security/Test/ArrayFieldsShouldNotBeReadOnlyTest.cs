//
// Unit tests for ArrayFieldsShouldNotBeReadOnlyRule
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
using Gendarme.Rules.Security;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Security {

	public class HasStaticPublicReadonlyArray {
		public static readonly string [] Array;
	}

	public class HasPublicReadonlyArray {
		public readonly string [] Array;
	}

	public class HasProtectedReadonlyArray {
		protected readonly string [] Array;
	}

	public class HasInternalReadonlyArray {
		internal readonly string [] Array;
	}

	public class HasPrivateReadonlyArray {
		private readonly string [] Array;
	}

	public class HasNoReadonlyArray {
		public readonly string NoArray;
	}

	public class HasPublicArray {
		public string [] Array;
	}

	[TestFixture]
	public class ArrayFieldsShouldNotBeReadOnlyTest {

		private ArrayFieldsShouldNotBeReadOnlyRule rule;
		private AssemblyDefinition assembly;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new ArrayFieldsShouldNotBeReadOnlyRule ();
		}

		public TypeDefinition GetTest (string name)
		{
			return assembly.MainModule.Types [name];
		}

		[Test]
		public void TestHasStaticPublicReadonlyArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasStaticPublicReadonlyArray");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPublicReadonlyArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPublicReadonlyArray");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasProtectedReadonlyArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasProtectedReadonlyArray");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasInternalReadonlyArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasInternalReadonlyArray");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPrivateReadonlyArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPrivateReadonlyArray");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasNoReadonlyArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasNoReadonlyArray");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPublicArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPublicArray");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}
	}
}
