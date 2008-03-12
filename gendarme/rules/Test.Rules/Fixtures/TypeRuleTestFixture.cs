//
// Test.Rules.Fixtures.TypeRuleTestFixture<T>
// Base class for type rule test fixtures that simplifies writing unit tests for Gendarme.
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
//  (C) 2008 Daniel Abramov
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

using Gendarme.Framework;
using Test.Rules.Helpers;

using Mono.Cecil;

namespace Test.Rules.Fixtures {
	
	/// <summary>
	/// This class should be inherited by type rule testing fixtures.
	/// </summary>	/// <typeparam name="TTypeRule">Type of rule to be tested.</typeparam>	public abstract class TypeRuleTestFixture<TTypeRule> : RuleTestFixture<TTypeRule, TypeDefinition>		where TTypeRule : ITypeRule, new () {
		
		/// <summary>		/// Asserts that the rule does not apply to the type.		/// </summary>		/// <typeparam name="T">Type to check.</typeparam>		protected void AssertRuleDoesNotApply<T> ()		{			base.AssertRuleDoesNotApply (DefinitionLoader.GetTypeDefinition<T> ());		}
	
		/// <summary>
		/// Asserts that the rule has been executed successfully. 
		/// </summary>
		/// <typeparam name="T">Type to check.</typeparam>
		protected void AssertRuleSuccess<T> ()
		{
			base.AssertRuleSuccess (DefinitionLoader.GetTypeDefinition<T> ());
		}
		
		/// <summary>
		/// Asserts that the rule has failed to execute successfully. 
		/// </summary>
		/// <typeparam name="T">Type to check.</typeparam>
		protected void AssertRuleFailure<T> ()
		{
			base.AssertRuleFailure (DefinitionLoader.GetTypeDefinition<T> ());
		}

		/// <summary>
		/// Asserts that the rule has failed to execute successfully. 
		/// </summary>
		/// <param name="expectedCount">Expected defects count.</param>
		/// <typeparam name="T">Type to check.</typeparam>
		protected void AssertRuleFailure<T> (int expectedCount)
		{
			base.AssertRuleFailure (DefinitionLoader.GetTypeDefinition<T> (), expectedCount);
		}
	}
}