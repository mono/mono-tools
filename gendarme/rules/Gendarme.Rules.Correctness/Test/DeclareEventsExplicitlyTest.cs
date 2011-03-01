//
// Unit tests for DeclareEventsExplicitlyRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

using Gendarme.Rules.Correctness;
using NUnit.Framework;

using Test.Rules.Definitions;
using Test.Rules.Fixtures;

namespace Tests.Rules.Correctness {

	[TestFixture]
	public class DeclareEventsExplicitlyTest : TypeRuleTestFixture<DeclareEventsExplicitlyRule> {

		struct StructInstanceCorrect {
			public event EventHandler<EventArgs> MyEvent;
		}

		struct StructInstanceIncorrect {
			public EventHandler<EventArgs> MyEvent;
		}

		class GenericClassStaticCorrect {
			public static event EventHandler<EventArgs> MyEvent;
		}

		class GenericClassStaticIncorect {
			public static EventHandler<EventArgs> MyEvent;
			public event EventHandler<EventArgs> MyEvent2;
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			// interface cannot define fields
			AssertRuleDoesNotApply (SimpleTypes.Interface);
		}

		[Test]
		public void Success ()
		{
			AssertRuleSuccess<StructInstanceCorrect> ();
			AssertRuleSuccess<GenericClassStaticCorrect> ();
		}

		[Test]
		public void Failure ()
		{
			AssertRuleFailure<StructInstanceIncorrect> (1);
			AssertRuleFailure<GenericClassStaticIncorect> (1);
		}
	}
}
