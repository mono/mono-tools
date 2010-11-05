//
// Unit tests for AvoidLargeStructureRule
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

using Mono.Cecil;
using Mono.Cecil.Metadata;
using Gendarme.Framework;
using Gendarme.Rules.Performance;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Performance {

#pragma warning disable 169, 649

	[TestFixture]
	public class AvoidLargeStructureTest : TypeRuleTestFixture<AvoidLargeStructureRule> {

		private int GetSize (Type type)
		{
			return (int) AvoidLargeStructureRule.SizeOfStruct (DefinitionLoader.GetTypeDefinition (type));
		}

		[Test]
		public void Class ()
		{
			AssertRuleDoesNotApply<AvoidLargeStructureTest> ();
		}

		// Also note that this rule deals with the managed size of a structure, 
		// not it's unmanaged representation (i.e. [StructLayout] attribute is ignored).

		// note: we need unsafe to call sizeof on structs

		struct Empty {
		}

		[Test]
		public unsafe void StructEmpty ()
		{
			AssertRuleSuccess<Empty> ();
			// note: under MS runtime sizeof(Empty) returns 1
			// while is returns 0 under Mono
			// Assert.AreEqual (sizeof (Empty), GetSize (type), "Size");
		}

		struct Small {
			byte b;
		}

		[Test]
		public unsafe void StructSmall ()
		{
			AssertRuleSuccess<Small> ();
			Assert.AreEqual (sizeof (Small), GetSize (typeof (Small)), "Size");
		}

		struct DualBytes {
			byte b1;
			byte b2;
		}

		[Test]
		public unsafe void StructDualBytes ()
		{
			AssertRuleSuccess<DualBytes> ();
			Assert.AreEqual (sizeof (DualBytes), GetSize (typeof (DualBytes)), "Size");
		}

		struct DualSmall {
			Small s1;
			Small s2;
		}

		[Test]
		public unsafe void StructDualSmall ()
		{
			AssertRuleSuccess<DualSmall> ();
			Assert.AreEqual (sizeof (DualSmall), GetSize (typeof (DualSmall)), "Size");
		}

		struct Misaligned {
			byte b;
			char c;
			int i;
		}

		[Test]
		public unsafe void StructMisaligned ()
		{
			AssertRuleSuccess<Misaligned> ();
			Assert.AreEqual (sizeof (Misaligned), GetSize (typeof (Misaligned)), "Size");
		}

		// 20 bytes - due to misalignment of bytes and chars
		struct MisalignedLarge {
			byte b1;
			char c1;
			byte b2;
			char c2;
			byte b3;
			char c3;
			byte b4;
			char c4;
			int length;
		}

		[Test]
		public unsafe void StructMisalignedLarge ()
		{
			AssertRuleFailure<MisalignedLarge> ();
			Assert.AreEqual (sizeof (MisalignedLarge), GetSize (typeof (MisalignedLarge)), "Size");
		}

		// 16 bytes - due to "correct" alignment of bytes and chars
		struct AlignedLarge {
			byte b1;
			byte b2;
			byte b3;
			byte b4;
			char c1;
			char c2;
			char c3;
			char c4;
			int length;
		}

		[Test]
		public unsafe void StructAlignedLarge ()
		{
			AssertRuleSuccess<AlignedLarge> ();
			Assert.AreEqual (sizeof (AlignedLarge), GetSize (typeof (AlignedLarge)), "Size");
		}

		struct Half {
			double d;
		}

		[Test]
		public unsafe void StructHalf ()
		{
			AssertRuleSuccess<Half> ();
			Assert.AreEqual (sizeof (Half), GetSize (typeof (Half)), "Size");
		}

		struct FiveBytes {
			float d;
			byte b;
		}

		[Test]
		public unsafe void StructFiveBytes ()
		{
			AssertRuleSuccess<FiveBytes> ();
			Assert.AreEqual (sizeof (FiveBytes), GetSize (typeof (FiveBytes)), "Size");
		}

		struct NineBytes {
			double d;
			byte b;
		}

		[Test]
		public unsafe void StructNineBytes ()
		{
			AssertRuleSuccess<NineBytes> ();
			Assert.AreEqual (sizeof (NineBytes), GetSize (typeof (NineBytes)), "Size");
		}

		struct Limit {
			int a;
			float d;
			long l;
		}

		[Test]
		public unsafe void StructLimit ()
		{
			AssertRuleSuccess<Limit> ();
			Assert.AreEqual (sizeof (Limit), GetSize (typeof (Limit)), "Size");
		}

		struct ComposedUnderLimit {
			Half empty;
			Small small;
		}

		[Test]
		public unsafe void StructComposedUnderLimit ()
		{
			AssertRuleSuccess<ComposedUnderLimit> ();
			Assert.AreEqual (sizeof (ComposedUnderLimit), GetSize (typeof (ComposedUnderLimit)), "Size");
		}

		struct ComposedLimit {
			Half empty;
			Half full;
		}

		[Test]
		public unsafe void StructComposedLimit ()
		{
			AssertRuleSuccess<ComposedLimit> ();
			Assert.AreEqual (sizeof (ComposedLimit), GetSize (typeof (ComposedLimit)), "Size");
		}

		struct ComposedOverLimit {
			Limit limit;
			Small small;
		}

		[Test]
		public unsafe void StructComposedOverLimit ()
		{
			AssertRuleFailure<ComposedOverLimit> ();
			Assert.AreEqual (sizeof (ComposedOverLimit), GetSize (typeof (ComposedOverLimit)), "Size");
		}

		// other types defined in TypeCode enum
		struct LessCommonTypes {
			Char c;
			DateTime date;
			Decimal value;
			//DBNull dbnull;
			//object o;
		}
		// note: is we add a DBNull or object inside the struct then CSC will report a CS0208
		// when we call sizeof, even with unsafe, on the type.

		[Test]
		public unsafe void StructLessCommonTypes ()
		{
			AssertRuleFailure<ComposedOverLimit> ();
			// DateTime is 8 bytes under MS while it has 16 bytes on Mono
			//Assert.AreEqual (8, sizeof (DateTime), "DateTime");
			//Assert.AreEqual (16, sizeof (Decimal), "Decimal");
			//Assert.AreEqual (sizeof (LessCommonTypes), GetSize (typeof (LessCommonTypes)), "Size");
		}

		/* error CS0523: Struct member 'Inner.x' of type 'Inner' causes a cycle in the struct layout
		struct Inner {
			Inner x;
		}
		*/

		/* error CS05223: two times
		struct Outer {
			Inner x;
		}

		struct Inner {
			Outer x;
		}
		*/

		// from Mono.Cecil

		struct Elem {

			public bool Simple;
			public bool String;
			public bool Type;
			public bool BoxedValueType;

			public Type FieldOrPropType;
			public object Value;

			public TypeReference ElemType;
		}

		struct FixedArg {
			bool SzArray;
			uint NumElem;
			Elem [] Elems;
		}

		[Test]
		public unsafe void Array ()
		{
			AssertRuleSuccess<FixedArg> ();
			// note: sizeof (FixedArg) does not work (well compile) because of the array
			//Assert.AreEqual (sizeof (FixedArg), GetSize (typeof (FixedArg)), "Size");
			Assert.AreEqual (12, GetSize (typeof (FixedArg)), "Size");
		}

		struct BunchOfEnums {
			ConsoleColor cc;
			ConsoleKey ck;
			ConsoleModifiers cm;
			ConsoleSpecialKey csk;
			DateTimeKind kind;
			DayOfWeek dow;
		}

		[Test]
		public unsafe void Enums ()
		{
			AssertRuleFailure<BunchOfEnums> (1);
			Assert.AreEqual (24, GetSize (typeof (BunchOfEnums)), "Size");
		}

		struct WithStaticField {
			static bool init;
			BunchOfEnums boe1;
			BunchOfEnums boe2;
		}

		[Test]
		public unsafe void Static ()
		{
			AssertRuleFailure<WithStaticField> (1);
			// static does not cound
			Assert.AreEqual (48, GetSize (typeof (WithStaticField)), "Size");
			Assert.AreEqual (Severity.Medium, Runner.Defects [0].Severity, "Severity");
		}

		struct High {
			WithStaticField wsf1;
			WithStaticField wsf2;
		}

		[Test]
		public unsafe void HighSeverity ()
		{
			AssertRuleFailure<High> (1);
			Assert.AreEqual (96, GetSize (typeof (High)), "Size");
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "Severity");
		}

		struct Critical {
			High wsf1;
			High wsf2;
			High wsf3;
			High wsf4;
		}

		[Test]
		public unsafe void CriticalSeverity ()
		{
			AssertRuleFailure<Critical> (1);
			Assert.AreEqual (384, GetSize (typeof (Critical)), "Size");
			Assert.AreEqual (Severity.Critical, Runner.Defects [0].Severity, "Severity");
		}
	}
}
