//
// Unit tests for TypesWithNativeFieldsShouldBeDisposableRule
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
using System.Runtime.InteropServices;

using Gendarme.Framework;
using Gendarme.Rules.Design;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Helpers;

namespace Test.Rules.Design {

	class NoNativeFields {
		int A;
		object b;
	}

	class NativeFieldsImplementsIDisposeable : IDisposable {
		object A;
		IntPtr B;

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	class NativeFieldsExplicit : IDisposable {
		object A;
		IntPtr B;

		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}
	}


	class NativeFieldsIntPtr : ICloneable {
		object A;
		IntPtr B;

		public object Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	class NativeFieldsUIntPtr : ICloneable {
		object A;
		UIntPtr B;

		public object Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	class NativeFieldsHandleRef : ICloneable {
		object A;
		System.Runtime.InteropServices.HandleRef B;

		public object Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	abstract class AbstractNativeFields : IDisposable {
		object A;
		System.Runtime.InteropServices.HandleRef B;

		public abstract void Dispose ();
	}

	abstract class AbstractNativeFields2 : IDisposable {
		object A;
		System.Runtime.InteropServices.HandleRef B;

		public abstract void Dispose ();


		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	class NativeFieldsArray : ICloneable {
		object A;
		UIntPtr [] B;

		public object Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	struct StructWithNativeFields {
		IntPtr a;
		UIntPtr b;
		HandleRef c;
	}

	class NativeStaticFieldsArray {
		object A;
		static UIntPtr [] B;
	}


	[TestFixture]
	public class TypesWithNativeFieldsShouldBeDisposableTest {

		private TypesWithNativeFieldsShouldBeDisposableRule rule;
		private AssemblyDefinition assembly;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new TypesWithNativeFieldsShouldBeDisposableRule ();
			runner = new TestRunner (rule);
		}

		public TypeDefinition GetTest (string name)
		{
			return assembly.MainModule.Types [name];
		}

		[Test]
		public void TestNoNativeFields ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NoNativeFields");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNativeFieldsImplementsIDisposeable ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NativeFieldsImplementsIDisposeable");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNativeFieldsExplicit ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NativeFieldsExplicit");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNativeFieldsIntPtr ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NativeFieldsIntPtr");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNativeFieldsUIntPtr ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NativeFieldsUIntPtr");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNativeFieldsHandleRef ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NativeFieldsHandleRef");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestAbstractNativeFields ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.AbstractNativeFields");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (2, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestAbstractNativeFields2 ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.AbstractNativeFields2");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (2, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestNativeFieldsArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NativeFieldsArray");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void TestStructWithNativeFields ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.StructWithNativeFields");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void TestNativeStaticFieldsArray ()
		{
			TypeDefinition type = GetTest ("Test.Rules.Design.NativeStaticFieldsArray");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
