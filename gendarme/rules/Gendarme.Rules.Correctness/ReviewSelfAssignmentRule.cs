//
// Gendarme.Rules.Correctness.ReviewSelfAssignmentRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008, 2010 Novell, Inc (http://www.novell.com)
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
	// SA: Self assignment of field (SA_FIELD_SELF_ASSIGNMENT)
	// SA: Self assignment of local variable (SA_LOCAL_SELF_ASSIGNMENT)

	/// <summary>
	/// This rule checks for variables or fields that are assigned to themselves. 
	/// This won't change the value of the variable (or fields) but should be reviewed
	/// since it could be a typo that hides a real issue in the code.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public class Bad {
	///	private int value;
	///	
	///	public Bad (int value)
	///	{
	///		// argument is assigned to itself, this.value is unchanged
	///		value = value;
	///	}
	///}
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public class Good {
	///	private int value;
	///	
	///	public Good (int value)
	///	{
	///		this.value = value;
	///	}
	///}
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This method assigns a variable or field to itself.")]
	[Solution ("Verify the code logic. This is likely a typo resulting in an incorrect assignment.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class ReviewSelfAssignmentRule : Rule, IMethodRule {

		// contains Stfld, Stsfld, Starg and Starg_S
		static OpCodeBitmask mask = new OpCodeBitmask (0x10000, 0x2400000000000000, 0x0, 0x200);

		static bool Compare (Instruction left, Instruction right, MethodDefinition method)
		{
			if (left == null)
				return (right == null);
			else if (right == null)
				return false;

			// is it on the same instance ?
			Instruction origin_left = left.TraceBack (method);
			Instruction origin_right = right.TraceBack (method);

			// if this is an array access the it must be the same element
			if (origin_left.IsLoadElement () && origin_right.IsLoadElement ()) {
				if (!CompareOperand (origin_left.Previous, origin_right.Previous, method))
					return false;
			} else {
				if (!CompareOperand (origin_left, origin_right, method))
					return false;
			}

			return Compare (origin_left, origin_right, method);
		}

		static bool CompareOperand (Instruction left, Instruction right, MethodDefinition method)
		{
			object operand_left = left.GetOperand (method);
			object operand_right = right.GetOperand (method);
			if (operand_left == null)
				return (operand_right == null);
			return operand_left.Equals (operand_right);
		}

		void CheckFields (Instruction ins, Instruction next, MethodDefinition method, bool isStatic)
		{
			FieldDefinition field = ins.GetField ();
			if ((field != null) && (field == next.GetField ())) {
				// instance fields need extra comparison using method
				if (isStatic || Compare (next, ins, method)) {
					string msg = String.Format (CultureInfo.InvariantCulture, "{0} field '{1}' of type '{2}'.",
						isStatic ? "Static" : "Instance", field.Name, field.FieldType.GetFullName ());
					Runner.Report (method, ins, Severity.Medium, Confidence.Normal, msg);
				}
			}
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// exclude methods that don't store fields or arguments
			if (!mask.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {

				Instruction next = ins.Next;
				if (next == null)
					continue;

				if (next.OpCode.Code == Code.Stfld) {
					// is it the same (instance) field ?
					CheckFields (ins, next, method, false);
				} else if (next.OpCode.Code == Code.Stsfld) {
					// is it the same (static) field ?
					CheckFields (ins, next, method, true);
// too much false positive because compilers add their own variables 
// and don't always "play well" with them
#if false
				} else if (ins.IsLoadLocal () && next.IsStoreLocal ()) {
					VariableDefinition variable = next.GetVariable (method);
					if (variable == ins.GetVariable (method)) {
						// the compiler often introduce it's own variable
						if (!variable.Name.StartsWith ("V_"))
							msg = String.Format ("Variable '{0}' of type '{1}'.", variable.Name, variable.VariableType.GetFullName ());
					}
#endif
				} else if (ins.IsLoadArgument () && next.IsStoreArgument ()) {
					ParameterDefinition parameter = next.GetParameter (method);
					if (parameter == ins.GetParameter (method)) {
						string msg = String.Format (CultureInfo.InvariantCulture, 
							"Parameter '{0}' of type '{1}'.", 
							parameter.Name, parameter.ParameterType.GetFullName ());
						Runner.Report (method, ins, Severity.Medium, Confidence.Normal, msg);
					}
				}
			}
			return Runner.CurrentRuleResult;
		}
#if false
		public void Bitmask ()
		{
			OpCodeBitmask mask = new OpCodeBitmask ();
			mask.Set (Code.Stfld);
			mask.Set (Code.Stsfld);
			/* if/when the local variables case is fixed 
			mask.Set (Code.Stloc_0);
			mask.Set (Code.Stloc_1);
			mask.Set (Code.Stloc_2);
			mask.Set (Code.Stloc_3);
			mask.Set (Code.Stloc);
			mask.Set (Code.Stloc_S); */
			mask.Set (Code.Starg);
			mask.Set (Code.Starg_S);
			Console.WriteLine (mask);
		}
#endif
	}
}

