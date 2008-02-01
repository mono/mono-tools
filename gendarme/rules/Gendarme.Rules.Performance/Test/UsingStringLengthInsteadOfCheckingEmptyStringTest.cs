// 
// Unit tests for UsingStringLengthInsteadOfCheckingEmptyStringRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//
// Copyright (c) <2007> Nidhi Rawal
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
using Gendarme.Rules.Performance;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Performance
{
	[TestFixture]
	public class UsingStringLengthInsteadOfCheckingEmptyStringTest {
		
		public class UsingStringEquals
		{
			string s = "";
			public static void Main (string [] args)
			{
				UsingStringEquals u = new UsingStringEquals ();
				if (u.s.Equals ("")) {
				}
			}
		}
		
		public class UsingStringLength
		{
			string s = "";
			public static void Main (string [] args)
			{
				UsingStringLength u = new UsingStringLength ();
				if (u.s.Length == 0) {
				}
			}
		}
		
		public class UsingEquqlsWithNonStringArg
		{
			int i = 0;
			public static void Main (string [] args)
			{
				UsingEquqlsWithNonStringArg u = new UsingEquqlsWithNonStringArg ();
				if (u.i.Equals (1)) {
				}
			}
		}
		
		public class AnotherUseOfEqualsWithEmptyString
		{
			string s = "abc";
			public static void Main (string [] args)
			{
				AnotherUseOfEqualsWithEmptyString a = new AnotherUseOfEqualsWithEmptyString ();
				bool b = a.s.Equals ("");
			}
		}
		
		public class OneMoreUseOfEqualsWithEmptyString
		{
			string s = "";
			public static void Main (string [] args)
			{
				OneMoreUseOfEqualsWithEmptyString o = new OneMoreUseOfEqualsWithEmptyString ();
				if (o.s.Equals ("")) {
					bool b = o.s.Equals ("");
				}
			}
		}
		
		public class UsingEqualsWithNonEmptyString
		{
			string s = "";
			public static void Main (string [] args)
			{
				UsingEqualsWithNonEmptyString u = new UsingEqualsWithNonEmptyString ();
				if (u.s.Equals ("abc")) {
				}
			}
		}	
		
		private IMethodRule methodRule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		MessageCollection messageCollection;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			methodRule = new UsingStringLengthInsteadOfCheckingEmptyStringRule();
			messageCollection = null;
		}
		
		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Performance.UsingStringLengthInsteadOfCheckingEmptyStringTest/" + name;
			return assembly.MainModule.Types[fullname];
		}
		
		[Test]
		public void usingStringEqualsTest ()
		{
			type = GetTest ("UsingStringEquals");
			foreach (MethodDefinition method in type.Methods) {
				messageCollection = methodRule.CheckMethod (method, new MinimalRunner ());
			}
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}
		
		[Test]
		public void usingStringLengthTest ()
		{
			type = GetTest ("UsingStringLength");
			foreach (MethodDefinition method in type.Methods) {
				messageCollection = methodRule.CheckMethod (method, new MinimalRunner ());
			}
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void usingEquqlsWithNonStringArgTest ()
		{
			type = GetTest ("UsingEquqlsWithNonStringArg");
			foreach (MethodDefinition method in type.Methods) {
				messageCollection = methodRule.CheckMethod (method, new MinimalRunner ());
			}
			Assert.IsNull (messageCollection);
		}
		
		[Test]
		public void anotherUseOfEqualsWithEmptyStringTest ()
		{
			type = GetTest ("AnotherUseOfEqualsWithEmptyString");
			foreach (MethodDefinition method in type.Methods) {
				messageCollection = methodRule.CheckMethod (method, new MinimalRunner ());
			}
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (1, messageCollection.Count);
		}
		
		[Test]
		public void oneMoreUseOfEqualsWithEmptyStringTest ()
		{
			type = GetTest ("OneMoreUseOfEqualsWithEmptyString");
			foreach (MethodDefinition method in type.Methods) {
				messageCollection = methodRule.CheckMethod (method, new MinimalRunner ());
			}
			Assert.IsNotNull (messageCollection);
			Assert.AreEqual (2, messageCollection.Count);
		}
		
		[Test]
		public void usingEqualsWithNonEmptyStringTest ()
		{
			type = GetTest ("UsingEqualsWithNonEmptyString");
			foreach (MethodDefinition method in type.Methods) {
				messageCollection = methodRule.CheckMethod (method, new MinimalRunner ());
			}
			Assert.IsNull (messageCollection);
		}
	}
}
