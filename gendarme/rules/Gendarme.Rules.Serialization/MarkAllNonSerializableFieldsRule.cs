//
// Gendarme.Rules.Serialization.MarkAllNonSerializableFieldsRule
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2008 Néstor Salceda
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
using Gendarme.Framework;
using Gendarme.Framework.Rocks;
using Mono.Cecil;

namespace Gendarme.Rules.Serialization {

	/// <summary>
	/// This rule checks for serializable types, i.e. decorated with the <c>[Serializable]</c>
	/// attribute, and checks to see if all its fields are serializable as well. If not the rule will 
	/// fire unless the field is decorated with the <c>[NonSerialized]</c> attribute.
	/// The rule will also warn if the field type is an interface as it is not possible,
	/// before execution time, to know for certain if the type can be serialized or not.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// class NonSerializableClass {
	/// }
	/// 
	/// [Serializable]
	/// class SerializableClass {
	///	NonSerializableClass field;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// class NonSerializableClass {
	/// }
	/// 
	/// [Serializable]
	/// class SerializableClass {
	///	[NonSerialized]
	///	NonSerializableClass field;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This type is Serializable, but contains fields that aren't serializable which can cause runtime errors when instances are serialized.")]
	[Solution ("Make sure you are marking all non-serializable fields with the NonSerialized attribute or implement custom serialization.")]
	[FxCopCompatibility ("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields")]
	public class MarkAllNonSerializableFieldsRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// if type is not serializable or has not any fields or does not implements a custom serialization
			if (!type.IsSerializable || !type.HasFields || type.Implements ("System.Runtime.Serialization", "ISerializable"))
				return RuleResult.DoesNotApply;

			foreach (FieldDefinition field in type.Fields) {
				if (!field.IsNotSerialized && !field.IsStatic) { 
					TypeDefinition fieldType = field.FieldType.Resolve ();
					if (fieldType == null)
						continue;

					if (fieldType.IsInterface) {
						string msg = String.Format (CultureInfo.InvariantCulture,
							"Serialization of interface {0} as field {1} unknown until runtime", 
							fieldType, field.Name);
						Runner.Report (field, Severity.Critical, Confidence.Low, msg);
						continue;
					}
					if (!fieldType.IsEnum && !fieldType.IsSerializable) {
						string msg = String.Format (CultureInfo.InvariantCulture,
							"The field {0} isn't serializable.", field.Name);
						Runner.Report (field, Severity.Critical, Confidence.High, msg);
					}
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
