//
// Gendarme.Rules.Design.DoNotDeclareProtectedFieldsInSealedTypeRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule ensures that <c>sealed</c> types (i.e. types that you can't inherit from) 
	/// do not define family (<c>protected</c> in C#) fields or methods. Instead make the
	/// member private so that its accessibility is not misleading. 
	/// </summary>
	/// <example>
	/// Bad example (field):
	/// <code>
	/// public sealed class MyClass {
	///	protected int someValue;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (method):
	/// <code>
	/// public sealed class MyClass {
	///	protected int GetAnswer ()
	///	{
	///		return 42;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (field):
	/// <code>
	/// public sealed class MyClass {
	///	private int someValue; 
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example (method):
	/// <code>
	/// public sealed class MyClass {
	///	private int GetAnswer ()
	///	{
	///		return 42;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>Prior to Gendarme 2.2 this rule applied only to fields and was named DoNotDeclareProtectedFieldsInSealedClassRule</remarks>

	[Problem ("This sealed type contains family (protected in C#) fields and/or methods.")]
	[Solution ("Change the access to private to make it clear that the type is not intended to be subclassed.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1047:DoNotDeclareProtectedMembersInSealedTypes")]
	public class DoNotDeclareProtectedMembersInSealedTypeRule: Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to sealed types, but not to enum, value types and delegate (sealed)
			if (!type.IsSealed || type.IsEnum || type.IsValueType || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				if (field.IsFamily) {
					Runner.Report (field, Severity.Low, Confidence.Total);
				}
			}

			foreach (MethodDefinition method in type.Methods) {
				if (method.IsFamily) {
					// make sure it's not an override of an ancestor
					if (!method.IsOverride ())
						Runner.Report (method, Severity.Low, Confidence.Total);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
