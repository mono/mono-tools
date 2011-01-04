//
// Unit tests for AvoidUnusedInternalResourceRule
//
// Authors:
//	Antoine Vandecreme <ant.vand@gmail.com>
//
// Copyright (C) 2011 Antoine Vandecreme
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
using System.Reflection;

using Gendarme.Rules.Globalization;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Mono.Cecil;

namespace Tests.Rules.Globalization {

	[TestFixture]
	public sealed class AvoidUnusedInternalResourceTest : MethodRuleTestFixture<AvoidUnusedInternalResourceRule> {

		public class CallingClass {
			private void Call ()
			{
				Console.WriteLine (InternalResource.CalledString);
				Console.WriteLine (PublicResource.CalledString);

				Console.WriteLine (InternalResource.ImageUsed.Size);
			}
		}

		[Test]
		public void InternalResources ()
		{
			AssertRuleSuccess<InternalResource> ("get_CalledString");
			AssertRuleFailure<InternalResource> ("get_UncalledString");

			AssertRuleSuccess<InternalResource> ("get_ImageUsed");
			AssertRuleFailure<InternalResource> ("get_ImageUnused");
		}

		[Test]
		public void PublicResources ()
		{
			AssertRuleDoesNotApply<PublicResource> ("get_CalledString");
			AssertRuleDoesNotApply<PublicResource> ("get_UncalledString");
		}

		[Test]
		public void NotResources ()
		{
			AssertRuleDoesNotApply<CallingClass> ("Call");
		}
	}

}
