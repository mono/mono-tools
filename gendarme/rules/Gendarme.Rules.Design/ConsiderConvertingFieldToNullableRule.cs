//
// Gendarme.Rules.Design.ConsiderConvertingFieldToNullableRule
//
// Authors:
//	Cedric Vivier <cedricv@neonux.com>
//
// Copyright (c) 2008 Cedric Vivier
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
using System.Globalization;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	/// <summary>
	/// This rule checks for pairs of fields which seem to provide the same
	/// functionality as a single nullable field. If the assembly targets version 2.0, 
	/// or more recent, of the CLR then the rule will fire to let you know that a 
	/// nullable field can be used instead. The rule will ignore assemblies targeting
	/// earlier versions of the CLR.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Bad {
	///	bool hasFoo;
	///	int foo;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Good {
	///	int? foo;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This field looks like it can be simplified using a nullable type.")]
	[Solution ("Change the field's type to a nullable type or ignore the defect.")]
	public class ConsiderConvertingFieldToNullableRule : Rule, ITypeRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// Nullable cannot be used if the assembly target runtime is earlier than 2.0
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = (e.CurrentModule.Runtime >= TargetRuntime.Net_2_0);
			};
		}

		static bool IsHasField (FieldReference fd, ref string prefix, ref string suffix)
		{
			if (!fd.FieldType.IsNamed ("System", "Boolean"))
				return false;

			string name = fd.Name;
			if (name.Length < 4)
				return false;

			if (ExtractRemainder (name, "has", ref suffix)) {
				prefix = string.Empty;
				return true;
			}
			if (ExtractRemainder (name, "_has", ref suffix)) {
				prefix = "_";
				return true;
			}
			if (ExtractRemainder (name, "m_has", ref suffix)) {
				prefix = "m_";
				return true;
			}

			return false;
		}

		static bool ExtractRemainder (string full, string prefix, ref string suffix)
		{
			if ((full.Length > prefix.Length) && full.StartsWith (prefix, StringComparison.OrdinalIgnoreCase)) {
				suffix = full.Substring(prefix.Length);
				return true;
			}
			return false;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum || !type.HasFields || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			//collect *has* fields
			foreach (FieldDefinition fd in type.Fields) {
				if (!fd.FieldType.IsValueType || fd.IsSpecialName || fd.HasConstant || fd.IsInitOnly)
					continue;

				string prefix = null, suffix = null;
				if (IsHasField(fd, ref prefix, ref suffix)
					&& HasValueTypeField(type, string.Concat(prefix,suffix)) ) {
					//TODO: check if they are both used in the same method? does the complexity worth it?
					string s = (Runner.VerbosityLevel > 0)
						? String.Format (CultureInfo.InvariantCulture, 
							"Field '{0}' should probably be a nullable if '{1}' purpose is to inform if '{0}' has been set.", 
							fd.Name, suffix)
						: string.Empty;
					Runner.Report (fd, Severity.Low, Confidence.Low, s);
				}
			}

			return Runner.CurrentRuleResult;
		}

		private static bool HasValueTypeField (TypeDefinition type, string name)
		{
			return (null != GetValueTypeField (type, name));
		}

		private static FieldDefinition GetValueTypeField (TypeDefinition type, string name)
		{
			foreach (FieldDefinition field in type.Fields) {
				if (field.FieldType.IsValueType
					&& !field.FieldType.GetElementType ().IsNamed ("System", "Nullable`1")
					&& 0 == string.Compare(name, field.Name, StringComparison.OrdinalIgnoreCase))
					return field;
			}
			return null;
		}

	}

}

