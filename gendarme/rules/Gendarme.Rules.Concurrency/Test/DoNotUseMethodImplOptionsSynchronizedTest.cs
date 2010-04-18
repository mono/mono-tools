//
// Unit tests for DoNotUseMethodImplOptionsSynchronizedRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Gendarme.Rules.Concurrency;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Concurrency {

	[TestFixture]
	public class DoNotUseMethodImplOptionsSynchronizedTest : MethodRuleTestFixture<DoNotUseMethodImplOptionsSynchronizedRule> {

		[MethodImpl (MethodImplOptions.Synchronized)]
		public void Synchronized ()
		{
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<DoNotUseMethodImplOptionsSynchronizedTest> ("Synchronized");
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		public extern void NotSynchronized ();

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<DoNotUseMethodImplOptionsSynchronizedTest> ("NotSynchronized");
		}

		public event EventHandler<EventArgs> CompilerGeneratedSynchronized;

		[Test]
		public void DoesNotApply ()
		{
			MethodDefinition md = DefinitionLoader.GetMethodDefinition<DoNotUseMethodImplOptionsSynchronizedTest> ("add_CompilerGeneratedSynchronized");
			if (!md.IsSynchronized)
				Assert.Ignore ("newer versions of CSC (e.g. 10.0) does not set the Synchronized");

			AssertRuleDoesNotApply (md);
			AssertRuleDoesNotApply<DoNotUseMethodImplOptionsSynchronizedTest> ("remove_CompilerGeneratedSynchronized");
		}
	}
}
