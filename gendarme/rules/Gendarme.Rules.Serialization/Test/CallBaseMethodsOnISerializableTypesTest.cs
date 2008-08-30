//
// Unit tests for CallBaseMethodsOnISerializableTypesRule
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
using System.Runtime.Serialization;
using Gendarme.Rules.Serialization;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Serialization {
	[TestFixture]
	public class CallBaseMethodsOnISerializableTypesTest : TypeRuleTestFixture<CallBaseMethodsOnISerializableTypesRule> {
	
		[Serializable]
		class Base : ISerializable {
			int myValue;

			public Base ()
			{
			}

			protected Base (SerializationInfo info, StreamingContext context)
			{
				myValue = info.GetInt32 ("myValue");
			}

			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("myValue", myValue);
			}
		}

		[Serializable]
		class BadDerived : Base {
			int otherValue;
	
			protected BadDerived (SerializationInfo info, StreamingContext context)
			{
				otherValue = info.GetInt32 ("otherValue");
			}
	
			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("otherValue", otherValue);
			}
		}

		[Serializable]
		class GoodDerived : Base {
			int otherValue;
	
			protected GoodDerived (SerializationInfo info, StreamingContext context) : base (info, context)
			{
				otherValue = info.GetInt32 ("otherValue");
			}

			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("otherValue", otherValue);
				base.GetObjectData (info, context);
			}
		}

		[Serializable]
		class GoodDerivedOnlyInConstructor : Base {
			int otherValue;

			protected GoodDerivedOnlyInConstructor (SerializationInfo info, StreamingContext context) : base (info, context)
			{
				otherValue = info.GetInt32 ("otherValue");
			}

			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("otherValue", otherValue);
			}
		}

		[Serializable]
		class GoodDerivedOnlyInGetObjectData : Base {
			int otherValue;

			protected GoodDerivedOnlyInGetObjectData (SerializationInfo info, StreamingContext context) 
			{
				otherValue = info.GetInt32 ("otherValue");
			}

			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("otherValue", otherValue);
				base.GetObjectData (info, context);
			}
		}


		[Serializable]
		class DefaultSerialization {
		}


		[Serializable]
		class Derived : Base {
			protected Derived (SerializationInfo info, StreamingContext context) : base (info, context) 
			{
			}
		}

		//See the ArgumentException - SystemException - Exception
		[Serializable]
		class ThirdGeneration : Derived {
			int otherValue;

			protected ThirdGeneration (SerializationInfo info, StreamingContext context) : base (info, context)
			{
				otherValue = info.GetInt32 ("otherValue");
			}

			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				base.GetObjectData (info, context);
				info.AddValue ("otherValue", otherValue);
			}
		}

		[Test]
		public void SkipOnCanonicalScenariosTest ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
		}

		[Test]
		public void SkipOnDefaultSerializationTest ()
		{
			AssertRuleDoesNotApply<DefaultSerialization> ();
		}

		[Test]
		public void SkipOnBaseTest ()
		{
			AssertRuleDoesNotApply<Base> ();
		}

		[Test]
		public void SuccessOnGoodDerivedTest ()
		{
			AssertRuleSuccess<GoodDerived> ();
		}

		[Test]
		public void FailOnBadDerivedTest ()
		{
			AssertRuleFailure<BadDerived> (2);
		}

		[Test]
		public void SuccessOnMultipleLevelDerivedTest ()
		{
			AssertRuleSuccess<ThirdGeneration> ();
		}

		[Test]
		public void FailOnGoodDerivedOnlyInConstructorTest ()
		{
			AssertRuleFailure<GoodDerivedOnlyInConstructor> (1);
		}

		[Test]
		public void FailOnGoodDerivedOnlyInGetObjectDataTest ()
		{
			AssertRuleFailure<GoodDerivedOnlyInGetObjectData> (1);
		}

		class NoSerializationConstructor : Base {
			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		[Test]
		public void NoSerializationConstructorTest ()
		{
			AssertRuleFailure<NoSerializationConstructor> (1);
		}

		class MiddleMan : Base {
		}

		class Top : MiddleMan {
			protected Top (SerializationInfo info, StreamingContext context)
			{
			}

			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		[Test]
		public void InheritanceTest ()
		{
			AssertRuleFailure<Top> (2);
		}
	}
}
