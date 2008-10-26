//
// EnumNotEndsWithEnumOrFlagsSuffixRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007 Néstor Salceda
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
using System.Globalization;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	/// <summary>
	/// This rule ensure that enumeration type namess do not end with either <c>Enum</c> or
	/// <c>Flags</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public enum MyCustomValueEnum {
	///	Foo,
	///	Bar 
	/// } 
	/// 
	/// [Flags]
	/// public enum MyCustomValuesFlags {
	///	Foo,
	///	Bar,
	///	AllValues = Foo | Bar 
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public enum MyCustomValue {
	///	Foo,
	///	Bar 
	/// } 
	/// 
	/// [Flags]
	/// public enum MyCustomValues {
	///	Foo,
	///	Bar,
	///	AllValues = Foo | Bar 
	/// }
	/// </code>
	/// </example>

	[Problem ("This type is an enumeration and, by convention, its name should not end with either Enum or Flags.")]
	[Solution ("Remove the Enum or Flags suffix in enumeration name.")]
	public class EnumNotEndsWithEnumOrFlagsSuffixRule : Rule, ITypeRule {

		private static bool EndsWithSuffix (string suffix, string typeName)
		{
			int pos = typeName.Length - suffix.Length;
			if (pos < 0)
				return false;

			return (String.Compare (typeName, pos, suffix, 0, suffix.Length, true, CultureInfo.InvariantCulture) == 0);
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule applies only to enums
			if (!type.IsEnum)
				return RuleResult.DoesNotApply;

			if (!type.IsFlags ()) {
				if (EndsWithSuffix ("Enum", type.Name)) {
					Runner.Report (type, Severity.Medium, Confidence.High, "Enum name should not end with the 'Enum'.");
				}
			} else {
				if (EndsWithSuffix ("Flags", type.Name)) {
					Runner.Report (type, Severity.Medium, Confidence.High, "Enum name should not end with the 'Flags'.");
				}
			}
			return Runner.CurrentRuleResult;
		}
	}
}
