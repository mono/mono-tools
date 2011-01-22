//
// Unit tests for ConsiderAddingInterfaceRule
//
// Authors:
//	Cedric Vivier  <cedricv@neonux.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Cedric Vivier
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Rules.Design;

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Design {

	interface IEmptyInterface {
	}

	interface INeverImplementedInterface {
		bool Never(string bar);
	}

	interface INeverImplementedInterface2 {
		bool Never2(string bar);
	}

	interface IAlreadyImplementedInterface {
		void Already();
	}

	interface IBaseImplementedInterface {
		bool Base(string bar);
	}

	interface INotPubliclyImplementedInterface {
		void NotPublic();
	}

	interface IPartiallyImplementedInterface {
		bool Foo(string bar);
		bool Bar(string foo);
	}

	interface IAlmostImplementedInterface {
		bool Almost(string bar);
	}

	interface IImplementedInterface {
		bool Implement(string bar);
	}

	interface IBothInterface {
		void Both();
	}

	interface IImplementedInterface2 {
		bool Implement(string bar);
		void Implement2();
		IEnumerable<int> GenericArgs(IEnumerable<int> l);
		bool GenericMethod<T>(T l);
	}

	interface IExtra : IImplementedInterface2 {
		string Name { get; }
	}

	// looks like IExtra but it does not implement anything from IImplementedInterface2
	public class Extra {
		public string Name { 
			get { return String.Empty; } 
		}
	}

	static class Never2BecauseStatic {
		public static bool Never2(string bar) { return true; }
	}

	class AlreadyImplemented : IAlreadyImplementedInterface {
		public void Already() {}
	}

	class BaseImplementingInterface : IBaseImplementedInterface {
		public bool Base(string bar) { return true; }
	}

	class BaseImplementsInterface : BaseImplementingInterface {
		public void NotBase() {}
	}

	class BaseImplementsInterface2 : BaseImplementsInterface {
		public void NotBase2() {}
	}

	class NotPubliclyImplemented {
		protected void NotPublic() {}
	}

	class NotPubliclyImplemented2 {
		private void NotPublic() {}
	}

	class NotPubliclyImplemented3 {
		static void NotPublic() {}
	}

	class PartiallyImplemented1 {
		public bool Foo(string bar) { return true; }
	}

	class PartiallyImplemented2 {
		public bool Bar(string foo) { return true; }
	}

	class AlmostImplemented1 { //because bar is int not string
		public bool Almost(int bar) { return true; }
	}

	class AlmostImplemented2 { //because return is int not bool
		public int Almost(string bar) { return 0; }
	}

	class Implemented {
		public int Noise(string bar) { return 0; }
		public int Noise2() { return 0; }
		public bool Implement(string bar) { return true; }//!
		public void Noise3(string n) {}
	}

	class ImplementedB {
		public bool Implement(string bar) { return true; }
		private void Both() {}
	}

	class Implemented2 {
		public bool Noise(string bar) { return true; }
		public bool Noise2() { return true; }

		public bool Implement(string bar) { return true; }//!
		public void Implement2() {}

		public void Both() { }

		public IEnumerable<int> GenericArgs(IEnumerable<int> l) { return null; }//!
		public bool GenericMethod<T>(T l) { return true; }
	}

	interface IUnconstrainedGeneric<T> {

		T GetIt ();
	}

	interface IConstrainedGeneric<T> where T : IFormattable {

		T GetIt ();
	}

	class ConstrainedGeneric<T> where T : IDisposable {

		public T GetIt () 
		{
			return default(T); 
		}
	}

	class ConstrainedGenericSame<T> where T : IFormattable {

		public T GetIt ()
		{
			return default (T);
		}
	}

	public interface IUnconstrained {

		void SetIt<T> (T value);
	}

	public interface IConstrained {

		void SetIt<T> (T value) where T : IFormattable;
	}

	class Constrained {

		public void SetIt<T>(T value) where T : IDisposable
		{
		}
	}

	class ConstrainedSame {

		public void SetIt<T> (T value) where T : IFormattable
		{
		}
	}

	[TestFixture]
	public class ConsiderAddingInterfaceTest : TypeRuleTestFixture<ConsiderAddingInterfaceRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<IEmptyInterface> ();
			AssertRuleDoesNotApply (SimpleTypes.Enum);   // enum
			AssertRuleDoesNotApply (SimpleTypes.Class);  // class
			AssertRuleDoesNotApply (SimpleTypes.Structure); // struct
			AssertRuleDoesNotApply (SimpleTypes.Delegate);  // delegate
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<INeverImplementedInterface> ();
			AssertRuleSuccess<INeverImplementedInterface2> ();
			AssertRuleSuccess<IAlreadyImplementedInterface> ();
			AssertRuleSuccess<IBaseImplementedInterface> ();
			AssertRuleSuccess<INotPubliclyImplementedInterface> ();
			AssertRuleSuccess<IPartiallyImplementedInterface> ();
		}

		[Test]
		public void Failure ()
		{
			//4 because IImplementedInterface2 could also derive from it
			AssertRuleFailure<IImplementedInterface> (4);

			AssertRuleFailure<IBothInterface> (1);
			AssertRuleFailure<IImplementedInterface2> (1);
		}

		[Test] // bnc 665161
		public void BaseInterface ()
		{
			// Extra implement IExtra but not IImplementedInterface2 (which IExtra is derived from)
			AssertRuleSuccess<IExtra> ();
		}

		[Test] // bnc 665161
		public void GenericConstraints ()
		{
			// generic constraints on types
			AssertRuleSuccess<IUnconstrainedGeneric<IFormattable>> ();
			AssertRuleSuccess<IConstrainedGeneric<IFormattable>> ();
			// generic constraints on methods
			AssertRuleSuccess<IUnconstrained> ();
			AssertRuleSuccess<IConstrained> ();
		}
	}
}

