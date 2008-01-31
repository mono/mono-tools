//
// Gendarme.Rules.Design.AvoidPublicInstanceFieldsRule
//
// Authors:
//	Adrian Tsai <adrian_tsai@hotmail.com>
//
// Copyright (c) 2007 Adrian Tsai
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

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Design {

	public class AvoidPublicInstanceFieldsRule : ITypeRule {

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			// rule doesn't apply on enums
			if (type.IsEnum)
				return runner.RuleSuccess;

			MessageCollection results = null;
			Location loc = null;

			foreach (FieldDefinition fd in type.Fields) {

				if (!fd.IsPublic || fd.IsSpecialName || fd.IsStatic || fd.HasConstant || fd.IsInitOnly)
					continue;

				if (results == null) {
					results = new MessageCollection ();
					loc = new Location (type);
				}

				if (fd.FieldType.IsArray ()) {
					results.Add (new Message ("Consider converting the public instance field \'" + fd.Name +
						"\' into a private field and a \'Set" + Char.ToUpper (fd.Name [0]) + fd.Name.Substring (1)
						+ "\' method.", loc, MessageType.Warning));
				} else {
					results.Add (new Message ("Public instance field \'" + fd.Name + "\' should probably be a property.", loc, MessageType.Warning));
				}
			}

			if (results == null)
				return runner.RuleSuccess;
			return results;
		}
	}
}
