//
// Unit tests for UseGenericEventHandlerRule
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
using Mono.Cecil;

using Gendarme.Rules.Design.Generic;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Design.Generic {

	[TestFixture]
	public class UseGenericEventHandlerTest : TypeRuleTestFixture<UseGenericEventHandlerRule> {

		[Test]
		public void NotApplicable ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Class);
			AssertRuleDoesNotApply (SimpleTypes.Enum);
			AssertRuleDoesNotApply (SimpleTypes.Interface);
			AssertRuleDoesNotApply (SimpleTypes.Structure);
		}
		
		[Test]
		public void NotApplicableBefore2_0 ()
		{
			// ensure that the rule does not apply for types defined in 1.x assemblies
			TypeDefinition violator = DefinitionLoader.GetTypeDefinition<UseGenericEventHandlerRule> ();
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

		public delegate void Bad (object sender, EventArgs e);

		public delegate void BadInherited (object sender, AssemblyLoadEventArgs e);
		
		[Test]
		public void BadDelegates ()
		{
			AssertRuleFailure<Bad> (1);
			AssertRuleFailure<BadInherited> (1);
		}

		public delegate bool WrongSignatureReturnValue (object sender, EventArgs e);
		public delegate void WrongSignatureNoObject (string sender, EventArgs e);
		public delegate void WrongSignatureExtraParameter (object sender, string name, EventArgs e);
		public delegate void WrongSignatureNoEventArgs (object sender, object e);
		
		[Test]
		public void GenericInterfaceNotImplementedReturnsFailure ()
		{
			AssertRuleSuccess<WrongSignatureReturnValue> ();
			AssertRuleSuccess<WrongSignatureNoObject> ();
			AssertRuleSuccess<WrongSignatureExtraParameter> ();
			AssertRuleSuccess<WrongSignatureNoEventArgs> ();
		}
	}
}
