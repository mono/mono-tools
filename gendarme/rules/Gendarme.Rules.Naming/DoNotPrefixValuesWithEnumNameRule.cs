//
// Gendarme.Rules.Naming.DoNotPrefixValuesWithEnumNameRule
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
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

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule checks for <c>enum</c> values that are prefixed with the enumeration type
	/// name. This is typical in C/C++ application but unneeded in .NET since the <c>enum</c> 
	/// type name must be specified anyway when used.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public enum Answer {
	///	AnswerYes,
	///	AnswerNo,
	///	AnswerMaybe,
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public enum Answer {
	///	Yes,
	///	No,
	///	Maybe
	/// }
	/// </code>
	/// </example>

	[Problem ("This enumeration contains value names that start with the enum's name.")]
	[Solution ("Change the values so that they do not include the enum's type name.")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1712:DoNotPrefixEnumValuesWithTypeName")]
	public class DoNotPrefixValuesWithEnumNameRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsEnum)
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				// this excludes special "value__"
				if (!field.IsStatic)
					continue;

				if (field.Name.StartsWith (type.Name, StringComparison.OrdinalIgnoreCase)) {
					Runner.Report (field, Severity.Medium, Confidence.High);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
