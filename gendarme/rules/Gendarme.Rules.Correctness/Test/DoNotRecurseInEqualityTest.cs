//
// Unit tests for DoNotRecurseInEqualityRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//
// Copyright (C) 2008 Jesse Jones
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
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

using Gendarme.Rules.Correctness;
using Mono.Cecil;
using NUnit.Framework;

using Test.Rules.Definitions;
using  Test.Rules.Helpers;
using Test.Rules.Fixtures;

namespace Test.Rules.Correctness {

	[TestFixture]
	public class DoNotRecurseInEqualityTest : MethodRuleTestFixture<DoNotRecurseInEqualityRule> {

		private sealed class GoodCase {
			public static bool operator== (GoodCase lhs, GoodCase rhs)
			{
				if (object.ReferenceEquals (lhs, rhs))
					return true;
			
				if ((object) lhs == null || (object) rhs == null)
					return false;
			
				return lhs.Name == rhs.Name && lhs.Address == rhs.Address;
			}

			public static bool operator!= (GoodCase lhs, GoodCase rhs)
			{
				return !(lhs == rhs);
			}
			
			public void Recurses ()
			{
				Recurses();			// this belongs to BadRecursiveInvocationRule, not DoNotRecurseInEqualityRule
			}
			
			public string Name {get; set;}
			public string Address {get; set;}
		}

		private sealed class BadEquality {
			public static bool operator== (BadEquality lhs, BadEquality rhs)
			{
				if (object.ReferenceEquals (lhs, rhs))
					return true;
			
				if (lhs == null || rhs == null)
					return false;
			
				return lhs.Name == rhs.Name && lhs.Address == rhs.Address;
			}

			public static bool operator!= (BadEquality lhs, BadEquality rhs)
			{
				return !(lhs == rhs);
			}
			
			public string Name {get; set;}
			public string Address {get; set;}
		}
		
		private sealed class BadInequality {
			public static bool operator== (BadInequality lhs, BadInequality rhs)
			{
				if (object.ReferenceEquals (lhs, rhs))
					return true;
			
				if ((object) lhs == null || (object) rhs == null)
					return false;
			
				return lhs.Name == rhs.Name && lhs.Address == rhs.Address;
			}

			public static bool operator!= (BadInequality lhs, BadInequality rhs)
			{
				if (object.ReferenceEquals (lhs, rhs))
					return false;
			
				if (lhs != null && rhs != null)
					return true;
			
				return lhs.Name != rhs.Name || lhs.Address != rhs.Address;
			}
			
			public string Name {get; set;}
			public string Address {get; set;}
		}

		private sealed class BadGenericEquality<T> {
			public static bool operator== (BadGenericEquality<T> lhs, BadGenericEquality<T> rhs)
			{
				if (object.ReferenceEquals (lhs, rhs))
					return true;
			
				if (lhs == null || rhs == null)
					return false;
			
				return lhs.Name.Equals(rhs.Name) && lhs.Address.Equals(rhs.Address);
			}

			public static bool operator!= (BadGenericEquality<T> lhs, BadGenericEquality<T> rhs)
			{
				return !(lhs == rhs);
			}
			
			public T Name {get; set;}
			public T Address {get; set;}
		}
		
		[Test]
		public void DoesNotApply ()
		{
			AssertRuleDoesNotApply (SimpleMethods.ExternalMethod);
			AssertRuleDoesNotApply (SimpleMethods.EmptyMethod);
			AssertRuleDoesNotApply<GoodCase> ("Recurses");
		}
		
		[Test]
		public void Test ()	
		{
			AssertRuleSuccess<GoodCase> ("op_Equality");
			AssertRuleSuccess<GoodCase> ("op_Inequality");

			AssertRuleFailure<BadEquality> ("op_Equality", 2);
			AssertRuleSuccess<BadEquality> ("op_Inequality");

			AssertRuleSuccess<BadInequality> ("op_Equality");
			AssertRuleFailure<BadInequality> ("op_Inequality", 2);
		}		

		[Test]
		public void GenericsTest ()	
		{
			Type type = typeof (BadGenericEquality<>);
			TypeDefinition td = DefinitionLoader.GetTypeDefinition (type);
			
			MethodDefinition md = DefinitionLoader.GetMethodDefinition (td, "op_Equality", null);
			AssertRuleFailure (md, 2);

			md = DefinitionLoader.GetMethodDefinition (td, "op_Inequality", null);
			AssertRuleSuccess (md);
		}		
	}
}
