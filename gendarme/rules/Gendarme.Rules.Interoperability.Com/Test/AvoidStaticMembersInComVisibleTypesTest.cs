// 
// Tests.Rules.Interoperability.Com.AvoidStaticMembersInComVisibleTypesTest
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.Runtime.InteropServices;

using Mono.Cecil;
using Gendarme.Rules.Interoperability.Com;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace Test.Rules.Interoperability.Com {

	[TestFixture]
	public class AvoidStaticMembersInComVisibleTypesTest : MethodRuleTestFixture<AvoidStaticMembersInComVisibleTypesRule> {

		[ComVisible (true)]
		public class ComVisibleClass {
			public static void Bad ()
			{
			}

			[ComVisible (false)]
			public static void GoodInvisible ()
			{
			}

			[ComVisible (true)]
			public static void BadExplicitly ()
			{
			}

			private static void DoesNotApplyPrivate ()
			{
			}

			public void DoesNotApplyInstance ()
			{
			}

			[ComRegisterFunction]
			public static void DoesNotApplyRegister ()
			{
			}

			[ComUnregisterFunction]
			public static void DoesNotApplyUnregister ()
			{
			}

			public static void DoesNotApplyGeneric<T> ()
			{
			}

			public static event EventHandler DoesNotApplyEvent
			{
				add
				{
					DoesNotApplyEvent += value;
				}
				remove
				{
					DoesNotApplyEvent -= value;
				}
			}

			public static int DoesNotApplyProperty { get; set; }

			public static ComVisibleClass operator +(ComVisibleClass o1, ComVisibleClass o2)
			{
				return new ComVisibleClass();
			}

			[ComVisible (true)]
			public delegate void DoesNotApplyDelegate ();
		}

		[ComVisible (false)]
		public class ComInvisibleClass {
			public static void DoesNotApply ()
			{
			}
		}

		public class NoAttributesClass {
			public static void DoesNotApply ()
			{
			}
		}

		[ComVisible (true)]
		public interface Interface {
			void DoesNotApply ();
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<ComVisibleClass> ("DoesNotApplyPrivate");
			AssertRuleDoesNotApply<ComVisibleClass> ("DoesNotApplyInstance");
			AssertRuleDoesNotApply<ComVisibleClass> ("DoesNotApplyRegister");
			AssertRuleDoesNotApply<ComVisibleClass> ("DoesNotApplyUnregister");
			AssertRuleDoesNotApply<ComVisibleClass> ("op_Addition");
			AssertRuleDoesNotApply<ComVisibleClass> ("add_DoesNotApplyEvent");
			AssertRuleDoesNotApply<ComVisibleClass> ("remove_DoesNotApplyEvent");
			AssertRuleDoesNotApply<ComVisibleClass> ("get_DoesNotApplyProperty");
			AssertRuleDoesNotApply<ComVisibleClass> ("set_DoesNotApplyProperty");
			AssertRuleDoesNotApply<ComVisibleClass.DoesNotApplyDelegate> ("Invoke");
			AssertRuleDoesNotApply<ComInvisibleClass> ("DoesNotApply");
			AssertRuleDoesNotApply<NoAttributesClass> ("DoesNotApply");
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<ComVisibleClass> ("GoodInvisible");
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ComVisibleClass> ("Bad");
		}
	}
}
