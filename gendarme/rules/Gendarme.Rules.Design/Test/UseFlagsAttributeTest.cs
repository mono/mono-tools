// 
// Unit tests for UseFlagsAttributeRule
//
// Authors:
//	Jesse Jones  <jesjones@mindspring.com>
//
// Copyright (C) 2009
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Rules.Design;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Design {

	[TestFixture]
	public sealed class UseFlagsAttributeTest : TypeRuleTestFixture<UseFlagsAttributeRule> {

		[Flags]		
		private enum NotApplicable1 {	// has FlagsAttribute
			One = 1,
			Two = 2,
			Four = 4,
		}
	
		private enum Good1 {	// not enough non-zero values
			Zero,
			One,
			Two,
		}
	
		private enum Good2 {	// values are sequential
			One = 1,
			Two = 2,
			Three = 3,
		}
		
		private enum Good3 {	// values are sequential
			One = 1,
			Two = 2,
			Three = 3,
			Four = 4,
			Five = 5,
			Six = 6,
		}
		
		private enum Good4 {	// last value is not a bitmask
			One = 1,
			Two = 2,
			Four = 4,
			Fifteen = 15,
		}

		private enum Good5 {	// if a value is negative we assume it can't be a flag
			One = 1,
			Two = 2,
			Four = 4,
			MinusOne = -1,
		}

		private enum Good6 {	// sequential (with duplicate values)
			UIS_SET        = 1,
			UIS_CLEAR      = 2,
			UIS_INITIALIZE = 3,
			UISF_HIDEFOCUS = 0x1,
			UISF_HIDEACCEL = 0x2,
			UISF_ACTIVE    = 0x4
		}

		private enum Bad1 {	
			One = 1,
			Two = 2,
			Four = 4,
		}
	
		private enum Bad2 {	
			One = 1,
			Four = 4,
			Two = 2,
		}
	
		private enum Bad3 {	
			One = 1,
			Two = 2,
			Four = 4,
			AliasedFour = 4,
		}
	
		private enum Bad4 {	
			One = 1,
			Two = 2,
			Four = 4,
			All = One | Two | Four,
		}

		private enum Bad5 : long {	// long is a bit of a special case for the rule
			One = 1,
			Four = 4,
			Two = 2,
		}

		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Delegate);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
			AssertRuleDoesNotApply<NotApplicable1> ();
		}

		[Test]
		public void Cases ()
		{
			AssertRuleSuccess<Good1> ();
			AssertRuleSuccess<Good2> ();
			AssertRuleSuccess<Good3> ();
			AssertRuleSuccess<Good4> ();
			AssertRuleSuccess<Good5> ();
			AssertRuleSuccess<Good6> ();

			AssertRuleFailure<Bad1> ();
			AssertRuleFailure<Bad2> ();
			AssertRuleFailure<Bad3> ();
			AssertRuleFailure<Bad4> ();
			AssertRuleFailure<Bad5> ();
		}
	}
}
