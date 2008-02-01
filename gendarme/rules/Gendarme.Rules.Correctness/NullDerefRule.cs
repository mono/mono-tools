/*
 * NullDerefRule.cs: looks for potential instances of null-pointer
 * dereferencing.
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
using Mono.Cecil;
using Gendarme.Framework;

namespace Gendarme.Rules.Correctness {

public class NullDerefRule : IMethodRule {

    public MessageCollection CheckMethod (MethodDefinition method, Runner runner)
    {
        if(method.Body == null)
            return runner.RuleSuccess;

        CFG cfg = new CFG(method);
        if(runner.Debug) {
            cfg.PrintBasicBlocks();
            cfg.PrintDot();
        }

        MessageCollection messages = new MessageCollection();
        NonNullAttributeCollector nnaCollector =
            new NonNullAttributeCollector();
        IDataflowAnalysis analysis = new NullDerefAnalysis(method, messages,
                nnaCollector, runner);
        Dataflow dataflow = new Dataflow(cfg, analysis);
        dataflow.Compute();
        if(messages.Count > 0)
            return messages;
        else
            return runner.RuleSuccess;
    }
}

}
