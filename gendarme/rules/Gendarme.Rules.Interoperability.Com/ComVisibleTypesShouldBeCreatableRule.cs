//
// Gendarme.Rules.Interoperability.Com.ComVisibleTypesShouldBeCreatableRule
//
// Authors:
//	Nicholas Rioux
// 
// Copyright (C) 2010 Nicholas Rioux
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

namespace Gendarme.Rules.Interoperability.Com {

	/// <summary>
	/// This rule checks for ComVisible reference types which have a public parameterized constructor, 
	/// but lack a default public constructor.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	///	[ComVisible (true)]
	///	public class BadClass {
	///		public BadClass (int param) {
	///		
	///		}
	///	}
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	///	[ComVisible (true)]
	///	public class GoodClass {
	///		public GoodClass () {
	///		
	///		}
	///		public GoodClass (int param) {
	///			
	///		}
	///	}
	/// </code>
	/// </example>

	[Problem ("ComVisible reference types should declare a default public constructor.")]
	[Solution ("Either add a default public constructor, or remove the ComVisibleAttribute from the type.")]
	[FxCopCompatibility ("Microsoft.Interoperability", "CA1409:ComVisibleTypesShouldBeCreatable")]
	public class ComVisibleTypesShouldBeCreatableRule : Rule, ITypeRule {
		public RuleResult CheckType (TypeDefinition type)
		{
			// Check only for reference types with attributes.
			if (!type.IsClass || type.IsValueType || !type.HasCustomAttributes)
				return RuleResult.DoesNotApply;

			// Ensure class is explicitly ComVisible.
			if (!type.IsTypeComVisible ())
				return RuleResult.DoesNotApply;

			// Report success if a default public constructor is found or no parameterized constructor is found.
			bool hasParameterizedCtor = false;
			bool hasDefaultCtor = false;
			foreach (var ctor in type.Methods) {
				if (!ctor.IsConstructor)
					continue;

				if (ctor.IsPublic && ctor.HasParameters) {
					hasParameterizedCtor = true;
					continue;
				}
				if (ctor.IsPublic)
					hasDefaultCtor = true;
			}
			if (!hasParameterizedCtor || hasDefaultCtor)
				return RuleResult.Success;
				
			Runner.Report (type, Severity.Medium, Confidence.Total);

			return Runner.CurrentRuleResult;
		}
	}
}
