//
// Unit tests for ImplementISerializableCorrectlyRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// 	(C) 2008 Néstor Salceda
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
using System.Runtime.Serialization;
using Gendarme.Rules.Serialization;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Serialization {
	[TestFixture]
	public class ImplementISerializableCorrectlyTest : TypeRuleTestFixture<ImplementISerializableCorrectlyRule> {

		[Serializable]
		class AutomaticSerialization {
		}

		[Serializable]
		class ImplementationWithNonSerialized : ISerializable {
			int foo;
			[NonSerialized]
			string bar;

			protected ImplementationWithNonSerialized (SerializationInfo info, StreamingContext context)
			{
				foo = info.GetInt32 ("foo");
			}

			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", foo);
			}
		}

		[Serializable]
		class ImplementationWithoutNonSerialized : ISerializable {
			int foo;
			string bar;

			protected ImplementationWithoutNonSerialized (SerializationInfo info, StreamingContext context)
			{
				foo = info.GetInt32 ("foo");
			}

			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", foo);
			}
		}


		[Serializable]
		class TrickyImplementationWithoutNonSerialized : ISerializable {
			int foo;
			string bar;

			protected TrickyImplementationWithoutNonSerialized (SerializationInfo info, StreamingContext context)
			{
				foo = info.GetInt32 ("foo");
			}

			private void AddValue (string name, object value)
			{
			}

			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", foo);
				AddValue ("bar", bar);
			}
		}


		[Serializable]
		class ImplementationWithout2NonSerializedFields : ISerializable {
			int foo;
			string bar;
			object myObject;

			protected ImplementationWithout2NonSerializedFields (SerializationInfo info, StreamingContext context)
			{
				foo = info.GetInt32 ("foo");
			}

			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", foo);
			}
		}

		[Test]
		public void SkipOnCanonicalScenariosTest ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
		}

		[Test]
		public void SkipOnAutomaticSerializationTest ()
		{
			AssertRuleDoesNotApply<AutomaticSerialization> ();
		}

		[Test]
		public void SuccessOnImplementationWithNonSerializedTest ()
		{
			AssertRuleSuccess<ImplementationWithNonSerialized> ();
		}

		[Test]
		public void FailOnImplementationWithoutNonSerializedTest ()
		{
			AssertRuleFailure<ImplementationWithoutNonSerialized> (1);
		}

		[Test]
		public void FailOnTrickyImplementationWithoutNonSerializedTest () 
		{
			AssertRuleFailure<TrickyImplementationWithoutNonSerialized> ();
		}

		[Test]
		public void FailOnImplementationWithout2NonSerializedFieldsTest ()
		{
			AssertRuleFailure<ImplementationWithout2NonSerializedFields> (2);
		}

		[Serializable]
		class SerializableWithoutVirtualMethod : ISerializable {
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		[Serializable]
		sealed class SealedSerializableWithoutVirtualMethod : ISerializable {
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		[Serializable]
		class SerializableWithVirtualMethod : ISerializable {
			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		[Serializable]
		class SerializableWithOverridenMethod : SerializableWithVirtualMethod {
			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		[Test]
		public void FailOnSerializableWithoutVirtualMethodTest ()
		{
			AssertRuleFailure<SerializableWithoutVirtualMethod> (1);
		}

		[Test]
		public void SuccessOnSealedSerializableWithoutVirtualMethodTest ()
		{
			AssertRuleSuccess<SealedSerializableWithoutVirtualMethod> ();
		}

		[Test]
		public void SuccessOnSerializableWithVirtualMethodTest ()
		{
			AssertRuleSuccess<SerializableWithVirtualMethod> ();
		}

		[Test]
		public void SuccessOnSerializableWithOverridenMethodTest ()
		{
			AssertRuleSuccess<SerializableWithOverridenMethod> ();
		}	

		[Serializable]
		class SerializableWithConstsAndStatic : ISerializable {
			const int Result = 42;
			static int Foo = 50;

			protected SerializableWithConstsAndStatic (SerializationInfo info, StreamingContext context)
			{
			}

			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		[Test]
		public void SuccessOnSerializableWithConstsAndStaticTest ()
		{
			AssertRuleSuccess<SerializableWithConstsAndStatic> ();
		}

		[Serializable]
		class SerializableThroughProperties : ISerializable {
			int foo;

			public SerializableThroughProperties ()
			{
			}

			protected SerializableThroughProperties (SerializationInfo info, StreamingContext context)
			{
				if (info.GetBoolean ("special"))
					foo = info.GetInt32 ("foo");
				else
					foo = 0;
			}
			
			int Foo {
				get {
					return foo;
				}
			}

			bool Special {
				get { return true; }
			}
			
			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", Foo);
				info.AddValue ("special", Special);
			}
		}

		[Test]
		public void SuccessOnSerializableThroughPropertiesTest ()
		{
			AssertRuleSuccess<SerializableThroughProperties> ();
		}

		[Serializable]
		class SerializableThroughPropertiesAndOneNonSerialized : ISerializable {
			int foo;
			int bar;

			protected SerializableThroughPropertiesAndOneNonSerialized (SerializationInfo info, StreamingContext context)
			{
				foo = info.GetInt32 ("foo");
			}
			
			int Foo {
				get {
					return foo;
				}
			}
			
			public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", Foo);
			}
		}

		[Test]
		public void FailOnSerializableThroughPropertiesAndOneNonSerializedTest ()
		{
			AssertRuleFailure<SerializableThroughPropertiesAndOneNonSerialized> (1);
		}

		[Serializable]
		sealed class OperatingSystem : ISerializable {
			private System.PlatformID _platform;
			private Version _version;
			private string _servicePack = String.Empty;
			
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("_platform", _platform);
				info.AddValue ("_version", _version);
				info.AddValue ("_servicePack", _servicePack);
			}
		}
		
		[Test]
		public void SuccessOnOperatingSystemTest ()
		{
			AssertRuleSuccess<OperatingSystem> ();
		}

		[Serializable]
		sealed class AddValueWithMoreParameters : ISerializable {
			int foo;

			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("foo", foo, typeof (int));
			}
		}

		[Test]
		public void SuccessOnAddValueWithMoreParametersTest ()
		{
			AssertRuleSuccess<AddValueWithMoreParameters> ();
		}

		[Serializable]
		class InheritWithNewInstanceFields : SerializableThroughProperties {
			object field_to_be_serialized;
			int more_field_to_be_serialized;
		}

		[Serializable]
		class InheritWithNewStaticFields : SerializableThroughProperties {
			static InheritWithNewStaticFields Empty;
		}

		[Serializable]
		class InheritWithoutNewFields : SerializableThroughProperties {
		}

		[Test]
		public void InheritFromISerializableType ()
		{
			AssertRuleFailure<InheritWithNewInstanceFields> (2);
			AssertRuleSuccess<InheritWithNewStaticFields> ();
			AssertRuleSuccess<InheritWithoutNewFields> ();
		}
	}
}
