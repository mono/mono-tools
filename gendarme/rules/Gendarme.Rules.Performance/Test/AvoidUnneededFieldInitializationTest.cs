//
// Unit tests for AvoidUnneededFieldInitializationRule
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
	public class AvoidUnneededFieldInitializationTest : MethodRuleTestFixture<AvoidUnneededFieldInitializationRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no body
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// not ctor
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			// generated code
			AssertRuleDoesNotApply (SimpleMethods.GeneratedCodeMethod);
		}

		class ClassWithBadStaticCtor {
			public static int i;
			static object o;

			static ClassWithBadStaticCtor ()
			{
				i = 0;
				o = null;
			}

			public ClassWithBadStaticCtor ()
			{
				// setting static fields from an instance ctor
				i = 1;
				o = null;
			}
		}

		class ClassWithGoodStaticCtor {
			static string s;

			static ClassWithGoodStaticCtor ()
			{
				// leave this alone, it's meaning is *very* unclear
				ClassWithBadStaticCtor.i = 0;
				s = String.Empty;
			}

			int i = 0; // ignored by csc, but not [g]mcs

			public ClassWithGoodStaticCtor ()
			{
				// don't touch 'i' inside ctor
			}

			public override string ToString ()
			{
				return String.Format ("{0}: {1}", s, i);
			}
		}

		struct StructWithBadStaticCtor {
			static sbyte i;
			static string s;

			static StructWithBadStaticCtor ()
			{
				i = 0;
				s = null;
			}

			public StructWithBadStaticCtor (sbyte sb)
			{
				// setting static fields from an instance ctor
				i = sb;
				s = String.Empty;
			}
		}

		struct StructWithStatic {
			static string s;
			DayOfWeek dow;

			public StructWithStatic (string str)
			{
				// no point in setting a default value to a static string
				s = null;
				// however all struct ctor MUST initialize all instance fields
				dow = DayOfWeek.Sunday;
			}
		}

		struct Struct {
			string s;
			DayOfWeek dow;

			public Struct (string str)
			{
				s = str;
				// all struct ctor MUST initialize all instance fields
				dow = DayOfWeek.Sunday;
			}
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ClassWithBadStaticCtor> (".cctor", 2);
			AssertRuleFailure<ClassWithBadStaticCtor> (".ctor", 1);

			AssertRuleFailure<StructWithBadStaticCtor> (".cctor", 2);

			AssertRuleFailure<StructWithStatic> (".ctor", 1);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<ClassWithGoodStaticCtor> (".cctor");
			//AssertRuleSuccess<ClassWithGoodStaticCtor> (".ctor");

			AssertRuleSuccess<StructWithBadStaticCtor> (".ctor");
			AssertRuleSuccess<Struct> (".ctor");
		}
	}
}
