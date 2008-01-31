//
// Gendarme.Rules.Security.ArrayFieldsShouldNotBeReadOnlyRule
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

namespace Gendarme.Rules.Security {

	public class ArrayFieldsShouldNotBeReadOnlyRule : ITypeRule {

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			if (type.IsInterface || type.IsEnum)
				return runner.RuleSuccess;

			MessageCollection results = null;
			foreach (FieldDefinition field in type.Fields) {
				if (field.IsInitOnly && field.IsVisible () && field.FieldType.IsArray ()) { //IsInitOnly == readonly
					if (results == null)
						results = new MessageCollection ();
					Location loc = new Location (field);
					Message msg = new Message ("This array field is InitOnly (readonly). The readonly attribute does not apply to the elements of the array.", loc, MessageType.Warning);
					results.Add (msg);
				}
			}

			return results;
		}
	}
}
