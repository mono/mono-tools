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
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule checks all private fields inside each type to see if some of them are not
	/// being used. This could be a leftover from debugging or testing code or a more 
	/// serious typo where a wrong field is being used. In any case this makes the type bigger
	/// than it needs to be which can affect performance when a large number of instances
	/// exist.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Bad {
	///	int level;
	///	bool b;
	///	
	///	public void Indent ()
	///	{
	///		level++;	
	/// #if DEBUG
	///		if (b) Console.WriteLine (level);
	/// #endif
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Good {
	///	int level;
	/// #if DEBUG
	///	bool b;
	/// #endif
	///	
	///	public void Indent ()
	///	{
	///		level++;	
	/// #if DEBUG
	///		if (b) Console.WriteLine (level);
	/// #endif
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This type contains private fields which seem to be unused.")]
	[Solution ("Remove unused fields to reduce the memory required by the type or correct the use of the field.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
	public class AvoidUnusedPrivateFieldsRule : Rule, ITypeRule {

		private HashSet<FieldDefinition> fields = new HashSet<FieldDefinition> ();

		// all instruction that load and store fields (instance or static)
		static OpCodeBitmask LoadStoreFields = new OpCodeBitmask (0x0, 0x3F00000000000000, 0x0, 0x0);

		public RuleResult CheckType (TypeDefinition type)
		{
			// rule does not apply to enums, interfaces and delegates
			// or types and structures without fields
			// nor does it apply to generated code (quite common for anonymous types)
			if (type.IsEnum || type.IsInterface || !type.HasFields || type.IsDelegate () || type.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// copy all fields into an hashset
			fields.Clear ();
			foreach (FieldDefinition field in type.Fields) {
				if (!field.IsPrivate || field.IsLiteral || field.IsGeneratedCode ())
					continue;

				fields.Add (field);
			}

			// scan all methods, including constructors, to find if the field is used
			if (fields.Count > 0) {
				CheckFieldsUsageInType (type);

				// scan nested types becuase they also have access to private fields of their parent
				if (type.HasNestedTypes) {
					foreach (TypeDefinition nested in type.NestedTypes)
						CheckFieldsUsageInType (nested);
				}

				// check remaining (private) fields in the set
				foreach (FieldDefinition field in fields) {
					Runner.Report (field, Severity.Medium, Confidence.Normal);
				}
			}
			return Runner.CurrentRuleResult;
		}

		private void CheckFieldsUsageInType (TypeDefinition type)
		{
			foreach (MethodDefinition method in type.Methods) {
				if (!method.HasBody)
					continue;

				// don't check the method if it does not access any field
				if (!OpCodeEngine.GetBitmask (method).Intersect (LoadStoreFields))
					continue;

				foreach (Instruction ins in method.Body.Instructions) {
					FieldDefinition fd = ins.GetField ();
					if (fd == null)
						continue;

					fields.Remove (fd);
				}
				
				if (fields.Count == 0)
					break;				
			}
		}

#if false
		public void Bitmask ()
		{
			OpCodeBitmask fields = new OpCodeBitmask ();
			fields.Set (Code.Ldfld);
			fields.Set (Code.Ldflda);
			fields.Set (Code.Ldsfld);
			fields.Set (Code.Ldsflda);
			fields.Set (Code.Stfld);
			fields.Set (Code.Stsfld);
			Console.WriteLine (fields);
		}
#endif
	}
}
