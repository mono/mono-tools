// 
// Tests.Rules.Interoperability.Com.ReviewComRegistrationMethodsTest
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
	public class ReviewComRegistrationMethodsTest : TypeRuleTestFixture<ReviewComRegistrationMethodsRule> {

		[ComVisible(false)]
		public class ComInvisible {
			[ComRegisterFunction]
			public void Register ()
			{
			}
		}

		[ComVisible (true)]
		private class ExternallyInvisible {
			[ComRegisterFunction]
			public void Register ()
			{
			}
		}

		[ComVisible (true)]
		public class GoodCase {
			[ComRegisterFunction]
			private void Register ()
			{
			}

			[ComUnregisterFunction]
			internal void Unregister ()
			{
			}
		}

		[ComVisible (true)]
		public class BadVisibleRegistrators {
			[ComRegisterFunction]
			public void Register ()
			{
			}

			[ComUnregisterFunction]
			public void Unregister ()
			{
			}
		}


		[ComVisible (true)]
		public class BadOnlyUnregister {
			[ComUnregisterFunction]
			private void Unregister ()
			{
			}
		}

		[ComVisible (true)]
		public class BadOnlyPublicUnregister {
			[ComUnregisterFunction]
			public void Unregister ()
			{
			}
		}

		[ComVisible (true)]
		public class BadOnlyRegister {
			[ComRegisterFunction]
			private void Register ()
			{
			}
		}

		[ComVisible (true)]
		public class BadOnlyPublicRegister {
			[ComRegisterFunction]
			public void Register ()
			{
			}
		}


		[ComVisible (true)]
		public class BadPublicRegisterPrivateUnregister {
			[ComRegisterFunction]
			public void Register ()
			{
			}

			[ComUnregisterFunction]
			private void Unregister ()
			{
			}
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<ComInvisible> ();
			AssertRuleDoesNotApply<ExternallyInvisible> ();
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodCase> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadVisibleRegistrators> (2);
			AssertRuleFailure<BadOnlyUnregister> (1);
			AssertRuleFailure<BadOnlyPublicUnregister> (2);
			AssertRuleFailure<BadOnlyRegister> (1);
			AssertRuleFailure<BadOnlyPublicRegister> (2);
			AssertRuleFailure<BadPublicRegisterPrivateUnregister> (1);
		}
	}
}
