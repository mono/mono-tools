// 
// Unit tests for MissingSerializationConstructorRule
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
using Gendarme.Rules.Serialization;

using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Serialization {

	[Serializable]
	public class ClassWithoutConstructor : ISerializable {

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
		}
	}

	[Serializable]
	public class UnsealedClassWrongCtorVisibility : ISerializable {

		public UnsealedClassWrongCtorVisibility (SerializationInfo info, StreamingContext context)
		{
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
		}
	}

	[Serializable]
	public sealed class SealedClassWrongCtorVisibility : ISerializable {

		protected SealedClassWrongCtorVisibility (SerializationInfo info, StreamingContext context)
		{
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
		}
	}

	[Serializable]
	public class PerfectUnsealedClass : ISerializable {

		protected PerfectUnsealedClass (SerializationInfo info, StreamingContext context)
		{
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
		}
	}

	[Serializable]
	public sealed class PerfectSealedClass : ISerializable {

		private PerfectSealedClass (SerializationInfo info, StreamingContext context)
		{
		}

		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
		}
	}

	[TestFixture]
	public class MissingSerializationConstructorTest {

		private ITypeRule rule;
		private TestRunner runner;
		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new MissingSerializationConstructorRule ();
			runner = new TestRunner (rule);
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Serialization." + name;
			return assembly.MainModule.Types [fullname];
		}

		[Test]
		public void DoesNotApply ()
		{
			TypeDefinition type = GetTest ("MissingSerializationConstructorTest");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type), "! ISerializable");
		}

		[Test]
		public void Success ()
		{
			TypeDefinition type = GetTest ("PerfectUnsealedClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "PerfectUnsealedClass");

			type = GetTest ("PerfectSealedClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "PerfectSealedClass");
		}

		[Test]
		public void Failure ()
		{
			TypeDefinition type = GetTest ("ClassWithoutConstructor");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "ClassWithoutConstructor");

			type = GetTest ("UnsealedClassWrongCtorVisibility");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "UnsealedClassWrongCtorVisibility");

			type = GetTest ("SealedClassWrongCtorVisibility");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "SealedClassWrongCtorVisibility");
		}
	}
}
