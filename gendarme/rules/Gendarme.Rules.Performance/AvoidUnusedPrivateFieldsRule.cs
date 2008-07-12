//
// Gendarme.Rules.Performance.AvoidUnusedPrivateFieldsRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	[Problem ("This method calls several times into some properties. This is expensive for virtual properties or when the property cannot be inlined.")]
	[Solution ("Refactor your code to avoid the multiple calls by caching the value.")]
	public class AvoidUnusedPrivateFieldsRule : Rule, ITypeRule {

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to enums and interfaces
			// nor does it apply to generated code (quite common for anonymous types)
			if (type.IsEnum || type.IsInterface || (type.Fields.Count == 0) || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// copy all fields into an hashset
			HashSet<FieldDefinition> fields = new HashSet<FieldDefinition> ();
			Dictionary<object, FieldDefinition> literals = new Dictionary<object, FieldDefinition> ();
			foreach (FieldDefinition field in type.Fields) {
				if (!field.IsPrivate)
					continue;

				if (field.IsLiteral && field.HasConstant && (field.Constant != null)) {
					if (!literals.ContainsKey (field.Constant))
						literals.Add (field.Constant, field);
				}

				fields.Add (field);
			}

			// scan all methods, including constructors, to find if the field is used
			foreach (MethodDefinition method in type.AllMethods ()) {
				if (!method.HasBody)
					continue;

				foreach (Instruction ins in method.Body.Instructions) {
					object operand = ins.GetOperand (method);
					if (operand == null)
						continue;
					FieldDefinition fd = null;
					FieldReference fr = (operand as FieldReference);
					if (fr != null)
						fd = fr.Resolve (); // resolving is required for generic types
					if (fd == null)
						literals.TryGetValue (operand, out fd);
					if (fd != null)
						fields.Remove (fd);
				}
			}

			// check remaining (private) fields in the set
			foreach (FieldDefinition field in fields) {
				Runner.Report (field, Severity.Medium, Confidence.Normal);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
