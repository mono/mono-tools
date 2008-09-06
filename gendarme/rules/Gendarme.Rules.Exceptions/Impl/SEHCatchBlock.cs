using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {
	
	internal sealed class SEHCatchBlock : SEHHandlerBlock {
	
		public SEHCatchBlock ()
		{
		}

#if false
		private TypeReference type_reference = null;
		
		public TypeReference ExceptionType {
			get { return type_reference; }
			set { type_reference = value; }
		}
#endif
	}	
}
