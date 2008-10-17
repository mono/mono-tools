using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Exceptions.Impl {

	internal sealed class ExecutionPathFactory {
	
		public ExecutionPathFactory ()
		{
		}

		public IList<ExecutionPathCollection> CreatePaths (Instruction start, Instruction end)
		{
			if (start == null)
				throw new ArgumentNullException ("start");
			if (end == null)
				throw new ArgumentNullException ("end");

			List<ExecutionPathCollection> paths = new List<ExecutionPathCollection> ();
			CreatePathHelper (start, end, new ExecutionPathCollection (), paths);
			return paths;
		}

		private void CreatePathHelper (Instruction start, Instruction end, 
			ExecutionPathCollection path, List<ExecutionPathCollection> completedPaths)
		{
			ExecutionBlock curBlock = new ExecutionBlock ();
			curBlock.First = start;

			Instruction cur = start;
			bool stop = false;
			do {
				switch (cur.OpCode.FlowControl) {
				case FlowControl.Branch:
				case FlowControl.Cond_Branch:
					if (cur.OpCode == OpCodes.Switch) {
						Instruction[] targetOffsets = (Instruction[])cur.Operand;
						foreach (Instruction target in targetOffsets) {
							if (!path.Contains (target)) {
								curBlock.Last = cur;
								path.Add (curBlock);
								CreatePathHelper (target, 
										end, 
										path.Clone (),
										completedPaths);
							}
						}
						stop = true;
					} else if (cur.OpCode == OpCodes.Leave || 
							   cur.OpCode == OpCodes.Leave_S) {
						curBlock.Last = cur;
						path.Add (curBlock);
						completedPaths.Add (path);
						stop = true;
						break;
					} else {
						Instruction target = (Instruction)cur.Operand;
						if (!path.Contains (target)) {
							curBlock.Last = cur;
							path.Add (curBlock);
							CreatePathHelper (target, 
									end, 
									path.Clone (),
									completedPaths);
						} 
						if (!path.Contains (cur.Next)) {
							curBlock = new ExecutionBlock ();
							curBlock.First = cur.Next;
						} else {
							stop = true;
						}
					}
					break;
				case FlowControl.Throw:
				case FlowControl.Return:
					curBlock.Last = cur;
					path.Add (curBlock);
					completedPaths.Add (path);
					stop = true;
					break;
				default:
					break;
				}

				if (cur.Next != null && cur != end && !stop)
					cur = cur.Next;
				else
					stop = true;
			} 
			while (!stop);
		}
	}
}
