//
// Gendarme.Rules.Smells.AvoidLargeClassesRule class
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Smells {
	//SUGGESTION: Public / Private ratios.
	//SUGGESTION: Lines of Code.
	//SUGGESTION: Weird distributions.
	public class AvoidLargeClassesRule : ITypeRule {

		private MessageCollection messageCollection;
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
			return type.Fields.Count >= MaxFields;
		}

		private void CheckForClassFields (TypeDefinition type)
		{
			if (IsTooLarge (type))
				AddMessage (type, "This class contains a lot of fields.  This is a sign for the Large Class Smell", MessageType.Error);
		}

		private void AddMessage (TypeDefinition type, string summary, MessageType messageType)
		{
			Location location = new Location (type);
			Message message = new Message (summary, location, messageType);
			if (messageCollection == null)
				messageCollection = new MessageCollection ();
			messageCollection.Add (message);
		}

		private static bool HasPrefixedFields (string prefix, TypeDefinition type)
		{
			if (prefix == String.Empty)
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

		private static bool ExitsCommonPrefixes (TypeDefinition type)
		{
			foreach (FieldDefinition field in type.Fields) {
				// skip special, constant and read-only fields
				if (field.IsSpecialName || field.HasConstant || field.IsInitOnly)
					continue;
				// skip C# 3 fields generated for auto-implemented properties
				if (field.IsGeneratedCode ())
					continue;

				string prefix = GetFieldPrefix (field);
				if (HasPrefixedFields (prefix, type))
					return true;
			}
			return false;
		}

		private void CheckForCommonPrefixesInFields (TypeDefinition type)
		{
			if (ExitsCommonPrefixes (type))
				AddMessage (type, "This type contains some fields with the same prefix.  Although this isn't bad, it's a sign for extract a class, for avoid the Large Class smell.", MessageType.Warning);
		}

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			messageCollection = null;

			if (type.IsEnum)
				return runner.RuleSuccess;

			if (type.IsGeneratedCode ())
				return runner.RuleSuccess;

			CheckForClassFields (type);
			CheckForCommonPrefixesInFields (type);

			if (messageCollection == null || messageCollection.Count == 0)
				return runner.RuleSuccess;
			return messageCollection;
		}
	}
}
