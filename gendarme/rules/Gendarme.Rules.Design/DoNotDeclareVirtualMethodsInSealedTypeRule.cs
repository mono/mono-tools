//
// Gendarme.Rules.Design.DoNotDeclareVirtualMethodsInSealedTypeRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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
	/// This rule ensure that <c>sealed</c> types (i.e. types that you can't inherit from) 
	/// do not define new <c>virtual</c> methods. Such methods would only be useful in
	/// sub-types. Note that some compilers, like C# and VB.NET compilers, do not allow you
	/// to define such methods.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public sealed class MyClass {
	///	// note that C# compilers won't allow this to compile
	///	public virtual int GetAnswer ()
	///	{
	///		return 42;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public sealed class MyClass {
	///	public int GetAnswer ()
	///	{
	///		return 42;
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This sealed type introduces new virtual methods.")]
	[Solution ("Change the accessibility to public or private to represent its true intended use.")]
	[FxCopCompatibility ("Microsoft.Design", "CA1048:DoNotDeclareVirtualMembersInSealedTypes")]
	public class DoNotDeclareVirtualMethodsInSealedTypeRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to sealed types, but not to enum, value types and delegate (sealed)
			if (!type.IsSealed || type.IsEnum || type.IsValueType || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			foreach (MethodDefinition method in type.Methods) {
				// method is virtual and not final (sealed)
				if (method.IsVirtual && !method.IsFinal) {
					// so just make sure it's not an override of an ancestor
					if (!method.IsOverride ())
						Runner.Report (method, Severity.Low, Confidence.Total);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
