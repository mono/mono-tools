//
// Gendarme.Rules.Naming.UseCorrectPrefixRule class
//
// Authors:
//      Daniel Abramov <ex@vingrad.ru>
//
//  (C) 2007 Daniel Abramov
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

namespace Gendarme.Rules.Naming {

	public class UseCorrectPrefixRule : ITypeRule {

		private static bool IsCorrectTypeName (string name)
		{
			if (name.Length < 3)
				return true;
			bool bad = name [0] == 'C' && char.IsUpper (name [1]) && char.IsLower (name [2]);
			return !bad;
		}

		private static bool IsCorrectInterfaceName (string name)
		{
			if (name.Length < 3)
				return false;
			return name [0] == 'I' && char.IsUpper (name [1]);
		}

		public MessageCollection CheckType (TypeDefinition typeDefinition, Runner runner)
		{
			Location location = new Location (typeDefinition);
			if (typeDefinition.IsInterface) {
				if (!IsCorrectInterfaceName (typeDefinition.Name)) { // interfaces should look like 'ISomething'
					Message message = new Message (string.Format ("The '{0}' interface name doesn't have the required 'I' prefix. Acoording to existing naming conventions, all interface names should begin with the 'I' letter followed by another capital letter.", typeDefinition.Name), location, MessageType.Error);
					return new MessageCollection (message);
				}
			} else {
				if (!IsCorrectTypeName (typeDefinition.Name)) { // class should _not_ look like 'CSomething"
					Message message = new Message (string.Format ("The '{0}' type name starts with 'C' prefix but, according to existing naming conventions, type names should not have any specific prefix.", typeDefinition.Name), location, MessageType.Error);
					return new MessageCollection (message);
				}
			}
			return runner.RuleSuccess;
		}
	}
}
