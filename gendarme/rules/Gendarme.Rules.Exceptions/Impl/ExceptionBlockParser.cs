using System;
using System.Collections;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions {

	internal static class ExceptionBlockParser {

		private static SEHGuardedBlock GetGuardedBlock (ExceptionHandler eh, Dictionary<Instruction, SEHGuardedBlock> blocks)
		{
			SEHGuardedBlock guardedBlock;
			if (!blocks.TryGetValue (eh.TryStart, out guardedBlock)) {
//				guardedBlock = new SEHGuardedBlock (eh.TryStart, eh.TryEnd);
				guardedBlock = new SEHGuardedBlock ();
				blocks.Add (eh.TryStart, guardedBlock);
			}
			return guardedBlock;
		}

		public static ICollection GetExceptionBlocks (MethodDefinition method)
		{
			if (!method.HasBody)
				return null;

			Dictionary<Instruction, SEHGuardedBlock> blockStarts = new Dictionary<Instruction, SEHGuardedBlock> ();
			ExceptionHandlerCollection ehc = method.Body.ExceptionHandlers;

			// Parse the exception handlers now
			foreach (ExceptionHandler eh in ehc) {
				if (eh.Type == ExceptionHandlerType.Catch) {
					SEHGuardedBlock guardedBlock = GetGuardedBlock (eh, blockStarts);

					SEHCatchBlock cb = new SEHCatchBlock ();
//					cb.ExceptionType = eh.CatchType;
					cb.Start = eh.HandlerStart;
					cb.End = eh.HandlerEnd;
//					cb.Type = SEHHandlerType.Catch;
					guardedBlock.SEHHandlerBlocks.Add (cb);
				} else if (eh.Type == ExceptionHandlerType.Finally) {
					SEHGuardedBlock guardedBlock = GetGuardedBlock (eh, blockStarts);

					SEHHandlerBlock hb = new SEHHandlerBlock ();
					hb.Start = eh.HandlerStart;
					hb.End = eh.HandlerEnd;
//					hb.Type = SEHHandlerType.Finally;
					guardedBlock.SEHHandlerBlocks.Add (hb);
				}
			}

			return blockStarts.Values;
		}
	}
}
