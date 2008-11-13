//
// Unit tests for AvoidTypeGetTypeForConstantStringsRule
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

using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Performance {

	[TestFixture]
	public class AvoidTypeGetTypeForConstantStringsTest : MethodRuleTestFixture <AvoidTypeGetTypeForConstantStringsRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no CALL or CALLVIRT
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		// http://lists.ximian.com/archives/public/mono-patches/2008-June/121564.html

		System.Type booleanType;
		System.Type stringType;
		System.Type intType;
		System.Type typeType;
		System.Type shortType;

		public void CacheTypesBad ()
		{
			booleanType = System.Type.GetType ("System.Boolean");
			stringType = System.Type.GetType ("System.String");
			intType = System.Type.GetType ("System.Int32");
			typeType = System.Type.GetType ("System.Type");
			shortType = System.Type.GetType ("System.Int16");
		}

		public void CacheTypesGood ()
		{
			booleanType = typeof (bool);
			stringType = typeof (string);
			intType = typeof (int);
			typeType = typeof (Type);
			shortType = typeof (short);
		}

		[Test]
		public void Cache ()
		{
			AssertRuleFailure<AvoidTypeGetTypeForConstantStringsTest> ("CacheTypesBad", 5);
			AssertRuleDoesNotApply<AvoidTypeGetTypeForConstantStringsTest> ("CacheTypesGood");
		}

		public System.Type GetType0 (string s)
		{
			s += "a"; // to get a LDSTR
			return s.GetType ();
		}

		public System.Type GetType1 (string s)
		{
			s += "a"; // to get a LDSTR
			return System.Type.GetType (s);
		}

		string instance_field;

		public System.Type GetType2 ()
		{
			instance_field += "x"; // to get a LDSTR
			return System.Type.GetType (instance_field, true);
		}

		static string static_field;

		public System.Type GetType3 ()
		{
			static_field += "x"; // to get a LDSTR
			return System.Type.GetType (static_field, true, true);
		}

		[Test]
		public void GetTypes ()
		{
			AssertRuleSuccess<AvoidTypeGetTypeForConstantStringsTest> ("GetType0");
			AssertRuleSuccess<AvoidTypeGetTypeForConstantStringsTest> ("GetType1");
			AssertRuleSuccess<AvoidTypeGetTypeForConstantStringsTest> ("GetType2");
			AssertRuleSuccess<AvoidTypeGetTypeForConstantStringsTest> ("GetType3");
		}

		public class Type {
			public static System.Type GetType (string s)
			{
				return null;
			}
		}

		public System.Type GetType ()
		{
			return Type.GetType ("System.Int32");
		}

		[Test]
		public void LookAlike ()
		{
			AssertRuleSuccess<AvoidTypeGetTypeForConstantStringsTest> ("GetType");
		}

		public System.Type GetTypeFromAnotherAssembly ()
		{
			return System.Type.GetType ("System.Drawing.Printing.SysPrn, System.Drawing");
		}

		private bool IsMonoRuntime ()
		{
			return (null != System.Type.GetType ("Mono.Runtime"));
		}

		[Test]
		public void GoodReasons ()
		{
			// there are some good reasons to use a string
			// e.g. loading a non-visible type (not easy to detect)
			// or more commonly getting a type from an assembly that is not loaded
			AssertRuleSuccess<AvoidTypeGetTypeForConstantStringsTest> ("GetTypeFromAnotherAssembly");
			// or the supported way to detect if we are running on mono runtime
			AssertRuleSuccess<AvoidTypeGetTypeForConstantStringsTest> ("IsMonoRuntime");
		}
	}
}
