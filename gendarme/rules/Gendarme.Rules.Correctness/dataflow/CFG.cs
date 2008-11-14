/*
 * CFG.cs: code for control flow graphs -- graphs made up of basic
 * blocks.
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

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using Gendarme.Framework;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.Correctness {

public class CFG : Graph { 

    [NonNull] private InstructionCollection instructions;
    [NonNull] private MethodDefinition method;
    [NonNull] private MethodPrinter printer;
    private BasicBlock entryPoint;

    public MethodPrinter Printer {
        get { return printer; }
    }

    public BasicBlock EntryPoint {
        get { return entryPoint; }
    }

    public CFG([NonNull] MethodDefinition method)
    {
        printer = new MethodPrinter (method);
        Init(method.Body.Instructions, method);
    }

    public bool BeginsCatch(BasicBlock bb)
    {
        ExceptionHandler handler = printer.StartsHandlerRegion (bb.FirstInstruction);
        if (handler != null && handler.Type == ExceptionHandlerType.Catch)
            return true;
        return false;
    }

    private void Init([NonNull] InstructionCollection instructions,
            [NonNull] MethodDefinition method)
    {
        this.instructions = instructions;
        this.method = method;
        BuildGraph();
    }

    private static bool HasNext([NonNull] Instruction instruction)
    {
        if(instruction.OpCode.FlowControl == FlowControl.Branch)
            return false;
        if(instruction.OpCode.FlowControl == FlowControl.Throw)
            return false;
        if(instruction.OpCode.FlowControl == FlowControl.Return)
            return false;
        else
            return true;
    }

    private BasicBlock GetNearestFinally(Instruction insn, Hashtable insnBB)
    {
        BasicBlock nearest = null;
        InstructionCollection insns = method.Body.Instructions;
        int width = insns[insns.Count - 1].Offset;
        foreach(ExceptionHandler handler in method.Body.ExceptionHandlers) {
            if(handler.Type == ExceptionHandlerType.Finally) {
                if(insn.Offset >= handler.TryStart.Offset &&
                        insn.Offset < handler.TryEnd.Offset &&
                        (handler.TryEnd.Offset -
                         handler.TryStart.Offset) < width)
                    nearest = (BasicBlock)insnBB[handler.HandlerStart.Offset];
            }
        }
        return nearest;
    }

    private void BuildGraph() {
        BasicBlock tail = null;
        Instruction prevInsn = null;
        BasicBlock prevBB = null;
        int currentInsnNum = 0;
        Hashtable insnBB = new Hashtable();
        BasicBlock exit = new BasicBlock(instructions);
        exit.first = exit.last = 0;
        exit.isExit = true;
        AddNode(exit);

        foreach(Instruction insn in instructions) {
            if (printer.IsLeader (insn, prevInsn)) {
                tail = new BasicBlock(instructions);
                tail.first = currentInsnNum;
                AddNode(tail);
                if(prevBB != null) {
                    prevBB.last = currentInsnNum - 1;
                    if(HasNext(instructions[currentInsnNum - 1])) {
                        CFGEdge edge = new CFGEdge(prevBB, tail,
                                CFGEdgeType.Forward);
                        AddEdge(edge);
                    }
                }
            }
            insnBB.Add(insn.Offset, tail);
            prevInsn = insn;
            prevBB = tail;
            currentInsnNum++;
        }
        if(prevBB != null) {
            prevBB.last = currentInsnNum - 1;
        }

        foreach(Instruction insn in instructions) {
            if ((printer.EndsTryRegion (insn) != null) ||
                    (printer.EndsHandlerRegion (insn) != null)) { 
                BasicBlock finallyBB = GetNearestFinally(insn, insnBB);
                if(finallyBB != null)
                    AddEdge(new CFGEdge((BasicBlock)insnBB[insn.Offset],
                                finallyBB, CFGEdgeType.Forward));
            }

            if(insn.OpCode.FlowControl == FlowControl.Return) {
                if(insn.OpCode.Code == Code.Endfinally &&
                        insn.Next != null) {
                    AddEdge(new CFGEdge((BasicBlock)insnBB[insn.Offset],
                                (BasicBlock)insnBB[insn.Next.Offset],
                                CFGEdgeType.Forward));
                } else {
                    AddEdge(new CFGEdge((BasicBlock)insnBB[insn.Offset],
                                exit, CFGEdgeType.Return));
                }
            }
            /*
            if((insn.OpCode.Value != OpCodeConstants.Leave) &&
                    (insn.OpCode.Value != OpCodeConstants.Endfinally)) {
                    */
                int[] targets = MethodPrinter.BranchTargets (insn);
                if(targets == null)
                    continue;
                foreach(int target in targets) {
                    AddEdge(new CFGEdge((BasicBlock)insnBB[insn.Offset],
                                (BasicBlock)insnBB[target],
                                CFGEdgeType.Branch));
                }
            /*} */
        }

        entryPoint = (BasicBlock)insnBB[0];
    }

    public void PrintDot() {
        string name = method.DeclaringType.Name + "." + method.Name + ".dot";
        FileMode mode = FileMode.Create;
        StreamWriter writer = new StreamWriter(new FileStream(name, mode));
		writer.WriteLine ("digraph {0} (", method.Name);
        foreach(Node node in Nodes) {
            BasicBlock bb = (BasicBlock)node;
	   	writer.WriteLine ("    \"{0}\" [ label = \"{1}\" ];", bb, bb);
        }

        foreach(Edge edge in Edges) {
            CFGEdge ce = (CFGEdge)edge;
            writer.Write("    \"{0}\" -> \"{1}\"", ce.Start, ce.End);
            if(ce.Type == CFGEdgeType.Branch) {
		    writer.WriteLine (" [ label = \"branch\" ];");
            } else if(ce.Type == CFGEdgeType.Forward) {
		    writer.WriteLine (" [ label = \"forward\" ];");
            } else if(ce.Type == CFGEdgeType.Return) {
		    writer.WriteLine (" [ label = \"return\" ];");
            } else if(ce.Type == CFGEdgeType.Exception) {
		    writer.WriteLine (" [ label = \"exception\" ];");
            } else {
		    writer.WriteLine (" [ label = \"unknown\" ];");
            }
        }
        writer.WriteLine (")");
        writer.Close();
    }
}

}
