//
// Gendarme.Rules.Correctness.ReviewSelfAssignmentRule
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	// rule idea credits to FindBug - http://findbugs.sourceforge.net/
	// SA: Self assignment of field (SA_FIELD_SELF_ASSIGNMENT)
	// SA: Self assignment of local variable (SA_LOCAL_SELF_ASSIGNMENT)

	[Problem ("This method assign the variable or field to itself.")]
	[Solution ("Verify the code logic. This is likely a typo which result in a wrong assignment.")]
	public class ReviewSelfAssignmentRule : Rule, IMethodRule {

		static bool Compare (Instruction left, Instruction right, MethodDefinition method)
		{
			// is it the same field ?
			FieldDefinition field = right.GetField ();
			if (field != left.GetField ())
				return false;

			// is it on the same instance ?
			Instruction origin_left = left.TraceBack (method);
			Instruction origin_right = right.TraceBack (method);

			// if this is an array access the it must be the same element
			if (origin_left.IsLoadElement ())
				origin_left = origin_left.Previous;
			if (origin_right.IsLoadElement ())
				origin_right = origin_right.Previous;

			object operand_left = origin_left.GetOperand (method);
			object operand_right = origin_right.GetOperand (method);
			if (operand_left == null)
				return (operand_right == null);
			return operand_left.Equals (operand_right);
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {

				string msg = null;
				Instruction next = ins.Next;
				if (next == null)
					continue;

				if (next.OpCode.Code == Code.Stfld) {
					if (Compare (next, ins, method)) {
						FieldDefinition field = ins.GetField ();
						msg = String.Format ("Instance field '{0}' of type '{1}'.", field.Name, field.FieldType.FullName);
					}
				} else if (next.OpCode.Code == Code.Stsfld) {
					FieldDefinition field = next.GetField ();
					if (field == ins.GetField ()) {
						msg = String.Format ("Static field '{0}' of type '{1}'.", field.Name, field.FieldType.FullName);
					}
// too much false positive because compilers add their own variables 
// and don't always "play well" with them
#if false
				} else if (ins.IsLoadLocal () && next.IsStoreLocal ()) {
					VariableDefinition variable = next.GetVariable (method);
					if (variable == ins.GetVariable (method)) {
						// the compiler often introduce it's own variable
						if (!variable.Name.StartsWith ("V_"))
							msg = String.Format ("Variable '{0}' of type '{1}'.", variable.Name, variable.VariableType.FullName);
					}
#endif
				} else if (ins.IsLoadArgument () && next.IsStoreArgument ()) {
					ParameterDefinition parameter = next.GetParameter (method);
					if (parameter == ins.GetParameter (method)) {
						msg = String.Format ("Parameter '{0}' of type '{1}'.", parameter.Name, parameter.ParameterType.FullName);
					}
				}

				if (msg != null)
					Runner.Report (method, ins, Severity.Medium, Confidence.Normal, msg);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
