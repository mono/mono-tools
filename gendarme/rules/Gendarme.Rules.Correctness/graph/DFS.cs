/*
 * DFS.cs: performs a depth-first search on a graph.
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

using System.Collections;

namespace Gendarme.Rules.Correctness {

public class DFS {
    private const int WHITE = 1;
    private const int GRAY = 2;
    private const int BLACK = 3;

    [NonNull] private ArrayList orderedNodes;
    [NonNull] private Graph graph;
    [NonNull] private Node initial;
    [NonNull] private Hashtable colors;

    public DFS([NonNull] Graph graph, [NonNull] Node initial)
    {
        this.orderedNodes = new ArrayList();
        this.graph = graph;
        this.initial = initial;
        this.colors = new Hashtable(graph.NodeCount);
    }

    public void Traverse() 
    {
        foreach(object o in graph.Nodes)
            colors[o] = WHITE;

        /* Start with given initial node */
        TraverseInternal(initial);

        /* Cover any nodes not reachable from the given initial node. */
        foreach(object o in graph.Nodes)
            if((int)colors[o] == WHITE)
                TraverseInternal((Node)o);

        orderedNodes.Reverse();
    }

    private void TraverseInternal(Node node)
    {
        colors[node] = GRAY;
        foreach(object o in graph.Successors(node)) {
            Node succ = (Node)o;
            if((int)colors[succ] == WHITE)
                TraverseInternal(succ);
        }
        orderedNodes.Add(node);
        colors[node] = BLACK;
    }

    public IList OrderedNodes {
        [NonNull]
        get { return orderedNodes; }
    }
}

}
