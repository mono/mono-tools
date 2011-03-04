// 
// Tests.Rules.BadPractice.DoNotDecreaseVisibilityTest
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Gendarme.Rules.BadPractice ;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace Test.Rules.BadPractice {

	[TestFixture]
	public class DoNotDecreaseVisibilityTest : MethodRuleTestFixture<DoNotDecreaseVisibilityRule> {

		public class TestCase {

			public class Base {
				public void Public ()
				{
				}

				protected bool Protected (int x)
				{
					return x == 0;
				}

				internal int Internal ()
				{
					return -1;
				}

				private float Private (float f)
				{
					return f;
				}
			}

			public class BadInheritor : Base {
				private new void Public ()
				{
				}

				private new bool Protected (int x)
				{
					return x == 1;
				}

				private new int Internal ()
				{
					return -1;
				}

				private new float Private (float f)
				{
					return -f;
				}
			}

			public class NoInheritance {
				private new void Public ()
				{
				}

				private new bool Protected (int x)
				{
					return x == 1;
				}

				private new int Internal ()
				{
					return -1;
				}

				private new float Private (float f)
				{
					return -f;
				}
			}

			// c# cannot seal the method without making it an override
			// and an override cannot change visibility
#if false
			public class FinalInheritor : Base {
				private new sealed void Public ()
				{
				}

				private new sealed bool Protected (int x)
				{
					return x == 1;
				}

				private new sealed int Internal ()
				{
					return -1;
				}

				private new float Private (float f)
				{
					return -f;
				}
			}
#endif
			public sealed class Sealed : Base {
				private new void Public ()
				{
				}

				private new bool Protected (int x)
				{
					return x == 1;
				}

				private new int Internal ()
				{
					return -1;
				}

				private new float Private (float f)
				{
					return -f;
				}
			}

			public class StaticCtor {
				static StaticCtor ()
				{
				}
			}

			public class StaticCtorInheritor : StaticCtor {
				static StaticCtorInheritor ()
				{
				}
			}
		}

		[Test]
		public void DoesNotApply ()
		{
			// not private
			AssertRuleDoesNotApply<TestCase.Base> ("Public");
			AssertRuleDoesNotApply<TestCase.Base> ("Protected");
			AssertRuleDoesNotApply<TestCase.Base> ("Internal");
#if false
			// method is sealed (final)
			AssertRuleDoesNotApply<TestCase.FinalInheritor> ("Public");
			AssertRuleDoesNotApply<TestCase.FinalInheritor> ("Protected");
			AssertRuleDoesNotApply<TestCase.FinalInheritor> ("Internal");
			AssertRuleDoesNotApply<TestCase.FinalInheritor> ("Private");
#endif
			// type is sealed
			AssertRuleDoesNotApply<TestCase.Sealed> ("Public");
			AssertRuleDoesNotApply<TestCase.Sealed> ("Protected");
			AssertRuleDoesNotApply<TestCase.Sealed> ("Internal");
			AssertRuleDoesNotApply<TestCase.Sealed> ("Private");
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<TestCase.Base> ("Private");

			AssertRuleSuccess<TestCase.NoInheritance> ("Public");
			AssertRuleSuccess<TestCase.NoInheritance> ("Protected");
			AssertRuleSuccess<TestCase.NoInheritance> ("Internal");
			AssertRuleSuccess<TestCase.NoInheritance> ("Private");

			AssertRuleSuccess<TestCase.BadInheritor> ("Internal");
			AssertRuleSuccess<TestCase.BadInheritor> ("Private");

			AssertRuleSuccess<TestCase.StaticCtor> (".cctor");
			AssertRuleSuccess<TestCase.StaticCtorInheritor> (".cctor");
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<TestCase.BadInheritor> ("Public");
			AssertRuleFailure<TestCase.BadInheritor> ("Protected");
		}
	}
}
