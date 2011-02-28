//
// Gendarme.Framework.StackEntryAnalysis
//
// Authors:
//	Andreas Noever <andreas.noever@gmail.com>
//
//  (C) 2008 Andreas Noever
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework.Rocks;

namespace Gendarme.Framework.Helpers {

	/// <summary>
	/// This class can be used to find all usages of a reference on the stack.
	/// Currently used for:
	/// Gendarme.Rules.BadPractice.CheckNewExceptionWithoutThrowRule
	/// Gendarme.Rules.BadPractice.CheckNewThreadWithoutStartRule
	/// Gendarme.Rules.Interoperability.GetLastErrorMustBeCalledRightAfterPInvokeRule
	/// </summary>
	public class StackEntryAnalysis {

		[Serializable]
		enum StoreType {
			None,
			Local,
			Argument,
			Field,
			StaticField,
			Out,
		}

		/// <summary>
		/// Saves information about a local variable slot (argument or local variable).
		/// Used to keep track of assignments.
		/// </summary>
		struct StoreSlot : IEquatable<StoreSlot> {
			public readonly StoreType Type;
			public readonly int Slot;
			public StoreSlot (StoreType type, int slot)
			{
				this.Type = type;
				this.Slot = slot;
			}

			/// <summary>
			/// Use this to check if an instruction accesses a StoreSlot. True if this is not a StoreSlot.
			/// </summary>
			public bool IsNone {
				get {
					return this.Type == StoreType.None;
				}
			}

			public static bool operator == (StoreSlot a, StoreSlot b)
			{
				return a.Slot == b.Slot && a.Type == b.Type;
			}

			public static bool operator != (StoreSlot a, StoreSlot b)
			{
				return a.Slot != b.Slot || a.Type != b.Type;
			}

			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;
				if (!(obj is StoreSlot))
					return false;
				StoreSlot other = (StoreSlot) obj;
				return this == other;
			}

			public bool Equals (StoreSlot other)
			{
				return this == other;
			}

			public override int GetHashCode ()
			{
				return Slot.GetHashCode () ^ Type.GetHashCode ();
			}
		}

		/// <summary>
		/// Wraps an instruction and a stack of leave statements used to get to this instruction.
		/// Needed to do correct analysis in finally blocks.
		/// </summary>
		struct InstructionWithLeave : IEquatable<InstructionWithLeave> {
			public static readonly InstructionWithLeave Empty = new InstructionWithLeave ();

			public readonly Instruction Instruction;
			public readonly Instruction [] LeaveStack;

			public InstructionWithLeave (Instruction instruction)
			{
				this.Instruction = instruction;
				this.LeaveStack = null;
			}

			private InstructionWithLeave (Instruction instruction, Instruction [] leaveStack)
			{
				this.Instruction = instruction;
				this.LeaveStack = leaveStack;
			}

			/// <summary>
			/// Returns a new InstructionWithLeave with leave pushed onto the stack.
			/// </summary>
			/// <param name="instruction">The new instruction.</param>
			/// <param name="leave">The leave instruction to push onto the stack.</param>
			/// <returns>A new InstructionWithLeave</returns>
			public InstructionWithLeave Push (Instruction instruction, Instruction leave)
			{
				Instruction [] newStack;
				if (this.LeaveStack != null) {
					newStack = new Instruction [LeaveStack.Length + 1];
					Array.Copy (LeaveStack, newStack, LeaveStack.Length);
					newStack [LeaveStack.Length] = leave;
				} else {
					newStack = new Instruction [] { leave };
				}
				return new InstructionWithLeave (instruction, newStack);
			}

			/// <summary>
			/// Returns a new InstructionWithLeave with the same LeaveStack and another instruction.
			/// </summary>
			/// <param name="instruction">The new instruction.</param>
			/// <returns>a new InstructionWithLeave</returns>
			public InstructionWithLeave Copy (Instruction instruction)
			{
				return new InstructionWithLeave (instruction, this.LeaveStack);
			}

			/// <summary>
			/// Returns a new InstructionWithLeave with the one leave statement popped and instruction set to the operand of the popped leave statement.
			/// </summary>
			/// <returns>a new InstructionWithLeave</returns>
			public InstructionWithLeave Pop ()
			{
				Instruction [] newStack = null;
				if (LeaveStack == null)
					return new InstructionWithLeave ();
				if (LeaveStack.Length != 1) {
					newStack = new Instruction [LeaveStack.Length - 1];
					Array.Copy (LeaveStack, newStack, newStack.Length);
				}
				return new InstructionWithLeave ((Instruction) this.LeaveStack [this.LeaveStack.Length - 1].Operand, newStack);
			}

			public override bool Equals (object obj)
			{
				if (obj is InstructionWithLeave)
					return Equals ((InstructionWithLeave) obj);
				return false;
			}

			public bool Equals (InstructionWithLeave other)
			{
				if (Instruction != other.Instruction)
					return false;

				if (LeaveStack == null)
					return (other.LeaveStack == null);

				if (other.LeaveStack == null)
					return false;

				if (LeaveStack.Length != other.LeaveStack.Length)
					return false;

				for (int i = 0; i < LeaveStack.Length; i++) {
					if (LeaveStack [i] != other.LeaveStack [i])
						return false;
				}
				return true;
			}

			public override int GetHashCode ()
			{
				int hc = 0;
				
				unchecked {
					hc ^= Instruction.GetHashCode ();
					if (LeaveStack != null) {
						foreach (Instruction ins in LeaveStack)
							hc ^= ins.GetHashCode ();
					}
				}
				
				return hc;
			}

			public static bool operator == (InstructionWithLeave left, InstructionWithLeave right)
			{
				return left.Equals (right);
			}

			public static bool operator != (InstructionWithLeave left, InstructionWithLeave right)
			{
				return !left.Equals (right);
			}
		}

		public MethodDefinition Method {
			get; private set;
		}

		private MethodBody Body {
			get { return Method.Body; }
		}

		public StackEntryAnalysis (MethodDefinition method)
		{
			this.Method = method;
		}

		//static lists to save allocations.
		private static List<KeyValuePair<InstructionWithLeave, int>> UsedBy = new List<KeyValuePair<InstructionWithLeave, int>> ();
		private static List<KeyValuePair<InstructionWithLeave, int>> AlternativePaths = new List<KeyValuePair<InstructionWithLeave, int>> ();

		/// <summary>
		/// Searches a method for usage of the value pushed onto the stack by the specified instruction.
		/// </summary>
		/// <param name="ins">The instruction.</param>
		/// <returns>An array of UsageResults containing the instructions that use the value and the stack offset of the entry at that instruction. A StackOffset of 0 means right on top of the stack.</returns>
		public StackEntryUsageResult [] GetStackEntryUsage (Instruction ins)
		{
			if (ins == null)
				throw new ArgumentNullException ("ins");

			/* In the main loop we search for all usages of a StackEntry.
			 * Then we check each usage for a store (to a local variable or an argument), search for corrosponding loads and search for usages of the new Stackentry.
			 * This continues until no stores are found. */

			UsedBy.Clear ();
			AlternativePaths.Clear ();

			AlternativePaths.Add (new KeyValuePair<InstructionWithLeave, int> (new InstructionWithLeave (ins.Next), 0));

			int lastAlternativesCount = 0;
			int lastUsedByCount = 0;

			while (lastAlternativesCount != AlternativePaths.Count) { //continue until no more alternatives have been found (by CheckUsedBy)

				for (int i = lastAlternativesCount; i < AlternativePaths.Count; i++) { //find the instruction that pops the value and follow all branches
					var result = FollowStackEntry (AlternativePaths [i].Key, AlternativePaths [i].Value);
					if (result.Key.Instruction != null)
						UsedBy.AddIfNew (result); //add to usedby list
				}
				lastAlternativesCount = AlternativePaths.Count; //check each path only once.

				CheckUsedBy (lastUsedByCount);
				lastUsedByCount = UsedBy.Count;
			}

			//build return value
			StackEntryUsageResult [] results = new StackEntryUsageResult [UsedBy.Count];
			for (int i = 0; i < results.Length; i++)
				results [i] = new StackEntryUsageResult (UsedBy [i].Key.Instruction, UsedBy [i].Value);
			return results;
		}

		/// <summary>
		/// Iterates over all Instructions inside UsedBy and spawns a new alternative if necessary.
		/// </summary>
		/// <param name="start">The first index to progress.</param>
		private void CheckUsedBy (int start)
		{
			for (int ii = start; ii < UsedBy.Count; ii++) {
				InstructionWithLeave use = UsedBy [ii].Key;

				StoreSlot slot = GetStoreSlot (use.Instruction); //check if this is a store instruction

				bool removeFromUseBy = false; //ignore the use

				if (use.Instruction.OpCode.Code == Code.Castclass) {
					removeFromUseBy = true;
					AlternativePaths.AddIfNew (new KeyValuePair<InstructionWithLeave, int> (use.Copy (use.Instruction.Next), 0));
				} else if (use.Instruction.OpCode.Code == Code.Pop) {//pop is not a valid usage
					removeFromUseBy = true;
				} else if (!slot.IsNone) {
					if (slot.Type == StoreType.Argument || slot.Type == StoreType.Local)
						removeFromUseBy = true; //temporary save
					foreach (var ld in this.FindLoad (use.Copy (use.Instruction.Next), slot)) { //start searching at the next instruction
						AlternativePaths.AddIfNew (new KeyValuePair<InstructionWithLeave, int> (ld.Copy (ld.Instruction.Next), 0));
					}
				}
				if (removeFromUseBy) {
					UsedBy.RemoveAt (ii);
					ii--;
				}
			}
		}

		/// <summary>
		/// Follows the instructions until the specified stack entry is accessed.
		/// </summary>
		/// <param name="startInstruction">The first instruction.</param>
		/// <param name="stackEntryDistance">The distance of the stack entry from the top of the stack. 0 means right on top.</param>
		/// <returns>The instruction that pops the stackEntry and the distance of the entry to the top of the stack. If no valid instruction if found the method returns InstructionWithLeave.Empty.</returns>
		private KeyValuePair<InstructionWithLeave, int> FollowStackEntry (InstructionWithLeave startInstruction, int stackEntryDistance)
		{
			Instruction ins = startInstruction.Instruction;

			while (true) {
				int pop = ins.GetPopCount (this.Method);
				int push = ins.GetPushCount ();

				if (pop > stackEntryDistance)  //does this instruction pop the stack entry 
					return new KeyValuePair<InstructionWithLeave, int> (startInstruction.Copy (ins), stackEntryDistance);

				stackEntryDistance -= pop;
				stackEntryDistance += push;

				//fetch ne next instruction
				object alternativeNext;
				Instruction nextInstruction = GetNextInstruction (ins, out alternativeNext);

				if (nextInstruction == null)
					return new KeyValuePair<InstructionWithLeave, int> (); //return / throw / endfinally

				if (nextInstruction.OpCode.Code == Code.Leave || nextInstruction.OpCode.Code == Code.Leave_S)
					return new KeyValuePair<InstructionWithLeave, int> (); //leave clears the stack, the entry is gone.

				if (alternativeNext != null) { //branch / switch					
					Instruction oneTarget = alternativeNext as Instruction;
					if (oneTarget != null) { //branch
						AlternativePaths.AddIfNew (new KeyValuePair<InstructionWithLeave, int> (startInstruction.Copy (oneTarget), stackEntryDistance));
					} else { //switch
						foreach (Instruction switchTarget in (Instruction []) alternativeNext)
							AlternativePaths.AddIfNew (new KeyValuePair<InstructionWithLeave, int> (startInstruction.Copy (switchTarget), stackEntryDistance));
					}
				}

				if (nextInstruction.OpCode.FlowControl == FlowControl.Branch || nextInstruction.OpCode.FlowControl == FlowControl.Cond_Branch) {
					AlternativePaths.AddIfNew (new KeyValuePair<InstructionWithLeave, int> (startInstruction.Copy (nextInstruction), stackEntryDistance));
					return new KeyValuePair<InstructionWithLeave, int> (); //end of block
				}
				ins = nextInstruction;
			}
		}


		//static lists to save allocations.
		private static List<InstructionWithLeave> LoadAlternatives = new List<InstructionWithLeave> ();
		private static List<InstructionWithLeave> LoadResults = new List<InstructionWithLeave> ();

		/// <summary>
		/// Follows the codeflow starting at a given instruction and finds all loads for a given slot.
		/// Continues and follows all branches until the slot is overwritten or the method returns / throws.
		/// </summary>
		/// <param name="insWithLeave">The first instruction to start the search at.</param>
		/// <param name="slot">The slot to search.</param>
		/// <returns>An array of instructions that load from the slot.</returns>
		private List<InstructionWithLeave> FindLoad (InstructionWithLeave insWithLeave, StoreSlot slot)
		{
			LoadAlternatives.Clear ();
			LoadResults.Clear ();

			LoadAlternatives.Add (insWithLeave);


			for (int i = 0; i < LoadAlternatives.Count; i++) { //loop over all branches, more will get added inside the loop
				insWithLeave = LoadAlternatives [i]; //the first instruction of the block (contains the leave stack)

				Instruction ins = insWithLeave.Instruction; //the current instruction
				while (ins != null) {

					if (GetStoreSlot (ins) == slot) //check if the slot gets overwritten
						break;

					if (slot == GetLoadSlot (ins))
						LoadResults.AddIfNew (insWithLeave.Copy (ins)); //continue, might be loaded again

					//we simply branch to every possible catch block.
					IList<ExceptionHandler> ehc = null;
					if (Body.HasExceptionHandlers) {
						ehc = Body.ExceptionHandlers;
						foreach (ExceptionHandler handler in ehc) {
							if (handler.HandlerType != ExceptionHandlerType.Catch)
								continue;
							if (ins.Offset < handler.TryStart.Offset || ins.Offset >= handler.TryEnd.Offset)
								continue;
							LoadAlternatives.AddIfNew (insWithLeave.Copy (handler.HandlerStart));
						}
					}

					//Code.Leave leaves a try/catch block. Search for the finally block.
					if (ins.OpCode.Code == Code.Leave || ins.OpCode.Code == Code.Leave_S) {
						bool handlerFound = false;
						if (ehc != null) {
							foreach (ExceptionHandler handler in ehc) {
								if (handler.HandlerType != ExceptionHandlerType.Finally)
									continue;
								if (handler.TryStart.Offset > ins.Offset || handler.TryEnd.Offset <= ins.Offset)
									continue;
								LoadAlternatives.AddIfNew (insWithLeave.Push (handler.HandlerStart, ins)); //push the leave instruction onto the leave stack
								handlerFound = true;
								break;
							}
						}
						if (!handlerFound) //no finally found (try/catch without finally)
							LoadAlternatives.AddIfNew (insWithLeave.Copy ((Instruction) ins.Operand));
						break;

					}

					if (ins.OpCode.Code == Code.Endfinally) { //pop the last leave instruction and branch to it
						LoadAlternatives.AddIfNew (insWithLeave.Pop ());
						break;
					}

					//fetch the next instruction (s)
					object alternativeNext;
					ins = GetNextInstruction (ins, out alternativeNext);
					if (ins == null)
						break;

					if (alternativeNext != null) {
						Instruction oneTarget = alternativeNext as Instruction;
						if (oneTarget != null) { //normal branch
							LoadAlternatives.AddIfNew (insWithLeave.Copy (oneTarget));
						} else { //switch statement
							foreach (Instruction switchTarget in (Instruction []) alternativeNext)
								LoadAlternatives.AddIfNew (insWithLeave.Copy (switchTarget));
						}
					}

					if (ins.OpCode.FlowControl == FlowControl.Branch || ins.OpCode.FlowControl == FlowControl.Cond_Branch) {
						if (ins.OpCode.Code != Code.Leave && ins.OpCode.Code != Code.Leave_S) {
							LoadAlternatives.AddIfNew (insWithLeave.Copy (ins)); //add if new, avoid infinity loop
							break;
						}
					}
				}
			}
			return LoadResults;
		}

		/// <summary>
		/// Helper method that returns the next Instruction.
		/// </summary>
		/// <param name="ins">The instruction</param>
		/// <param name="alternative">If the instruction is a branch, the branch target is returned. For a switch statemant an array of targets is returned.</param>
		/// <returns>The next instruction that would be executed by the runtime.</returns>
		public static Instruction GetNextInstruction (Instruction ins, out object alternative)
		{
			if (ins == null)
				throw new ArgumentNullException ("ins");

			alternative = null;
			switch (ins.OpCode.FlowControl) {
			case FlowControl.Branch:
				return (Instruction) ins.Operand;
			case FlowControl.Cond_Branch:
				alternative = ins.Operand;
				return ins.Next;
			case FlowControl.Call:
			case FlowControl.Next:
			case FlowControl.Meta:
			case FlowControl.Break: //debugging breakpoint
				return ins.Next;
			case FlowControl.Return:
			case FlowControl.Throw:
				return null;
			default:
				throw new NotImplementedException ("FlowControl: " + ins.OpCode.FlowControl.ToString () + " is not supported.");

			}
		}

		/// <summary>
		/// Checks if an instruction is a load and returns the slot it loads from.
		/// </summary>
		/// <param name="ins">The instruction</param>
		/// <returns>If the instruction is a load returns the slot to load. Check slot.IsNone() to see if this instruction is a load.</returns>
		private StoreSlot GetLoadSlot (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
				return new StoreSlot (StoreType.Local, ins.OpCode.Code - Code.Ldloc_0);
			case Code.Ldloc_S:
			case Code.Ldloc:
				return new StoreSlot (StoreType.Local, ((VariableDefinition) ins.Operand).Index);

			case Code.Ldfld:
				//TODO: we do not check what instance is on the stack
				return new StoreSlot (StoreType.Field, ((FieldReference) ins.Operand).MetadataToken.ToInt32 ());
			case Code.Ldsfld:
				return new StoreSlot (StoreType.StaticField, ((FieldReference) ins.Operand).MetadataToken.ToInt32 ());

			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
				return new StoreSlot (StoreType.Argument, ins.OpCode.Code - Code.Ldarg_0);
			case Code.Ldarg_S:
			case Code.Ldarg: {
					int sequence = ((ParameterDefinition) ins.Operand).Index + 1;
					if (!this.Method.HasThis)
						sequence--;
					return new StoreSlot (StoreType.Argument, sequence);
				}

			case Code.Ldind_I:
			case Code.Ldind_I1:
			case Code.Ldind_I2:
			case Code.Ldind_I4:
			case Code.Ldind_I8:
			case Code.Ldind_R4:
			case Code.Ldind_R8:
			case Code.Ldind_Ref:
			case Code.Ldind_U1:
			case Code.Ldind_U2:
			case Code.Ldind_U4:
				//TODO: improve stack check
				while (ins.Previous != null) { //quick fix for out parameters.
					ins = ins.Previous;
					StoreSlot last = GetLoadSlot (ins);
					if (last.Type == StoreType.Argument)
						return new StoreSlot (StoreType.Out, last.Slot);
				}
				goto default;
			default:
				return new StoreSlot (StoreType.None, -1);
			}
		}
		/// <summary>
		/// Checks if an instruction is a store and returns the slot.
		/// </summary>
		/// <param name="ins">The instruction</param>
		/// <returns>If the instruction is a store returns the slot to store. Check slot.IsNone() to see if this instruction is a store.</returns>
		private StoreSlot GetStoreSlot (Instruction ins)
		{
			switch (ins.OpCode.Code) {
			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
				return new StoreSlot (StoreType.Local, ins.OpCode.Code - Code.Stloc_0);
			case Code.Stloc_S:
			case Code.Stloc:
				return new StoreSlot (StoreType.Local, ((VariableDefinition) ins.Operand).Index);

			case Code.Stfld:
				//TODO: we do not check what instance is on the stack
				return new StoreSlot (StoreType.Field, ((FieldReference) ins.Operand).MetadataToken.ToInt32 ());
			case Code.Stsfld:
				return new StoreSlot (StoreType.StaticField, ((FieldReference) ins.Operand).MetadataToken.ToInt32 ());

			case Code.Starg_S: //store arg (not ref / out etc)
			case Code.Starg: {
					int sequence = ((ParameterDefinition) ins.Operand).Index + 1;
					if (!this.Method.HasThis)
						sequence--;
					return new StoreSlot (StoreType.Argument, sequence);
				}

			case Code.Stind_I:
			case Code.Stind_I1:
			case Code.Stind_I2:
			case Code.Stind_I4:
			case Code.Stind_I8:
			case Code.Stind_R4:
			case Code.Stind_R8:
			case Code.Stind_Ref:
				//TODO: improve stack check
				while (ins.Previous != null) { //quick fix for out parameters.
					ins = ins.Previous;
					StoreSlot last = GetLoadSlot (ins);
					if (last.Type == StoreType.Argument)
						return new StoreSlot (StoreType.Out, last.Slot);
				}
				goto default;

			default:
				return new StoreSlot (StoreType.None, -1);
			}
		}
	}
}
