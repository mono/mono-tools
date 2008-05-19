// 
// Unit tests for ObsoleteMessagesShouldNotBeEmptyRule
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
using Gendarme.Rules.BadPractice;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.BadPractice {

#pragma warning disable 67, 169, 612, 618

	[TestFixture]
	public class ObsoleteMessagesShouldNotBeEmptyTest : TypeRuleTestFixture<ObsoleteMessagesShouldNotBeEmptyRule> {

		[Obsolete]
		public class ObsoleteEmptyClass {
		}

		[Obsolete ("use something else")]
		internal class ObsoleteClass {
		}

		public class ClassWithObsoleteEmptyStuff {

			[Obsolete] // 1
			public const int ObsoletedEmptyConstField = 411;

			[Obsolete (null)] // 2
			static int ObsoleteEmptyStaticField;

			[Obsolete ("")] // 3
			public int ObsoleteEmptyField;

			[Obsolete] // 4
			public ClassWithObsoleteEmptyStuff (int x)
			{
			}

			[Obsolete (null)] // 5
			public void ObsoleteEmptyMethod ()
			{
			}

			[Obsolete ("")] // 6
			protected int ObsoleteEmptyProperty
			{
				get { return 42; }
			}

			[Obsolete] // 7
			event EventHandler ObsoleteEvent;
		}

		internal class ClassWithObsoleteStuff {
			[Obsolete ("just because")]
			private ClassWithObsoleteStuff ()
			{
			}

			[Obsolete ("!")]
			public void ObsoleteMethod ()
			{
			}

			[Obsolete (" ")]
			protected int ObsoleteProperty {
				get { return 42; }
			}
		}

		[Test]
		public void Class ()
		{
			AssertRuleFailure<ObsoleteEmptyClass> (1);
			AssertRuleFailure<ClassWithObsoleteEmptyStuff> (7);

			AssertRuleSuccess<ObsoleteClass> ();
			AssertRuleSuccess<ClassWithObsoleteStuff> ();
		}

		[Obsolete]
		delegate void ObsoleteEmptyDelegate ();

		[Obsolete ("not needed anymore")]
		delegate void ObsoleteDelegate ();

		[Test]
		public void Delegate ()
		{
			AssertRuleFailure<ObsoleteEmptyDelegate> (1);

			AssertRuleSuccess<ObsoleteDelegate> ();
		}

		[Obsolete ("")]
		protected enum ObsoleteEmptyEnum {
		}

		[Obsolete ("not needed anymore")]
		protected enum ObsoleteEnum {
		}

		[Test]
		public void Enum ()
		{
			AssertRuleFailure<ObsoleteEmptyEnum> (1);

			AssertRuleSuccess<ObsoleteEnum> ();
		}

		[Obsolete (null)]
		interface ObsoleteEmptyInterface {

			[Obsolete (null)]
			void ObsoleteEmptyMethod ();

			[Obsolete ("")]
			int ObsoleteEmptyProperty { get; set; }
		}

		[Obsolete ("not needed anymore")]
		interface ObsoleteInterface {
			int Property { get; set; }
		}

		[Test]
		public void Interface ()
		{
			AssertRuleFailure<ObsoleteEmptyInterface> (3);

			AssertRuleSuccess<ObsoleteInterface> ();
		}

		[Obsolete (null)]
		private struct ObsoleteEmptyStruct {
		}

		public struct StructWithObsoleteEmptyMembers {
			[Obsolete ("")]
			string Why;
		}

		[Obsolete ("not needed anymore")]
		public struct ObsoleteStruct {
		}

		public struct StructWithObsoleteMembers {
			[Obsolete ("not needed anymore")]
			string Why;
		}

		[Test]
		public void Struct ()
		{
			AssertRuleFailure<ObsoleteEmptyStruct> (1);
			AssertRuleFailure<StructWithObsoleteEmptyMembers> (1);

			AssertRuleSuccess<ObsoleteStruct> ();
			AssertRuleSuccess<StructWithObsoleteMembers> ();
		}
	}
}
