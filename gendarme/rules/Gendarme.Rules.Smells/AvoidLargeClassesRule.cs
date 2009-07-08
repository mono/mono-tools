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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {

	// TODO: The summary should explain what a field prefix is. 
	// What do field prefixes have to do with large classes? 
	// The problem text says that the issue with large classes is that "duplicated code will not 
	// be far away". This does not seem to me to be the real issue with large classes. The 
	// problem is more that they are difficult to understand and to maintain. 
	// Can the default be changed?

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

		private static bool IsTooLarge (TypeDefinition type)
		{
			int counter = 0;
			//Skip the constants and others related from this check.
			//It's common use a class for store constants, by
			//example: Gendarme.Framework.MethodSignatures.
			foreach (FieldDefinition field in type.Fields) {
				if (field.IsSpecialName || field.HasConstant || field.IsInitOnly)
					continue;
				if (field.IsGeneratedCode ())
					continue;
				counter++;
			}
			return counter >= MaxFields;
		}

		private RuleResult CheckForClassFields (TypeDefinition type)
		{
			if (IsTooLarge (type)) {
				Runner.Report (type, Severity.High, Confidence.High, "This type contains a lot of fields.");
			}
			return RuleResult.Success;	
		}

		private static bool HasPrefixedFields (string prefix, TypeDefinition type)
		{
			if (prefix.Length == 0)
				return false;

			int counter = 0;
			foreach (FieldDefinition field in type.Fields) {
				if (field.Name.StartsWith (prefix))
					counter++;
			}
			return counter > 1;
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

		private static string GetFieldPrefix (FieldDefinition field)
		{
			int index = GetIndexOfFirst (field.Name, delegate (char character) {return Char.IsNumber (character);});
			if (index != -1)
				return field.Name.Substring (0, index);

			index = GetIndexOfFirst (field.Name, delegate (char character) {return Char.IsUpper (character);});
			if (index != -1)
				return field.Name.Substring (0, index);

			index = GetIndexOfFirstDash (field.Name);
			if (index != -1)
				return field.Name.Substring (0, index);

			return String.Empty;
		}

		private bool ExitsCommonPrefixes (TypeDefinition type)
		{
			foreach (FieldDefinition field in type.Fields) {
				// skip special, constant and read-only fields
				if (field.IsSpecialName || field.HasConstant || field.IsInitOnly)
					continue;
				// skip C# 3 fields generated for auto-implemented properties
				if (field.IsGeneratedCode ())
					continue;

				string prefix = GetFieldPrefix (field);
				if (HasPrefixedFields (prefix, type)) {
					Runner.Report (type, Severity.Medium, Confidence.High, "This type contains common prefixes.");
					return true;
				}
			}
			return false;
		}

		private RuleResult CheckForCommonPrefixesInFields (TypeDefinition type)
		{
			if (ExitsCommonPrefixes (type))
				return RuleResult.Failure;
			return RuleResult.Success;
		}

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum || !type.HasFields || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			CheckForClassFields (type);
			CheckForCommonPrefixesInFields (type);

			return Runner.CurrentRuleResult;
		}
	}
}
