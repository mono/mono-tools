using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {
	
	internal sealed class SEHGuardedBlock {
	
		private List<SEHHandlerBlock> handler_blocks;
		
		public SEHGuardedBlock ()
		{
			handler_blocks = new List<SEHHandlerBlock> ();
		}
#if false
		private Instruction start;
		private Instruction end;

		public SEHGuardedBlock (Instruction start, Instruction end)
			: this ()
		{
			this.start = start;
			this.end = end;
		}

		public Instruction Start {
			get { return start; }
			set { start = value; }			
		}

		public Instruction End {
			get { return end; }
			set { end = value; }
		}
#endif
		public ICollection<SEHHandlerBlock> SEHHandlerBlocks {
			get { return handler_blocks; }
		}
	}
}
