/*
 * IEdge.cs: the interface graph edges must implement.
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

public interface IEdge {
    Node Start { get; }
    Node End { get; }
}

}
