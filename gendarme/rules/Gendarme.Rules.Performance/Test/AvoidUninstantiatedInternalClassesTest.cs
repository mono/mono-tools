// 
// Unit tests for AvoidUninstantiatedInternalClassesRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Gendarme.Framework;
using Gendarme.Rules.Performance;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Performance {

	internal class UninstantiatedInternalClass {

		public void display ()
		{
		}

		public static void Main (string [] args)
		{
		}
	}

	internal class InstantiatedInternalClass {

		public void display ()
		{
		}

		public static void Main (string [] args)
		{
			InstantiatedInternalClass i = new InstantiatedInternalClass ();
			i.display ();
		}
	}

	internal class NestedInternalUninstantiatedClass {
		public class NestedClass {
			public void display ()
			{
			}
		}
	}

	internal class NestedInternalInstantiatedClass {
		public class NestedClass {
			public static void Main (string [] args)
			{
				NestedInternalInstantiatedClass n = new NestedInternalInstantiatedClass ();
			}
		}
	}

	public class NonInternalClassNotInstantiated {
		public void display ()
		{
		}
	}

	public static class StaticClass {
		public static void display ()
		{
		}
	}

	// note: we now consider this valid for this rule (it should be flagged that display isn't used by anyone elsewhere)
	internal class MethodContainingObjectCallIsNotCalled {
		public void display ()
		{
			MethodContainingObjectCallIsNotCalled n = new MethodContainingObjectCallIsNotCalled ();
		}
	}

	internal interface IFace {
		void display ();
	}

	public class NestedEnumReturnInstantiated {
		private enum PrivateEnum {
			Good,
			Bad,
			Unsure
		}

		private static PrivateEnum GetMe ()
		{
			return PrivateEnum.Good;
		}
	}

	public class NestedEnumUsingInstantiated {
		private enum PrivateEnum {
			Good,
			Bad,
			Unsure
		}

		private static bool GetMe ()
		{
			PrivateEnum pe = PrivateEnum.Bad;
			return (pe == PrivateEnum.Good);
		}
	}

	public class NestedEnumExternInstantiated {
		private enum PrivateEnum {
			Good,
			Bad,
			Unsure
		}

		[DllImport ("outofhere.dll")]
		private static extern PrivateEnum GetMe ();
	}

	public class NestedEnumExternOutInstantiated {
		private enum PrivateEnum {
			Good,
			Bad,
			Unsure
		}

		[DllImport ("outofhere.dll")]
		private static extern void GetMe (out PrivateEnum me);
	}

	public class NestedEnumNotInstantiated {
		private enum PrivateEnum {
			Good,
			Bad,
			Unsure
		}
	}

	public class NestedEnumUsedAsParameter {
		private enum PrivateEnum {
			Good,
			Bad,
			Unsure
		}
		private bool Get (PrivateEnum p)
		{
			return false;
		}
	}

	public class NestedConstValue {
		public class Strings {
			public const string Hello = "Allo";
		}
	}

	public class ClassWithDelegate {
		delegate void MyOwnDelegate (int x);
	}

	public abstract class ClassWithArray {
		private class ExistAsArrayOnly {
			int value;
		}

		private ExistAsArrayOnly [] Get ()
		{
			return null;
		}
	}


	[TestFixture]
	public class AvoidUninstantiatedInternalClassesTest {
		
		private ITypeRule typeRule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		MessageCollection messageCollection;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			typeRule = new AvoidUninstantiatedInternalClassesRule ();
			messageCollection = null;
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Performance." + name;
			return assembly.MainModule.Types[fullname];
		}
		
		[Test]
		public void uninstantiatedInternalClassTest ()
		{
			type = GetTest ("UninstantiatedInternalClass");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}
		
		[Test]
		public void instantiatedInternalClassTest ()
		{
			type = GetTest ("InstantiatedInternalClass");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void nestedInternalUninstantiatedClassTest ()
		{
			type = GetTest ("NestedInternalUninstantiatedClass");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}
		
		[Test]
		public void nestedInternalInstantiatedClassTest ()
		{
			type = GetTest ("NestedInternalInstantiatedClass");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void nonInternalClassNotInstantiatedTest ()
		{
			type = GetTest ("NonInternalClassNotInstantiated");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void staticClassTest ()
		{
			type = GetTest ("StaticClass");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void methodContainingObjectCallIsNotCalledTest ()
		{
			type = GetTest ("MethodContainingObjectCallIsNotCalled");
			Assert.IsNull (typeRule.CheckType (type, new MinimalRunner ()));
		}
		
		[Test]
		public void iFaceTest ()
		{
			type = GetTest ("IFace");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void NestedEnumReturnInstantiated ()
		{
			type = GetTest ("NestedEnumReturnInstantiated/PrivateEnum");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void NestedEnumUsingInstantiated ()
		{
			type = GetTest ("NestedEnumUsingInstantiated/PrivateEnum");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void NestedEnumExternInstantiated ()
		{
			type = GetTest ("NestedEnumExternInstantiated/PrivateEnum");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void NestedEnumExternOutInstantiated ()
		{
			type = GetTest ("NestedEnumExternOutInstantiated/PrivateEnum");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void NestedEnumNotInstantiated ()
		{
			type = GetTest ("NestedEnumNotInstantiated/PrivateEnum");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
		}
		
		[Test]
		public void NestedEnumUsedAsParameter ()
		{
			type = GetTest ("NestedEnumUsedAsParameter/PrivateEnum");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void StructExistAsArrayOnly ()
		{
			type = GetTest ("ClassWithArray/ExistAsArrayOnly");
			messageCollection = typeRule.CheckType (type, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}
	}
}
