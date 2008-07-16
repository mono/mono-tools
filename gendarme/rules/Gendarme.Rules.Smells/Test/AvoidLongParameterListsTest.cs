//
// Unit Test for AvoidLongParameterLists Rule.
//
// Authors:
//      Néstor Salceda <nestor.salceda@gmail.com>
//
//      (C) 2007-2008 Néstor Salceda
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
using System.Reflection;
using System.Runtime.InteropServices;

using Mono.Cecil;
using NUnit.Framework;
using Gendarme.Framework;
using Gendarme.Rules.Smells;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

namespace Test.Rules.Smells {
	
	[TestFixture]
	public class AvoidLongParameterListsTest : TypeRuleTestFixture<AvoidLongParameterListsRule> {

		class ExternalMethodWrapper {
			[DllImport ("gdiplus.dll")]
			static internal extern int GdipCreateLineBrushFromRectI (object rect, int color1, int color2, int linearGradientMode, int wrapMode, IntPtr brush);
		}

		[Test]
		public void ExternalMethodTest ()
		{
			AssertRuleDoesNotApply<ExternalMethodWrapper> ();
		}

		class MethodWithoutLongParametersWrapper {
			public void SimpleMethod () {}
			public void MethodWithoutLongParameterList (int x, char c, object obj, bool j, string f) {}
		}

		[Test]
		public void MethodWithoutParametersTest  ()
		{
			AssertRuleSuccess<MethodWithoutLongParametersWrapper> ();
		}


		class MethodWithLongParametersWrapper {
			public void MethodWithLongParameterList (int x, char c, object obj, bool j, string f, float z, double u, short s, int v, string[] array) {}
		}

		[Test]
		public void MethodWithLongParametersTest ()
		{
			AssertRuleFailure<MethodWithLongParametersWrapper> (1);
		}
			
		class OverloadedMethodWrapper {
			public void MethodWithLongParameterList (int x, char c, object obj, bool j, string f, float z, double u, short s, int v, string[] array) {}
			public void MethodWithLongParameterList (int x, char c) {}
		}
	
		[Test]
		public void OverloadedMethodWrapperTest () 
		{
			AssertRuleSuccess<OverloadedMethodWrapper> ();
		}

		class FailingOverloadedMethodWrapper {
			public void MethodWithLongParameterList (int x, char c, object obj, bool j, string f, float z, double u, short s, int v, string[] array) {}
			public void MethodWithLongParameterList (int x, char c, object obj, bool j, string f, float z, double u, short s) {}
		}

		[Test]
		public void FailingOverloadedMethodWrapperTest () 
		{
			//Only report de smaller fail
			AssertRuleFailure<FailingOverloadedMethodWrapper> (1);
		}

		class VariousViolationsMethodWrapper {
			[DllImport ("gdiplus.dll")]
			static internal extern int GdipCreateLineBrushFromRectI (object rect, int color1, int color2, int linearGradientMode, int wrapMode, IntPtr brush);
			public void SimpleMethod () {}
			public void FirstOverload (int x, char c, object obj, bool j, string f) {}
			public void FirstOverload (int x, char c, object obj, bool j, string f, float z, double u, short s, int v, string[] array) {}
			public void SecondOverload (int x, char c) {}
			public void SecondOverload (int x, char c, object obj, bool j, string f, float z, double u, short s, int v, string[] array) {}
			public void ThirdOverload (int x, char c, object obj, bool j, string f, float z, double u, short s) {}
			public void ThirdOverload (int x, char c, object obj, bool j, string f, float z, double u, short s, int v, string[] array) {}
			public void FourthOverload (int x, char c, object obj, bool j, string f, float z, double u, short s) {}
			public void FourthOverload (int x, char c, object obj, bool j, string f, float z, double u, short s, int v, string[] array) {}
		}

		[Test]
		public void VariousViolationsMethodWrapperTest ()
		{
			//Fail the Third and Fourth Overloads
			AssertRuleFailure<VariousViolationsMethodWrapper> (2);
		}

		class ShortConstructorWrapper {
			public ShortConstructorWrapper (int x, float f, char c) {}
		}

		[Test]
		public void ShortConstructorWrapperTest ()
		{
			AssertRuleSuccess<ShortConstructorWrapper> ();
		}

		class LongConstructorWrapper {
			public LongConstructorWrapper (int x, float f, char c, string str, object obj, double d, short s, object[] array) {}
		}

		[Test]
		public void LongConstructorWrapperTest ()
		{
			AssertRuleFailure<LongConstructorWrapper> (1);
		}

		class ShortDelegateWrapper {
			public delegate void ShortDelegate (int x, char c);
		}

		[Test]
		public void ShortDelegateWrapperTest ()
		{
			AssertRuleSuccess<ShortDelegateWrapper.ShortDelegate> ();
		}

		class LongDelegateWrapper {
			public delegate void LongDelegate (int x, float f, char c, string str, object obj, double d, short s, object[] array);
		}

		[Test]
		public void LongDelegateWrapperTest ()
		{
			AssertRuleFailure<LongDelegateWrapper.LongDelegate> (1);
		}

		class StaticConstructor {

			static StaticConstructor ()
			{
			}

			public StaticConstructor (int a, bool b, char c, double d, Enum e, float f)
			{
			}
		}

		[Test]
		public void StaticConstructorTest ()
		{
			AssertRuleFailure<StaticConstructor> (1);
		}

		[Test]
		public void MulticastDelegateTest ()
		{
			AssertRuleSuccess<MulticastDelegate> ();
		}
	}
}
