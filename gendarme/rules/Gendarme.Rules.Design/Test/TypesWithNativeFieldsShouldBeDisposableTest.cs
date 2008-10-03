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
using System.Runtime.InteropServices;

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

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
	public class TypesWithNativeFieldsShouldBeDisposableTest : TypeRuleTestFixture<TypesWithNativeFieldsShouldBeDisposableRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		[Test]
		public void TestNoNativeFields ()
		{
			AssertRuleSuccess<NoNativeFields> ();
		}

		[Test]
		public void TestNativeFieldsImplementsIDisposeable ()
		{
			AssertRuleSuccess<NativeFieldsImplementsIDisposeable> ();
		}

		[Test]
		public void TestNativeFieldsExplicit ()
		{
			AssertRuleSuccess<NativeFieldsExplicit> ();
		}

		[Test]
		public void TestNativeFieldsIntPtr ()
		{
			AssertRuleFailure<NativeFieldsIntPtr> (1);
		}

		[Test]
		public void TestNativeFieldsUIntPtr ()
		{
			AssertRuleFailure<NativeFieldsUIntPtr> (1);
		}

		[Test]
		public void TestNativeFieldsHandleRef ()
		{
			AssertRuleFailure<NativeFieldsHandleRef> (1);
		}

		[Test]
		public void TestAbstractNativeFields ()
		{
			AssertRuleFailure<AbstractNativeFields> (2);
		}

		[Test]
		public void TestAbstractNativeFields2 ()
		{
			AssertRuleFailure<AbstractNativeFields2> (2);
		}

		[Test]
		public void TestNativeFieldsArray ()
		{
			AssertRuleFailure<NativeFieldsArray> (1);
		}

		[Test]
		public void TestStructWithNativeFields ()
		{
			AssertRuleDoesNotApply<StructWithNativeFields> ();
		}
		
		[Test]
		public void TestNativeStaticFieldsArray ()
		{
			AssertRuleSuccess<NativeStaticFieldsArray> ();
		}
	}
}
