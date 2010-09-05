using System;
using Gendarme.Rules.Performance;
using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Tests.Rules.Performance {
	[TestFixture]
	public class PreferInterfaceConstraintOnGenericForBaseTypeInterfaceTest :
		MethodRuleTestFixture<PreferInterfaceConstraintOnGenericForBaseTypeInterface> {

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

			private void MethodWithGenericInterfaceOfBaseTypeParameter2 (IComparable<int> comparable)
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
			AssertRuleFailure<TestCase> ("MethodWithInterfaceOfBaseTypeParameter");
		}

		[Test]
		public void MethodWithGenericInterfaceOfBaseTypeParameter ()
		{
			AssertRuleFailure<TestCase> ("MethodWithGenericInterfaceOfBaseTypeParameter");
		}

		[Test]
		public void MethodWithGenericInterfaceOfBaseTypeParameter2 ()
		{
			AssertRuleFailure<TestCase> ("MethodWithGenericInterfaceOfBaseTypeParameter2");
		}

		[Test]
		public void MethodWithMultipleParameters ()
		{
			AssertRuleFailure<TestCase> ("MethodWithMultipleParameters");
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