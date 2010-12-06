//
// Test.Rules.BadPractice.UseFileOpenOnlyWithFileAccessTest
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Security.AccessControl;

using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;


namespace Test.Rules.BadPractice {

	[TestFixture]
	public class UseFileOpenOnlyWithFileAccessTest : MethodRuleTestFixture<UseFileOpenOnlyWithFileAccessRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no CALL[VIRT] instruction
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public void BadMethod1 ()
		{
			var f = File.Open ("HelloWorld.cs", FileMode.Open);
		}

		public void BadMethod2 ()
		{
			var f = File.Open ("HelloWorld.cs", FileMode.Append);
			// unrelated call
			string a = f.ToString ();
			var g = new FileStream ("HelloWorld.cs", FileMode.Truncate);
			// unrelated code
			if (a.Length == 42)
				return;
			else
				return;

		}

		public void BadMethod3()
		{
			var f = File.Open ("HelloWorld.cs", FileMode.CreateNew);
			var g = new FileStream ("HelloWorld.cs", FileMode.OpenOrCreate);
			var h = new IsolatedStorageFileStream ("HelloWorld.cs", FileMode.Create);
		}

		public void GoodMethod ()
		{
			var f = File.Open ("HelloWorld.cs", FileMode.Open, FileAccess.Read);
			var g = new FileStream ("HelloWorld.cs", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			var h = new IsolatedStorageFileStream ("HelloWorld.cs", FileMode.Create, FileAccess.Write);
			
			// unrelated code
			List<string> ls = new List<string> { "a", "b" };
			ls.Clear ();

			var i = new FileStream ("HelloWorld.cs", FileMode.Open, FileSystemRights.Read,
					FileShare.Read, 8, FileOptions.None);
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<UseFileOpenOnlyWithFileAccessTest> ("BadMethod1", 1);
			AssertRuleFailure<UseFileOpenOnlyWithFileAccessTest> ("BadMethod2", 2);
			AssertRuleFailure<UseFileOpenOnlyWithFileAccessTest> ("BadMethod3", 3);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<UseFileOpenOnlyWithFileAccessTest> ("GoodMethod");
		}
	}
}
