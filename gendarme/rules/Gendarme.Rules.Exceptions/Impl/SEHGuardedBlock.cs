using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {
	
	internal sealed class SEHGuardedBlock : ISEHGuardedBlock {
	
		private Instruction start;
		private Instruction end;
		private SEHHandlerBlockCollection handler_blocks;
		
		public SEHGuardedBlock ()
		{
			handler_blocks = new SEHHandlerBlockCollection ();
		}
		
		public Instruction Start {
			get { return start; }
			set { start = value; }			
		}

		public Instruction End {
			get { return end; }
			set { end = value; }
		}
		
		public ISEHHandlerBlock[] SEHHandlerBlocks {
			get {
				ISEHHandlerBlock[] ret =
					new ISEHHandlerBlock [handler_blocks.Count];
				handler_blocks.CopyTo (ret, 0);
				return ret;
			}
		}

		public SEHHandlerBlockCollection SEHHandlerBlocksInternal {
			get { return handler_blocks; }
		}
	}
}
