//
// Gendarme.Rules.Design.UsingCloneWithoutImplementingICloneableRule
//
// Authors:
//	Nidhi Rawal <sonu2404@gmail.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (c) <2007> Nidhi Rawal
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

	public class UsingCloneWithoutImplementingICloneableRule: ITypeRule {

		// copy-pasted from gendarme\rules\Gendarme.Rules.BadPractice\CloneMethodShouldNotReturnNullRule.cs
		private static bool IsCloneMethod (MethodDefinition method)
		{
			return (method.Name == "Clone" && (method.Parameters.Count == 0));
		}

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			// rule applies to type that doesn't implement System.IClonable
			if (type.Implements ("System.ICloneable"))
				return runner.RuleSuccess;

			MessageCollection mc = null;
			foreach (MethodDefinition method in type.Methods) {
				// we check for methods name Clone
				if (!IsCloneMethod (method))
					continue;

				// that return System.Object, e.g. public object Clone()
				// or the current type, e.g. public <type> Clone()
				if ((method.ReturnType.ReturnType.FullName == "System.Object") || (method.ReturnType.ReturnType == type)) {
					Location location = new Location (type);
					Message message = new Message ("A Clone() method is provided but System.ICloneable is not implemented", location, MessageType.Error);
					if (mc == null)
						mc = new MessageCollection (message);
					else
						mc.Add (message);
				}
			}

			return mc;
		}
	}
}
