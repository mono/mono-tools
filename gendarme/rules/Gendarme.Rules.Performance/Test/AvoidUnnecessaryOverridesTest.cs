//
// Unit tests for AvoidUnnecessaryOverridesRule
//
// Authors:
//	N Lum <nol888@gmail.com
//
// Copyright (C) 2010 N Lum
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
using System.Security.Permissions;

using Gendarme.Rules.Performance;

using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Tests.Rules.Performance {

	[TestFixture]
	public class AvoidUnnecessaryOverridesTest : MethodRuleTestFixture<AvoidUnnecessaryOverridesRule> {

		private class TestBaseClass {

			~TestBaseClass ()
			{
				Console.WriteLine ("the end");
			}

			public string NonVirtualDoSomething (int i)
			{
				return i.ToString ();
			}

			public virtual string DoSomething (string s)
			{
				return s;
			}

			public virtual string DoSomething ()
			{
				return ":D";
			}

			public virtual void DoNothing ()
			{
			}
		}


		abstract class AbstractTestClass : TestBaseClass {
			~AbstractTestClass ()
			{
				Console.WriteLine ("abstract");
			}

			public abstract void DoSomething (int i);

			public override void DoNothing ()
			{
				base.DoNothing ();
			}
		}

		private class TestClassGood : TestBaseClass {
			public override string DoSomething (string s)
			{
				return base.DoSomething ();
			}
			[STAThread]
			public override string DoSomething ()
			{
				return base.DoSomething ();
			}
			[FileIOPermission (SecurityAction.Demand)]
			public override string ToString ()
			{
				return base.ToString ();
			}
			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;
				else
					return base.Equals (obj);
			}
		}

		private class TestClassAlsoGood : ApplicationException {
			public override bool Equals (object obj)
			{
				if (obj.GetType () != typeof (TestClassAlsoGood))
					return false;

				return base.Equals (obj);
			}
		}

		private class TestClassBad : TestBaseClass {
			public override string ToString ()
			{
				return base.ToString ();
			}
			public override string DoSomething (string s)
			{
				return base.DoSomething (s);
			}
			public override string DoSomething ()
			{
				return base.DoSomething ();
			}
		}

		private class TestClassAlsoBad : ApplicationException {
			public override Exception GetBaseException ()
			{
				return base.GetBaseException ();
			}
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<TestClassGood> ("DoSomething", new Type [] { typeof (string) });
			AssertRuleSuccess<TestClassGood> ("DoSomething", Type.EmptyTypes);
			AssertRuleSuccess<TestClassGood> ("Equals");
			AssertRuleSuccess<TestClassGood> ("ToString");
			AssertRuleSuccess<TestClassAlsoGood> ("Equals");
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<TestClassBad> ("ToString", 1);
			AssertRuleFailure<TestClassBad> ("DoSomething", new Type [] { typeof (string) }, 1);
			AssertRuleFailure<TestClassBad> ("DoSomething", Type.EmptyTypes, 1);
			AssertRuleFailure<TestClassAlsoBad> ("GetBaseException", 1);
			AssertRuleFailure<AbstractTestClass> ("DoNothing", 1);
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<TestBaseClass> ("NonVirtualDoSomething");
			AssertRuleDoesNotApply<TestClassGood> (".ctor");
			AssertRuleDoesNotApply<TestClassBad> (".ctor");
			AssertRuleDoesNotApply<AbstractTestClass> ("DoSomething");
		}

		public class BaseClass {
			public virtual string DoSomething (int i)
			{
				return i.ToString();
			}
		}

		public class GoodClass : BaseClass {
			public string Property { get; set; }

			public override string DoSomething (int i)
			{
				Property = base.DoSomething (i);
				return Property;
			}
		}

		[Test]
		public void Bug663492 ()
		{
			AssertRuleSuccess<GoodClass> ("DoSomething");
		}
	}
}
