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
using System.Diagnostics;
using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	[Problem ("This method, or property, might dereference a null pointer, or cause other code to do so.")]
	[Solution ("Examine the detailed listing of problem locations, and ensure that the variables in question cannot be null.")]
	public class NullDerefRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (method.Body == null)
				return RuleResult.DoesNotApply;

			CFG cfg = new CFG (method);

			// requires -v -v
			if (Runner.VerbosityLevel > 0) {
				Trace.WriteLine(string.Empty);
				Trace.WriteLine("-------------------------------------");
				Trace.WriteLine(method.ToString());
				if (Runner.VerbosityLevel > 2)
					cfg.PrintDot ();
			}

			NonNullAttributeCollector nnaCollector = new NonNullAttributeCollector();
			NullDerefAnalysis analysis = new NullDerefAnalysis (method, nnaCollector, Runner);
			analysis.Verbose = Runner.VerbosityLevel > 1;

			Dataflow dataflow = new Dataflow (cfg, analysis);
			analysis.Verbose = Runner.VerbosityLevel > 1;
			dataflow.Compute ();

			return Runner.CurrentRuleResult;
		}
	}
}
