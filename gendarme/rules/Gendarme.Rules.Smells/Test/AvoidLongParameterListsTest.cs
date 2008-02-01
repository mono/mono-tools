//
// Unit Test for AvoidLongParameterLists Rule.
//
// Authors:
//      Néstor Salceda <nestor.salceda@gmail.com>
//
//      (C) 2007 Néstor Salceda
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

namespace Test.Rules.Smells {
	[TestFixture]
	public class AvoidLongParameterListsTest {
		private IMethodRule rule;
		private AssemblyDefinition assembly;
		private MethodDefinition method;
		private TypeDefinition type;
		private MessageCollection messageCollection;

		[TestFixtureSetUp]
		public void FixtureSetUp () 
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			type = assembly.MainModule.Types["Test.Rules.Smells.AvoidLongParameterListsTest"];
			rule = new AvoidLongParameterListsRule ();
			messageCollection = null;
		}

		private MethodDefinition GetMethodForTest (string methodName, Type[] parameterTypes) 
		{
			if (parameterTypes == Type.EmptyTypes) {
				if (type.Methods.GetMethod (methodName).Length == 1)
					return type.Methods.GetMethod (methodName)[0];
			}
			return type.Methods.GetMethod (methodName, parameterTypes);
		}

		public void MethodWithoutParameters () 
		{
		}

		public void MethodWithoutLongParameterList (int x, char c, object obj, bool j, string f) 
		{
		}

		public void MethodWithLongParameterList (int x, char c, object obj, bool j, string f, float z, double u, short s, int v, string[] array)
		{
		}

		public void OverloadedMethod () 
		{
		}

		public void OverloadedMethod (int x) 
		{
		}

		public void OverloadedMethod (int x, char c, object obj, bool j, string f, float z, double u, short s, int v, string[] array) 
		{
		}

		public void OtherOverloaded (int x, char c, object obj, bool j, string f, float z, double u)
		{
		}

		public void OtherOverloaded (int x, char c, object obj, bool j, string f, float z, double u, short s)
		{
		}

		// copy-paste from Mono's System.Drawing, types changed to avoid referencing assembly
		[DllImport ("gdiplus.dll")]
		static internal extern int GdipCreateLineBrushFromRectI (object rect, int color1, int color2, int linearGradientMode, int wrapMode, IntPtr brush);

		[Test]
		public void MethodWithoutParametersTest () 
		{
			method = GetMethodForTest ("MethodWithoutParameters", Type.EmptyTypes);
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void MethodwithLongParameterListTest () 
		{
			method = GetMethodForTest ("MethodWithLongParameterList", Type.EmptyTypes);
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}

		[Test]
		public void MethodWithoutLongParameterList () 
		{
			method = GetMethodForTest ("MethodWithoutLongParameterList", Type.EmptyTypes);
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void OverloadedMethodTest () 
		{
			method = GetMethodForTest ("OverloadedMethod", Type.EmptyTypes);
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void OverloadedMethodWithParametersTest () 
		{
			method = GetMethodForTest ("OverloadedMethod", new Type[] {typeof (int)});
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void LongOverloadedMethodWithLittleOverloadTest () 
		{
			method = GetMethodForTest ("OverloadedMethod", new Type[] {typeof (int), typeof (char), typeof (object), typeof (bool), typeof (string), typeof (float), typeof (double), typeof (short), typeof (int), typeof (string[])});
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNull (messageCollection);
		}

		[Test]
		public void LongOverloadedMethodWithoutLittleOverloadTest () 
		{
			method = GetMethodForTest ("OtherOverloaded", new Type[] {typeof (int), typeof (char), typeof (object), typeof (bool), typeof (string), typeof (float), typeof (double)});
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}

		[Test]
		public void OtherLongOverloadedMethodWithoutLittleOverloadTest () 
		{
			method = GetMethodForTest ("OtherOverloaded", new Type[] {typeof (int), typeof (char), typeof (object), typeof (bool), typeof (string), typeof (float), typeof (double), typeof (short)});
			messageCollection = rule.CheckMethod (method, new MinimalRunner ());
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}

		[Test]
		public void IgnoreExternalMethods ()
		{
			method = GetMethodForTest ("GdipCreateLineBrushFromRectI", new Type[] {typeof (object), typeof (int), typeof (int), typeof (int), typeof (int), typeof (IntPtr) });
			Assert.IsNull (rule.CheckMethod (method, new MinimalRunner ()));
		}
	}
}
