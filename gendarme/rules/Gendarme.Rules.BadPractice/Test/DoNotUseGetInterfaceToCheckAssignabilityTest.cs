// 
// Unit tests for DoNotUseGetInterfaceToCheckAssignabilityRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using Gendarme.Framework;
using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class DoNotUseGetInterfaceToCheckAssignabilityTest : MethodRuleTestFixture<DoNotUseGetInterfaceToCheckAssignabilityRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no IL
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// no CALL[VIRT] instruction
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		public interface IConvertible {
		}

		class Convertible : IConvertible {
		}

		// in thie case it's clear that the code can be re-written to avoid using a
		// string for the interface
		public bool IsAssignableUsingGetInterface (Type type)
		{
			return (type.GetInterface ("IConvertible") != null);
		}

		public bool IsAssignableUsingGetInterfaceCase (Type type, bool ignoreCase)
		{
			return (type.GetInterface ("IConvertible", ignoreCase) != null);
		}

		[Test]
		public void Bad ()
		{
			Assert.IsTrue (IsAssignableUsingGetInterface (typeof (Convertible)), "my.IConvertible");
			AssertRuleFailure<DoNotUseGetInterfaceToCheckAssignabilityTest> ("IsAssignableUsingGetInterface", 1);
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "1");

			Assert.IsTrue (IsAssignableUsingGetInterfaceCase (typeof (Convertible), true), "my.IConvertible-false");
			AssertRuleFailure<DoNotUseGetInterfaceToCheckAssignabilityTest> ("IsAssignableUsingGetInterfaceCase", 1);
			Assert.AreEqual (Confidence.Normal, Runner.Defects [0].Confidence, "1");
		}

		// in this case it's difficult to know if the code can be re-written to use a type
		// fo the interface
		public bool IsAssignableUsingGetInterfaceNotConstant (Type type, string interfaceName)
		{
			return (type.GetInterface (interfaceName) != null);
		}

		public bool IsAssignableUsingGetInterfaceNotConstantCase (Type type, string interfaceName, bool ignoreCase)
		{
			return (type.GetInterface (interfaceName, ignoreCase) != null);
		}

		[Test]
		public void Bad_LowConfidence ()
		{
			Assert.IsTrue (IsAssignableUsingGetInterfaceNotConstant (typeof (Convertible), "IConvertible"), "my.IConvertible");
			AssertRuleFailure<DoNotUseGetInterfaceToCheckAssignabilityTest> ("IsAssignableUsingGetInterfaceNotConstant", 1);
			Assert.AreEqual (Confidence.Low, Runner.Defects [0].Confidence, "1");

			Assert.IsTrue (IsAssignableUsingGetInterfaceNotConstantCase (typeof (Convertible), "IConvertible", false), "my.IConvertible-false");
			AssertRuleFailure<DoNotUseGetInterfaceToCheckAssignabilityTest> ("IsAssignableUsingGetInterfaceNotConstantCase", 1);
			Assert.AreEqual (Confidence.Low, Runner.Defects [0].Confidence, "2");
		}

		public Type GetTypeInterface (Type type, string name)
		{
			// correct usage of Type.GetInterface
			return type.GetInterface (name);
		}

		[Test]
		public void Ok ()
		{
			Assert.AreEqual (typeof (IConvertible), GetTypeInterface (typeof (Convertible), "IConvertible"), "Usage");
			AssertRuleSuccess<DoNotUseGetInterfaceToCheckAssignabilityTest> ("GetTypeInterface");
		}

		public bool IsAssignable (Type type, Type assign)
		{
			return assign.IsAssignableFrom (type);
		}

		[Test]
		public void Good ()
		{
			Assert.IsFalse (IsAssignable (typeof (Convertible), typeof (System.IConvertible)), "System.IConvertible");
			AssertRuleSuccess<DoNotUseGetInterfaceToCheckAssignabilityTest> ("IsAssignable");
		}

		public void CallToAnotherGetInterfaceMethod ()
		{
			GetInterface ();
		}

		[Test]
		public void GetInterface ()
		{
			AssertRuleSuccess<DoNotUseGetInterfaceToCheckAssignabilityTest> ("CallToAnotherGetInterfaceMethod");
		}
	}
}
