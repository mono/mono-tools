using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {
	
	internal sealed class SEHGuardedBlock {
	
		private Instruction start;
		private Instruction end;
		private List<SEHHandlerBlock> handler_blocks;
		
		public SEHGuardedBlock ()
		{
			handler_blocks = new List<SEHHandlerBlock> ();
		}
		
		public Instruction Start {
			get { return start; }
			set { start = value; }			
		}

		public Instruction End {
			get { return end; }
			set { end = value; }
		}

		public ICollection<SEHHandlerBlock> SEHHandlerBlocks {
			get { return handler_blocks; }
		}
	}
}
