//
// Unit Tests for CentralizePInvokesIntoNativeMethodsTypeRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Security;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Rules.Interoperability;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

// [SuppressUnmanagedCodeSecurity] attributes are reversed
namespace Bad.Attributes {
	[SuppressUnmanagedCodeSecurity]
	internal sealed class NativeMethods {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	internal sealed class SafeNativeMethods {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	internal sealed class UnsafeNativeMethods {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}
}

namespace Bad.Instantiate {
	internal class NativeMethods {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	[SuppressUnmanagedCodeSecurity]
	internal class SafeNativeMethods {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	[SuppressUnmanagedCodeSecurity]
	internal class UnsafeNativeMethods {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}
}

namespace Bad.Visibility {
	public sealed class NativeMethods {

		private NativeMethods ()
		{
		}

		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	[SuppressUnmanagedCodeSecurity]
	public sealed class SafeNativeMethods {

		private SafeNativeMethods ()
		{
		}

		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	[SuppressUnmanagedCodeSecurity]
	public sealed class UnsafeNativeMethods {

		private UnsafeNativeMethods ()
		{
		}

		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}
}

namespace Test.Rules.Interoperability {

	internal sealed class NativeMethods {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	[SuppressUnmanagedCodeSecurity]
	internal sealed class SafeNativeMethods {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	[SuppressUnmanagedCodeSecurity]
	internal sealed class UnsafeNativeMethods {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	// type name does not follow convention
	internal sealed class NativeCode {
		[DllImport ("User32.dll")]
		internal static extern Boolean MessageBeep (UInt32 beepType);
	}

	[TestFixture]
	public class CentralizePInvokesIntoNativeMethodsTypeTypeTest : TypeRuleTestFixture<CentralizePInvokesIntoNativeMethodsTypeRule> {

		[Test]
		public void DoesNotApply ()
		{
			// not a good type name
			AssertRuleDoesNotApply (SimpleTypes.Class);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<NativeMethods> ();
			AssertRuleSuccess<SafeNativeMethods> ();
			// it's good but we'll report an Audit defect since this needs reviewing
			AssertRuleFailure<UnsafeNativeMethods> (1);
			Assert.AreEqual (Severity.Audit, Runner.Defects [0].Severity);
		}

		[Test]
		public void BadAttributes ()
		{
			AssertRuleFailure<Bad.Attributes.NativeMethods> (1);
			Assert.AreEqual (Severity.Critical, Runner.Defects [0].Severity, "NativeMethods");

			AssertRuleFailure<Bad.Attributes.SafeNativeMethods> (1);
			Assert.AreEqual (Severity.Critical, Runner.Defects [0].Severity, "SafeNativeMethods");

			// bad attribute + audit
			AssertRuleFailure<Bad.Attributes.UnsafeNativeMethods> (2);
			Assert.AreEqual (Severity.Critical, Runner.Defects [0].Severity, "UnsafeNativeMethods");
			Assert.AreEqual (Severity.Audit, Runner.Defects [1].Severity, "UnsafeNativeMethods-audit");
		}

		[Test]
		public void BadInstantiate ()
		{
			AssertRuleFailure<Bad.Instantiate.NativeMethods> (1);
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "NativeMethods");

			AssertRuleFailure<Bad.Instantiate.SafeNativeMethods> (1);
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "SafeNativeMethods");

			// bad attribute + audit
			AssertRuleFailure<Bad.Instantiate.UnsafeNativeMethods> (2);
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "UnsafeNativeMethods");
			Assert.AreEqual (Severity.Audit, Runner.Defects [1].Severity, "UnsafeNativeMethods-audit");
		}

		[Test]
		public void BadVisibility ()
		{
			AssertRuleFailure<Bad.Visibility.NativeMethods> (1);
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "NativeMethods");

			AssertRuleFailure<Bad.Visibility.SafeNativeMethods> (1);
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "SafeNativeMethods");

			// bad attribute + audit
			AssertRuleFailure<Bad.Visibility.UnsafeNativeMethods> (2);
			Assert.AreEqual (Severity.High, Runner.Defects [0].Severity, "UnsafeNativeMethods");
			Assert.AreEqual (Severity.Audit, Runner.Defects [1].Severity, "UnsafeNativeMethods-audit");
		}
	}

	[TestFixture]
	public class CentralizePInvokesIntoNativeMethodsTypeMethodTest : MethodRuleTestFixture<CentralizePInvokesIntoNativeMethodsTypeRule> {

		[Test]
		public void DoesNotApply ()
		{
			// no pinvoke (not a good type name either)
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<NativeMethods> ("MessageBeep");
			AssertRuleSuccess<SafeNativeMethods> ("MessageBeep");
			AssertRuleSuccess<UnsafeNativeMethods> ("MessageBeep");
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<NativeCode> ("MessageBeep", 1);
		}
	}
}

