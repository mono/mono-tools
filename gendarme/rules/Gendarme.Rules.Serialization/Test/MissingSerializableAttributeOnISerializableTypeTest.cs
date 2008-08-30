// 
// Unit tests for MissingSerializableAttributeOnISerializableTypeRule
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
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Serialization {

	[Serializable]
	public class ClassWithAttributeOnly {
	}

	public class ClassWithoutAttribute : ISerializable {

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
		}
	}

	[Serializable]
	public class ClassWithAttribute : ISerializable {

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
		}
	}

	// this class is not [Serializable]
	public class InheritFromISerializableClass : ClassWithAttribute {
	}

	[Serializable]
	public class SerializableInheritFromISerializableClass : ClassWithAttribute {
	}

	[TestFixture]
	public class MissingSerializableAttributeOnISerializableTypeTest : TypeRuleTestFixture<MissingSerializableAttributeOnISerializableTypeRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Interface);

			AssertRuleDoesNotApply<MissingSerializableAttributeOnISerializableTypeTest> ();
			AssertRuleDoesNotApply<ClassWithAttributeOnly> ();
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<ClassWithAttribute> ();
			AssertRuleSuccess<SerializableInheritFromISerializableClass> ();
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure<ClassWithoutAttribute> (1);
			AssertRuleFailure<InheritFromISerializableClass> (1);
		}
	}
}
