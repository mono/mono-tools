// 
// Gendarme.Rules.Design.AvoidPropertiesWithoutGetAccessorRule
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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

	public class AvoidPropertiesWithoutGetAccessorRule : IMethodRule {

		public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
		{
			// rule applies to setters
			if (!method.IsSetter)
				return runner.RuleSuccess;

			// rule applies

			// check if there is a getter for the same property
			string name = method.Name.Replace ("set_", "get_");
			TypeDefinition type = method.DeclaringType as TypeDefinition;
			// inside the declaring type
			foreach (MethodDefinition md in type.Methods) {
				// look for the getter name
				if (md.Name == name) {
					// and confirm it's getter
					if (md.IsGetter)
						return runner.RuleSuccess;
				}
			}

			Message msg = new Message ("The property only provide a set (and no get)", new Location (method), MessageType.Warning);
			return new MessageCollection (msg);
		}
	}
}
