using System;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions.Impl {

	internal sealed class ExecutionBlock {
	
		private Instruction firstInstruction;
		private Instruction lastInstruction;

		public ExecutionBlock ()
		{
		}

		public Instruction First {
			get { return firstInstruction; }
			set { firstInstruction = value; }
		}

		public Instruction Last {
			get { return lastInstruction; }
			set { lastInstruction = value; }
		}

		public bool Contains (Instruction instruction)
		{
			if (firstInstruction == null || lastInstruction == null ||
				firstInstruction.Offset > lastInstruction.Offset) {
				return false;
			} else {
				return ((instruction.Offset >= firstInstruction.Offset) &&
					    (instruction.Offset <= lastInstruction.Offset));
			}
		}

		public ExecutionBlock Clone ()
		{
			ExecutionBlock other = new ExecutionBlock ();
			other.First = firstInstruction;
			other.Last = lastInstruction;
			return other;
		}
	}
}
