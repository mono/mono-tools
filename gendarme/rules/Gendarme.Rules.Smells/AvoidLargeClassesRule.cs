//
// Gendarme.Rules.Smells.AvoidLargeClassesRule class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007-2008 Néstor Salceda
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
using System.Collections.Generic;
using System.Globalization;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {

	/// <summary>
	/// This rule allows developers to measure the classes size. When a 
	/// class is trying to doing a lot of work, then you probabily have 
	/// the Large Class smell.
	///
	/// This rule will fire if a type contains too many fields (over 25 by default) or
	/// has fields with common prefixes. If the rule does fire then the type should
	/// be reviewed to see if new classes should be extracted from it.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class MyClass {
	///	int x, x1, x2, x3;
	///	string s, s1, s2, s3;
	///	DateTime bar, bar1, bar2;
	///	string[] strings, strings1; 
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class MyClass {
	/// 	int x;
	/// 	string s;
	/// 	DateTime bar; 
	/// }
	/// </code>
	/// </example>

	//SUGGESTION: Public / Private ratios.
	//SUGGESTION: Lines of Code.
	//SUGGESTION: Weird distributions.
	[Problem ("The class is trying to do too much.  Generally if a class is too large, duplicated code will not be far away.")]
	[Solution ("You can apply the Extract Class or Extract Subclass refactoring.")]
	public class AvoidLargeClassesRule : Rule,ITypeRule {

		private static int maxFields = 25;

		public static int MaxFields {
			get {
				return maxFields;
			}
			set {
				maxFields = value;
			}
		}

		List<FieldDefinition> fields = new List<FieldDefinition> ();

		private int GetNonConstantFieldsCount (TypeDefinition type)
		{
			fields.Clear ();
			//Skip the constants and others related from this check.
			//It's common use a class for store constants, by
			//example: Gendarme.Framework.MethodSignatures.
			foreach (FieldDefinition field in type.Fields) {
				if (field.IsSpecialName || field.HasConstant || field.IsInitOnly)
					continue;
				if (field.IsGeneratedCode ())
					continue;
				fields.Add (field);
			}
			return fields.Count;
		}

		private int CountPrefixedFields (string prefix, int start)
		{
			int counter = 1; // include self
			for (int i = fields.Count - 1; i > start; i--) {
				FieldDefinition field = fields [i];
				if (field.Name.StartsWith (prefix, StringComparison.Ordinal)) {
					fields.RemoveAt (i);
					counter++;
				}
			}
			return counter;
		}

		private static int GetIndexOfFirst (string value, Predicate<char> predicate)
		{
			foreach (char character in value)
				if (predicate (character))
					return value.IndexOf (character);
			return -1;
		}

		private static int GetIndexOfFirstDash (string value)
		{
			bool valueTruncated = false;
			if (value.IndexOf ('_') == 1) {
				value = value.Substring (2, value.Length - 2);
				valueTruncated = true;
			}

			foreach (char character in value) {
				if (character.Equals ('_'))
					return value.IndexOf (character) + (valueTruncated? 2 : 0);
			}
			return -1;
		}

		private static string GetFieldPrefix (MemberReference field)
		{
			string name = field.Name;
			int index = GetIndexOfFirst (name, delegate (char character) {return Char.IsNumber (character);});
			if (index != -1)
				return name.Substring (0, index);

			index = GetIndexOfFirst (name, delegate (char character) {return Char.IsUpper (character);});
			if (index != -1)
				return name.Substring (0, index);

			index = GetIndexOfFirstDash (name);
			if (index != -1)
				return name.Substring (0, index);

			return String.Empty;
		}

		private bool CheckForFieldCommonPrefixes (IMetadataTokenProvider type)
		{
			for (int i=0; i < fields.Count; i++) {
				FieldDefinition field = fields [i];

				string prefix = GetFieldPrefix (field);
				if (prefix.Length == 0)
					continue;
				int count = CountPrefixedFields (prefix, i);
				if (count > 1) {
					string msg = String.Format (CultureInfo.InvariantCulture,
						"This type contains fields common prefixes: {0} fields prefixed with '{1}'.",
						count, prefix);
					Runner.Report (type, Severity.Medium, Confidence.High, msg);
				}
			}
			return false;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum || !type.HasFields || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			int fcount = GetNonConstantFieldsCount (type);
			if (fcount > MaxFields) {
				string msg = String.Format (CultureInfo.InvariantCulture,
					"This type contains a lot of fields ({0} versus maximum of {1}).",
					fcount, MaxFields);
				Runner.Report (type, Severity.High, Confidence.High, msg);
			}

			// skip check if there are only constants or generated fields
			if (fcount > 0)
				CheckForFieldCommonPrefixes (type);

			return Runner.CurrentRuleResult;
		}
	}
}
