/*
 * Edge.cs: generic edge abstraction for arbitrary graphs.
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

public class Edge : IEdge {

    [NonNull] protected Node start;
    [NonNull] protected Node end;

    public Node Start {
        [NonNull]
        get { return start; }
    }

    public Node End {
        [NonNull]
        get { return end; }
    }
}

}
