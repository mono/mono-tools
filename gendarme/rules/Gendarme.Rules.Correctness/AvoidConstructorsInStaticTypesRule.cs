//
// Gendarme.Rules.Correctness.AvoidConstructorsInStaticTypesRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
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
using Mono.Cecil.Cil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	public class AvoidConstructorsInStaticTypesRule : ITypeRule {

		private const string MessageString = "Types with no instance fields or methods should not have public instance constructors";

		public MessageCollection CheckType (TypeDefinition type, Runner runner)
		{
			// rule applies only if the type as method or fields
			if (type.Methods.Count == 0 && type.Fields.Count == 0)
				return runner.RuleSuccess;

			// rule applies only if all methods are static
			foreach (MethodDefinition method in type.Methods) {
				if (!method.IsStatic)
					return runner.RuleSuccess;
			}

			// rule applies only if all fields are static
			foreach (FieldDefinition field in type.Fields) {
				if (!field.IsStatic)
					return runner.RuleSuccess;
			}

			// rule applies only if the type isn't compiler generated
			if (type.IsGeneratedCode ())
				return runner.RuleSuccess;

			// rule applies!
			MessageCollection mc = null;
			foreach (MethodDefinition ctor in type.Constructors) {
				if (!ctor.IsStatic && ctor.IsPublic) {
					Location location = new Location (ctor);
					Message message = new Message(MessageString, location, MessageType.Error);
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

