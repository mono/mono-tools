//
// Unit Test for UsePreferredTermsRule
//
// Authors:
//      Abramov Daniel <ex@vingrad.ru>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2007 Abramov Daniel
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
using System.Reflection;

using Gendarme.Rules.Naming;
using Mono.Cecil;

using NUnit.Framework;
using Test.Rules.Fixtures;

// 5 bad terms are used in this namespace
// note: ShouldntBe needs to be in one word (no dot) otherwise UseCoorectCasingRule will spot it (same assembly)
namespace Cancelled.ComPlus.Indices.ShouldntBe.Writeable {

	public class WouldntLogOutOrSignOff {

		public bool WontAllowIt;

		public bool arentBad;

		public bool AccessWerentLogged {
			get { return true; }
		}

		internal void HadntThinkOfIt ()
		{
		}

		void CantArgueAboutThat ()
		{
		}
	}

	abstract public class CouldntLogInOrSignOn {

		WouldntLogOutOrSignOff DoesntMatterAnymore;

		abstract public bool WasntHere { get; }

		internal void DontTryThisAtHome ()
		{
		}

		public event EventHandler<EventArgs> HaventVoted;
	}
}

namespace Test.Rules.Naming {

	[TestFixture]
	public class UsePreferredTermsAssemblyTest : AssemblyRuleTestFixture<UsePreferredTermsRule> {

		AssemblyDefinition assembly;
		bool parentMatches;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			string unit = Assembly.GetExecutingAssembly ().Location;
			assembly = AssemblyDefinition.ReadAssembly (unit);
		}

		[Test]
		public void AssemblyName ()
		{
			string name = assembly.Name.Name;
			try {
				assembly.Name.Name = "This.Isnt.A.Nice.Assembly.Name";
				// bad name and one bad namespace with 5 failures
				AssertRuleFailure (assembly, 6);
			}
			finally {
				assembly.Name.Name = name;
			}
		}

		[Test]
		public void Namespace ()
		{
			// Type_With_Underscore
			AssertRuleFailure (assembly, 5);
		}
	}

	[TestFixture]
	public class UsePreferredTermsTypeTest : TypeRuleTestFixture<UsePreferredTermsRule> {

		[Test]
		public void Bad ()
		{
			// 3 failures in the type name, 2 in fields (including one event)
			AssertRuleFailure<Cancelled.ComPlus.Indices.ShouldntBe.Writeable.CouldntLogInOrSignOn> (5);
			// 3 failures in the type name, 2 in a field
			AssertRuleFailure<Cancelled.ComPlus.Indices.ShouldntBe.Writeable.WouldntLogOutOrSignOff> (5);
		}

		[Test]
		public void Good ()
		{
			// good type name and good fields names
			AssertRuleSuccess<UsePreferredTermsAssemblyTest> ();
		}
	}

	[TestFixture]
	public class UsePreferredTermsMethodTest : MethodRuleTestFixture<UsePreferredTermsRule> {

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<Cancelled.ComPlus.Indices.ShouldntBe.Writeable.WouldntLogOutOrSignOff> ("get_AccessWerentLogged", 1);
			AssertRuleFailure<Cancelled.ComPlus.Indices.ShouldntBe.Writeable.WouldntLogOutOrSignOff> ("HadntThinkOfIt", 1);
			AssertRuleFailure<Cancelled.ComPlus.Indices.ShouldntBe.Writeable.WouldntLogOutOrSignOff> ("CantArgueAboutThat", 1);

			AssertRuleFailure<Cancelled.ComPlus.Indices.ShouldntBe.Writeable.CouldntLogInOrSignOn> ("DontTryThisAtHome", 1);
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<UsePreferredTermsMethodTest> ("Bad");
			AssertRuleSuccess<UsePreferredTermsMethodTest> ("GetParentFoo");
		}

		private void GetParentFoo () {
		}
	}
}
