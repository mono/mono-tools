//
// Unit tests for DisposableFieldsShouldBeDisposedRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class DisposableFieldsShouldBeDisposedTest : TypeRuleTestFixture<DisposableFieldsShouldBeDisposedRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Interface);

			// SimpleTypes.Class does not implement IDisposable
			AssertRuleDoesNotApply (SimpleTypes.Class);

			// [GeneratedCode]
			AssertRuleDoesNotApply (SimpleTypes.GeneratedType);
		}

		abstract class AbstractDisposable : IDisposable {

			abstract public void Dispose ();
		}

		abstract class AbstractExplicitDisposable : IDisposable {

			void IDisposable.Dispose ()
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void Abstract ()
		{
			AssertRuleSuccess<AbstractDisposable> ();
			AssertRuleSuccess<AbstractExplicitDisposable> ();
		}

		class FalsePositive : IDisposable {
			int A;
			object[] b;

			public void Dispose () //no warning
			{
				throw new NotImplementedException ();
			}
		}

		[Test]
		public void TestFalsePositive ()
		{
			AssertRuleSuccess<FalsePositive> ();
		}


		class Disposable : IDisposable {
			public Disposable A;
			public virtual void Dispose () //no warning
			{
				A.Dispose ();
			}
		}

		[Test]
		public void TestDisposable ()
		{
			AssertRuleSuccess<Disposable> ();
		}


		class ExtendsDispose : Disposable, IDisposable {
			public override void Dispose () //warning: should call base
			{
			}
		}

		class ExtendsExplictDispose : Disposable, IDisposable {

			void IDisposable.Dispose () //warning: should call base
			{
			}
		}

		class ExtendsExternalDispose : Disposable, IDisposable {
			// can't use DllImport here since we can't declare Dispose as static
			[MethodImpl (MethodImplOptions.InternalCall)]
			extern public override void Dispose ();
		}

		class ExtendsExternalDisposeBool : Disposable, IDisposable {
			public override void Dispose ()
			{
				Dispose (true);
			}

			// can't use DllImport here since we can't declare Dispose as static
			[MethodImpl (MethodImplOptions.InternalCall)]
			extern public void Dispose (bool dispose);
		}

		[Test]
		public void TestExtendsDispose ()
		{
			AssertRuleFailure<ExtendsDispose> (1);
			AssertRuleFailure<ExtendsExplictDispose> (1);
			AssertRuleFailure<ExtendsExternalDispose> (1);
			AssertRuleFailure<ExtendsExternalDisposeBool> (1);
		}
		

		class ExtendsDisposeCallsBase : Disposable, IDisposable {
			public override void Dispose () //no warning
			{
				base.Dispose ();
			}
		}

		[Test]
		public void TestExtendsDisposeCallsBase ()
		{
			AssertRuleSuccess<ExtendsDisposeCallsBase> ();
		}


		class ExtendsDispose2 : Disposable, IDisposable {
			public Disposable B;
			void Dispose () //warn: should dispose B
			{
				base.Dispose ();
			}
		}

		[Test]
		public void TestExtendsDispose2 ()
		{
			AssertRuleFailure<ExtendsDispose2> (1);
		}
		

		class DisposeableFieldsCorrect : IDisposable {
			object A;
			Disposable B;

			public void Dispose () //no warning
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsCorrect ()
		{
			AssertRuleSuccess<DisposeableFieldsCorrect> ();
		}


		class MultipleDisposeableFieldsCorrect : IDisposable {
			object A;
			Disposable B;
			Disposable C;

			public void Dispose () //no warning
			{
				B.Dispose ();
				C.Dispose ();
			}
		}

		[Test]
		public void TestMultipleDisposeableFieldsCorrect ()
		{
			AssertRuleSuccess<MultipleDisposeableFieldsCorrect> ();
		}

		
		class DisposeableFieldsIncorrect : IDisposable {
			object A;
			Disposable B;

			public void Dispose () //warn
			{
				A = B;
				int f = B.GetHashCode ();
			}
		}

		[Test]
		public void TestDisposeableFieldsIncorrect ()
		{
			AssertRuleFailure<DisposeableFieldsIncorrect> (1);
		}
		

		class DisposeableFieldsDisposePattern : IDisposable {
			object A;
			Disposable B;

			public void Dispose () //warn
			{
				A = B;
				Dispose (true);
			}

			private void Dispose (bool disposing)
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsDisposePattern ()
		{
			AssertRuleSuccess<DisposeableFieldsDisposePattern> ();
		}


		class DisposeableFieldsExplicit : IDisposable {
			object A;
			Disposable B;

			void IDisposable.Dispose ()
			{
				B.Dispose ();
			}

		}

		[Test]
		public void TestDisposeableFieldsExplicit ()
		{
			AssertRuleSuccess<DisposeableFieldsExplicit> ();
		}


		class DisposeableFieldsTwoCorrect : IDisposable {
			object A;
			Disposable B;

			void IDisposable.Dispose ()
			{
				B.Dispose ();
			}

			void Dispose ()
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsTwoCorrect ()
		{
			AssertRuleSuccess<DisposeableFieldsTwoCorrect> ();
		}

		class DisposeableFieldsTwoIncorrect : IDisposable {
			object A;
			Disposable B;

			void IDisposable.Dispose ()
			{

			}

			void Dispose ()
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsTwoIncorrect ()
		{
			AssertRuleFailure<DisposeableFieldsTwoIncorrect> (1);
		}


		class DisposeableFieldsWithStaticExplicit : IDisposable {
			object A;
			Disposable B;
			static Disposable C;

			void IDisposable.Dispose ()
			{
				B.Dispose ();
			}
		}

		[Test]
		public void TestDisposeableFieldsWithStaticExplicit ()
		{
			AssertRuleSuccess<DisposeableFieldsWithStaticExplicit> ();
		}

		class DelegatedInsideDispose : IDisposable {

			static BackgroundWorker worker = new BackgroundWorker ();
			Disposable B;

			public DelegatedInsideDispose ()
			{
				B = new Disposable ();
			}

			public void Dispose ()
			{
				worker.DoWork += delegate (object o, DoWorkEventArgs e) {
					B.Dispose ();
				};
				worker.RunWorkerAsync ();
			}
		}

		[Test]
		[Ignore ("rule cannot be certain the delegate will be called")]
		public void TestDelegatedInsideDispose ()
		{
			AssertRuleSuccess<DelegatedInsideDispose> ();
		}

		class DelegatedOutsideDispose : IDisposable {

			static BackgroundWorker worker = new BackgroundWorker ();
			Disposable B;

			public DelegatedOutsideDispose ()
			{
				B = new Disposable ();
				worker.DoWork += delegate (object o, DoWorkEventArgs e) {
					B.Dispose ();
				};
			}

			public void Dispose ()
			{
				worker.RunWorkerAsync ();
			}
		}

		[Test]
		[Ignore ("rule cannot be certain which and if the delegate will be called")]
		public void TestDelegatedOutsideDispose ()
		{
			AssertRuleSuccess<DelegatedOutsideDispose> ();
		}

		// test case provided by Guillaume Gautreau
		public class AutomaticProperties : IDisposable {

			// this is not a field, source wise, but the compiler will generated one
			// that Gendarme will notice
			public IDisposable TestField { get; set; }

			public void Dispose ()
			{
				this.Dispose (true);
				GC.SuppressFinalize (this);
			}

			protected virtual void Dispose (bool disposing)
			{
				if (disposing) {
					if (this.TestField != null) {
						this.TestField.Dispose ();
					}
				}
			}
		}

		[Test]
		public void AutomaticProperties_BackingField ()
		{
			AssertRuleSuccess<AutomaticProperties> ();
		}
	}
}
