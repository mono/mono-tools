//
// Unit tests for AvoidDeclaringCustomDelegatesRule
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
using Mono.Cecil;

using Gendarme.Rules.Design.Generic;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Design.Generic {

	[TestFixture]
	public class AvoidDeclaringCustomDelegatesTest : TypeRuleTestFixture<AvoidDeclaringCustomDelegatesRule> {

		[Test]
		public void NotApplicableBefore2_0 ()
		{
			// ensure that the rule does not apply for types defined in 1.x assemblies
			TypeDefinition violator = DefinitionLoader.GetTypeDefinition<AvoidDeclaringCustomDelegatesRule> ();
			TargetRuntime realRuntime = violator.Module.Runtime;
			try {

				// fake assembly runtime version and do the check
				violator.Module.Runtime = TargetRuntime.Net_1_1;
				Rule.Active = true;
				Rule.Initialize (Runner);
				Assert.IsFalse (Rule.Active, "Active");
			}
			catch {
				// rollback
				violator.Module.Runtime = realRuntime;
				Rule.Active = true;
			}
		}

		[Test]
		public void NotApplicable ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}

		delegate void VoidNoParameter ();
		delegate void VoidOneParameter (int a);
		delegate void VoidTwoParameters (int a, string b);
		delegate void VoidThreeParameters (int a, string b, object c);
		delegate void VoidFourParameters (int a, string b, object c, float d);
		delegate void Void17Parameters (int a, string b, object c, float d, double e, short f, byte g, long h,
			int i, string j, object k, float l, double m, short n, byte o, long p, DateTime q);

		[Test]
		public void ReplaceWithAction ()
		{
			AssertRuleFailure<VoidNoParameter> ();
			AssertRuleFailure<VoidOneParameter> ();
			AssertRuleFailure<VoidTwoParameters> ();
			AssertRuleFailure<VoidThreeParameters> ();
			AssertRuleFailure<VoidFourParameters> ();
			// we don't test 5-16 since they will be valid for NET_4_0
			// having more than 16 parameters is another issue (unrelated to this rule)
			AssertRuleFailure<Void17Parameters> ();
		}

		delegate int NonVoidNoParameter ();
		delegate int NonVoidOneParameter (int a);
		delegate int NonVoidTwoParameters (int a, string b);
		delegate int NonVoidThreeParameters (int a, string b, object c);
		delegate int NonVoidFourParameters (int a, string b, object c, float d);
		delegate long NonVoid17Parameters (int a, string b, object c, float d, double e, short f, byte g, long h,
			int i, string j, object k, float l, double m, short n, byte o, long p, DateTime q);

		[Test]
		public void ReplaceWithFunc ()
		{
			AssertRuleFailure<NonVoidNoParameter> ();
			AssertRuleFailure<NonVoidOneParameter> ();
			AssertRuleFailure<NonVoidTwoParameters> ();
			AssertRuleFailure<NonVoidThreeParameters> ();
			AssertRuleFailure<NonVoidFourParameters> ();
			// we don't test 5-16 since they will be valid for NET_4_0
			// having more than 16 parameters is another issue (unrelated to this rule)
			AssertRuleFailure<NonVoid17Parameters> ();
		}

		delegate int RefDelegate (ref int a);
		delegate void OutDelegate (out int a);
		delegate void ParamsDelegate (params object[] args);

		[Test]
		public void SpecialCases ()
		{
			AssertRuleSuccess<RefDelegate> ();
			AssertRuleSuccess<OutDelegate> ();
			AssertRuleSuccess<ParamsDelegate> ();
		}
	}
}

