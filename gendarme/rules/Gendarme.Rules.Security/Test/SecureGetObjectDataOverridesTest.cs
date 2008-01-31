//
// Unit tests for SecureGetObjectDataOverridesRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
			}
		}

		public class InheritISerializableClass : NameValueCollection {

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

		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private ModuleDefinition module;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			module = assembly.MainModule;
			rule = new SecureGetObjectDataOverridesRule ();
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Security.SecureGetObjectDataOverridesTest/" + name;
			return assembly.MainModule.Types[fullname];
		}

		private MethodDefinition GetObjectData (TypeDefinition type)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == "GetObjectData")
					return method;
			}
			return null;
		}

		[Test]
		public void Serializable ()
		{
			TypeDefinition type = GetTest ("SerializableClass");
			// there's no GetObjectData method here so the test should never fail
			foreach (MethodDefinition method in type.Methods) {
				Assert.IsNull (rule.CheckMethod (method, new MinimalRunner ()));
			}
		}

		[Test]
		public void ISerializable ()
		{
			TypeDefinition type = GetTest ("ISerializableClass");
			MethodDefinition method = GetObjectData (type);
			Assert.IsNotNull (rule.CheckMethod (method, new MinimalRunner ()));
		}

		[Test]
		public void InheritISerializable ()
		{
			TypeDefinition type = GetTest ("InheritISerializableClass");
			MethodDefinition method = GetObjectData (type);
			Assert.IsNotNull (rule.CheckMethod (method, new MinimalRunner ()));
		}

		[Test]
		public void LinkDemand ()
		{
			TypeDefinition type = GetTest ("LinkDemandClass");
			MethodDefinition method = GetObjectData (type);
			Assert.IsNull (rule.CheckMethod (method, new MinimalRunner ()));
		}

		[Test]
		public void InheritanceDemand ()
		{
			TypeDefinition type = GetTest ("InheritanceDemandClass");
			MethodDefinition method = GetObjectData (type);
			Assert.IsNotNull (rule.CheckMethod (method, new MinimalRunner ()));
		}

		[Test]
		public void Demand ()
		{
			TypeDefinition type = GetTest ("DemandClass");
			MethodDefinition method = GetObjectData (type);
			Assert.IsNull (rule.CheckMethod (method, new MinimalRunner ()));
		}

		[Test]
		public void DemandWrongPermission ()
		{
			TypeDefinition type = GetTest ("DemandWrongPermissionClass");
			MethodDefinition method = GetObjectData (type);
			Assert.IsNotNull (rule.CheckMethod (method, new MinimalRunner ()));
		}
	}
}
