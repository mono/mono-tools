//
// ComVisibleShouldInheritFromComVisibleTest.cs
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.Collections;
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Rules.Interoperability.Com;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

namespace Test.Rules.Interoperability.Com {

	[TestFixture]
	public class ComVisibleShouldInheritFromComVisibleTest : TypeRuleTestFixture<ComVisibleShouldInheritFromComVisibleRule> {

		[ComVisible (false)]
		public class ComInvisibleClass {
		}

		[ComVisible (true)]
		public class ComVisibleClass {
		}

		[ComVisible (true)]
		public class ComVisibleInheritsFromInvisibleClass : ComInvisibleClass {
		}

		[ComVisible (true)]
		class NotReallyComVisibleInheritsFromInvisibleClass : ComInvisibleClass {
		}

		[ComVisible (false)]
		public class ComInvisibleInheritsFromVisibleClass : ComVisibleClass {
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<ComVisibleClass> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<ComVisibleInheritsFromInvisibleClass> ();
		}

		[Test]
		public void DoesNotApply ()
		{
			// BaseType is null
			AssertRuleDoesNotApply<ICollection> ();

			// not visible / no ComVisible attributes in inheritance chain
			AssertRuleDoesNotApply (SimpleTypes.Class);

			AssertRuleDoesNotApply<ComInvisibleClass> ();
			AssertRuleDoesNotApply<ComInvisibleInheritsFromVisibleClass> ();
			AssertRuleDoesNotApply<NotReallyComVisibleInheritsFromInvisibleClass> ();
		}
	}
}
