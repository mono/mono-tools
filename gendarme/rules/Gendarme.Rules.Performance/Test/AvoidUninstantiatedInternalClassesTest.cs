// 
// Unit tests for AvoidUninstantiatedInternalClassesRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Performance;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Performance {

#pragma warning disable 169, 414

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

	internal struct InternalUninstantiatedStruct {

		public void display ()
		{
		}

		public static void Main (string [] args)
		{
		}
	}

	internal struct InternalInstantiatedStruct {

		public void display ()
		{
		}

		public static void Main (string [] args)
		{
			InternalInstantiatedStruct i = new InternalInstantiatedStruct ();
			i.display ();
		}
	}

	// people may use the following two patterns to have
	// static classes in C# 1.
	internal sealed class InternalSealedClassWithPrivateCtor {
		private InternalSealedClassWithPrivateCtor ()
		{
		}

		public static void Foo ()
		{
		}
	}

	internal abstract class InternalAbstractClassWithPrivateCtor {
		private InternalAbstractClassWithPrivateCtor ()
		{
		}

		public static void Bar ()
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

	internal class Item<T> {

		T item;

		public Item ()
		{
		}

		public Item (T item)
		{
			this.item = item;
		}

		public T GetItem ()
		{
			return item;
		}
	}

	internal class Items<T,V> {

		T item1;
		V item2;

		public Items (T item1, V item2)
		{
			this.item1 = item1;
			this.item2 = item2;
		}
	}

	[TestFixture]
	public class AvoidUninstantiatedInternalClassesTest : TypeRuleTestFixture<AvoidUninstantiatedInternalClassesRule> {
		
		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Performance." + name;
			return assembly.MainModule.GetType (fullname);
		}
		
		[Test]
		public void UninstantiatedInternalClassTest ()
		{
			AssertRuleFailure<UninstantiatedInternalClass> (1);
		}
		
		[Test]
		public void InstantiatedInternalClassTest ()
		{
			AssertRuleSuccess<InstantiatedInternalClass> ();
		}
		
		[Test]
		public void NestedInternalUninstantiatedClassTest ()
		{
			AssertRuleFailure<NestedInternalUninstantiatedClass> (1);
		}
		
		[Test]
		public void NestedInternalInstantiatedClassTest ()
		{
			AssertRuleSuccess<NestedInternalInstantiatedClass> ();
		}
		
		[Test]
		public void NonInternalClassNotInstantiatedTest ()
		{
			AssertRuleDoesNotApply<NonInternalClassNotInstantiated> ();
		}

		[Test]
		public void InternalInstantiatedStructTest ()
		{
			AssertRuleSuccess<InternalInstantiatedStruct> ();
		}

		[Test]
		public void InternalUninstantiatedStructTest ()
		{
			AssertRuleFailure<InternalUninstantiatedStruct> ();
		}

		[Test]
		public void InternalSealedClassWithPrivateCtor ()
		{
			AssertRuleDoesNotApply<InternalSealedClassWithPrivateCtor> ();
		}

		[Test]
		public void InternalAbstractClassWithPrivateCtorTest ()
		{
			AssertRuleDoesNotApply<InternalAbstractClassWithPrivateCtor> ();
		}

		[Test]
		public void StaticClassTest ()
		{
			AssertRuleDoesNotApply (GetTest ("StaticClass"));
		}
		
		[Test]
		public void MethodContainingObjectCallIsNotCalledTest ()
		{
			AssertRuleSuccess<MethodContainingObjectCallIsNotCalled> ();
		}
		
		[Test]
		public void IFaceTest ()
		{
			AssertRuleDoesNotApply<IFace> ();
		}

		[Test]
		public void NestedEnumReturnInstantiated ()
		{
			AssertRuleSuccess (GetTest ("NestedEnumReturnInstantiated/PrivateEnum"));
		}

		[Test]
		public void NestedEnumUsingInstantiated ()
		{
			AssertRuleSuccess (GetTest ("NestedEnumUsingInstantiated/PrivateEnum"));
		}

		[Test]
		public void NestedEnumExternInstantiated ()
		{
			AssertRuleSuccess (GetTest ("NestedEnumExternInstantiated/PrivateEnum"));
		}

		[Test]
		public void NestedEnumExternOutInstantiated ()
		{
			AssertRuleSuccess (GetTest ("NestedEnumExternOutInstantiated/PrivateEnum"));
		}

		[Test]
		public void NestedEnumNotInstantiated ()
		{
			AssertRuleFailure (GetTest ("NestedEnumNotInstantiated/PrivateEnum"), 1);
		}
		
		[Test]
		public void NestedEnumUsedAsParameter ()
		{
			AssertRuleSuccess (GetTest ("NestedEnumUsedAsParameter/PrivateEnum"));
		}

		[Test]
		public void StructExistAsArrayOnly ()
		{
			AssertRuleSuccess (GetTest ("ClassWithArray/ExistAsArrayOnly"));
		}

		[Test]
		public void Generics_ItemT ()
		{
			Item<int> item = new Item<int> (1);
			// used just above
			AssertRuleSuccess <Item<int>> ();

			AssertRuleFailure<Items<int,int>> (1);
		}

		[Test]
		public void EntryPoint ()
		{
			MethodDefinition main = DefinitionLoader.GetMethodDefinition<InstantiatedInternalClass> ("Main");
			try {
				main.DeclaringType.Module.Assembly.EntryPoint = main;
				AssertRuleSuccess (main.DeclaringType.Resolve ());
			}
			finally {
				main.DeclaringType.Module.Assembly.EntryPoint = null;
			}
		}

		/// <summary>This namespace contains the command-line options processing library distributed by the mono team.</summary>
		internal sealed class NamespaceDoc {
		}

		[Test]
		public void MonoDoc ()
		{
			AssertRuleDoesNotApply<NamespaceDoc> ();
		}
	}
}
