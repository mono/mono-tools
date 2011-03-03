//
// Gendarme.Rules.Correctness.ReviewDoubleAssignmentRule
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
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	// rule idea credits to FindBug - http://findbugs.sourceforge.net/
	// SA: Double assignment of field (SA_FIELD_DOUBLE_ASSIGNMENT)
	// SA: Double assignment of local variable (SA_LOCAL_DOUBLE_ASSIGNMENT)

	/// <summary>
	/// This rule checks for variables or fields that are assigned multiple times 
	/// using the same value. This won't change the value of the variable (or fields) 
	/// but should be reviewed since it could be a typo that hides a real issue in
	/// the code.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Bad {
	///	private int x, y;
	///	
	///	public Bad (int value)
	///	{
	///		// x is assigned twice, but y is not assigned
	///		x = x = value;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Good {
	///	private int x, y;
	///	
	///	public Good (int value)
	///	{
	///		// x = y = value; was the original meaning but since it's confusing...
	///		x = value;
	///		y = value;
	///	}
	///}
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method assigns the same value twice to the same variable or field.")]
	[Solution ("Verify the code logic. This is likely a typo where the second assignment is unneeded or should have been used with another variable/field.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ReviewDoubleAssignmentRule : Rule, IMethodRule {

		static string CheckDoubleAssignementOnInstanceFields (MethodDefinition method, Instruction ins, Instruction next)
		{
			// for an instance fiels the pattern is more complex because we must check that we're assigning to the same instance
			// first we go forward: DUP, STLOC, STFLD, LDLOC, STFLD

			Instruction load = next.Next;
			if ((load == null) || !load.IsLoadLocal ())
				return String.Empty;

			// check that this is the same variable
			VariableDefinition vd1 = ins.GetVariable (method);
			VariableDefinition vd2 = load.GetVariable (method);
			if (vd1.Index != vd2.Index)
				return String.Empty;

			Instruction stfld = load.Next;
			if ((stfld == null) || (stfld.OpCode.Code != Code.Stfld))
				return String.Empty;

			// check that we're assigning the same field twice
			FieldReference fd1 = (next.Operand as FieldReference);
			FieldReference fd2 = (stfld.Operand as FieldReference);
			if (fd1.MetadataToken.RID != fd2.MetadataToken.RID)
				return String.Empty;

			// backward: DUP, (possible CONV), LD (value to be duplicated), LD instance, LD instance
			if (stfld.TraceBack (method).GetOperand (method) != next.TraceBack (method).GetOperand (method))
				return String.Empty;

			return String.Format (CultureInfo.InvariantCulture, "Instance field '{0}' on same variable '{1}'.", fd1.Name, vd1.Name);
		}

		static string CheckDoubleAssignement (MethodDefinition method, Instruction ins, Instruction next)
		{
			// for a static field the pattern is
			// DUP, STSFLD, STSFLD
			if (ins.OpCode.Code == Code.Stsfld) {
				if (next.OpCode.Code != Code.Stsfld)
					return String.Empty;

				// check that we're assigning the same field twice
				FieldDefinition fd1 = ins.GetField ();
				FieldDefinition fd2 = next.GetField ();
				if (fd1.MetadataToken.RID != fd2.MetadataToken.RID)
					return String.Empty;

				return String.Format (CultureInfo.InvariantCulture, "Static field '{0}'.", fd1.Name);
			} else if (ins.IsStoreLocal ()) {
				// for a local variable the pattern is
				// DUP, STLOC, STLOC
				VariableDefinition vd2 = next.GetVariable (method);
				// check that we're assigning the same variable twice
				if (vd2 != null) {
					VariableDefinition vd1 = ins.GetVariable (method);
					if (vd1.Index != vd2.Index)
						return String.Empty;

					return String.Format (CultureInfo.InvariantCulture, "Local variable '{0}'.", vd1.Name);
				} else if (next.OpCode.Code == Code.Stfld) {
					// instance fields are a bit more complex...
					return CheckDoubleAssignementOnInstanceFields (method, ins, next);
				}
			}
			return String.Empty;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// exclude methods that don't have any DUP instruction
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Dup))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if (ins.OpCode.Code != Code.Dup)
					continue;

				Instruction next = ins.Next;
				if (next == null)
					continue;

				Instruction nn = next.Next;
				if (nn == null)
					continue;

				string msg = CheckDoubleAssignement (method, next, nn);
				if (msg.Length > 0)
					Runner.Report (method, ins, Severity.Medium, Confidence.Normal, msg);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
