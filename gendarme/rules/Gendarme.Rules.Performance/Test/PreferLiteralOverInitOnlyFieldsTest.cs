//
// Unit tests for PreferLiteralOverInitOnlyFieldsRule
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
	public class PreferLiteralOverInitOnlyFieldsTest : TypeRuleTestFixture<PreferLiteralOverInitOnlyFieldsRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		public class ClassWithConstants {
			public const sbyte one = 1;
			public const short two = 2;
			public const int three = 3;
			public const long four = 4;
			public const float five = 5.0f;
			public const double six = 6.0d;
			public const string seven = "7";
		}

		public struct StructureWithConstants {
			const byte one = 1;
			const ushort two = 2;
			const uint three = 3;
			const ulong four = 4;
			const float five = 5.0f;
			const double six = 6.0d;
			const string seven = "7";
		}

		[Test]
		public void NoStaticConstructor ()
		{
			AssertRuleDoesNotApply<ClassWithConstants> ();
			AssertRuleDoesNotApply<StructureWithConstants> ();
		}

		public class ClassWithStatics {
			public static sbyte one = 1;
			public static short two = 2;
			public static int three = 3;
			public static long four = 4;
			public static float five = 5.0f;
			public static double six = 6.0d;
			public static string seven = "7";
		}

		public struct StructureWithStatics {
			public static byte one = 1;
			public static ushort two = 2;
			public static uint three = 3;
			public static ulong four = 4;
			public static float five = 5.0f;
			public static double six = 6.0d;
			public static string seven = "7";
		}

		[Test]
		public void NotReadOnly ()
		{
			AssertRuleSuccess<ClassWithStatics> ();
			AssertRuleSuccess<StructureWithStatics> ();
		}

		public class ClassWithReadOnly {
			static readonly sbyte one = 1;
			static readonly short two = 2;
			static readonly int three = 3;
			static readonly long four = 4;
			static readonly float five = 5.0f;
			static readonly double six = 6.0d;
			static readonly string seven = "7";
		}

		public struct StructureWithReadOnly {
			public static readonly byte one = 1;
			public static readonly ushort two = 2;
			public static readonly uint three = 3;
			public static readonly ulong four = 4;
			public static readonly float five = 5.0f;
			public static readonly double six = 6.0d;
			public static readonly string seven = "7";
		}

		[Test]
		public void ReadOnly ()
		{
			AssertRuleFailure<ClassWithReadOnly> (7);
			AssertRuleFailure<StructureWithReadOnly> (7);
		}

		class ClassWithNoStaticField {
			static ClassWithNoStaticField ()
			{
				Console.WriteLine (ClassWithConstants.one);
			}
		}

		struct SructureWithNoStaticField {
			static SructureWithNoStaticField ()
			{
				Console.WriteLine (StructureWithReadOnly.one);
			}
		}

		[Test]
		public void NoStaticField ()
		{
			AssertRuleDoesNotApply<ClassWithNoStaticField> ();
			AssertRuleDoesNotApply<SructureWithNoStaticField> ();
		}

		class ClassSettingOutsideField {
			static ClassSettingOutsideField ()
			{
				ClassWithStatics.one = -1;
			}
		}

		struct StructureSettingOutsideField {
			static StructureSettingOutsideField ()
			{
				StructureWithStatics.one = 1;
			}
		}

		[Test]
		public void SettingOutsideField ()
		{
			AssertRuleSuccess<ClassSettingOutsideField> ();
			AssertRuleSuccess<StructureSettingOutsideField> ();
		}
	}
}
