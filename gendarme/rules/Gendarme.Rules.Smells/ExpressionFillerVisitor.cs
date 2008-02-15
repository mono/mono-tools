//
// Gendarme.Rules.Smells.ExpresionFillerVisitor class
//
// Authors:
//	Néstor Salceda <nestor.salceda@gmail.com>
//
// 	(C) 2007-2008 Néstor Salceda
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
using System.Collections;

using Mono.Cecil.Cil;

namespace Gendarme.Rules.Smells {

	internal sealed class ExpressionFillerVisitor : BaseCodeVisitor {
		private IList expressionContainer;
		private Expression currentExpression;

		public ExpressionFillerVisitor () : base () {}

		public override void VisitMethodBody (MethodBody methodBody)
		{
			expressionContainer = new ArrayList ();
			currentExpression = null;
		}

		private static bool IsAcceptable (Instruction instruction)
		{
			return instruction.OpCode.FlowControl == FlowControl.Call ||
				instruction.OpCode.FlowControl == FlowControl.Branch ||
				instruction.OpCode.FlowControl == FlowControl.Cond_Branch ||
                		instruction.OpCode.Code == Code.Ceq ||
				instruction.OpCode.Code == Code.Stfld;
		}

		private bool CanCreateANewExpression () 
		{
			return currentExpression == null || 
				(currentExpression != null && currentExpression.Count != 0);
		}

		private void CreateExpressionAndAddToExpressionContainer ()
		{
			if (CanCreateANewExpression ()) {
				currentExpression = new Expression ();
				expressionContainer.Add (currentExpression);
			}
		}

		private static bool IsDelimiter (Instruction instruction)
		{
			return instruction.OpCode.Code == Code.Ldarg_0 ||
				instruction.OpCode.FlowControl == FlowControl.Branch;
		}

		private void AddToExpression (Instruction instruction)
		{
			if (currentExpression == null)
				CreateExpressionAndAddToExpressionContainer ();
			currentExpression.Add (instruction);
		}

		public override void VisitInstructionCollection (InstructionCollection instructionCollection)
		{
			foreach (Instruction instruction in instructionCollection) {
				if (IsDelimiter (instruction))
					CreateExpressionAndAddToExpressionContainer ();
				if (IsAcceptable (instruction))
					AddToExpression (instruction);
			}
		}

		public ICollection Expressions {
			get {
				return expressionContainer;
			}
		}
	}
}
