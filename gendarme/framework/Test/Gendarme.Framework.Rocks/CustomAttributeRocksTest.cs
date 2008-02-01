// 
// Unit tests for CustomAttributeRocks
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using Gendarme.Framework.Rocks;

using Mono.Cecil;
using NUnit.Framework;

namespace Test.Framework.Rocks {

	[TestFixture]
	public class CustomAttributeRocksTest {

		private AssemblyDefinition assembly;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Contains_Null ()
		{
			CustomAttributeCollection cac = new CustomAttributeCollection (null);
			cac.ContainsType ((string) null);
		}

		[Test]
		public void Contains ()
		{
			TypeDefinition type = assembly.MainModule.Types ["Test.Framework.Rocks.CustomAttributeRocksTest"];
			CustomAttributeCollection cac = type.CustomAttributes;
			Assert.IsTrue (cac.ContainsType ("NUnit.Framework.TestFixtureAttribute"), "NUnit.Framework.TestFixtureAttribute");
			Assert.IsFalse (cac.ContainsType ("NUnit.Framework.TestFixture"), "NUnit.Framework.TestFixture");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ContainsAny_Null ()
		{
			CustomAttributeCollection cac = new CustomAttributeCollection (null);
			cac.ContainsAnyType (null);
		}

		[Test]
		public void ContainsAny ()
		{
			TypeDefinition type = assembly.MainModule.Types ["Test.Framework.Rocks.CustomAttributeRocksTest"];
			CustomAttributeCollection cac = type.CustomAttributes;
			Assert.IsTrue (cac.ContainsAnyType (new string[] {
				"NUnit.Framework.TestFixtureAttribute",
				null,
				"System.ICloneable"
			}), "NUnit.Framework.TestFixtureAttribute");
			Assert.IsFalse (cac.ContainsAnyType (new string[] {}), "NUnit.Framework.TestFixture");
		}
	}
}
