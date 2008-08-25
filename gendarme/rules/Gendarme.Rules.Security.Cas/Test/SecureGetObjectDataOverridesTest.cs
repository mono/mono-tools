//
// Unit tests for SecureGetObjectDataOverridesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005,2008 Novell, Inc (http://www.novell.com)
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
using System.Security.Permissions;

using Gendarme.Rules.Security.Cas;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Security.Cas {

	[TestFixture]
	public class SecureGetObjectDataOverridesTest : TypeRuleTestFixture<SecureGetObjectDataOverridesRule> {

		[Serializable]
		public class SerializableClass {
		}

		public class ISerializableClass : ISerializable {

			public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class InheritISerializableClass : ISerializableClass {

			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class InheritISerializableWithoutOverrideClass : ISerializableClass {

			public InheritISerializableWithoutOverrideClass ()
			{
			}
		}

		public class LinkDemandClass: ISerializable {

			[SecurityPermission (SecurityAction.LinkDemand, SerializationFormatter = true)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class AlternateSyntaxLinkDemandClass : ISerializable {

			[SecurityPermission (SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class InheritanceDemandClass: ISerializable {

			[SecurityPermission (SecurityAction.InheritanceDemand, SerializationFormatter = true)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class AlternateSyntaxInheritanceDemandClass : ISerializable {

			[SecurityPermission (SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class DemandClass: ISerializable {

			[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class AlternateSyntaxDemandClass : ISerializable {

			[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class DemandWrongPermissionClass: ISerializable {

			[SecurityPermission (SecurityAction.Demand, ControlAppDomain = true)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		[Test]
		public void SerializableWithoutGetObjectData ()
		{
			// there's no GetObjectData method here so the test does not apply
			AssertRuleDoesNotApply<SerializableClass> ();
			AssertRuleDoesNotApply<InheritISerializableWithoutOverrideClass> ();
		}

		[Test]
		public void GetObjectDataWithoutDemand ()
		{
			AssertRuleFailure<ISerializableClass> ();
			AssertRuleFailure<InheritISerializableClass> ();
		}

		[Test]
		public void GetObjectDataWithSerializationFormatterDemand ()
		{
			AssertRuleSuccess<DemandClass> ();
			AssertRuleSuccess<LinkDemandClass> ();
			// not enough
			AssertRuleFailure<InheritanceDemandClass> ();
		}

		[Test]
		public void GetObjectDataWithSerializationFormatterDemand_UsingFlags ()
		{
			AssertRuleSuccess<AlternateSyntaxDemandClass> ();
			AssertRuleSuccess<AlternateSyntaxLinkDemandClass> ();
			// not enough
			AssertRuleFailure<AlternateSyntaxInheritanceDemandClass> ();
		}

		[Test]
		public void GetObjectDataWithWrongDemand ()
		{
			AssertRuleFailure<DemandWrongPermissionClass> ();
		}
	}
}
