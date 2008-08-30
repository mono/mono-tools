// 
// Unit tests for UseCorrectSignatureForSerializationMethodsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Runtime.Serialization;

using Gendarme.Rules.Serialization;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Serialization {

	[Serializable]
	public class BadClass {

		[OnSerializing]
		public void Serializing (StreamingContext context)
		{
			// bad visibility, should be private
		}

/* this compiles but nunit cause an exception when calling System.Reflection.Assembly.GetExportedTypes() */
#if false
		[OnSerialized]
		private void Serialized (SerializationInfo info)
		{
			// bad parameter type
		}

		[OnDeserializing]
		private void Deserializing (SerializationInfo info, StreamingContext context)
		{
			// bad parameter types (count)
		}

		[OnDeserialized]
		private bool Deserializing (StreamingContext context)
		{
			// bad return value
			return false;
		}
#endif
	}

	[Serializable]
	public class OkClass {

		[OnSerializing, OnDeserializing]
		private void Lizing (StreamingContext context)
		{
		}

		[OnSerialized, OnDeserialized]
		private void Lized (StreamingContext context)
		{
		}
	}

	public class NotSerializableClass {

		[OnSerializing, OnDeserializing]
		private void Lizing (StreamingContext context)
		{
		}

		[OnSerialized, OnDeserialized]
		private void Lized (StreamingContext context)
		{
		}
	}

	[TestFixture]
	public class UseCorrectSignatureForSerializationMethodsTest : MethodRuleTestFixture<UseCorrectSignatureForSerializationMethodsRule> {

		[OnSerializing]
		private void Serializing (StreamingContext context)
		{
			// method is ok but it's type is not [Serializable]
		}

		[Test]
		public void DoesNotApply ()
		{
			// constructor (default)
			AssertRuleDoesNotApply<UseCorrectSignatureForSerializationMethodsTest> (".ctor");
			// method without a serialization attribute
			AssertRuleDoesNotApply<UseCorrectSignatureForSerializationMethodsTest> ("DoesNotApply");
		}

		[Test]
		public void Ok ()
		{
			AssertRuleSuccess<OkClass> ("Lizing");
			AssertRuleSuccess<OkClass> ("Lized");
		}

		[Test]
		public void BadSignatures ()
		{
			AssertRuleFailure<BadClass> ("Serializing", 1);
#if false
			AssertRuleFailure<BadClass> ("Serialized", 1);
			AssertRuleFailure<BadClass> ("Deserializing", 1);
			AssertRuleFailure<BadClass> ("OnDeserialized", 1);
#endif
		}

		[Test]
		public void NotSerializable ()
		{
			AssertRuleFailure<NotSerializableClass> ("Lizing", 1);
			AssertRuleFailure<NotSerializableClass> ("Lized", 1);
		}
	}
}
