//
// Unit tests for UseSuppressFinalizeOnIDisposableTypeWithFinalizerRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005-2006,2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class UseSuppressFinalizeOnIDisposableTypeWithFinalizerTest : TypeRuleTestFixture<UseSuppressFinalizeOnIDisposableTypeWithFinalizerRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		class NoDestructorClass {
		}

		[Test]
		public void NoDestructor ()
		{
			AssertRuleDoesNotApply<NoDestructorClass> ();
		}

		class DestructorClass {

			IntPtr ptr;

			public DestructorClass ()
			{
				ptr = (IntPtr) 1;
			}
			
			public IntPtr Handle {
				get { return ptr; }
			}

			~DestructorClass ()
			{
				ptr = IntPtr.Zero;
			}
		}

		[Test]
		public void Destructor ()
		{
			AssertRuleDoesNotApply<DestructorClass> ();
		}

		class IDisposableNoDestructorWithoutSuppressFinalizeClass: IDisposable {

			public void Dispose ()
			{
			}
		}

		[Test]
		public void IDisposableNoDestructorWithoutSuppressFinalize ()
		{
			AssertRuleDoesNotApply<IDisposableNoDestructorWithoutSuppressFinalizeClass> ();
		}

		class IDisposableNoDestructorWithSuppressFinalizeClass: IDisposable {

			public void Dispose ()
			{
				GC.SuppressFinalize (this);
			}
		}

		[Test]
		public void IDisposableNoDestructorWithSuppressFinalize ()
		{
			AssertRuleDoesNotApply<IDisposableNoDestructorWithSuppressFinalizeClass> ();
		}

		class IDisposableDestructorWithoutSuppressFinalizeClass: IDisposable {

			~IDisposableDestructorWithoutSuppressFinalizeClass ()
			{
			}

			public void Dispose ()
			{
			}
		}

		[Test]
		public void IDisposableDestructorWithoutSuppressFinalize ()
		{
			AssertRuleFailure<IDisposableDestructorWithoutSuppressFinalizeClass> (1);
		}

		class IDisposableDestructorWithSuppressFinalizeClass: IDisposable {

			~IDisposableDestructorWithSuppressFinalizeClass ()
			{
			}

			public void Dispose ()
			{
				GC.SuppressFinalize (this);
			}
		}

		[Test]
		public void IDisposableDestructorWithSuppressFinalize ()
		{
			AssertRuleSuccess<IDisposableDestructorWithSuppressFinalizeClass> ();
		}

		class ExplicitIDisposableDestructorWithoutSuppressFinalizeClass: IDisposable {

			~ExplicitIDisposableDestructorWithoutSuppressFinalizeClass ()
			{
			}

			void IDisposable.Dispose ()
			{
			}
		}

		[Test]
		public void ExplicitIDisposableDestructorWithoutSuppressFinalize ()
		{
			AssertRuleFailure<ExplicitIDisposableDestructorWithoutSuppressFinalizeClass> (1);
		}

		class ExplicitIDisposableDestructorWithSuppressFinalizeClass: IDisposable {

			~ExplicitIDisposableDestructorWithSuppressFinalizeClass ()
			{
			}

			void IDisposable.Dispose ()
			{
				GC.SuppressFinalize (this);
			}
		}

		[Test]
		public void ExplicitIDisposableDestructorWithSuppressFinalize ()
		{
			AssertRuleSuccess<ExplicitIDisposableDestructorWithSuppressFinalizeClass> ();
		}

		class BothIDisposableDestructorWithoutSuppressFinalizeClass : IDisposable {

			~BothIDisposableDestructorWithoutSuppressFinalizeClass ()
			{
			}

			void IDisposable.Dispose ()
			{
			}

			public void Dispose ()
			{
			}
		}

		[Test]
		public void BothIDisposableDestructorWithoutSuppressFinalize ()
		{
			AssertRuleFailure<BothIDisposableDestructorWithoutSuppressFinalizeClass> (2);
		}

		class IndirectClass : IDisposable {

			~IndirectClass ()
			{
			}

			void IDisposable.Dispose ()
			{
				Dispose ();
			}

			public void Dispose ()
			{
				ReallyDispose ();
			}

			void ReallyDispose ()
			{
				GC.SuppressFinalize (this);
			}
		}

		[Test]
		public void Indirect ()
		{
			AssertRuleSuccess<IndirectClass> ();
		}

		class IndirectDeepClass : IDisposable {

			~IndirectDeepClass ()
			{
			}

			void IDisposable.Dispose ()
			{
				Dispose ();
			}

			public void Dispose ()
			{
				AskForDisposal ();
			}

			void AskForDisposal ()
			{
				CouldDispose ();
			}

			void CouldDispose ()
			{
				ReallyDispose ();
			}

			void ReallyDispose ()
			{
				GC.SuppressFinalize (this);
			}
		}

		[Test]
		public void IndirectDeep ()
		{
			// one level too deep for the explicit Dispose method
			AssertRuleFailure<IndirectDeepClass> (1);
		}

		abstract class AbstractClass : IDisposable {

			~AbstractClass ()
			{
			}

			abstract public void Dispose ();
		}

		[Test]
		public void Abstract ()
		{
			AssertRuleSuccess<IndirectClass> ();
		}

		class PInvokeClass : IDisposable {

			~PInvokeClass ()
			{
			}

			public void Dispose ()
			{
				UnmanagedDispose ();
			}

			[DllImport ("liberty.so")]
			static extern void UnmanagedDispose ();
		}

		[Test]
		public void PInvoke ()
		{
			AssertRuleFailure<PInvokeClass> (1);
		}
	}
}
