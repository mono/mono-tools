//
// Unit tests for PreferGenericsOverRefObjectRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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

using Gendarme.Rules.Design.Generic;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Design.Generic {

	[TestFixture]
	public class PreferGenericsOverRefObjectTest : MethodRuleTestFixture<PreferGenericsOverRefObjectRule> {

		public int Property {
			get { return -1; }
			set { ;}
		}

		public event EventHandler<EventArgs> Event;

		[Test]
		public void DoesNotApply ()
		{
			// no parameters
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply (SimpleMethods.GeneratedCodeMethod);

			AssertRuleDoesNotApply<PreferGenericsOverRefObjectTest> ("get_Property");
			AssertRuleDoesNotApply<PreferGenericsOverRefObjectTest> ("set_Property");

			AssertRuleDoesNotApply<PreferGenericsOverRefObjectTest> ("add_Event");
			AssertRuleDoesNotApply<PreferGenericsOverRefObjectTest> ("remove_Event");
		}

		[Test]
		public void NoRefOrOut ()
		{
			AssertRuleSuccess (SimpleMethods.ExternalMethod);
		}

		public int GetRef (string s, ref object obj)
		{
			int iresult;
			double dresult;
			if (Int32.TryParse (s, out iresult)) {
				obj = iresult;
			} else if (Double.TryParse (s, out dresult)) {
				obj = dresult;
			} else {
				obj = null;
			}
			return s.Length;
		}

		// 'out' is a special case of 'ref'
		public int GetOut(string s, out object obj)
		{
			int iresult;
			double dresult;
			if (Int32.TryParse (s, out iresult)) {
				obj = iresult;
			} else if (Double.TryParse (s, out dresult)) {
				obj = dresult;
			} else {
				obj = null;
			}
			return s.Length;
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<PreferGenericsOverRefObjectTest> ("GetRef");
			AssertRuleFailure<PreferGenericsOverRefObjectTest> ("GetOut");
		}

		public int GetGenericRef<T> (string s, ref T obj)
		{
			if (obj.GetType () == typeof (int))
				obj = (T) (object) Int32.Parse (s);
			else if (obj.GetType () == typeof (double))
				obj = (T) (object) Double.Parse (s);
			else
				obj = default (T);
			return s.Length;
		}

		public int GetGenericOut<T> (string s, out T obj)
		{
			obj = default (T);
			if (obj.GetType () == typeof (int))
				obj = (T) (object) Int32.Parse (s);
			else if (obj.GetType () == typeof (double))
				obj = (T) (object) Double.Parse (s);
			return s.Length;
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<PreferGenericsOverRefObjectTest> ("GetGenericRef");
			AssertRuleSuccess<PreferGenericsOverRefObjectTest> ("GetGenericOut");
		}

		public bool Try (out object o)
		{
			o = this;
			return true;
		}

		[Test]
		public void SpecialCase ()
		{
			AssertRuleDoesNotApply<PreferGenericsOverRefObjectTest> ("SpecialCase");
		}
	}
}
