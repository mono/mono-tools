//
// Unit Test for UseCorrectSuffix Rule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//      Abramov Daniel <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2007 Néstor Salceda
//  (C) 2007 Abramov Daniel
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security;

using Gendarme.Rules.Naming;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Naming {

	public class CorrectAttribute : Attribute {
	}

	public class IncorrectAttr : Attribute {
	}

	public class OtherAttribute : CorrectAttribute {
	}
	
	public class OtherAttr : CorrectAttribute {
	}
	
	public class CorrectContextStaticAttribute : ContextStaticAttribute {
	}
	
	public class OtherClass {
	}
	
	public class YetAnotherClass : Random {
	}
	
	public class InterfaceImplementer : ICollection {
		
		public int Count {
			get { throw new NotImplementedException(); }
		}

		public bool IsSynchronized {
			get { throw new NotImplementedException (); }
		}

		public object SyncRoot {
			get { throw new NotImplementedException (); }
		}

		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException();
		}

		public void CopyTo (Array array, int index)
		{
			throw new NotImplementedException();
		}
	}
	
	public class CorrectICollectionCollection : InterfaceImplementer {
	}
	
	public class IncorrectICollectionCol : InterfaceImplementer {
	}
	
	public class MultipleInterfaceImplementer : IEnumerable, IPermission {		
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException();
		}

		public void FromXml (SecurityElement e)
		{
			throw new NotImplementedException();
		}

		public SecurityElement ToXml ()
		{
			throw new NotImplementedException();
		}

		public IPermission Copy ()
		{
			throw new NotImplementedException();
		}

		public void Demand ()
		{
			throw new NotImplementedException();
		}

		public IPermission Intersect (IPermission target)
		{
			throw new NotImplementedException();
		}

		public bool IsSubsetOf (IPermission target)
		{
			throw new NotImplementedException();
		}

		public IPermission Union (IPermission target)
		{
			throw new NotImplementedException();
		}
	}
	
	public class CorrectMultipleInterfaceImplementerPermission : MultipleInterfaceImplementer {
	}

	public class CorrectMultipleInterfaceImplementerCollection : MultipleInterfaceImplementer {
	}
	
	public class IncorrectMultipleInterfaceImplementer : MultipleInterfaceImplementer {
	}
       
	public class DerivingClassImplementingInterfaces : EventArgs, IEnumerable, IPermission {		 
		
		public IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException();
		}

		public void FromXml (SecurityElement e)
		{
			throw new NotImplementedException();
		}

		public SecurityElement ToXml ()
		{
			throw new NotImplementedException();
		}

		public IPermission Copy ()
		{
			throw new NotImplementedException();
		}

		public void Demand ()
		{
			throw new NotImplementedException();
		}

		public IPermission Intersect (IPermission target)
		{
			throw new NotImplementedException();
		}

		public bool IsSubsetOf (IPermission target)
		{
			throw new NotImplementedException();
		}

		public IPermission Union (IPermission target)
		{
			throw new NotImplementedException();
		}
	}
	
	public class CorrectDerivingClassImplementingInterfacesEventArgs : DerivingClassImplementingInterfaces {
	}
	
	public class IncorrectDerivingClassImplementingInterfacesCollection : DerivingClassImplementingInterfaces { 
	}
	
	public class IncorrectDerivingClassImplementingInterfaces : DerivingClassImplementingInterfaces { 
	}

	public class CorrectCollection<T> : Collection<T> {
	}

	public class CollectionIncorrect<T> : Collection<T> {
	}

	public class CorrectDictionary<T, V> : Dictionary<T, V> {
	}

	public class DictionaryIncorrect<T, V> : Dictionary<T, V> {
	}

	[TestFixture]
	public class UseCorrectSuffixTest : TypeRuleTestFixture<UseCorrectSuffixRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.GeneratedType);
		}

		[Test]
		public void TestOneLevelInheritanceIncorrectName () 
		{
			AssertRuleFailure<IncorrectAttr> (1);
		}
		
		[Test]
		public void TestOneLevelInheritanceCorrectName () 
		{
			AssertRuleSuccess<CorrectAttribute> ();
		}
		
		[Test]
		public void TestVariousLevelInheritanceCorrectName () 
		{
			AssertRuleSuccess<OtherAttribute> ();
		}
		
		[Test]
		public void TestVariousLevelInheritanceIncorrectName () 
		{
			AssertRuleFailure<OtherAttr> (1);
		}
		
		[Test]
		public void NeedToBeResolvedTypes () 
		{
			AssertRuleSuccess<CorrectContextStaticAttribute> ();
			AssertRuleSuccess<OtherClass> ();
			AssertRuleSuccess<YetAnotherClass> ();
		}
		
		[Test]
		public void TestInterfaceImplementerCorrectName ()
		{
			AssertRuleSuccess<CorrectICollectionCollection> ();
		}

 		[Test]
		public void TestInterfaceImplementerIncorrectName () 
		{
			AssertRuleFailure<IncorrectICollectionCol> (1);
		}			       
		
		[Test]
		public void TestMultipleInterfaceImplementerCorrectName ()
		{
			AssertRuleSuccess<CorrectMultipleInterfaceImplementerPermission> ();
		}     

		[Test]
		public void TestMultipleInterfaceImplementerAnotherCorrectName ()
		{
			AssertRuleSuccess<CorrectMultipleInterfaceImplementerCollection> ();
		}				     
		
       		[Test]
		public void TestMultipleInterfaceImplementerIncorrectName () 
		{
			AssertRuleFailure<IncorrectMultipleInterfaceImplementer> (1);
		}			       
		
		[Test]
		public void TestDerivingClassImplementingInterfacesCorrectName ()
		{
			AssertRuleSuccess<CorrectDerivingClassImplementingInterfacesEventArgs> ();
		}      
		
		[Test]
		public void TestDerivingClassImplementingInterfacesIncorrectName ()
		{
			AssertRuleFailure<IncorrectDerivingClassImplementingInterfaces> (1);
		}      
		
		[Test]
		public void TestDerivingClassImplementingInterfacesAnotherIncorrectName ()
		{
			AssertRuleFailure<IncorrectDerivingClassImplementingInterfacesCollection> (1);
		}

		[Test]
		public void GenericCollection ()
		{
			AssertRuleSuccess<CorrectCollection<int>> ();
			AssertRuleFailure<CollectionIncorrect<int>> (1);
		}

		[Test]
		public void GenericDictionary ()
		{
			AssertRuleSuccess<CorrectDictionary<int,int>> ();
			AssertRuleFailure<DictionaryIncorrect<int,int>> (1);
		}

		class My {
		}

		class MyDelegate {
		}

		enum MyEnum {
		}

		[Flags]
		enum MyFlags {
		}

		class MyEx {
		}

		class MyImpl {
		}

		[Test]
		public void CheckShouldNeverBeUsedSuffixes ()
		{
			AssertRuleSuccess<My> ();
			AssertRuleFailure<MyDelegate> (1);
			AssertRuleFailure<MyEnum> (1);
			AssertRuleFailure<MyFlags> (1);
			AssertRuleFailure<MyEx> (1);
			AssertRuleFailure<MyImpl> (1);
		}

		class MyCollection : EventArgs {
		}

		[Test]
		public void TwiceBadSuffix ()
		{
			// 1- "'Collection' should used only for implementing ICollection or IEnumerable or inheriting Queue, Stack, DataSet and DataTable."
			// 2- "The type name does not end with 'EventArgs' suffix. Append it to the type name."
			AssertRuleFailure<MyCollection> (2);
		}

		// from EnumNotEndsWithEnumOrFlagsSuffixTest.cs

		public enum ReturnValue {
			Foo,
			Bar
		}

		public enum ReturnValueEnum {
			Foo,
			Bar
		}

		[Flags]
		public enum ReturnValues {
			Foo,
			Bar
		}

		[Flags]
		public enum ReturnValuesFlags {
			Foo,
			Bar
		}

		public enum returnvalueenum {
			Foo,
			Bar
		}

		[Flags]
		public enum returnvaluesflags {
			Foo,
			Bar
		}

		[Test]
		public void EnumName ()
		{
			AssertRuleSuccess<ReturnValue> ();
			AssertRuleFailure<ReturnValueEnum> (1);
			AssertRuleFailure<returnvalueenum> (1);
		}

		[Test]
		public void FlagsName ()
		{
			AssertRuleSuccess<ReturnValues> ();
			AssertRuleFailure<ReturnValuesFlags> (1);
			AssertRuleFailure<returnvaluesflags> (1);
		}
	}
}
