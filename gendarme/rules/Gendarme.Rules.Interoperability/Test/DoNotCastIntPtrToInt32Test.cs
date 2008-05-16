//
// Unit Tests for DoNotCastIntPtrToInt32Rule.
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

#pragma warning disable 169

namespace Test.Rules.Interoperability {
	
	[TestFixture]
	public unsafe class DoNotCastIntPtrToInt32Test : MethodRuleTestFixture<DoNotCastIntPtrToInt32Rule> {

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
			AssertRuleFailure<DoNotCastIntPtrToInt32Test> ("CastIntPtrToInt32", 1);
			AssertRuleFailure<DoNotCastIntPtrToInt32Test> ("CastIntPtrToUInt32", 1);
			AssertRuleSuccess<DoNotCastIntPtrToInt32Test> ("CastIntPtrToInt64");
			AssertRuleSuccess<DoNotCastIntPtrToInt32Test> ("CastIntPtrToUInt64");
			AssertRuleSuccess<DoNotCastIntPtrToInt32Test> ("CastIntPtrToIntPtr");
			AssertRuleSuccess<DoNotCastIntPtrToInt32Test> ("CastIntPtrToVoidPointer");
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
			AssertRuleFailure<DoNotCastIntPtrToInt32Test> ("CastUIntPtrToInt32", 1);
			AssertRuleFailure<DoNotCastIntPtrToInt32Test> ("CastUIntPtrToUInt32", 1);
			AssertRuleSuccess<DoNotCastIntPtrToInt32Test> ("CastUIntPtrToInt64");
			AssertRuleSuccess<DoNotCastIntPtrToInt32Test> ("CastUIntPtrToUInt64");
			AssertRuleSuccess<DoNotCastIntPtrToInt32Test> ("CastUIntPtrToIntPtr");
			AssertRuleSuccess<DoNotCastIntPtrToInt32Test> ("CastUIntPtrToVoidPointer");
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
			AssertRuleFailure<DoNotCastIntPtrToInt32Test> ("BadLoop", 1);
			AssertRuleSuccess<DoNotCastIntPtrToInt32Test> ("GoodLoop");
		}
	}
}
