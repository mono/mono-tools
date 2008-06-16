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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	// rule idea credits to FindBug - http://findbugs.sourceforge.net/
	// SA: Double assignment of field (SA_FIELD_DOUBLE_ASSIGNMENT)
	// SA: Double assignment of local variable (SA_LOCAL_DOUBLE_ASSIGNMENT)

	[Problem ("This method assign the same value twice to the same variable or field.")]
	[Solution ("Verify the code logic. This is likely a typo where the second assignment is unneeded or should have been assigned to another variable/field.")]
	public class ReviewDoubleAssignmentRule : Rule, IMethodRule {

		static string CheckDoubleAssignementOnInstanceFields (MethodDefinition method, Instruction ins, Instruction next)
		{
			// for an instance fiels the pattern is more complex because we must check that we're assigning to the same instance
			// first we go forward: DUP, STLOC, STFLD, LDLOC, STFLD

			Instruction load = next.Next;
			if ((load == null) || !load.IsLoadLocal ())
				return null;

			// check that this is the same variable
			VariableDefinition vd1 = ins.GetVariable (method);
			VariableDefinition vd2 = load.GetVariable (method);
			if (vd1.Index != vd2.Index)
				return null;

			Instruction stfld = load.Next;
			if ((stfld == null) || (stfld.OpCode.Code != Code.Stfld))
				return null;

			// check that we're assigning the same field twice
			FieldDefinition fd1 = next.GetField ();
			FieldDefinition fd2 = stfld.GetField ();
			if (fd1.MetadataToken.RID != fd2.MetadataToken.RID)
				return null;

			// backward: DUP, (possible CONV), LD (value to be duplicated), LD instance, LD instance
			ins = ins.Previous; // DUP
			int n = 1;
			object op1 = null;
			object op2 = null;
			while ((ins != null) && (n != -1)) {
				if (ins.OpCode.StackBehaviourPop == StackBehaviour.Pop0) {
					switch (ins.OpCode.StackBehaviourPush) {
					case StackBehaviour.Push1:
					case StackBehaviour.Pushi:
					case StackBehaviour.Pushref:
						if (n == 0)
							op2 = ins.GetOperand (method);
						if (n == -1)
							op1 = ins.GetOperand (method);
						n--;
						break;
					}
				}
				ins = ins.Previous;
			}
			if (op1 != op2)
				return null;

			return String.Format ("Instance field '{0}' on same variable '{1}'.", fd1.Name, op1);
		}

		static string CheckDoubleAssignement (MethodDefinition method, Instruction ins, Instruction next)
		{
			// for a static field the pattern is
			// DUP, STSFLD, STSFLD
			if (ins.OpCode.Code == Code.Stsfld) {
				if (next.OpCode.Code != Code.Stsfld)
					return null;

				// check that we're assigning the same field twice
				FieldDefinition fd1 = ins.GetField ();
				FieldDefinition fd2 = next.GetField ();
				if (fd1.MetadataToken.RID != fd2.MetadataToken.RID)
					return null;

				return String.Format ("Static field '{0}'.", fd1.Name);
			} else if (ins.IsStoreLocal ()) {
				// for a local variable the pattern is
				// DUP, STLOC, STLOC
				VariableDefinition vd2 = next.GetVariable (method);
				// check that we're assigning the same variable twice
				if (vd2 != null) {
					VariableDefinition vd1 = ins.GetVariable (method);
					if (vd1.Index != vd2.Index)
						return null;

					return String.Format ("Local variable '{0}'.", vd1.Name);
				} else if (next.OpCode.Code == Code.Stfld) {
					// instance fields are a bit more complex...
					return CheckDoubleAssignementOnInstanceFields (method, ins, next);
				}
			}
			return null;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
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
				if (msg != null)
					Runner.Report (method, ins, Severity.Medium, Confidence.Normal, msg);
			}
			return Runner.CurrentRuleResult;
		}
	}
}
