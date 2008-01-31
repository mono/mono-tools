using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {
	
	internal class SEHCatchBlock : SEHHandlerBlock, ISEHCatchBlock {
	
		private TypeReference type_reference = null;
		
		public SEHCatchBlock ()
		{
		}
		
		public TypeReference ExceptionType {
			get { return type_reference; }
			set { type_reference = value; }
		}		
	}	
}
