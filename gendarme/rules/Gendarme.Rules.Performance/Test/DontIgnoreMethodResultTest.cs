//
// Unit tests for DoNotIgnoreMethodResultRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
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

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading;

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.Performance {

	[TestFixture]
	public class DoNotIgnoreMethodResultTest : MethodRuleTestFixture<DoNotIgnoreMethodResultRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no body
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no Pop
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			// generated code
			AssertRuleDoesNotApply (SimpleMethods.GeneratedCodeMethod);
		}

		public class Item {

			public void Violations ()
			{
				"violationOne".ToUpper(CultureInfo.InvariantCulture);
				string violationTwo = "MediuM ";
				violationTwo.ToLower(CultureInfo.InvariantCulture).Trim();
			}

			public static void CreateItem ()
			{
				new Item ();
			}

			// StringBuilder is a special case when the method returns a StringBuilder
			public string StringBuilderOk ()
			{
				StringBuilder sb = new StringBuilder ();
				sb.Append ("test").Insert (2, true).Remove (0, 1).Replace ("oh", "ah").
					AppendFormat ("-{0}-", 1).AppendLine ();
				return sb.ToString ();
			}

			public void StringBuilderBad ()
			{
				StringBuilder sb = new StringBuilder ();
				sb.Append ("test").Insert (2, true).Remove (0, 1).Replace ("oh", "ah").
					AppendFormat ("-{0}-", 1).AppendLine ();
				// this is not ok
				sb.ToString ();
			}

			public void DirectoryOk ()
			{
				// the DirectoryInfo this returns may not be needed
				Directory.CreateDirectory ("mono");

				DirectoryInfo di = Directory.CreateDirectory ("2.0");
				// the DirectoryInfo this returns may not be needed
				di.CreateSubdirectory ("gac");
			}

			public void DirectoryBad ()
			{
				DirectoryInfo di = Directory.CreateDirectory ("mono");
				// this is not ok
				di.GetDirectories ();
			}

			public void PermissionSetOk ()
			{
				PermissionSet set = new PermissionSet (PermissionState.None);
				// the IPermission this returns may not be needed
				set.AddPermission (new GacIdentityPermission ());
				set.RemovePermission (typeof (GacIdentityPermission));
			}

			public void PermissionSetBad ()
			{
				PermissionSet set = new PermissionSet (PermissionState.None);
				// this is not ok
				set.Union (set);
			}

			void TimeoutCallback (object state)
			{
			}

			public void TimerOk ()
			{
				new Timer (TimeoutCallback, null, 3000, Timeout.Infinite);
			}

			public void Stack ()
			{
				Stack stack = new Stack ();
				stack.Pop ();
			}

			public void StackGenerics ()
			{
				Stack<int> stack = new Stack<int> ();
				stack.Pop ();
			}
		}

		[Test]
		public void TestStringMethods()
		{
			AssertRuleFailure<Item> ("Violations", 2);
		}

		[Test]
		public void TestConstructor()
		{
			AssertRuleFailure<Item> ("CreateItem", 1);
		}

		[Test]
		public void TestStringBuilder ()
		{
			AssertRuleSuccess<Item> ("StringBuilderOk");
			AssertRuleFailure<Item> ("StringBuilderBad", 1);
		}

		[Test]
		public void TestDirectory ()
		{
			AssertRuleSuccess<Item> ("DirectoryOk");
			AssertRuleFailure<Item> ("DirectoryBad", 1);
		}

		[Test]
		public void TestPermissionSet ()
		{
			AssertRuleSuccess<Item> ("PermissionSetOk");
			AssertRuleFailure<Item> ("PermissionSetBad", 1);
		}

		[Test]
		public void TestTimer ()
		{
			AssertRuleSuccess<Item> ("TimerOk");
		}

		[Test]
		public void StackPop ()
		{
			AssertRuleSuccess<Item> ("Stack");
			AssertRuleSuccess<Item> ("StackGenerics");
		}
	}
}
