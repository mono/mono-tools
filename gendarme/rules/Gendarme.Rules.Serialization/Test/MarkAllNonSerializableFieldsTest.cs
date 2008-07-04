//
// Unit tests for MarkAllNonSerializableFieldsRule
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
using System.Collections;
using System.Runtime.Serialization;
using Gendarme.Rules.Serialization;
using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Serialization {
	[TestFixture]
	public class MarkAllNonSerializableFieldsTest : TypeRuleTestFixture<MarkAllNonSerializableFieldsRule> {
		
		class NonSerializableClass {
		}

		[Serializable]
		class SerializableClass {
		}

		[Serializable]
		class SerializableWithoutMarks {
			NonSerializableClass nonSerializableClass;
			NonSerializableClass nonSerializableClass1;
		}

		[Serializable]
		class SerializableWithMarks {
			[NonSerialized]
			NonSerializableClass nonSerializableClass;
		}

		[Serializable]
		class SerializableWithSerializableFields {
			SerializableClass serializableClass;
		}

		class NonSerializableWithInnerClass {
			[Serializable]
			class Serializable {
			}
		}
		
		[Serializable]
		class CustomSerializationClass : ISerializable {
			NonSerializableClass nonSerializable;

			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		enum Values {
			Foo,
			Bar
		}

		[Serializable]
		class SerializableWithEnumClass {
			Values values;
		}

		[Serializable]
		class SerializableWithInterfaceClass {
			IList List = new ArrayList ();
		}

		[Serializable]
		class SerializableWithStaticFieldsClass {
			static NonSerializableClass nonSerializable;
		}

		[Test]
		public void SkipOnNonSerializableClassesTest ()
		{
			AssertRuleDoesNotApply<NonSerializableClass> ();
		}

		[Test]
		public void FailOnSerializableClassWithoutMarksTest ()
		{
			AssertRuleFailure<SerializableWithoutMarks> (2);
		}

		[Test]
		public void SuccessOnSerializableClassWithMarksTest ()
		{
			AssertRuleSuccess<SerializableWithMarks> ();
		}

		[Test]
		public void SuccessOnSerializableClassWithSerializableFieldsTest ()
		{
			AssertRuleSuccess<SerializableWithSerializableFields> ();
		}

		[Test]
		public void SkipOnNonSerializableWithInnerClassTest ()
		{
			AssertRuleDoesNotApply<NonSerializableWithInnerClass> ();
		}

		[Test]
		public void SkipOnCustomSerializationClassTest ()
		{
			//If you are doing a custom serialization there aren't
			//need to warn developers about the NonSerialized
			//fields, because the runtime usess the GetObjectData
			//method in order to retrieve the object state.
			AssertRuleDoesNotApply<CustomSerializationClass> ();
			//But, perhaps if you still marks the fields, you will
			//help others to understand better your code.
			//
			//Take a look at System.Collections.Generic.LinkedList
			//by example
		}

		[Test]
		public void SuccessOnSerializableWithEnumClassTest ()
		{
			AssertRuleSuccess<SerializableWithEnumClass> ();
		}

		[Test]
		public void FailOnSerializableWithInterfaceClassTest () 
		{
			//The SerializableAttribute can't be applied to an
			//interface, and we can only check the polimorphism in
			//run-time.
			//The chosen behaviour is warn.
			//And the solution could be:
			//	* Use the class instead of the interface.
			//	* Mark it, as NonSerializable
			//	* Use custom serialization
			AssertRuleFailure<SerializableWithInterfaceClass> ();
			//Perhaps a better analysis could try to look for the
			//concrete class, but it couldn't have a 100% of success
			//because the polimorphism is resolved at run-time.
		}

		[Test]
		public void SuccessOnSerializableWithStaticFieldsClassTest ()
		{
			AssertRuleSuccess<SerializableWithStaticFieldsClass> ();
		}
	}
}
