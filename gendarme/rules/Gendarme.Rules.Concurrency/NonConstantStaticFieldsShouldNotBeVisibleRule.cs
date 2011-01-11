//
// Gendarme.Rules.Concurrency.NonConstantStaticFieldsShouldNotBeVisibleRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
//  (C) 2008 Andreas Noever
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
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Concurrency {

	/// <summary>
	/// This rule warns if a non-constant public static field is found. 
	/// In a multi-threaded environment access to those fields must be synchronized. 
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class HasPublicStaticField {
	/// 	public static ComplexObject Field;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class FieldIsReadonly {
	/// 	public readonly static ComplexObject Field = new ComplexObject();
	/// }
	/// </code>
	/// <code>
	/// class UseThreadStatic {
	///	[ThreadStatic]
	///	public static ComplexObject Field;
	/// 
	///	public static InitializeThread () 
	///	{
	///		if (Field == null)
	///			Field = new ComplexObject ();
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type has some static fields that are not constant. They may represent problems in multithreaded applications.")]
	[Solution ("Change the field to read-only, or mark it [ThreadStatic], or make it non visible outside the assembly.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible")]
	public class NonConstantStaticFieldsShouldNotBeVisibleRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to interface and enumerations
			if (type.IsInterface || type.IsEnum || !type.HasFields || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				if (field.IsStatic && field.IsVisible () && !field.IsInitOnly && !field.IsLiteral) {
					Runner.Report (field, Severity.Medium, Confidence.High);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
