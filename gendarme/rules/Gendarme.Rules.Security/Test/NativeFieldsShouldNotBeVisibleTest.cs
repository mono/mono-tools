//
// Unit tests for NativeFieldsShouldNotBeVisibleRule
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

	public class HasPublicNativeField {
		public IntPtr Native;
	}

	public class HasProtectedNativeField {
		protected IntPtr Native;
	}

	public class HasInternalNativeField {
		internal IntPtr Native;
	}

	public class HasPublicReadonlyNativeField {
		public readonly IntPtr Native;
	}

	public class HasPublicNativeFieldArray {
		public IntPtr [] Native;
	}

	public class HasPublicReadonlyNativeFieldArray {
		public IntPtr [] Native;
	}

	public class HasPublicNativeFieldArrayArray {
		public IntPtr [] [] Native;
	}

	public class HasPublicNonNativeField {
		public object Field;
	}

	public class HasPrivateNativeField {
		private IntPtr Native;
	}

	[TestFixture]
	public class NativeFieldsShouldNotBeVisibleTest {

		private NativeFieldsShouldNotBeVisibleRule rule;
		private AssemblyDefinition assembly;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new NativeFieldsShouldNotBeVisibleRule ();
		}

		public TypeDefinition GetTest (string name)
		{
			return assembly.MainModule.Types [name];
		}

		[Test]
		public void TestHasPublicNativeField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPublicNativeField");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasProtectedNativeField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasProtectedNativeField");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasInternalNativeField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasInternalNativeField");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPublicReadonlyNativeField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPublicReadonlyNativeField");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPublicNativeFieldArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPublicNativeFieldArray");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPublicReadonlyNativeFieldArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPublicReadonlyNativeFieldArray");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPublicNativeFieldArrayArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPublicNativeFieldArrayArray");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}


		[Test]
		public void TestHasPublicNonNativeField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPublicNonNativeField");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestHasPrivateNativeField ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Security.HasPrivateNativeField");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}
	}
}
