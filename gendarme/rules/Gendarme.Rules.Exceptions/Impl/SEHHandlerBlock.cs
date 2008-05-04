using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {

	internal class SEHHandlerBlock {
	
		private Instruction start = null;
		private Instruction end = null;
		private SEHHandlerType type;
		
		public SEHHandlerBlock ()
		{
		}
		
		public Instruction Start {
			get { return start; }
			set { start = value; }
		}

		public Instruction End {
			get { return end; }
			set { end = value; }
		}
		
		public SEHHandlerType Type {
			get { return type; }
			set { type = value; }
		}
	}
}
