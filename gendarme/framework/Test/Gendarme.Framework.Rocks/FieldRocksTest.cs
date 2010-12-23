// 
// Unit tests for FieldRocks
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Andreas Noever <andreas.noever@gmail.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
// (C) 2008 Andreas Noever
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
using System.Linq;
using System.Reflection;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace Test.Framework.Rocks {

	[TestFixture]
	public class FieldRocksTest {

		[System.Runtime.CompilerServices.CompilerGeneratedAttribute]
		private static int cga = 1;

		[System.CodeDom.Compiler.GeneratedCodeAttribute ("unit test", "1.0")]
		protected double gca = 1.0;

		internal IntPtr ptr = IntPtr.Zero;

		private AssemblyDefinition assembly;

		private TypeDefinition type;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
			type = assembly.MainModule.GetType ("Test.Framework.Rocks.FieldRocksTest");
		}

		private FieldDefinition GetField (string fieldName)
		{
			foreach (FieldDefinition field in type.Fields) {
				if (field.Name == fieldName)
					return field;
			}
			Assert.Fail ("Field {0} was not found.", fieldName);
			return null;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void HasAttribute_Null ()
		{
			GetField ("assembly").HasAttribute (null);
		}

		[Test]
		public void HasAttribute ()
		{
			Assert.IsTrue (GetField ("cga").HasAttribute ("System.Runtime.CompilerServices.CompilerGeneratedAttribute"), "CompilerGeneratedAttribute");
			Assert.IsFalse (GetField ("cga").HasAttribute ("NUnit.Framework.TestFixtureAttribute"), "TestFixtureAttribute");
		}

		[Test]
		public void IsGeneratedCode_CompilerGenerated ()
		{
			Assert.IsTrue (GetField ("cga").IsGeneratedCode (), "IsCompilerGenerated");
			Assert.IsFalse (GetField ("assembly").IsGeneratedCode (), "FixtureSetUp");
		}

		[Test]
		public void IsGeneratedCode_GeneratedCode ()
		{
			Assert.IsTrue (GetField ("gca").IsGeneratedCode (), "IsCompilerGenerated");
			Assert.IsFalse (GetField ("assembly").IsGeneratedCode (), "FixtureSetUp");
		}

		static FieldDefinition GetField (TypeDefinition type, string name)
		{
			foreach (FieldDefinition field in type.Fields) {
				if (field.Name == name)
					return field;
			}
			Assert.Fail ("Field '{0}' not found!", name);
			return null;
		}

		[Test]
		public void IsVisible ()
		{
			TypeDefinition type = assembly.MainModule.GetType ("Test.Framework.Rocks.PublicType");
			Assert.IsTrue (GetField (type, "PublicField").IsVisible (), "PublicType.PublicField");
			Assert.IsTrue (GetField (type, "ProtectedField").IsVisible (), "PublicType.ProtectedField");
			Assert.IsFalse (GetField (type, "InternalField").IsVisible (), "PublicType.InternalField");
			Assert.IsFalse (GetField (type, "PrivateField").IsVisible (), "PublicType.PrivateField");

			type = assembly.MainModule.GetType ("Test.Framework.Rocks.PublicType/NestedPublicType");
			Assert.IsTrue (GetField (type, "PublicField").IsVisible (), "NestedPublicType.PublicField");
			Assert.IsTrue (GetField (type, "ProtectedField").IsVisible (), "NestedPublicType.ProtectedField");
			Assert.IsFalse (GetField (type, "PrivateField").IsVisible (), "NestedPublicType.PrivateField");

			type = assembly.MainModule.GetType ("Test.Framework.Rocks.PublicType/NestedProtectedType");
			Assert.IsTrue (GetField (type, "PublicField").IsVisible (), "NestedProtectedType.PublicField");

			type = assembly.MainModule.GetType ("Test.Framework.Rocks.PublicType/NestedPrivateType");
			Assert.IsFalse (GetField (type, "PublicField").IsVisible (), "NestedPrivateType.PublicField");

			type = assembly.MainModule.GetType ("Test.Framework.Rocks.InternalType");
			Assert.IsFalse (GetField (type, "PublicField").IsVisible (), "InternalType.PublicField");
		}

		[Test]
		public void Resolve ()
		{
			foreach (Instruction ins in type.Methods [0].Body.Instructions) {
				FieldReference field = (ins.Operand as FieldReference);
				if ((field != null) && !(field is FieldDefinition)) {
					FieldDefinition fd = field.Resolve ();
					Assert.AreEqual (field.Name, fd.Name, "Name");
					Assert.AreEqual (field.FieldType.FullName, fd.FieldType.FullName, "FieldType");
				}
			}
		}
	}
}
