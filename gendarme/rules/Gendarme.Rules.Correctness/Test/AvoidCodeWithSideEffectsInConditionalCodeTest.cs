//
// Unit test for AvoidCodeWithSideEffectsInConditionalCodeRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// 	(C) 2009 Jesse Jones
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

using System.Collections.Generic;
using System.Diagnostics;

using Mono.Cecil;
using Gendarme.Framework.Rocks;
using Gendarme.Rules.Correctness;

using NUnit.Framework;
using Test.Rules.Definitions;
using Test.Rules.Fixtures;
using Test.Rules.Helpers;

namespace Test.Rules.Correctness {
	[TestFixture]
	public class AvoidCodeWithSideEffectsInConditionalCodeTest : MethodRuleTestFixture<AvoidCodeWithSideEffectsInConditionalCodeRule> {
		
		internal sealed class TestCases {
			// Anything can be used with non-conditionally compiled methods.
			public void Good1 (int data)
			{
				NonConditionalCall (++data == 1);
				NonConditionalCall (data = 100);
			}
			
			// Most expressions are OK with conditional code.
			public void Good2 (int data)
			{
				ConditionalCall (data + 1);
				ConditionalCall (data > 0 ? 100 : 2);
				ConditionalCall (new string ('x', 32));
				ConditionalCall ("data " + data);
				
				data = 100;
				ConditionalCall (data);
				
				++data;
				ConditionalCall (data);
			}
			
			// Increment, decrement, and assign can't be used.
			public void Bad1 (int data)
			{
				ConditionalCall (++data);
				ConditionalCall (data++);
				
				ConditionalCall (--data);
				ConditionalCall (data--);
				
				ConditionalCall (data = 10);
			}
			
			// Can't write to locals.
			public void Bad2 (Dictionary<int, string> d)
			{
				string local;
				if (!d.TryGetValue (1, out local))
					local = "foo";
					
				ConditionalCall (local = "bar");
			}
			
			// Can't write to instance fields.
			public void Bad3 ()
			{
				ConditionalCall (instance_data = 10);
			}
			
			// Can't write to static fields.
			public void Bad4 ()
			{
				ConditionalCall (class_data = 10);
			}
			
			[Conditional ("DEBUG")]
			public void ConditionalCall (object data)
			{
			}
			
			public void NonConditionalCall (object data)
			{
			}
			
			private int instance_data;
			private static int class_data;
		}
		
		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
		}
		
		[Test]
		public void Cases ()
		{
			AssertRuleSuccess<TestCases> ("Good1");
			AssertRuleSuccess<TestCases> ("Good2");
			
			AssertRuleFailure<TestCases> ("Bad1", 5);

			// Bad2 can "avoid to fail" if debugging symbols are not found - that includes not being 
			// able to load Mono.Cecil.Pdb.dll (on Windows / CSC) or Mono.Cecil.Mdb.dll (xMCS)
			MethodDefinition md = DefinitionLoader.GetMethodDefinition<TestCases> ("Bad2");
			if (md.DeclaringType.Module.HasSymbols)
				AssertRuleFailure (md);

			AssertRuleFailure<TestCases> ("Bad3");
			AssertRuleFailure<TestCases> ("Bad4");
		}
	}
}
