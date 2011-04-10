//
// Unit tests for TypesWithDisposableFieldsShouldBeDisposableRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008, 2011 Novell, Inc (http://www.novell.com)
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

using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

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

		public DisposeableFieldsImplementsIDisposeable ()
		{
			B = new Disposable ();
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}
	}

	class DisposeableFieldsImplementsIDisposeableCorrectly : IDisposable {
		object A;
		Disposable B;

		public DisposeableFieldsImplementsIDisposeableCorrectly ()
		{
			B = new Disposable ();
		}

		public void Dispose ()
		{
			B.Dispose (); // not really correct but Dispose is called :)
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

	abstract class DisposeableFieldsImplementsIDisposeableAbstractAssigned : IDisposable {
		object A;
		Disposable B;

		protected DisposeableFieldsImplementsIDisposeableAbstractAssigned ()
		{
			B = new Disposable ();
		}

		public abstract void Dispose ();
	}

	public class DisposeableFieldsNeverAssigned : ICloneable {
		object A;
		Disposable B;

		public object Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	public class DisposeableFieldsNullAssigned : ICloneable {
		object A;
		Disposable B;

		public DisposeableFieldsNullAssigned ()
		{
			A = null;
			B = null;
		}

		public object Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	public class DisposeableFieldsAssigned : ICloneable {
		object A;
		Disposable B;

		public DisposeableFieldsAssigned ()
		{
			A = null;
			B = new Disposable ();
		}

		public object Clone ()
		{
			throw new NotImplementedException ();
		}
	}

	class DisposeableFieldsReferenced : ICloneable {
		object A;
		Disposable B;

		public DisposeableFieldsReferenced (Disposable instance)
		{
			A = null;
			B = instance;
		}

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

	class DisposeableFieldsArrayAssigned : ICloneable {
		object A;
		Disposable [] B;

		public object Clone ()
		{
			// the array itself is not not IDisposable
			B = new Disposable [10];
			A = B;
			return A;
		}
	}

	class DisposeableFieldsArrayMembers : ICloneable {
		object A;
		Disposable [] B;

		public object Clone ()
		{
			B = new Disposable [1];
			// assignation (newobj+stfld) does not need to to be inside ctor
			// note: fxcop does not catch this one
			B [0] = new Disposable ();
			A = B;
			return A;
		}
	}

	struct StructWithDisposeableFields {
		Disposable a;
		object b;

		public StructWithDisposeableFields (object obj)
		{
			b = obj;
			a = new Disposable ();
		}
	}

	class DisposeableStaticFieldsArray {
		object A;
		static Disposable [] B;

		static DisposeableStaticFieldsArray ()
		{
			B = new Disposable [1];
			B [0] = new Disposable ();
		}
	}

	// test case from https://bugzilla.novell.com/show_bug.cgi?id=671029

	interface ISession : IDisposable {
		void Query (string s);
	}

	class SomeRepository {
		ISession session;
		public SomeRepository (ISession session)
		{
			this.session = session;
		}
		public void DoSomeQuery ()
		{
			session.Query ("whatever");
		}
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
			AssertRuleSuccess<DisposeableFieldsImplementsIDisposeableCorrectly> ();
		}

		[Test]
		public void TestDisposeableFieldsExplicit ()
		{
			AssertRuleSuccess<DisposeableFieldsExplicit> ();
		}

		[Test]
		public void TestDisposeableFieldsImplementsIDisposeableAbstract ()
		{
			AssertRuleFailure<DisposeableFieldsImplementsIDisposeableAbstract> (1);
			AssertRuleFailure<DisposeableFieldsImplementsIDisposeableAbstractAssigned> (2);
		}

		[Test]
		public void TestDisposeableFields ()
		{
			AssertRuleSuccess<DisposeableFieldsNeverAssigned> ();
			AssertRuleSuccess<DisposeableFieldsNullAssigned> ();
			AssertRuleSuccess<DisposeableFieldsReferenced> ();
			AssertRuleFailure<DisposeableFieldsAssigned> (1);
		}

		[Test]
		public void TestDisposeableFieldsArray ()
		{
			AssertRuleSuccess<DisposeableFieldsArray> ();
			AssertRuleSuccess<DisposeableFieldsArrayAssigned> ();
			AssertRuleFailure<DisposeableFieldsArrayMembers> (1);
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

		[Test]
		public void Bug671029 ()
		{
			AssertRuleSuccess<SomeRepository> ();
		}
	}
}
