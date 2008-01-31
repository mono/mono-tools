//
// Unit tests for TypesWithDisposableFieldsShouldBeDisposableRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
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
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Design;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Design {

	class Disposable : IDisposable {
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	class NoDisposeableFields {
		int A;
		object b;
	}

	class DisposeableFieldsImplementsIDisposeable : IDisposable {
		object A;
		Disposable B;

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	class DisposeableFieldsExplicit : IDisposable {
		object A;
		Disposable B;

		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	abstract class DisposeableFieldsImplementsIDisposeableAbstract : IDisposable {
		object A;
		Disposable B;
		public void Dispose (object asd) { B.Dispose (); }
		public abstract void Dispose ();

	}

	class DisposeableFields : ICloneable {
		object A;
		Disposable B;

		public object Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	class DisposeableFieldsArray : ICloneable {
		object A;
		Disposable [] B;

		public object Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	struct StructWithDisposeableFields {
		Disposable a;
		object b;
	}

	class DisposeableStaticFieldsArray {
		object A;
		static Disposable [] B;
	}

	[TestFixture]
	public class TypesWithDisposableFieldsShouldBeDisposableTest {

		private TypesWithDisposableFieldsShouldBeDisposableRule rule;
		private AssemblyDefinition assembly;


		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new TypesWithDisposableFieldsShouldBeDisposableRule ();
		}

		public TypeDefinition GetTest (string name)
		{
			return assembly.MainModule.Types [name];
		}

		[Test]
		public void TestNoDisposeableFields ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NoDisposeableFields");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestDisposeableFieldsImplementsIDisposeable ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.DisposeableFieldsImplementsIDisposeable");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestDisposeableFieldsExplicit ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.DisposeableFieldsExplicit");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestDisposeableFieldsImplementsIDisposeableAbstract ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.DisposeableFieldsImplementsIDisposeableAbstract");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestDisposeableFields ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.DisposeableFields");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestDisposeableFieldsArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.DisposeableFieldsArray");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestStructWithDisposeableFields ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.StructWithDisposeableFields");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestDisposeableStaticFieldsArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.DisposeableStaticFieldsArray");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}
	}
}
