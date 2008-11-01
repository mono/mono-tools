//
// Unit Tests for DoNotAssumeIntPtrSizeRule.
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
using System.Runtime.InteropServices;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Rules.Interoperability;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Definitions;

#pragma warning disable 169

namespace Test.Rules.Interoperability {
	
	[TestFixture]
	public unsafe class DoNotAssumeIntPtrSizeTest : MethodRuleTestFixture<DoNotAssumeIntPtrSizeRule> {

		private int CastIntPtrToInt32 (IntPtr ptr)
		{
			return (int) ptr;
		}

		private uint CastIntPtrToUInt32 (IntPtr ptr)
		{
			return (uint) ptr;
		}

		private long CastIntPtrToInt64 (IntPtr ptr)
		{
			return (long) ptr;
		}

		private ulong CastIntPtrToUInt64 (IntPtr ptr)
		{
			return (ulong) ptr;
		}

		private IntPtr CastIntPtrToIntPtr (IntPtr ptr)
		{
			return (IntPtr) ptr;
		}

		private void* CastIntPtrToVoidPointer (IntPtr ptr)
		{
			return (void*) ptr;
		}

		[Test]
		public void TypeCastIntPtr ()
		{
			AssertRuleFailure<DoNotAssumeIntPtrSizeTest> ("CastIntPtrToInt32", 1);
			AssertRuleFailure<DoNotAssumeIntPtrSizeTest> ("CastIntPtrToUInt32", 1);
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("CastIntPtrToInt64");
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("CastIntPtrToUInt64");
			AssertRuleDoesNotApply<DoNotAssumeIntPtrSizeTest> ("CastIntPtrToIntPtr");
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("CastIntPtrToVoidPointer");
		}

		private int CastUIntPtrToInt32 (UIntPtr ptr)
		{
			return (int) ptr;
		}

		private uint CastUIntPtrToUInt32 (UIntPtr ptr)
		{
			return (uint) ptr;
		}

		private long CastUIntPtrToInt64 (UIntPtr ptr)
		{
			return (long) ptr;
		}

		private ulong CastUIntPtrToUInt64 (UIntPtr ptr)
		{
			return (ulong) ptr;
		}

		private UIntPtr CastUIntPtrToIntPtr (UIntPtr ptr)
		{
			return (UIntPtr) ptr;
		}

		private void* CastUIntPtrToVoidPointer (UIntPtr ptr)
		{
			return (void*) ptr;
		}

		[Test]
		public void TypeCastUIntPtr ()
		{
			AssertRuleFailure<DoNotAssumeIntPtrSizeTest> ("CastUIntPtrToInt32", 1);
			AssertRuleFailure<DoNotAssumeIntPtrSizeTest> ("CastUIntPtrToUInt32", 1);
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("CastUIntPtrToInt64");
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("CastUIntPtrToUInt64");
			AssertRuleDoesNotApply<DoNotAssumeIntPtrSizeTest> ("CastUIntPtrToIntPtr");
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("CastUIntPtrToVoidPointer");
		}

		private void BadLoop (IntPtr dest)
		{
			int ptr = dest.ToInt32 ();
			for (int i = 0; i < 16; i++) {
				Marshal.StructureToPtr (this, (IntPtr)ptr, false);
				ptr += 4;
			}
		}

		private void GoodLoop (IntPtr dest)
		{
			long ptr = dest.ToInt64 ();
			for (int i = 0; i < 16; i++) {
				Marshal.StructureToPtr (this, (IntPtr) ptr, false);
				ptr += 4;
			}
		}

		[Test]
		public void Convert ()
		{
			AssertRuleFailure<DoNotAssumeIntPtrSizeTest> ("BadLoop", 1);
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("GoodLoop");
		}

		public override bool Equals (object obj)
		{
			return base.Equals (obj);
		}

		public override int GetHashCode ()
		{
			 return base.GetHashCode();
		}

		[Test]
		public void DoesNotApply ()
		{
			// no IL for p/invokes
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			// rule does not apply to GetHashCode
			AssertRuleDoesNotApply<DoNotAssumeIntPtrSizeTest> ("GetHashCode");
		}

		// adapted from System.Drawing/System.Drawing.Printing/PrintingServicesUnix.cs
		public void ReadInt32 (IntPtr p)
		{
			for (int i = 0; i < 1000; i++) {
				// that won't work on 64 bits platforms
				p = (IntPtr) Marshal.ReadInt32 (p);
			}
		}

		public void ReadInt64 (IntPtr p)
		{
			for (int i = 0; i < 1000; i++) {
				// that won't work on 32 bits platforms
				p = (IntPtr) Marshal.ReadInt64 (p);
			}
		}

		public void ReadIntPtr (IntPtr p)
		{
			for (int i = 0; i < 1000; i++) {
				p = (IntPtr) Marshal.ReadIntPtr (p);
			}
		}
	
		[Test]
		public void MarshalRead ()
		{
			AssertRuleFailure<DoNotAssumeIntPtrSizeTest> ("ReadInt32", 1);
			AssertRuleFailure<DoNotAssumeIntPtrSizeTest> ("ReadInt64", 1);
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("ReadIntPtr");
		}

		public void WriteInt32 (IntPtr p)
		{
			for (int i = 0; i < 1000; i++) {
				// that won't work on 64 bits platforms
				Marshal.WriteInt32 (p, (int) p);
			}
		}

		public void WriteInt64 (IntPtr p)
		{
			// that will work on both 32/64 platform (even if its not the preferred way)
			Marshal.WriteInt64 (p, (long) p);
		}

		public void WriteIntPtr (IntPtr p)
		{
			Marshal.WriteIntPtr (p, p);
		}

		[Test]
		public void MarshalWrite ()
		{
			AssertRuleFailure<DoNotAssumeIntPtrSizeTest> ("WriteInt32", 1);
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("WriteInt64");
			AssertRuleSuccess<DoNotAssumeIntPtrSizeTest> ("WriteIntPtr");
		}
	}
}
