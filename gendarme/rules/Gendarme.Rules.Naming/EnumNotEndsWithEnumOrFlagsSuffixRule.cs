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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Naming {

	public class EnumNotEndsWithEnumOrFlagsSuffixRule : ITypeRule {

		private static bool EndsWithSuffix (string suffix, string typeName)
		{
			return typeName.EndsWith (suffix) || typeName.ToLower ().EndsWith (suffix.ToLower ());
		}

		public MessageCollection CheckType (TypeDefinition typeDefinition, Runner runner)
		{
			// rule applies only to enums
			if (!typeDefinition.IsEnum)
				return runner.RuleSuccess;

			if (!typeDefinition.IsFlags ()) {
				if (EndsWithSuffix ("Enum", typeDefinition.Name)) {
					Location location = new Location (typeDefinition);
					Message message = new Message ("Enum name should not end with the Enum suffix.", location, MessageType.Error);
					return new MessageCollection (message);
				}
			} else {
				if (EndsWithSuffix ("Flags", typeDefinition.Name)) {
					Location location = new Location (typeDefinition);
					Message message = new Message ("Enum name should not end with the Flags suffix.", location, MessageType.Error);
					return new MessageCollection (message);
				}
			}
			return runner.RuleSuccess;
		}
	}
}
