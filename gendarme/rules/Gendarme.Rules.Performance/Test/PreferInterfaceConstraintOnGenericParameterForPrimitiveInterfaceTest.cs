//
// Unit tests for PreferInterfaceConstraintOnGenericParameterForPrimitiveInterfaceRule
//
// Authors:
//	Julien Hoarau <madgnome@gmail.com>
//
// Copyright (C) 2010 Julien Hoarau
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
using Gendarme.Rules.Performance;
using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Tests.Rules.Performance {
	[TestFixture]
	public class PreferInterfaceConstraintOnGenericParameterForPrimitiveInterfaceTest :
		MethodRuleTestFixture<PreferInterfaceConstraintOnGenericParameterForPrimitiveInterfaceRule> {

		private class TestCase {
			private void MethodWithoutParameters ()
			{
				
			}

			private void MethodWithInterfaceOfBaseTypeParameter (IComparable comparable)
			{

			}

			private void MethodWithGenericInterfaceOfBaseTypeParameter<T> (IComparable<T> comparable)
			{

			}

			private void MethodWithGenericInterfaceOfBaseTypeParameter2a (IComparable<int> comparable)
			{

			}

			private void MethodWithGenericInterfaceOfBaseTypeParameter2b (IComparable<decimal> comparable)
			{

			}

			private void MethodWithGenericInterfaceOfBaseTypeParameter2c (IComparable<object> comparable)
			{

			}

			private void MethodWithInterfaceParameter (IDisposable disposable)
			{
				
			}

			private void MethodWithMultipleParameters (IDisposable disposable, IConvertible convertible)
			{
				
			}

			private void GenericMethodWithMultipleParameters<T> (IDisposable disposable, T convertible) 
				where T : IConvertible
			{
				
			}

			private void MethodWithInterfaceConstraintOnGeneric<T> (T comparable) where T : IComparable
			{
				
			}

			private void MethodWithInterfaceConstraintOnGeneric2<TComparable, TType> (TComparable comparable)
				where TComparable : IComparable<TType>
			{

			}

			private void MethodWithInterfaceConstraintOnGeneric3<TComparable> (TComparable comparable)
				where TComparable : IComparable<int>
			{

			}
		}


		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<TestCase> ("MethodWithoutParameters");
		}

		[Test]
		public void MethodWithInterfaceOfBaseTypeParameter ()
		{
			AssertRuleFailure<TestCase> ("MethodWithInterfaceOfBaseTypeParameter", 1);
		}

		[Test]
		public void MethodWithGenericInterfaceOfBaseTypeParameter ()
		{
			AssertRuleFailure<TestCase> ("MethodWithGenericInterfaceOfBaseTypeParameter", 1);
		}

		[Test]
		public void MethodWithGenericInterfaceOfBaseTypeParameter2 ()
		{
			AssertRuleFailure<TestCase> ("MethodWithGenericInterfaceOfBaseTypeParameter2a", 1);
			AssertRuleFailure<TestCase> ("MethodWithGenericInterfaceOfBaseTypeParameter2b", 1);
			AssertRuleSuccess<TestCase> ("MethodWithGenericInterfaceOfBaseTypeParameter2c");
		}

		[Test]
		public void MethodWithMultipleParameters ()
		{
			AssertRuleFailure<TestCase> ("MethodWithMultipleParameters", 1);
		}

		[Test]
		public void MethodWithInterfaceParameter ()
		{
			AssertRuleSuccess<TestCase> ("MethodWithInterfaceParameter");
		}

		[Test]
		public void MethodWithInterfaceConstraintOnGeneric ()
		{
			AssertRuleSuccess<TestCase> ("MethodWithInterfaceConstraintOnGeneric");
		}

		[Test]
		public void MethodWithInterfaceConstraintOnGeneric2 ()
		{
			AssertRuleSuccess<TestCase> ("MethodWithInterfaceConstraintOnGeneric2");
		}

		[Test]
		public void MethodWithInterfaceConstraintOnGeneric3 ()
		{
			AssertRuleSuccess<TestCase> ("MethodWithInterfaceConstraintOnGeneric3");
		}

		[Test]
		public void GenericMethodWithMultipleParameters ()
		{
			AssertRuleSuccess<TestCase> ("GenericMethodWithMultipleParameters");
		}
	}
}
