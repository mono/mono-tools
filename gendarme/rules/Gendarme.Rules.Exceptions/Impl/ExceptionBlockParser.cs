using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {

	public static class ExceptionBlockParser {
	
		public static ISEHGuardedBlock[] GetExceptionBlocks (MethodDefinition method)
		{
			Hashtable blockStarts = new Hashtable ();
			ExceptionHandlerCollection ehc = null;
			
			if (method.Body != null) {
				ehc = method.Body.ExceptionHandlers;
				// Parse the exception handlers now
				foreach (ExceptionHandler eh in ehc) {
					SEHGuardedBlock guardedBlock = null;

					if (eh.Type == ExceptionHandlerType.Catch) {
						if (!blockStarts.ContainsKey (eh.TryStart)) {
							guardedBlock = new SEHGuardedBlock ();
							guardedBlock.Start = eh.TryStart;
							guardedBlock.End = eh.TryEnd;
							blockStarts [eh.TryStart] = guardedBlock;
						} else {
							guardedBlock = (SEHGuardedBlock)
								blockStarts [eh.TryStart];
						}

						SEHCatchBlock cb = new SEHCatchBlock ();
						cb.ExceptionType = eh.CatchType;
						cb.Start = eh.HandlerStart;
						cb.End = eh.HandlerEnd;
						cb.Type = SEHHandlerType.Catch;
						guardedBlock.SEHHandlerBlocksInternal.Add (cb);
					}
					else if (eh.Type == ExceptionHandlerType.Finally) {
						if (!blockStarts.ContainsKey (eh.TryStart)) {
							guardedBlock = new SEHGuardedBlock ();
							guardedBlock.Start = eh.TryStart;
							guardedBlock.End = eh.TryEnd;
							blockStarts [eh.TryStart] = guardedBlock;
						} else {
							guardedBlock = (SEHGuardedBlock)blockStarts [eh.TryStart];
						}

						SEHHandlerBlock hb = new SEHHandlerBlock ();
						hb.Start = eh.HandlerStart;
						hb.End = eh.HandlerEnd;
						hb.Type = SEHHandlerType.Finally;
						guardedBlock.SEHHandlerBlocksInternal.Add (hb);
					}
				}
			}
			
			ISEHGuardedBlock[] ret = new ISEHGuardedBlock [blockStarts.Count];
			blockStarts.Values.CopyTo (ret,0);
			return ret;
		}
	}
}
