/*
 * IGraph.cs: the interface arbitrary graphs must implement.
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

public interface IGraph {
    IList Edges { get; }
    IList Nodes { get; }
    int EdgeCount { get; }
    int NodeCount { get; }
    void AddNode(Node node);
    bool ContainsNode(Node node);
    void AddEdge(Edge edge);
    bool ContainsEdge(Edge edge);
}

}
