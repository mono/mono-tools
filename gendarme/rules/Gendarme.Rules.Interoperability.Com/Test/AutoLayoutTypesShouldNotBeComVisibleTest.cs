//
// AutoLayoutTypesShouldNotBeComVisibleTest.cs
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
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Rules.Interoperability.Com;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;

[assembly: ComVisible (false)]

namespace Test.Rules.Interoperability.Com {

	[TestFixture]
	public class AutoLayoutTypesShouldNotBeComVisibleTest : TypeRuleTestFixture<AutoLayoutTypesShouldNotBeComVisibleRule> {

		[ComVisible (true)]
		[StructLayout (LayoutKind.Auto)]
		public struct Bad {
			ushort a;
			ushort b;
		}
		[ComVisible (true)]
		[StructLayout (LayoutKind.Explicit)]
		public struct GoodExplicit {
			[FieldOffset (0)]
			ushort a;
			[FieldOffset (2)]
			ushort b;
		}

		[ComVisible (true)]
		[StructLayout (LayoutKind.Sequential)]
		public struct GoodSequential {
			ushort a;
			ushort b;
		}

		[ComVisible (false)]
		[StructLayout (LayoutKind.Auto)]
		public struct DoesNotApplyComInvisible {
			ushort a;
			ushort b;
		}

		[StructLayout (LayoutKind.Auto)]
		public struct DoesNotApplyNoComVisibleAttribute {
			ushort a;
			ushort b;
		}

		[ComVisible (true)]
		[StructLayout (LayoutKind.Auto)]
		public class DoesNotApplyType {
			ushort a;
			ushort b;
		}

		[ComVisible (true)]
		[StructLayout (LayoutKind.Auto)]
		internal struct DoesNotApplyInternal {
			ushort a;
			ushort b;
		}

		[ComVisible (true)]
		[StructLayout (LayoutKind.Auto)]
		public struct DoesNotApplyGeneric<T> {
			ushort a;
			ushort b;
		}


		[Test]
		public void GoodTest ()
		{
			// public explicit layout struct with [ComVisible (true)]
			AssertRuleSuccess<GoodExplicit> ();
			// public sequential layout struct with [ComVisible (true)] 
			AssertRuleSuccess<GoodSequential> ();
		}

		[Test]
		public void BadTest ()
		{
			// public auto layout struct with [ComVisible (true)]
			AssertRuleFailure<Bad> ();
		}

		[Test]
		public void DoesNotApply ()
		{
			// public auto layout struct with [ComVisible (false)]
			AssertRuleDoesNotApply<DoesNotApplyComInvisible> ();
			// public auto layout class with [ComVisible (true)]
			AssertRuleDoesNotApply<DoesNotApplyType> ();
			// public generic auto layout struct with [ComVisible (true)]
			AssertRuleDoesNotApply<DoesNotApplyGeneric<Int32>> ();
			// internal auto layout struct with [ComVisible (true)]
			AssertRuleDoesNotApply<DoesNotApplyInternal> ();
			// public auto layout struct with no ComVisible attribute
			AssertRuleDoesNotApply<DoesNotApplyNoComVisibleAttribute> ();
			// public struct with no attributes
			AssertRuleDoesNotApply (SimpleTypes.Structure);

		}

		[ComVisible (true)]
		public enum Enum {
			Zero = 0
		}

		[Flags]
		[ComVisible (true)]
		public enum Flags {
			One = 1
		}

		[Test]
		public void SpecialCases ()
		{
			AssertRuleDoesNotApply<Enum> ();
			AssertRuleDoesNotApply<Flags> ();
		}
	}
}
