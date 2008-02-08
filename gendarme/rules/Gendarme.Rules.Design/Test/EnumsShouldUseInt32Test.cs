// 
// Unit tests for EnumsShouldUseInt32Rule
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
using System.Reflection;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;

namespace Test.Rules.Design {

	[TestFixture]
	public class EnumsShouldUseInt32Test {

		public enum DefaultEnum {
		}

		public enum ByteEnum : byte {
		}

		public enum SignedByteEnum : sbyte {
		}

		public enum ShortEnum : short {
		}

		public enum UnsignedShortEnum : ushort {
		}

		public enum IntEnum : int {
		}

		public enum UnsignedIntEnum : uint {
		}

		public enum LongEnum : long {
		}

		public enum UnsignedLongEnum : ulong {
		}

		private ITypeRule rule;
		private AssemblyDefinition assembly;
		private Runner runner;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyFactory.GetAssembly (unit);
			rule = new EnumsShouldUseInt32Rule ();
			runner = new MinimalRunner ();
		}

		private TypeDefinition GetTest (string name)
		{
			string fullname = "Test.Rules.Design.EnumsShouldUseInt32Test" + name;
			return assembly.MainModule.Types [fullname];
		}

		[Test]
		public void NotAnEnum ()
		{
			TypeDefinition type = GetTest (String.Empty);
			Assert.IsNull (rule.CheckType (type, runner));
		}

		[Test]
		public void Ok ()
		{
			TypeDefinition type = GetTest ("/DefaultEnum");
			Assert.IsNull (rule.CheckType (type, runner), "DefaultEnum");

			type = GetTest ("/IntEnum");
			Assert.IsNull (rule.CheckType (type, runner), "IntEnum");
		}

		[Test]
		public void Bad ()
		{
			// CLS compliant types: Byte, Int16 or Int64 (Int32 is CLS but Ok)

			TypeDefinition type = GetTest ("/ByteEnum");
			Assert.IsNotNull (rule.CheckType (type, runner), "ByteEnum");

			type = GetTest ("/ShortEnum");
			Assert.IsNotNull (rule.CheckType (type, runner), "ShortEnum");

			type = GetTest ("/LongEnum");
			Assert.IsNotNull (rule.CheckType (type, runner), "LongEnum");
		}

		[Test]
		public void ReallyBad ()
		{
			// i.e. using non-CLS compliant types, SByte, UInt16, UInt32 or UInt64

			TypeDefinition type = GetTest ("/SignedByteEnum");
			Assert.IsNotNull (rule.CheckType (type, runner), "SignedByteEnum");

			type = GetTest ("/UnsignedShortEnum");
			Assert.IsNotNull (rule.CheckType (type, runner), "UnsignedShortEnum");

			type = GetTest ("/UnsignedIntEnum");
			Assert.IsNotNull (rule.CheckType (type, runner), "UnsignedIntEnum");

			type = GetTest ("/UnsignedLongEnum");
			Assert.IsNotNull (rule.CheckType (type, runner), "UnsignedLongEnum");
		}
	}
}
