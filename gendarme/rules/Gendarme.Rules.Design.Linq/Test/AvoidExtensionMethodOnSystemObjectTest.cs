//
// Unit test for AvoidExtensionMethodOnSystemObjectRule
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
using System.Runtime.CompilerServices;

using Gendarme.Rules.Design.Linq;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Tests.Rules.Design.Linq {

	public static class Extensions {

		public static string NotAnExtension (object self)
		{
			return String.Empty;
		}

		public static string ExtendObject (this object self)
		{
			return String.Format ("'{0}', type '{1}', hashcode: {2}", 
				self.ToString (), self.GetType (), self.GetHashCode ());
		}

		public static string ExtendValueType (this int self)
		{
			return self.ToString ();
		}

		public static string ExtendInterface (this ICloneable self)
		{
			return self.ToString ();
		}

		public static string ExtendEnum (this DateTimeKind self)
		{
			return self.ToString ();
		}

		public static string ExtendType (this OperatingSystem self)
		{
			return self.ToString ();
		}
	}

	[TestFixture]
	public class AvoidExtensionMethodOnSystemObjectTest : MethodRuleTestFixture<AvoidExtensionMethodOnSystemObjectRule> {

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (typeof (Extensions), "NotAnExtension");
		}

		[Test]
		public void ExtendingSystemObject ()
		{
			AssertRuleFailure (typeof (Extensions), "ExtendObject", 1);
		}

		[Test]
		public void Extending ()
		{
			AssertRuleSuccess (typeof (Extensions), "ExtendValueType");
			AssertRuleSuccess (typeof (Extensions), "ExtendInterface");
			AssertRuleSuccess (typeof (Extensions), "ExtendEnum");
			AssertRuleSuccess (typeof (Extensions), "ExtendType");
		}
	}
}
