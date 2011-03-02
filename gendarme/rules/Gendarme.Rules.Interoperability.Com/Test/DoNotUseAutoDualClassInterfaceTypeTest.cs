// 
// Test.Rules.Interoperability.Com.DoNotUseAutoDualClassInterfaceTypeTest
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
	public class DoNotUseAutoDualClassInterfaceTypeTest : TypeRuleTestFixture<DoNotUseAutoDualClassInterfaceTypeRule> {

		[ComVisible (true)]
		[ClassInterface (ClassInterfaceType.AutoDual)]
		public class BadClass {
			// do something
		}

		[ComVisible (true)]
		[ClassInterface (2)]
		public class BadClassShortConstuctor {
			// do something
		}

		[ComVisible (false)]
		[ClassInterface (ClassInterfaceType.AutoDual)]
		public class DoesNotApplyInvisible {
			// do something
		}

		[ComVisible (true)]
		public class GoodNoInterfaceAttribute {
			// do something
		}

		[ComVisible (true)]
		[ClassInterface (ClassInterfaceType.None)]
		public class GoodClassNone : ICloneable {
			public object Clone ()
			{
				return new object ();
			}
		}

		[ComVisible (true)]
		[ClassInterface ((short)0)]
		public class GoodClassNoneShortConstructor : ICloneable {
			public object Clone ()
			{
				return new object ();
			}
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<DoesNotApplyInvisible> ();
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodClassNone> ();
			AssertRuleSuccess<GoodClassNoneShortConstructor> ();
			AssertRuleSuccess<GoodNoInterfaceAttribute> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadClass> ();
			AssertRuleFailure<BadClassShortConstuctor> ();
		}
	}
}
