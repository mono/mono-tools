//
// Test.Rules.Fixtures.MethodRuleTestFixture<TRule>
// Base class for method rule test fixtures that simplifies the process of writing unit tests for Gendarme.
//
// Authors:
//	Daniel Abramov <ex@vingrad.ru>
//
// Copyright (C) 2008 Daniel Abramov
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

using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Test.Rules.Helpers;

using Mono.Cecil;

namespace Test.Rules.Fixtures {
	
	/// <summary>
	/// Abstract class providing various helper methods that method test fixtures should inherit from.
	/// </summary>
	/// <typeparam name="TMethodRule">Type of rule to be tested.</typeparam>
	public abstract class MethodRuleTestFixture<TMethodRule> : RuleTestFixture<TMethodRule, MethodDefinition>
		where TMethodRule : IMethodRule, new () {

		/// <summary>
		/// Asserts that the rule does not apply to all methods of the type. 
		/// </summary>
		/// <typeparam name="T">Type containing the methods.</typeparam>
		protected void AssertRuleDoesNotApply<T> ()
		{
			foreach (MethodDefinition method in DefinitionLoader.GetTypeDefinition<T> ().Methods)
				base.AssertRuleDoesNotApply (method);
		}
		
		/// <summary>
		/// Asserts that the rule does not apply to the method. 
		/// </summary>
		/// <param name="method">Method to check.</param>
		/// <typeparam name="T">Type containing the method.</typeparam>
		protected void AssertRuleDoesNotApply<T> (string method)
		{
			base.AssertRuleDoesNotApply (DefinitionLoader.GetMethodDefinition<T> (method));
		}
		
		/// <summary>
		/// Asserts that the rule does not apply to the method. 
		/// </summary>
		/// <typeparam name="T">Type containing the method to test.</typeparam>
		/// <param name="method">Method name.</param>
		/// <param name="parameters">Parameter types.</param>
		protected void AssertRuleDoesNotApply<T> (string method, Type [] parameters)
		{
			base.AssertRuleDoesNotApply (DefinitionLoader.GetMethodDefinition<T> (method, parameters));
		}

		protected void AssertRuleDoesNotApply (Type type, string method)
		{
			TypeDefinition td = DefinitionLoader.GetTypeDefinition (type);
			base.AssertRuleDoesNotApply (DefinitionLoader.GetMethodDefinition (td, method, null));
		}
		
		/// <summary>
		/// Asserts that the rule has been executed successfully for each method in the type. 
		/// </summary>
		/// <typeparam name="T">Type containing the methods.</typeparam>
		protected void AssertRuleSuccess<T> ()
		{
			foreach (MethodDefinition method in DefinitionLoader.GetTypeDefinition<T> ().Methods)
				base.AssertRuleSuccess (method);
		}
		
		/// <summary>
		/// Asserts that the rule has been executed successfully. 
		/// </summary>
		/// <typeparam name="T">Type containing the method to test.</typeparam>
		/// <param name="method">Method name.</param>
		protected void AssertRuleSuccess<T> (string method)
		{
			base.AssertRuleSuccess (DefinitionLoader.GetMethodDefinition<T> (method));
		}

		/// <summary>		
		/// Asserts that the rule has been executed successfully. 
		/// </summary>
		/// <typeparam name="T">Type containing the method to test.</typeparam>
		/// <param name="method">Method name.</param>
		/// <param name="parameters">Parameter types.</param>
		protected void AssertRuleSuccess<T> (string method, Type [] parameters)
		{
			base.AssertRuleSuccess (DefinitionLoader.GetMethodDefinition<T> (method, parameters));
		}

		protected void AssertRuleSuccess (Type type, string method)
		{
			TypeDefinition td = DefinitionLoader.GetTypeDefinition (type);
			base.AssertRuleSuccess (DefinitionLoader.GetMethodDefinition (td, method, null));
		}

		/// <summary>
		/// Asserts that the rule has failed to execute successfully for each method in the type. 
		/// </summary>
		/// <typeparam name="T">Type containing the methods.</typeparam>
		protected void AssertRuleFailure<T> ()
		{
			foreach (MethodDefinition method in DefinitionLoader.GetTypeDefinition<T> ().Methods)
				base.AssertRuleFailure (method);
		}
		
		/// <summary>
		/// Asserts that the rule has failed to execute successfully for each method in the type. 
		/// </summary>
		/// <typeparam name="T">Type containing the methods.</typeparam>
		/// <param name="expectedCount">Expected defect count for each method.</param>
		protected void AssertRuleFailure<T> (int expectedCount)
		{
			foreach (MethodDefinition method in DefinitionLoader.GetTypeDefinition<T> ().Methods)
				base.AssertRuleFailure (method, expectedCount);
		}
		
		/// <summary>
		/// Asserts that the rule has failed to execute successfully. 
		/// </summary>
		/// <typeparam name="T">Type containing the method to test.</typeparam>
		/// <param name="method">Method name.</param>
		protected void AssertRuleFailure<T> (string method)
		{
			base.AssertRuleFailure (DefinitionLoader.GetMethodDefinition<T> (method));
		}
		
		/// <summary>
		/// Asserts that the rule has failed to execute successfully. 
		/// </summary>
		/// <typeparam name="T">Type containing the method to test.</typeparam>
		/// <param name="method">Method name.</param>
		/// <param name="expectedCount">Expected message count.</param>
		protected void AssertRuleFailure<T> (string method, int expectedCount)
		{
			base.AssertRuleFailure (DefinitionLoader.GetMethodDefinition<T> (method), expectedCount);
		}

		/// <summary>
		/// Asserts that the rule has failed to execute successfully. 
		/// </summary>
		/// <typeparam name="T">Type containing the method to test.</typeparam>
		/// <param name="method">Method name.</param>
		/// <param name="parameters">Parameter types.</param>
		protected void AssertRuleFailure<T> (string method, Type [] parameters)
		{
			base.AssertRuleFailure (DefinitionLoader.GetMethodDefinition<T> (method, parameters));
		}

		/// <summary>
		/// Asserts that the rule has failed to execute successfully. 
		/// </summary>
		/// <typeparam name="T">Type containing the method to test.</typeparam>
		/// <param name="method">Method name.</param>
		/// <param name="parameters">Parameter types.</param>
		/// <param name="expectedCount">Expected message count.</param>
		protected void AssertRuleFailure<T> (string method, Type [] parameters, int expectedCount)
		{
			base.AssertRuleFailure (DefinitionLoader.GetMethodDefinition<T> (method, parameters), expectedCount);
		}

		protected void AssertRuleFailure (Type type, string method, int expectedCount)
		{
			TypeDefinition td = DefinitionLoader.GetTypeDefinition (type);
			base.AssertRuleFailure (DefinitionLoader.GetMethodDefinition (td, method, null), expectedCount);
		}
	}
}
