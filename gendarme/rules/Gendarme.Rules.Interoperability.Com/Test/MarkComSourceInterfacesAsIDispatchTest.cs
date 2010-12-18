// 
// Test.Rules.Interoperability.Com.MarkComSourceInterfacesAsIDispatchTest
//
// Authors:
//	Nicholas Rioux
//
// Copyright (C) 2010 Nicholas Rioux
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

	[InterfaceType (ComInterfaceType.InterfaceIsIDispatch)]
	public interface IInterfaceIsIDispatch { }

	[InterfaceType (ComInterfaceType.InterfaceIsDual)]
	public interface IInterfaceIsDual { }

	[InterfaceType (2)]
	public interface IInterfaceIsIDispatchShort { }

	[InterfaceType (1)]
	public interface IInterfaceIsIUnknownShort { }

	[InterfaceType ((short) 0)]
	public interface IInterfaceIsDualShort { }

	public interface IBlankInterface { }

	[ComSourceInterfaces ("Test.Rules.Interoperability.Com.IInterfaceIsIDispatch")]
	public class GoodStringOverloadClass { }

	[ComSourceInterfaces (typeof (IInterfaceIsIDispatch))]
	public class GoodTypeOverloadClass { }

	[ComSourceInterfaces ("Test.Rules.Interoperability.Com.IInterfaceIsIDispatchShort")]
	public class GoodStringOverloadShortClass { }

	[ComSourceInterfaces (typeof (IInterfaceIsIDispatchShort))]
	public class GoodTypeOverloadShortClass { }

	[ComSourceInterfaces (typeof (IInterfaceIsDual))]
	public class BadWrongTypeClass { }

	[ComSourceInterfaces (typeof (IBlankInterface))]
	public class BadNoTypeClass { }

	[ComSourceInterfaces (typeof (IInterfaceIsDual), typeof(IBlankInterface))]
	public class BadTwoInterfacesClass { }

	[ComSourceInterfaces (typeof (IInterfaceIsDualShort))]
	public class BadWrongTypeShortClass { }

	[ComSourceInterfaces (typeof (IInterfaceIsIUnknownShort), typeof (IBlankInterface))]
	public class BadTwoInterfacesShortClass { }


	[TestFixture]
	public class MarkComSourceInterfacesAsIDispatchTest : TypeRuleTestFixture<MarkComSourceInterfacesAsIDispatchRule> {

		[Test]
		public void DoesNotApply ()
		{
			// No ComSourceInterfaces attribute.
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Structure);

			// Applies only to types.
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodStringOverloadClass> ();
			AssertRuleSuccess<GoodTypeOverloadClass> ();
		}

		[Test]
		public void GoodShort ()
		{
			AssertRuleSuccess<GoodStringOverloadShortClass> ();
			AssertRuleSuccess<GoodTypeOverloadShortClass> ();
		}

		[Test]
		public void Bad ()
		{
			// The specified interface is marked with the wrong ComInterfaceType
			AssertRuleFailure<BadWrongTypeClass> (1);

			// The specified interface isn't marked with an InterfaceTypeAttribute.
			AssertRuleFailure<BadNoTypeClass> (1);

			// Both of the above
			AssertRuleFailure<BadTwoInterfacesClass> (2);
		}

		[Test]
		public void BadShort ()
		{
			// variants using the 'short' based attribute
			AssertRuleFailure<BadWrongTypeShortClass> (1);
			AssertRuleFailure<BadTwoInterfacesShortClass> (2);
		}
	}
}
