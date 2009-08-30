//
// Gendarme.Rules.Naming.DoNotUseReservedInEnumValueNamesRule
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
	/// This rule checks for enumerations that contain values named <c>reserved</c>. This
	/// practice, often seen in C/C++ sources, is not needed in .NET since adding new
	/// values will not normally break binary compatibility. However renaming a <c>reserved</c> 
	/// enum value can since there is no way to prevent people from using the old value.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public enum Answer {
	///	Yes,
	///	No,
	///	Reserved
	///	// ^ renaming this to 'Maybe' would be a breaking change
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public enum Answer {
	///	Yes,
	///	No
	///	// we can add Maybe here without causing a breaking change
	///	// (but note that we may break code if we change the values of
	///	// existing enumerations)
	/// }
	/// </code>
	/// </example>

	[Problem ("This type is an enumeration that contains value(s) named 'reserved'.")]
	[Solution ("The 'reserved' value should be removed since there is no need to reserve enum values.")]
	[FxCopCompatibility ("Microsoft.Naming", "CA1700:DoNotNameEnumValuesReserved")]
	public class DoNotUseReservedInEnumValueNamesRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			if (!type.IsEnum)
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				// this excludes special "value__"
				if (!field.IsStatic)
					continue;

				if (field.Name.IndexOf ("RESERVED", StringComparison.OrdinalIgnoreCase) >= 0) {
					// High since removing/renaming the field can be a breaking change
					Runner.Report (field, Severity.High, Confidence.High);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
