//
// Unit Test for AvoidCodeDuplicatedInSameClass Rule.
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 - 2008 Néstor Salceda
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
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Rules.Smells;
using Mono.Cecil;
using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Smells {

	public class ClassWithoutCodeDuplicated {
		private IList myList;
		private IList otherList;

		public IList MyList {
			get {
				if (myList == null)
					myList = new ArrayList ();
				return myList;
			}
		}

		public IList OtherList {
			get {
				if (otherList == null)
					otherList = new ArrayList ();
				return otherList;
			}
		}

		public ClassWithoutCodeDuplicated () 
		{
			myList = new ArrayList ();
			myList.Add ("Foo");
			myList.Add ("Bar");
			myList.Add ("Baz");
		}

		private void PrintValuesInList () 
		{
			foreach (string value in myList) {
				Console.WriteLine (value);
			}      
		}

		private void PrintValuesInListUsingAForLoop () 
		{
			for (int index = 0; index < myList.Count; index++) {
				Console.WriteLine (myList[index]);	
			}
		}

		public void PrintAndAddANewValue () 
		{
			PrintValuesInList ();
			myList.Add ("FooReplied");
		}

		public void PrintAndRemoveANewValue () 
		{
			PrintValuesInList ();
			myList.Remove ("FooReplied");
		}
		
		public void PrintUsingAForLoopAndRemoveAValueIfNotExists () 
		{
			PrintValuesInListUsingAForLoop ();
			if (!myList.Contains ("Bar"))
				myList.Remove ("Bar");
		}
		
		public void PrintUsingAForLoopAndRemoveAValueIfExists () 
		{
			PrintValuesInListUsingAForLoop ();
			if (myList.Contains ("Bar"))
				myList.Remove ("Bar");
		}
	}
	
	public class ClassWithCodeDuplicated {
		private IList myList;

		public ClassWithCodeDuplicated () 
		{
			myList = new ArrayList ();
			myList.Add ("Foo");
			myList.Add ("Bar");
			myList.Add ("Baz");
		}

		public void PrintAndAddANewValue () 
		{
			foreach (string value in myList) {
				Console.WriteLine (value);
			}
			myList.Add ("FooReplied");
		}

		public void PrintAndRemoveANewValue () 
		{
			foreach (string value in myList) {
				Console.WriteLine (value);              
			}
			myList.Remove ("FooReplied");
		} 
		
		//This two methods contains code duplicated, but by the moment
		//the comparer can't detect it, because is a special case and
		//for the next improvements I will improve the compararer for
		//detect also subsets.
		/*
		public void ShowBannerAndAdd () 
		{
			Console.WriteLine ("Banner");
			Console.WriteLine ("Print");
			myList.Add ("MoreBar");
		}
		
		public void AddAndShowBanner () 
		{
			myList.Add ("MoreFoo");
			Console.WriteLine ("Banner");
			Console.WriteLine ("Print");
		}
		*/
		
		public void PrintUsingAForLoopAndAddAValue () 
		{
			for (int index = 0; index < myList.Count; index++) {
				Console.WriteLine (myList[index]);	
			}
			myList.Add ("MoreFoo");
		}
		
		public void PrintUsingAForLoopAndRemoveAValue () 
		{
			for (int index = 0; index < myList.Count; index++) {
				Console.WriteLine (myList[index]);	
			}
			myList.Remove ("MoreFoo");
		}
		
		public void IfConditionAndRemove () {
			if (myList.Contains ("MoreFoo") & myList.Contains ("MoreBar"))
				myList.Remove ("MoreFoo");
		}
		
		public void IfConditionAndRemoveReplied () {
			if (myList.Contains ("MoreFoo") & myList.Contains ("MoreFoo"))
				myList.Remove ("MoreFoo");
		}
	}
	
	public class UsingProperties {
		ArrayList x = new ArrayList ();
		ArrayList y = new ArrayList ();
		ArrayList z = new ArrayList ();

		public int X {
			get { return x.Count; }
		}

		public int Y  {
			get { return y.Count; }
		}

		public int Z {
			get { return z.Count; }
		}
	}
	

	[TestFixture]
	public class AvoidCodeDuplicatedInSameClassTest : TypeRuleTestFixture<AvoidCodeDuplicatedInSameClassRule> {
		[Test]
		public void TestClassWithoutCodeDuplicated () 
		{
			AssertRuleSuccess<ClassWithoutCodeDuplicated> ();
		}
		
		[Test]
		public void TestClassWithCodeDuplicated () 
		{
			AssertRuleFailure<ClassWithCodeDuplicated> (3);
		}

		[Test]
		public void TestUsingProperties ()
		{
			AssertRuleSuccess<UsingProperties> ();
		}
	}
}
