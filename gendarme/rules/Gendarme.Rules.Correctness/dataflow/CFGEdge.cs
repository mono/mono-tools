/*
 * CFGEdge.cs: edges with extra flow information for use in control flow
 * graphs.
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

namespace Gendarme.Rules.Correctness {

public class CFGEdge : Edge {
    private CFGEdgeType type;

    public CFGEdgeType Type {
        get { return type; }
    }

    public CFGEdge([NonNull] BasicBlock start, [NonNull] BasicBlock end,
            CFGEdgeType type)
    {
        this.start = start;
        this.end = end;
        this.type = type;
    }
}

public enum CFGEdgeType {
    Forward,
    Branch,
    Exception,
    Return
}

}
