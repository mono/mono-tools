// 
// Test.Rules.Gendarme.MissingEngineDependencyTest
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

using Mono.Cecil;
using Gendarme.Rules.Gendarme;
using Gendarme.Framework;
using Gendarme.Framework.Engines;

using NUnit.Framework;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;
using Test.Rules.Definitions;


namespace Test.Rules.Gendarme {

	[TestFixture]
	public class MissingEngineDependencyTest : TypeRuleTestFixture<MissingEngineDependencyRule> {

		class GoodNoEngines : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				int a = 0;
				Console.WriteLine (a.ToString ());
				return RuleResult.Success;
			}
		}

		[EngineDependency (typeof (OpCodeEngine))]
		class GoodEngineType : Rule, IMethodRule { 
			public RuleResult CheckMethod (MethodDefinition method)
			{
				var a = OpCodeEngine.GetBitmask (SimpleMethods.EmptyMethod);
				return RuleResult.Success;
			}
		}

		[EngineDependency ("Gendarme.Framework.Engines.OpCodeEngine")]
		class GoodEngineString : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				var a = OpCodeEngine.GetBitmask (SimpleMethods.EmptyMethod);
				return RuleResult.Success;
			}
		}

		[EngineDependency ("Gendarme.Framework.Engines.OpCodeEngine")]
		[EngineDependency (typeof (NamespaceEngine))]
		class GoodOpCodeAndNamespaceEngines : Rule, IMethodRule {
			public RuleResult CheckMethod (MethodDefinition method)
			{
				var a = OpCodeEngine.GetBitmask (SimpleMethods.EmptyMethod);
				var b = NamespaceEngine.Exists ("TestNamespace");
				return RuleResult.Success;
			}
		}

		class BadNoEngineDefined {
			public void DoSomething ()
			{
				var a = OpCodeEngine.GetBitmask (SimpleMethods.EmptyMethod);
				var b = NamespaceEngine.Exists ("TestNamespace");
			}
		}

		[EngineDependency (typeof (OpCodeEngine))]
		class BadOnlyOneEngine {
			public void DoSomething ()
			{
				var a = OpCodeEngine.GetBitmask (SimpleMethods.EmptyMethod);
				var b = NamespaceEngine.Exists ("TestNamespace");
			}
		}

		class GoodInherited : GoodEngineType {
			public void DoSomethingNew ()
			{
				var a = OpCodeEngine.GetBitmask (SimpleMethods.EmptyMethod);
			}
		}

		class BadOnlyOneInherited : GoodEngineType {
			public void DoSomethingNew ()
			{
				var a = OpCodeEngine.GetBitmask (SimpleMethods.EmptyMethod);
				var b = NamespaceEngine.Exists ("TestNamespace");
			}
		}



		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleTypes.Enum); // no methods
		}

		[Test]
		public void Good ()
		{
			AssertRuleSuccess<GoodEngineType> ();
			AssertRuleSuccess<GoodEngineString> ();
			AssertRuleSuccess<GoodNoEngines> ();
			AssertRuleSuccess<GoodOpCodeAndNamespaceEngines> ();
			AssertRuleSuccess<GoodInherited> ();
		}

		[Test]
		public void Bad ()
		{
			AssertRuleFailure<BadNoEngineDefined> (2);
			AssertRuleFailure<BadOnlyOneEngine> (1);
			AssertRuleFailure<BadOnlyOneInherited> (1);
		}
	}
}
