//
// Unit tests for MethodSignature
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Helpers;

using NUnit.Framework;

namespace Test.Framework {

	[TestFixture]
	public class MethodSignatureTest {

		private TypeDefinition type;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = System.Reflection.Assembly.GetExecutingAssembly ().Location;
			type = AssemblyDefinition.ReadAssembly (unit).MainModule.GetType ("Test.Framework.MethodSignatureTest");
		}

		private MethodDefinition GetMethod (string name, int parameters)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (method.Name == name) {
					if ((parameters == -1) || (method.Parameters.Count == parameters))
						return method;
				}
			}
			return null;
		}

		private MethodDefinition GetMethod (string name)
		{
			return GetMethod (name, -1);
		}

		[Test]
		public void TestDefaultConstructor ()
		{
			MethodSignature sig = new MethodSignature ();
			Assert.IsNull (sig.Name, "Name");
			Assert.IsNull (sig.Parameters, "Parameters");
			Assert.IsNull (sig.ReturnType, "ReturnType");
			Assert.AreEqual (String.Empty, sig.ToString (), "ToString");
		}

		[Test]
		public void MatchNull ()
		{
			Assert.IsFalse (new MethodSignature ().Matches (null));
		}

		[Test]
		public void AlwaysTrueCustomLogic ()
		{
			MethodSignature ms = new MethodSignature (null, (method) => (true));
			Assert.IsFalse (ms.Matches (null), "null");
			Assert.IsTrue (ms.Matches (GetMethod ("TestMatch")), "anything");
		}

		[Test]
		public void AlwaysTrueCustomLogic_CheckReturnValue ()
		{
			MethodSignature ms = new MethodSignature (null, "System.Void", (method) => (true));
			Assert.IsFalse (ms.Matches (null), "null");
			Assert.IsTrue (ms.Matches (GetMethod ("TestMatch")), "any name returning void");
			Assert.IsFalse (ms.Matches (GetMethod ("Method")), "bool");
		}

		public bool Method ()
		{
			return false;
		}

		public bool Method (bool parameter)
		{
			return false;
		}

		public bool Method (bool a, string b)
		{
			return false;
		}

		[Test]
		public void TestMatch ()
		{
			Assert.IsTrue (new MethodSignature ().Matches (GetMethod ("TestMatch")), "a");

			Assert.IsTrue (new MethodSignature ("TestMatch").Matches (GetMethod ("TestMatch")), "b");
			Assert.IsFalse (new MethodSignature ("TestMatch_").Matches (GetMethod ("TestMatch")), "c");

			Assert.IsTrue (new MethodSignature (null, "System.Void").Matches (GetMethod ("TestMatch")), "d");
			Assert.IsFalse (new MethodSignature (null, "System.Void_").Matches (GetMethod ("TestMatch")), "e");

			Assert.IsFalse (new MethodSignature (null, null, new string [1]).Matches (GetMethod ("TestMatch")), "f");
			Assert.IsFalse (new MethodSignature (null, null, new string [] { "System.Boolean" }).Matches (GetMethod ("Method", 0)), "g");
			Assert.IsFalse (new MethodSignature (null, null, new string [] { null }).Matches (GetMethod ("Method", 0)), "h");
			Assert.IsFalse (new MethodSignature (null, null, new string [] { "System.Object" }).Matches (GetMethod ("Method", 0)), "i");

			Assert.IsTrue (new MethodSignature (null, null, new string [] { "System.Boolean" }).Matches (GetMethod ("Method", 1)), "j");
			Assert.IsTrue (new MethodSignature (null, null, new string [] { null }).Matches (GetMethod ("Method", 1)), "k");
			Assert.IsFalse (new MethodSignature (null, null, new string [] { "System.Object" }).Matches (GetMethod ("Method", 1)), "l");

			Assert.IsFalse (new MethodSignature (null, null, new string [] { "System.Boolean" }).Matches (GetMethod ("Method", 2)), "m");
			Assert.IsFalse (new MethodSignature (null, null, new string [] { null }).Matches (GetMethod ("Method", 2)), "n");
			Assert.IsFalse (new MethodSignature (null, null, new string [] { "System.Object" }).Matches (GetMethod ("Method", 2)), "o");
		}

		[Test]
		public void TestToString ()
		{
			Assert.AreEqual (String.Empty, new MethodSignature ().ToString (), "empty");
			Assert.AreEqual (String.Empty, new MethodSignature (null, "System.Void").ToString (), "return value");
			Assert.AreEqual (String.Empty, new MethodSignature (null, "System.Void", new string [] { "System.Object" }).ToString (), "return value + one param");

			Assert.AreEqual ("Equals()", new MethodSignature ("Equals").ToString (), "name");
			Assert.AreEqual ("System.Boolean Equals()", new MethodSignature ("Equals", "System.Boolean").ToString (), "name + return value");
			Assert.AreEqual ("System.Boolean Equals(System.Object)", new MethodSignature ("Equals", "System.Boolean", new string[] { "System.Object" }).ToString (), "name + return value + one param");

			Assert.AreEqual ("System.Boolean Equals(A,B)", new MethodSignature ("Equals", "System.Boolean", new string [] { "A", "B" }).ToString (), "name + return value + two param");
		}
	}
}
