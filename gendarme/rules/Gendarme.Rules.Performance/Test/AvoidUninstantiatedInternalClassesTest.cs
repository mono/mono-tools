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
using Test.Rules.Helpers;

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
	public class AvoidUninstantiatedInternalClassesTest {
		
		private ITypeRule typeRule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			typeRule = new AvoidUninstantiatedInternalClassesRule ();
			runner = new TestRunner (typeRule);
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Performance." + name;
			return assembly.MainModule.Types[fullname];
		}
		
		[Test]
		public void UninstantiatedInternalClassTest ()
		{
			type = GetTest ("UninstantiatedInternalClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
			Assert.AreEqual (1, runner.Defects.Count);
		}
		
		[Test]
		public void InstantiatedInternalClassTest ()
		{
			type = GetTest ("InstantiatedInternalClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}
		
		[Test]
		public void NestedInternalUninstantiatedClassTest ()
		{
			type = GetTest ("NestedInternalUninstantiatedClass");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
			Assert.AreEqual (1, runner.Defects.Count);
		}
		
		[Test]
		public void NestedInternalInstantiatedClassTest ()
		{
			type = GetTest ("NestedInternalInstantiatedClass");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}
		
		[Test]
		public void NonInternalClassNotInstantiatedTest ()
		{
			type = GetTest ("NonInternalClassNotInstantiated");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void InternalSealedClassWithPrivateCtor ()
		{
			type = GetTest ("InternalSealedClassWithPrivateCtor");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void InternalAbstractClassWithPrivateCtorTest ()
		{
			type = GetTest ("InternalAbstractClassWithPrivateCtor");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void StaticClassTest ()
		{
			type = GetTest ("StaticClass");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}
		
		[Test]
		public void MethodContainingObjectCallIsNotCalledTest ()
		{
			type = GetTest ("MethodContainingObjectCallIsNotCalled");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}
		
		[Test]
		public void IFaceTest ()
		{
			type = GetTest ("IFace");
			Assert.AreEqual (RuleResult.DoesNotApply, runner.CheckType (type));
		}

		[Test]
		public void NestedEnumReturnInstantiated ()
		{
			type = GetTest ("NestedEnumReturnInstantiated/PrivateEnum");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void NestedEnumUsingInstantiated ()
		{
			type = GetTest ("NestedEnumUsingInstantiated/PrivateEnum");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void NestedEnumExternInstantiated ()
		{
			type = GetTest ("NestedEnumExternInstantiated/PrivateEnum");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void NestedEnumExternOutInstantiated ()
		{
			type = GetTest ("NestedEnumExternOutInstantiated/PrivateEnum");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void NestedEnumNotInstantiated ()
		{
			type = GetTest ("NestedEnumNotInstantiated/PrivateEnum");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type));
		}
		
		[Test]
		public void NestedEnumUsedAsParameter ()
		{
			type = GetTest ("NestedEnumUsedAsParameter/PrivateEnum");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void StructExistAsArrayOnly ()
		{
			type = GetTest ("ClassWithArray/ExistAsArrayOnly");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type));
		}

		[Test]
		public void Generics_ItemT ()
		{
			Item<int> item = new Item<int> (1);
			type = GetTest ("Item`1");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "Item<T>");

			type = GetTest ("Items`2");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "Items<T,V>");
		}
	}
}
