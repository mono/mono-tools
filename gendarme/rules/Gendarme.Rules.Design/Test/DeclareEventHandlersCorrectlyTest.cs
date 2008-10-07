//
// Unit tests for DeclareEventHandlersCorrectly rule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using System.Collections.Generic;
using Gendarme.Framework;
using Gendarme.Rules.Design;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;
using NUnit.Framework;

namespace Test.Rules.Design {
	[TestFixture]
	public class DeclareEventHandlesCorrectlyTest : TypeRuleTestFixture<DeclareEventHandlersCorrectlyRule> {
		[Test]
		public void SkipOnCanonicalScenariosTest ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
		}

		delegate void GoodDelegate (object sender, EventArgs e);
		class ClassWithGoodDelegate {
			public event GoodDelegate CustomEventA;
			public event GoodDelegate CustomEventB;
		}

		[Test]
		public void SuccessOnClassWithGoodDelegateTest ()
		{
			AssertRuleSuccess<ClassWithGoodDelegate> ();
		}

		delegate int DelegateReturningNonVoid (object sender, EventArgs e);
		class ClassWithDelegateReturningNonVoid {
			public event DelegateReturningNonVoid CustomEvent;
		}

		[Test]
		public void FailOnClassWithDelegateReturningNonVoidTest ()
		{
			AssertRuleFailure<ClassWithDelegateReturningNonVoid> (1);
		}

		delegate void DelegateWithOneParameter (int sender);
		class ClassWithDelegateWithOneParameter {
			public event DelegateWithOneParameter CustomEvent;
		}

		[Test]
		public void FailOnClassWithDelegateWithOneParameterTest ()
		{
			//The amount of parameters
			//And the type warning for the first
			AssertRuleFailure<ClassWithDelegateWithOneParameter> (2);
		}

		delegate void DelegateWithBadTypes (int sender, char e);
		class ClassWithDelegateWithBadTypes {
			public event DelegateWithBadTypes CustomEvent;
		}

		[Test]
		public void FailOnClassWithDelegateWithBadTypesTest ()
		{
			AssertRuleFailure<ClassWithDelegateWithBadTypes> (2);
		}

		delegate void DelegateWithObject (object sender, int e);
		class ClassWithDelegateWithObject {
			public event DelegateWithObject CustomEvent;
		}

		[Test]
		public void FailOnClassWithDelegateWithObjectTest ()
		{
			AssertRuleFailure<ClassWithDelegateWithObject> (1);
		}

		delegate void DelegateWithEventArgs (int sender, EventArgs e);
		class ClassWithDelegateWithEventArgs {
			public event DelegateWithEventArgs CustomEvent;
		}

		[Test]
		public void FailOnClassWithDelegateWithEventArgsTest ()
		{
			AssertRuleFailure<ClassWithDelegateWithEventArgs> (1);
		}

		delegate void DelegateWithoutSender (object obj, EventArgs e);
		class ClassWithDelegateWithoutSender {
			public event DelegateWithoutSender CustomEvent;
		}

		[Test]
		public void FailOnClassWithDelegateWithoutSenderTest ()
		{
			AssertRuleFailure<ClassWithDelegateWithoutSender> (1);
		}

		delegate void DelegateWithoutE (object sender, EventArgs eventArgs);
		class ClassWithDelegateWithoutE {
			public event DelegateWithoutE CustomEvent;
		}

		[Test]
		public void FailOnClassWithDelegateWithoutETest ()
		{
			AssertRuleFailure<ClassWithDelegateWithoutE> (1);
		}

		class ClassWithTwoFields {
			public event DelegateWithoutE CustomEvent;
			public event DelegateWithoutE CustomEvent1;
		}

		[Test]
		public void FailOnClassWithTwoFieldsTest ()
		{
			AssertRuleFailure<ClassWithTwoFields> (2);
		}

		delegate int SampleDelegate ();

		class ClassWithDelegate {
			SampleDelegate myDelegate;
		}

		[Test]
		public void SuccessOnClassWithDelegateTest ()
		{
			AssertRuleDoesNotApply<ClassWithDelegate> ();
		}
		
		class ClassWithGenericEventHandler {
			public event EventHandler<RunnerEventArgs> handler;
		}

		[Test]
		public void SuccessOnClassWithGenericEventHandlerTest ()
		{
			AssertRuleSuccess<ClassWithGenericEventHandler> ();
		}

		delegate void DelegateWithGenerics (object obj, List<int> list);
		class ClassWithGenericDelegate {
			public event DelegateWithGenerics CustomEvent;
		}

		public delegate void MyOwnEventHandler<T>(object sender, T e);
		class ClassWithNonEventHandlerEvent {
			public event MyOwnEventHandler<int> CustomEvent;
		}

		[Test]
		public void FailureOnClassWithBadDelegateUsingGenericsTest ()
		{
			AssertRuleFailure<ClassWithGenericDelegate> (3);
			AssertRuleFailure<ClassWithNonEventHandlerEvent> (1);
		}
	}
}
