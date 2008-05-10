// 
// Unit tests for ToStringReturnsNullRule
//
// Authors:
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

using Gendarme.Framework;
using Gendarme.Rules.BadPractice;
using Mono.Cecil;
using NUnit.Framework;

using Test.Rules.Helpers;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class ToStringReturnsNullTest {

		public class ToStringReturningNull {
			public override string ToString ()
			{
				// this is bad
				return null;
			}
		}
		
		public class ToStringReturningEmptyString {
			public override string ToString ()
			{
				// this is Ok
				return String.Empty;
			}
		} 
		
		public class ToStringReturningField {
			string s = "ab";
			public override string ToString ()
			{
				// this is Ok (even if we're not sure) ???
				return s;
			}
		}

		public class ToStringReturningConstField {
			const string s = "ab";
			public override string ToString ()
			{
				// this is Ok
				return s;
			}
		}

		public class ToStringReturningReadOnlyField {
			readonly string s = "ab";
			public override string ToString ()
			{
				// this is Ok
				return s;
			}
		}

		public class ToStringReturningNewString {
			public override string ToString ()
			{
				// this is Ok
				return new string ('!', 2);
			}
		}
		
		public class ToStringReturningStringFormat {
			public override string ToString ()
			{
				return String.Format ("{0}-{1}", 1, 2);
			}
		}

		public class ToStringReturningConvertToStringObject {
			public override string ToString ()
			{
				return Convert.ToString ((object)null);
			}
		}

		public class ToStringReturningConvertToStringString {
			public override string ToString ()
			{ 
				return Convert.ToString ((string)null);
			}
		}

		public class ToStringReturningTypeName {
			public override string ToString ()
			{
				return GetType ().FullName;
			}
		}
		
		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private TestRunner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new ToStringReturnsNullRule ();
			runner = new TestRunner (rule);
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.BadPractice.ToStringReturnsNullTest/" + name;
			return assembly.MainModule.Types[fullname];
		}
		
		[Test]
		public void ReturningNullTest ()
		{
			type = GetTest ("ToStringReturningNull");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void ReturningEmptyStringTest ()
		{
			type = GetTest ("ToStringReturningEmptyString");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void ReturningField ()
		{
			type = GetTest ("ToStringReturningField");
			// there's doubt but it's not easy (i.e. false positives) to be sure
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void ReturningConstField ()
		{
			type = GetTest ("ToStringReturningConstField");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void ReturningReadOnlyField ()
		{
			type = GetTest ("ToStringReturningReadOnlyField");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void ReturningNewString ()
		{
			type = GetTest ("ToStringReturningNewString");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}

		[Test]
		public void ReturningStringFormat ()
		{
			type = GetTest ("ToStringReturningStringFormat");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		public void ReturningConvertToStringObject ()
		{
			Assert.AreEqual (String.Empty, Convert.ToString ((object) null), "Convert.ToString(object)");
			type = GetTest ("ToStringReturningConvertToStringObject");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
		
		[Test]
		[Ignore ("requires to analyze what called methods returns")]
		public void ReturningConvertToStringString ()
		{
			// converting a null string to a string return null
			// however this is a special case since, most times, we won't know 
			// if the value passed to Convert.ToString is null or not
			Assert.IsNull (Convert.ToString ((string) null), "Convert.ToString((string)null)");
			type = GetTest ("ToStringReturningConvertToStringString");
			Assert.AreEqual (RuleResult.Failure, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (1, runner.Defects.Count, "Count");
		}

		[Test]
		public void ReturningTypeName ()
		{
			type = GetTest ("ToStringReturningTypeName");
			Assert.AreEqual (RuleResult.Success, runner.CheckType (type), "RuleResult");
			Assert.AreEqual (0, runner.Defects.Count, "Count");
		}
	}
}
