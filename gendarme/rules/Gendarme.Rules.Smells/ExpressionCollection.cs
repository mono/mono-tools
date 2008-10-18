//
// Gendarme.Rules.Smells.ExpressionCollection class
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
using System.Collections.Specialized;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Gendarme.Framework;

namespace Gendarme.Rules.Smells {

	internal sealed class ExpressionCollection : CollectionBase, IEquatable<ExpressionCollection> {

		public ExpressionCollection () : base () {}

		public void Add (Instruction instruction)
		{
			InnerList.Add (instruction);
		}

		public Instruction this[int index] {
			get {
				return (Instruction) InnerList[index];
			}
		}

		protected override void OnValidate (object value)
		{
			if (!(value is Instruction))
				throw new ArgumentException ("You should use this class with Mono.Cecil.Cil.Instruction", "value");
		}

		public override bool Equals (object obj)
		{
			return Equals (obj as ExpressionCollection);
		}

		public bool Equals (ExpressionCollection expression)
		{
			if (expression == null)
				return false;

			if (HasSameSize (expression))
				return CompareInstructionsInOrder (expression);

			return false;
		}

		private bool HasSameSize (ExpressionCollection expression)
		{
			return Count == expression.Count;
		}

		private bool CompareInstructionsInOrder (ExpressionCollection targetExpression)
		{
			bool equality = true;
			for (int index = 0; index < Count; index++) {
				Instruction instruction = this[index];
				Instruction targetInstruction = targetExpression[index];

				if (CheckEqualityForOpCodes (instruction, targetInstruction)) {
					if (instruction.OpCode.FlowControl == FlowControl.Call ||
						instruction.OpCode.Code == Code.Stfld) {
						equality = equality & (instruction.Operand == targetInstruction.Operand);
					}
				}
				else
					return false;;
			}
			return equality;
		}

		private static bool CheckEqualityForOpCodes (Instruction currentInstruction, Instruction targetInstruction)
		{
			if (currentInstruction.OpCode.Name == targetInstruction.OpCode.Name)
				return true;
			else {
				if (currentInstruction.OpCode.Code == Code.Call && targetInstruction.OpCode.Code == Code.Callvirt)
					return true;
				else if (currentInstruction.OpCode.Code == Code.Callvirt && targetInstruction.OpCode.Code == Code.Call)
					return true;
				else
					return false;
			}
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public override string ToString ()
		{
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append ("\tFor the expression:");
			stringBuilder.Append (Environment.NewLine);
			foreach (Instruction instruction in InnerList) {
				stringBuilder.Append (String.Format ("\t\tInstruction: {0} {1}", instruction.OpCode.Name, instruction.Operand));
				stringBuilder.Append (Environment.NewLine);
			}
			return stringBuilder.ToString ();
		}
	}
}
