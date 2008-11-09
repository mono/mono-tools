/*
 * Dataflow.cs: a generic dataflow analysis algorithm
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

namespace Gendarme.Rules.Correctness {

public class Dataflow {

    [NonNull] private CFG cfg;
    [NonNull] private IDictionary inFact;
    [NonNull] private IDictionary outFact;
    [NonNull] private IDataflowAnalysis analysis;

    public Dataflow([NonNull] CFG cfg, [NonNull] IDataflowAnalysis analysis)
    {
        this.cfg = cfg;
        this.analysis = analysis;
        this.inFact = new Hashtable();
        this.outFact = new Hashtable();
    }
    
	public bool Verbose { get; set; }

    /* This only does forward analysis so far. We might need to make it
     * do backward analysis at some point. */
    public void Compute()
    {
        /* Initialize nodes */
        foreach(Node node in cfg.Nodes) {
            outFact[node] = analysis.NewTop();
            if(cfg.BeginsCatch((BasicBlock)node)) {
                /* Catch nodes start with one thing on the stack. */
                inFact[node] = analysis.NewCatch();
            } else if(cfg.Predecessors(node).Count > 0) {
                inFact[node] = analysis.NewTop();
            } else {
                inFact[node] = analysis.NewEntry();
            }
        }
        DFS dfs = new DFS(cfg, cfg.EntryPoint);
        dfs.Traverse();

        /* Calculate the fixpoint of the dataflow equations. */
        bool changed;
        int iteration = 0;
        do {
        	if (Verbose)
				Trace.WriteLine(string.Format("-------- iteration {0}", iteration));
				
            iteration++;
            changed = false;
            foreach(object o in dfs.OrderedNodes) {
                Node node = (Node)o;
                foreach(object pred in cfg.Predecessors(node))
                    analysis.MeetInto(inFact[node], outFact[pred], false);

                object temp = ((ICloneable)inFact[node]).Clone();
                analysis.Transfer(node, inFact[node], temp, false);
                if(!temp.Equals(outFact[node])) {
                    changed = true;
                    /* No need to assign new out fact unless changed. */
                    outFact[node] = temp;
                }

            }
        } while(changed);

        /* Run one final iteration with checking enabled. This is where
         * warnings will be presented. Because nothing changed in the
         * last iteration, nothing should change during this one. The
         * loop iterates over the nodes in CFG order, rather than DFS
         * order, so that messages will be sorted by location. */
        if (Verbose)
			Trace.WriteLine("-------- final iteration");
				
        foreach(object o in cfg.Nodes) {
            Node node = (Node)o;
            foreach(object pred in cfg.Predecessors(node))
                analysis.MeetInto(inFact[node], outFact[pred], true);
            object temp = ((ICloneable)inFact[node]).Clone();
            analysis.Transfer(node, inFact[node], temp, true);
        }
    }
}

}
