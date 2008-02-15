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

	[Problem ("This method, or property, might dereference a null pointer, or cause other code to do so.")]
	[Solution ("Examine the detailed listing of problem locations, and ensure that the variables in question cannot be null.")]
	public class NullDerefRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (method.Body == null)
				return RuleResult.DoesNotApply;

			CFG cfg = new CFG (method);
			// requires -v -v
			if (Runner.VerbosityLevel > 1) {
				cfg.PrintBasicBlocks ();
				cfg.PrintDot ();
			}

			NonNullAttributeCollector nnaCollector = new NonNullAttributeCollector();
			IDataflowAnalysis analysis = new NullDerefAnalysis (method, nnaCollector, Runner);

			Dataflow dataflow = new Dataflow (cfg, analysis);
			dataflow.Compute ();

			return Runner.CurrentRuleResult;
		}
	}
}
