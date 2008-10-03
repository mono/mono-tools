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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

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
	public class TypesWithDisposableFieldsShouldBeDisposableTest : TypeRuleTestFixture<TypesWithDisposableFieldsShouldBeDisposableRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		[Test]
		public void TestNoDisposeableFields ()
		{
			AssertRuleSuccess<NoDisposeableFields> ();
		}

		[Test]
		public void TestDisposeableFieldsImplementsIDisposeable ()
		{
			AssertRuleSuccess<DisposeableFieldsImplementsIDisposeable> ();
		}

		[Test]
		public void TestDisposeableFieldsExplicit ()
		{
			AssertRuleSuccess<DisposeableFieldsExplicit> ();
		}

		[Test]
		public void TestDisposeableFieldsImplementsIDisposeableAbstract ()
		{
			AssertRuleFailure<DisposeableFieldsImplementsIDisposeableAbstract> (2);
		}

		[Test]
		public void TestDisposeableFields ()
		{
			AssertRuleFailure<DisposeableFields> (1);
		}

		[Test]
		public void TestDisposeableFieldsArray ()
		{
			AssertRuleFailure<DisposeableFieldsArray> (1);
		}

		[Test]
		public void TestStructWithDisposeableFields ()
		{
			AssertRuleDoesNotApply<StructWithDisposeableFields> ();
		}

		[Test]
		public void TestDisposeableStaticFieldsArray ()
		{
			AssertRuleSuccess<DisposeableStaticFieldsArray> ();
		}
	}
}
