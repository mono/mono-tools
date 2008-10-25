//
// Unit tests for PreferEventsOverMethodsRule
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

using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Fixtures;

namespace Test.Rules.Design {

	[TestFixture]
	public class PreferEventsOverMethodsTest : MethodRuleTestFixture<PreferEventsOverMethodsRule> {

		public class DoesNotApplyCases {
			public int AddOn {
				get { return 0; }
				set { ; }
			}

			public event EventHandler<EventArgs> RemoveOn;
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply<DoesNotApplyCases> ("get_AddOn");
			AssertRuleDoesNotApply<DoesNotApplyCases> ("set_AddOn");
			AssertRuleDoesNotApply<DoesNotApplyCases> ("add_RemoveOn");
			AssertRuleDoesNotApply<DoesNotApplyCases> ("remove_RemoveOn");
		}

		public class BadCases {

			public void AddOn ()
			{
			}

			public void RemoveOn ()
			{
			}

			public void FireMissile ()
			{
			}

			public void RaiseHell ()
			{
			}
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadCases> ("AddOn", 1);
			AssertRuleFailure<BadCases> ("RemoveOn", 1);
			AssertRuleFailure<BadCases> ("FireMissile", 1);
			AssertRuleFailure<BadCases> ("RaiseHell", 1);
		}

		public class GoodCases {

			public void GetAddOn ()
			{
			}

			public void SetRemoveOn ()
			{
			}

			public void MoonFire ()
			{
			}

			public void LowRaise ()
			{
			}
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodCases> ("GetAddOn");
			AssertRuleSuccess<GoodCases> ("SetRemoveOn");
			AssertRuleSuccess<GoodCases> ("MoonFire");
			AssertRuleSuccess<GoodCases> ("LowRaise");
		}
	}
}
