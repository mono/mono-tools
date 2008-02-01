//
// Unit tests for AvoidPublicInstanceFieldsRule
//
// Authors:
//	Adrian Tsai <adrian_tsai@hotmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) 2007 Adrian Tsai
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
using Gendarme.Rules.Design;
using Mono.Cecil;
using NUnit.Framework;

namespace Test.Rules.Design {

	[TestFixture]
	public class AvoidPublicInstanceFieldsTest {

		public class ShouldBeCaught {
			public int x;
		}

		public class ShouldBeIgnored {
			protected int y;
			private int z;
		}

		public class ArraySpecialCase {
			public byte [] key;
			public string [] files;
		}

		public class StaticSpecialCase {
			public static int x;
		}

		public class ConstSpecialCase {
			public const int x = 1;
		}

		public class ReadOnlySpecialCase {
			public readonly int x = 1;
		}

		public enum Enum {
			Zero,
			One
		}

		[Flags]
		public enum Flags {
			One,
			Two
		}


		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private TypeDefinition type;
		private ModuleDefinition module;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			module = assembly.MainModule;
			type = module.Types ["Test.Rules.Design.AvoidPublicInstanceFieldsTest"];
			rule = new AvoidPublicInstanceFieldsRule ();
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Design.AvoidPublicInstanceFieldsTest/" + name;
			return assembly.MainModule.Types [fullname];
		}

		[Test]
		public void TestShouldBeCaught ()
		{
			TypeDefinition type = GetTest ("ShouldBeCaught");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestShouldBeIgnored ()
		{
			TypeDefinition type = GetTest ("ShouldBeIgnored");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestArraySpecialCase ()
		{
			TypeDefinition type = GetTest ("ArraySpecialCase");
			Assert.IsNotNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestEnum ()
		{
			TypeDefinition type = GetTest ("Enum");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestFlags ()
		{
			TypeDefinition type = GetTest ("Flags");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestStaticSpecialCase ()
		{
			TypeDefinition type = GetTest ("StaticSpecialCase");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestConstSpecialCase ()
		{
			TypeDefinition type = GetTest ("ConstSpecialCase");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}

		[Test]
		public void TestReadOnlySpecialCase ()
		{
			TypeDefinition type = GetTest ("ReadOnlySpecialCase");
			Assert.IsNull (rule.CheckType (type, new MinimalRunner ()));
		}
	}
}
