//
// Gendarme.Rules.Maintainability.AvoidAlwaysNullFieldRule
//
// Authors:
//	Jesse Jones <jesjones@mindspring.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Jesse Jones
// Copyright (C) 2011 Novell, Inc (http://www.novell.com)
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

namespace Gendarme.Rules.Maintainability {

	/// <summary>
	/// A type has a private field whose value is always null.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// internal sealed class Bad {
	/// 	private List&lt;int&gt; values;
	/// 	
	/// 	public List&lt;int&gt; Values {
	/// 		get {
	/// 			return values;
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// internal sealed class Good {
	/// 	private List&lt;int&gt; values = new List&lt;int&gt;();
	/// 	
	/// 	public List&lt;int&gt; Values {
	/// 		get {
	/// 			return values;
	/// 		}
	/// 	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("This type has a private field whose value is always null.")]
	[Solution ("Either remove the field or properly initialize it.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public sealed class AvoidAlwaysNullFieldRule : Rule, ITypeRule {

		private HashSet <FieldReference> nullFields = new HashSet <FieldReference>();
		private HashSet <FieldReference> setFields = new HashSet <FieldReference>();
		private HashSet <FieldReference> usedFields = new HashSet <FieldReference>();
		
		private bool usesWinForms;
		
		private static OpCodeBitmask LoadStoreFields = new OpCodeBitmask (0x0, 0x3F00000000000000, 0x0, 0x0);

		static bool CheckForNullAssignment (Instruction ins)
		{
			Instruction previous = ins.Previous;
			if (!previous.Is (Code.Ldnull))
				return false;
			// handling "ternary if" is always a bit more complex
			previous = previous.Previous;
			if ((previous == null) || (previous.OpCode.FlowControl != FlowControl.Branch))
				return true;
			return (previous.Offset == ins.Offset);
		}

		private void CheckMethod (MethodDefinition method)
		{
			Log.WriteLine (this, method);	
			
			FieldDefinition field;
			if (method.HasBody && OpCodeEngine.GetBitmask (method).Intersect (LoadStoreFields)) {
				foreach (Instruction ins in method.Body.Instructions) {
					switch (ins.OpCode.Code) {
					case Code.Stfld:
					case Code.Stsfld:
						field = ins.GetField ();
						// if non-resolved then it will not be a field of this type
						if (field == null)
							continue;
						// FIXME: we'd catch more cases (and avoid some false positives) 
						// if we used a null value tracker.
						if (CheckForNullAssignment (ins)) {
							setFields.Add (field);
							Log.WriteLine (this, "{0} is set to null at {1:X4}", field.Name, ins.Offset);
						} else {
							nullFields.Remove (field);	
							Log.WriteLine (this, "{0} is set at {1:X4}", field.Name, ins.Offset);
						}
						break;

					case Code.Ldflda:	// if the field address is taken we have to assume the field has been set
					case Code.Ldsflda:
						field = ins.GetField ();
						// if non-resolved then it will not be a field of this type
						if (field == null)
							continue;
						nullFields.Remove (field);	
						Log.WriteLine (this, "{0} is set at {1:X4}", field.Name, ins.Offset);
						break;

					case Code.Ldfld:
					case Code.Ldsfld:
						field = ins.GetField ();
						// if non-resolved then it will not be a field of this type
						if (field == null)
							continue;
						usedFields.Add (field);
						Log.WriteLine (this, "{0} is used at {1:X4}", field.Name, ins.Offset);
						break;
					}
				}
			}

			Log.WriteLine (this);
		}
		
		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);
						
			// If the module does not reference SWF we can skip the type.Inherits 
			// check below.
			Runner.AnalyzeModule += (o, e) =>
			{
				usesWinForms = false;
				foreach (AssemblyNameReference name in e.CurrentModule.AssemblyReferences)
				{
					if (name.Name == "System.Windows.Forms")
						usesWinForms = true;
				}
			};
		}

		void CheckMethods (TypeDefinition type)
		{
			if (!type.HasMethods)
				return;

			IList<MethodDefinition> mc = type.Methods;
			for (int i = 0; i < mc.Count && nullFields.Count > 0; ++i)
				CheckMethod (mc [i]);
		}
		
		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum || type.IsInterface || !type.HasFields)
				return RuleResult.DoesNotApply;
				
			Log.WriteLine (this);
			Log.WriteLine (this, "----------------------------------");
			
			bool isWinFormControl = usesWinForms && type.Inherits ("System.Windows.Forms", "Control");

			// All fields start out as always null and unused.
			foreach (FieldDefinition field in type.Fields) {
				if (field.IsPrivate && !field.FieldType.IsValueType)
					if (!isWinFormControl || field.Name != "components")	// the winforms designer seems to like to leave this null
						nullFields.Add (field);
			}

			CheckMethods (type);
			if (type.HasNestedTypes) {
				foreach (TypeDefinition nested in type.NestedTypes)
					CheckMethods (nested);
			}
				
			// Report a defect if:
			// 1) The field is explicitly set to null and not used (if 
			// if is implicitly set to null and not used AvoidUnusedPrivateFieldsRule 
			// will catch it).
			setFields.IntersectWith (nullFields);	
			setFields.ExceptWith (usedFields);	
			if (setFields.Count > 0) {
				foreach (FieldDefinition field in setFields) {
					Log.WriteLine (this, "{0} is always null", field.Name);
					Runner.Report (field, Severity.Medium, Confidence.High);
				}
			}

			// 2) The field is always null and used somewhere.
			nullFields.IntersectWith (usedFields);
			if (nullFields.Count > 0) {
				foreach (FieldDefinition field in nullFields) {
					Log.WriteLine (this, "{0} is always null", field.Name);
					Runner.Report (field, Severity.Medium, Confidence.High);
				}
			}
			
			nullFields.Clear ();
			setFields.Clear ();
			usedFields.Clear ();
			
			return Runner.CurrentRuleResult;
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
