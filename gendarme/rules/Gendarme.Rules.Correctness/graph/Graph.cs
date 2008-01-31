/*
 * Graph.cs: greneric graph skeleton.
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

namespace Gendarme.Rules.Correctness {

public class Graph : IGraph {
    [NonNull] private IList edges;
    [NonNull] private IList nodes;
    [NonNull] private IDictionary predecessors;
    [NonNull] private IDictionary successors;

    public Graph()
    {
        edges = new ArrayList();
        nodes = new ArrayList();
        predecessors = new Hashtable();
        successors = new Hashtable();
    }

    public IList Edges {
        [NonNull]
        get { return edges; }
    }

    public IList Nodes {
        [NonNull]
        get { return nodes; }
    }

    public int EdgeCount {
        get { return edges.Count; }
    }

    public int NodeCount {
        get { return nodes.Count; }
    }

    public IList Predecessors([NonNull] Node node)
    {
        return (IList)predecessors[node];
    }

    public IList Successors([NonNull] Node node)
    {
        return (IList)successors[node];
    }

    public void AddNode([NonNull] Node /*Type*/ node)
    {
        nodes.Add(node);
        predecessors[node] = new ArrayList();
        successors[node] = new ArrayList();
    }

    public bool ContainsNode([NonNull] Node /*Type*/ node)
    {
        return nodes.Contains(node);
    }

    public void AddEdge([NonNull] Edge /*Type*/ edge)
    {
        edges.Add(edge);
        AddSuccessor(edge.Start, edge.End);
        AddPredecessor(edge.End, edge.Start);
    }

    public bool ContainsEdge([NonNull] Edge /*Type*/ edge)
    {
        return edges.Contains(edge);
    }

    private void AddPredecessor([NonNull] Node node, [NonNull] Node pred)
    {
        IList predList = (IList)predecessors[node];
        predList.Add(pred);
    }

    private void AddSuccessor([NonNull] Node node, [NonNull] Node succ)
    {
        IList succList = (IList)successors[node];
        succList.Add(succ);
    }
}

}
