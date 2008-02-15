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
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using SSP = System.Security.Permissions;

using Gendarme.Framework;
using Gendarme.Rules.Security;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Security {

	[TestFixture]
	public class SecureGetObjectDataOverridesTest {

		[Serializable]
		public class SerializableClass {

			public SerializableClass ()
			{
			}
		}

		public class ISerializableClass : ISerializable {

			public ISerializableClass ()
			{
			}

			public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class InheritISerializableClass : ISerializableClass {

			public InheritISerializableClass ()
			{
			}

			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class LinkDemandClass: ISerializable {

			public LinkDemandClass ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.LinkDemand, SerializationFormatter = true)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class InheritanceDemandClass: ISerializable {

			public InheritanceDemandClass ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.InheritanceDemand, SerializationFormatter = true)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class DemandClass: ISerializable {

			public DemandClass ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.Demand, SerializationFormatter = true)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class DemandWrongPermissionClass: ISerializable {

			public DemandWrongPermissionClass ()
			{
			}

			[SecurityPermission (SSP.SecurityAction.Demand, ControlAppDomain = true)]
			public void GetObjectData (SerializationInfo info, StreamingContext context)
			{
			}
		}

		private ITypeRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new SecureGetObjectDataOverridesRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Security.SecureGetObjectDataOverridesTest/" + name;
			return assembly.MainModule.Types[fullname];
		}

		[Test]
		public void Serializable ()
		{
			TypeDefinition type = GetTest ("SerializableClass");
			// there's no GetObjectData method here so the test should never fail
			Assert.AreEqual (RuleResult.DoesNotApply, rule.CheckType (type));
		}

		[Test]
		public void ISerializable ()
		{
			TypeDefinition type = GetTest ("ISerializableClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}

		[Test]
		public void InheritISerializable ()
		{
			TypeDefinition type = GetTest ("InheritISerializableClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}

		[Test]
		public void LinkDemand ()
		{
			TypeDefinition type = GetTest ("LinkDemandClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void InheritanceDemand ()
		{
			TypeDefinition type = GetTest ("InheritanceDemandClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}

		[Test]
		public void Demand ()
		{
			TypeDefinition type = GetTest ("DemandClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void DemandWrongPermission ()
		{
			TypeDefinition type = GetTest ("DemandWrongPermissionClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}
	}
}
