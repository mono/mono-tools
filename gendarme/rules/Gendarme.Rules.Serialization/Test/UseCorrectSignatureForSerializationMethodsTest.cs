// 
// Unit tests for ImplementSerializationEventsCorrectlyRule
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
using System.Reflection;
using System.Runtime.Serialization;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Serialization;

using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Helpers;

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
	public class UseCorrectSignatureForSerializationMethodsTest {

		[OnSerializing]
		private void Serializing (StreamingContext context)
		{
			// method is ok but it's type is not [Serializable]
		}

		private IRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new UseCorrectSignatureForSerializationMethodsRule ();
			runner = new TestRunner (rule);
		}

		private MethodDefinition GetTest (string type, string method)
		{
			string fullname = "Test.Rules.Serialization." + type;
			return assembly.MainModule.Types [fullname].GetMethod (method);
		}

		[Test]
		public void Ok ()
		{
			MethodDefinition method = GetTest ("OkClass", "Lizing");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "Lizing-Failure");
			Assert.AreEqual (0, runner.Defects.Count, "Lizing-Count");

			method = GetTest ("OkClass", "Lized");
			Assert.AreEqual (RuleResult.Success, runner.CheckMethod (method), "Lized-Failure");
			Assert.AreEqual (0, runner.Defects.Count, "Lized-Count");
		}

		[Test]
		public void BadSignatures ()
		{
			MethodDefinition method = GetTest ("BadClass", "Serializing");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Serializing-Failure");
			Assert.AreEqual (1, runner.Defects.Count, "Serializing-Count");
#if false
			method = GetTest ("BadClass", "Serialized");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Serialized-Failure");
			Assert.AreEqual (1, runner.Defects.Count, "Serialized-Count");
	
			method = GetTest ("BadClass", "Deserializing");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Deserializing-Failure");
			Assert.AreEqual (1, runner.Defects.Count, "Deserializing-Count");

			method = GetTest ("BadClass", "OnDeserialized");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "OnDeserialized-Failure");
			Assert.AreEqual (1, runner.Defects.Count, "OnDeserialized-Count");
#endif
		}

		[Test]
		public void NotSerializable ()
		{
			MethodDefinition method = GetTest ("NotSerializableClass", "Lizing");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Lizing-Failure");
			Assert.AreEqual (1, runner.Defects.Count, "Lizing-Count");

			method = GetTest ("NotSerializableClass", "Lized");
			Assert.AreEqual (RuleResult.Failure, runner.CheckMethod (method), "Lized-Failure");
			Assert.AreEqual (1, runner.Defects.Count, "Lized-Count");
		}
	}
}
