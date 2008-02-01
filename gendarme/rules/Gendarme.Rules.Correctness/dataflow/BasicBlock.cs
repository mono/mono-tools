/*
 * BasicBlock.cs: simple representation of basic blocks.
 *
 * Authors:
 *   Aaron Tomb <atomb@soe.ucsc.edu>
 *
 * Copyright (c) 2005 Aaron Tomb and the contributors listed
 * in the ChangeLog.
 *
 * This is free software, distributed under the MIT/X11 license.
 * See the included LICENSE.MIT file for details.
 **********************************************************************/

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Correctness {

public class BasicBlock : Node {
    /* All instructions in the method */
    [NonNull] private InstructionCollection instructions;

    /* Index of the first instruction in this basic block */
    internal int first;

    /* Index of the last instruction in this basic block */
    internal int last;

    internal bool isExit = false;
    internal bool isException = false;

    public BasicBlock([NonNull] InstructionCollection instructions)
    {
        this.instructions = instructions;
    }

    public InstructionCollection Instructions {
        [NonNull]
        get { return instructions; }
    }

    public Instruction FirstInstruction {
        get { return instructions[first]; }
    }

    [NonNull]
    public override string ToString() {
        if(isExit)
            return "exit";
        if(isException)
            return "exception";
        return instructions[first].Offset.ToString("X4") + "-" +
            instructions[last].Offset.ToString("X4");
    }
}

}
