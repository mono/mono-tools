//
// Unit tests for AvoidRedundancyInMethodNameRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (C) 2008 Cedric Vivier
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

using System;

using Gendarme.Framework;
using Gendarme.Rules.Naming;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Naming {

	class Package {
	}

	class PostOffice {
		public PostOffice () {}
		public virtual void SendPackage (Package package) {}
		public static bool IsPackageValid (Package package) { return true; }
		public static void CheckPackageValid (Package package) { }
		public static void SendPackageTo (Package package, string address) {}
	}

	class DerivedPostOffice : PostOffice {
		public override void SendPackage (Package package) {}
	}

	[TestFixture]
	public class AvoidRedundancyInMethodNameTest : MethodRuleTestFixture<AvoidRedundancyInMethodNameRule> {

		private void NoParam ()
		{
		}

		private bool Property
		{
			get { return true; }
		}

		private void ByRefParamPackage (ref Package package)
		{
		}

		private void OutParamPackage (out Package package)
		{
			package = null;
		}

		private void Shorter (Package package)
		{
		}

		private void ParseString (string text)
		{
		}

		private Package GetRealPackage (Package package)
		{
			return null;
		}

		private void SetPackage (Package package)
		{
		}

		private void PackageSomething (Package package, string something)
		{
		}

		private void Send (Package package)
		{
		}


		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<AvoidRedundancyInMethodNameTest> ("NoParam");
			AssertRuleDoesNotApply<AvoidRedundancyInMethodNameTest> ("get_Property");
			AssertRuleDoesNotApply<AvoidRedundancyInMethodNameTest> ("ByRefParamPackage");
			AssertRuleDoesNotApply<AvoidRedundancyInMethodNameTest> ("OutParamPackage");
			AssertRuleDoesNotApply<PostOffice> (".ctor");//constructor
			AssertRuleDoesNotApply<DerivedPostOffice> ("SendPackage");//override
			AssertRuleDoesNotApply<AvoidRedundancyInMethodNameTest> ("Shorter");//...than parameter type name
			AssertRuleDoesNotApply<AvoidRedundancyInMethodNameTest> ("SetPackage");//too vague
			AssertRuleDoesNotApply<AvoidRedundancyInMethodNameTest> ("Send");
		}

		[Test]
		public void Failure1 ()
		{
			AssertRuleFailure<PostOffice> ("SendPackage", 1);
			Assert.IsTrue (-1 != Runner.Defects [0].Text.IndexOf ("'Send'"), "SendPackage");
		}

		[Test]
		public void Failure2 ()
		{
			AssertRuleFailure<PostOffice> ("IsPackageValid", 1);
			Assert.IsTrue (-1 != Runner.Defects [0].Text.IndexOf ("'Test.Rules.Naming.Package' as property 'IsValid'"), "IsPackageValid");
		}

		[Test]
		public void Failure3 ()
		{
			AssertRuleFailure<PostOffice> ("CheckPackageValid", 1);
			Assert.IsTrue (-1 != Runner.Defects [0].Text.IndexOf ("'Test.Rules.Naming.Package' as method 'CheckValid'"), "CheckPackageValid");
		}

		[Test]
		public void Failure4 ()
		{
			AssertRuleFailure<PostOffice> ("SendPackageTo", 1);
			Assert.IsTrue (-1 != Runner.Defects [0].Text.IndexOf ("'Test.Rules.Naming.Package' as method 'SendTo'"), "SendPackageTo");
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<AvoidRedundancyInMethodNameTest> ("GetRealPackage");//return type is also parameter type
			AssertRuleSuccess<AvoidRedundancyInMethodNameTest> ("PackageSomething");//starts with parameter type name, most likely on purpose/action naming
			AssertRuleSuccess<AvoidRedundancyInMethodNameTest> ("ParseString");//third-party type
		}

	}

}

