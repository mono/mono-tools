/*
 * IDataflowAnalysis.cs: the interface specific dataflow analyses must
 * implement.
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

public interface IDataflowAnalysis {
    object NewTop();

    object NewEntry();
    
    object NewCatch();

    void MeetInto(object originalFact, object newFact, bool warn);

    void Transfer(Node node, object inFact, object outFact, bool warn);
}

}
