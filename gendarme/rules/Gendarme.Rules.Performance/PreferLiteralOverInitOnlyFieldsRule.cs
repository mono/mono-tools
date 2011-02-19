//
// Gendarme.Rules.Performance.PreferConstantOverStaticFieldsRule
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
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule looks for <c>InitOnly</c> fields (<c>readonly</c> in C#) that could be
	/// turned into <c>Literal</c> (<c>const</c> in C#) because their value is known at
	/// compile time. <c>Literal</c> fields don't need to be initialized (i.e. they don't
	/// force the compiler to add a static constructor to the type) resulting in less code and the 
	/// value (not a reference to the field) will be directly used in the IL (which is OK
	/// if the field has internal visibility, but is often problematic if the field is visible outside
	/// the assembly).
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class ClassWithReadOnly {
	///	static readonly int One = 1;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class ClassWithConst
	/// {
	///	const int One = 1;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.2</remarks>

	[Problem ("Static readonly fields were found where a literal (const) field could be used.")]
	[Solution ("Replace the static readonly fields with const(ant) fields.")]
	[EngineDependency (typeof (OpCodeEngine))]
	[FxCopCompatibility ("Microsoft.Performance", "CA1802:UseLiteralsWhereAppropriate")]
	public class PreferLiteralOverInitOnlyFieldsRule : Rule, ITypeRule {

		static OpCodeBitmask Constant = new OpCodeBitmask (0xFFFE00000, 0x2000000000000, 0x0, 0x0);
		static OpCodeBitmask Convert = new OpCodeBitmask (0x0, 0x80203FC000000000, 0x400F87F8000001FF, 0x0);

		public RuleResult CheckType (TypeDefinition type)
		{
			if (type.IsEnum || type.IsInterface || type.IsDelegate ())
				return RuleResult.DoesNotApply;

			// get the static constructor
			MethodDefinition cctor = type.Methods.FirstOrDefault (m => m.IsConstructor && m.IsStatic);
			if (cctor == null)
				return RuleResult.DoesNotApply;

			// check if we store things into static fields
			if (!OpCodeEngine.GetBitmask (cctor).Get (Code.Stsfld))
				return RuleResult.DoesNotApply;

			// verify each store we do in a static field
			foreach (Instruction ins in cctor.Body.Instructions) {
				if (ins.OpCode.Code != Code.Stsfld)
					continue;

				// make sure we assign to this type (and not another one)
				FieldReference fr = (ins.Operand as FieldReference);
				if (fr.DeclaringType != type)
					continue;
				// if it's this one then we have a FieldDefinition available
				FieldDefinition field = (fr as FieldDefinition);
				// check for static (we already know with Stsfld) and readonly
				if (!field.IsInitOnly)
					continue;

				// look at what is being assigned
				Instruction previous = ins.Previous;
				// while skipping conversions
				if (Convert.Get (previous.OpCode.Code))
					previous = previous.Previous;
				// and report constant stuff
				if (Constant.Get (previous.OpCode.Code)) {
					// adjust severity based on the field visibility and it's type
					Severity s = (field.FieldType.IsNamed ("System", "String") || !field.IsVisible ()) ?
						Severity.High : Severity.Medium;
					Runner.Report (field, s, Confidence.Normal);
				}
			}

			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask constant = new OpCodeBitmask ();
			constant.Set (Code.Ldstr);
			constant.Set (Code.Ldc_I4);
			constant.Set (Code.Ldc_I4_0);
			constant.Set (Code.Ldc_I4_1);
			constant.Set (Code.Ldc_I4_2);
			constant.Set (Code.Ldc_I4_3);
			constant.Set (Code.Ldc_I4_4);
			constant.Set (Code.Ldc_I4_5);
			constant.Set (Code.Ldc_I4_6);
			constant.Set (Code.Ldc_I4_7);
			constant.Set (Code.Ldc_I4_8);
			constant.Set (Code.Ldc_I4_M1);
			constant.Set (Code.Ldc_I4_S);
			constant.Set (Code.Ldc_I8);
			constant.Set (Code.Ldc_R4);
			constant.Set (Code.Ldc_R8);
			Console.WriteLine (constant);

			OpCodeBitmask convert = new OpCodeBitmask ();
			convert.Set (Code.Conv_I);
			convert.Set (Code.Conv_I1);
			convert.Set (Code.Conv_I2);
			convert.Set (Code.Conv_I4);
			convert.Set (Code.Conv_I8);
			convert.Set (Code.Conv_Ovf_I);
			convert.Set (Code.Conv_Ovf_I_Un);
			convert.Set (Code.Conv_Ovf_I1);
			convert.Set (Code.Conv_Ovf_I1_Un);
			convert.Set (Code.Conv_Ovf_I2);
			convert.Set (Code.Conv_Ovf_I2_Un);
			convert.Set (Code.Conv_Ovf_I4);
			convert.Set (Code.Conv_Ovf_I4_Un);
			convert.Set (Code.Conv_Ovf_I8);
			convert.Set (Code.Conv_Ovf_I8_Un);
			convert.Set (Code.Conv_Ovf_U);
			convert.Set (Code.Conv_Ovf_U_Un);
			convert.Set (Code.Conv_Ovf_U1);
			convert.Set (Code.Conv_Ovf_U1_Un);
			convert.Set (Code.Conv_Ovf_U2);
			convert.Set (Code.Conv_Ovf_U2_Un);
			convert.Set (Code.Conv_Ovf_U4);
			convert.Set (Code.Conv_Ovf_U4_Un);
			convert.Set (Code.Conv_Ovf_U8);
			convert.Set (Code.Conv_Ovf_U8_Un);
			convert.Set (Code.Conv_R_Un);
			convert.Set (Code.Conv_R4);
			convert.Set (Code.Conv_R8);
			convert.Set (Code.Conv_U);
			convert.Set (Code.Conv_U1);
			convert.Set (Code.Conv_U2);
			convert.Set (Code.Conv_U4);
			convert.Set (Code.Conv_U8);
			Console.WriteLine (convert);
		}
#endif
	}
}
