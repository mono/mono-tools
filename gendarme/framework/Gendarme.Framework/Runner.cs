// 
// Gendarme.Framework.Runner
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008-2011 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Framework {

	abstract public class Runner : IRunner {

		private Collection<Defect> defect_list = new Collection<Defect> ();
		private int defects_limit = Int32.MaxValue;
		private Bitmask<Severity> severity_bitmask = new Bitmask<Severity> (true);
		private Bitmask<Confidence> confidence_bitmask = new Bitmask<Confidence> (true);

		private Collection<IRule> rules = new Collection<IRule> ();
		private Collection<AssemblyDefinition> assemblies = new Collection<AssemblyDefinition> ();
		private int verbose_level;

		private IEnumerable<IAssemblyRule> assembly_rules;
		private IEnumerable<ITypeRule> type_rules;
		private IEnumerable<IMethodRule> method_rules;

		private IRule currentRule;
		private IMetadataTokenProvider currentTarget;
		private IIgnoreList ignoreList;
		private int defectCountBeforeCheck;
		private object [] engine_dependencies;

		public event EventHandler<RunnerEventArgs> AnalyzeAssembly;
		public event EventHandler<RunnerEventArgs> AnalyzeModule;
		public event EventHandler<RunnerEventArgs> AnalyzeType;
		public event EventHandler<RunnerEventArgs> AnalyzeMethod;
		
		protected IRule CurrentRule {
			get { return currentRule; }
			set { currentRule = value; }
		}

		protected IMetadataTokenProvider CurrentTarget {
			get { return currentTarget; }
			set { currentTarget = value; }
		}

		public IIgnoreList IgnoreList {
			get {
				if (ignoreList == null)
					throw new InvalidOperationException ("No IgnoreList has been set for this runner.");
				return ignoreList;
			}
			set { ignoreList = value; }
		}

		public Collection<IRule> Rules {
			get { return rules; }
		}
 
		public Collection<AssemblyDefinition> Assemblies {
			get { return assemblies; }
		}

		public Collection<Defect> Defects {
			get { return defect_list; }
		}

		public int DefectsLimit {
			get { return defects_limit; }
			set {
				if (value < 0)
					throw new ArgumentException ("Cannot be negative", "DefectsLimit");
				defects_limit = value;
			}
		}

		public Bitmask<Severity> SeverityBitmask {
			get { return severity_bitmask; }
		}

		public Bitmask<Confidence> ConfidenceBitmask {
			get { return confidence_bitmask; }
		}

		public int VerbosityLevel {
			get { return verbose_level; }
			set { verbose_level = value; }
		}

		private EngineController ec;
		public EngineController Engines { 
			get {
				if (ec == null)
					ec = new EngineController (this);
				return ec;
			}
		}

		// once every assembly are loaded *and* all the rules are known -> we initialized all rules.
		// this ensure that the list of assemblies is available at rule initialization time
		// which allows caching information and treating the assemblies as "a set"
		public virtual void Initialize ()
		{
			AnalyzeAssembly = null;
			AnalyzeModule = null;
			AnalyzeType = null;
			AnalyzeMethod = null;

			AssemblyResolver resolver = AssemblyResolver.Resolver;
			resolver.AssemblyCache.Clear ();

			foreach (AssemblyDefinition assembly in assemblies) {
				assembly.MainModule.LoadDebuggingSymbols ();
				resolver.CacheAssembly (assembly);
			}

			foreach (Rule rule in rules) {
				try {
					// don't initialize rules that *we*, the runner, don't want
					// to execute later (this also avoids the rule logic to reset
					// the Active property during optimizations)
					if (rule.Active)
						rule.Initialize (this);
				}
				catch (Exception e) {
					// if something goes wrong in initialization we desactivate the rule
					if (VerbosityLevel > 0)
						Console.Error.WriteLine (e);
					rule.Active = false;
				}
			}

			engine_dependencies = GetType ().GetCustomAttributes (typeof (EngineDependencyAttribute), true);
			if (engine_dependencies.Length > 0) {
				// subscribe to each engine the rule depends on
				foreach (EngineDependencyAttribute eda in engine_dependencies) {
					Engines.Subscribe (eda.EngineType);
				}
			}
		
			Engines.Build (assemblies);

			assembly_rules = rules.OfType<IAssemblyRule> ();
			type_rules = rules.OfType<ITypeRule> ();
			method_rules = rules.OfType<IMethodRule> ();
		}

		private bool Filter (Severity severity, Confidence confidence, IMetadataTokenProvider location)
		{
			if (!SeverityBitmask.Get (severity) || !ConfidenceBitmask.Get (confidence))
				return false;
			// for Assembly | Type | Methods we can ignore before executing the rule
			// but for others (e.g. Parameters, Fields...) we can only ignore the results
			return !IgnoreList.IsIgnored (currentRule, location);
		}

		public virtual void Report (Defect defect)
		{
			if (defect == null)
				throw new ArgumentNullException ("defect");

			if (!Filter (defect.Severity, defect.Confidence, defect.Location))
				return;
				
			if (IgnoreList.IsIgnored (defect.Rule, defect.Target))
				return;

			defect_list.Add (defect);
		}

		public void Report (IMetadataTokenProvider metadata, Severity severity, Confidence confidence)
		{
			// check here to avoid creating the Defect object
			if (!Filter (severity, confidence, metadata))
				return;

			Defect defect = new Defect (currentRule, currentTarget, metadata, severity, confidence);
			Report (defect);
		}

		public void Report (IMetadataTokenProvider metadata, Severity severity, Confidence confidence, string message)
		{
			// check here to avoid creating the Defect object
			if (!Filter (severity, confidence, metadata))
				return;

			Defect defect = new Defect (currentRule, currentTarget, metadata, severity, confidence, message);
			Report (defect);
		}

		public void Report (MethodDefinition method, Instruction ins, Severity severity, Confidence confidence)
		{
			// check here to avoid creating the Defect object
			if (!Filter (severity, confidence, method))
				return;

			Defect defect = new Defect (currentRule, currentTarget, method, ins, severity, confidence);
			Report (defect);
		}

		public void Report (MethodDefinition method, Instruction ins, Severity severity, Confidence confidence, string message)
		{
			// check here to avoid creating the Defect object
			if (!Filter (severity, confidence, method))
				return;

			Defect defect = new Defect (currentRule, currentTarget, method, ins, severity, confidence, message);
			Report (defect);
		}

		public void Reset ()
		{
			defectCountBeforeCheck = 0;
			Defects.Clear ();
			SeverityBitmask.SetAll ();
			ConfidenceBitmask.SetAll ();
		}

		private void OnEvent (EventHandler<RunnerEventArgs> handler, RunnerEventArgs e)
		{
			if (handler != null)
				handler (this, e);
		}

		static bool VisibilityCheck (ApplicabilityScope scope, bool visible)
		{
			switch (scope) {
			case ApplicabilityScope.Visible:
				return visible;
			case ApplicabilityScope.NonVisible:
				return !visible;
			default:
				return true;
			}
		}

		// protected since a higher-level (e.g. GUI) runner might want to override
		// them to update it's user interface
		protected virtual void OnAssembly (RunnerEventArgs e)
		{
			OnEvent (AnalyzeAssembly, e);

			foreach (IAssemblyRule rule in assembly_rules) {
				defectCountBeforeCheck = Defects.Count;
				// stop if we reach the user defined defect limit
				if (defectCountBeforeCheck >= DefectsLimit)
					break;

				// ignore the rule on some user defined assemblies
				if (IgnoreList.IsIgnored (rule, e.CurrentAssembly))
					continue;

				currentRule = rule;
				rule.CheckAssembly (e.CurrentAssembly);
			}
		}

		protected virtual void OnModule (RunnerEventArgs e)
		{
			OnEvent (AnalyzeModule, e);

			// Since it has never been used in the previous years 
			// this version of the Gendarme framework doesn't 
			// support IModuleRule. Nor do we support ignore on 
			// modules.
		}

		protected virtual void OnType (RunnerEventArgs e)
		{
			OnEvent (AnalyzeType, e);

			foreach (ITypeRule rule in type_rules) {
				defectCountBeforeCheck = Defects.Count;
				// stop if we reach the user defined defect limit
				if (defectCountBeforeCheck >= DefectsLimit)
					break;

				// ignore if the visibility does not match user selection 
				ApplicabilityScope scope = rule.ApplicabilityScope;
				if ((scope != ApplicabilityScope.All) && !VisibilityCheck (scope, e.CurrentType.IsVisible ()))
					continue;

				// ignore the rule on some user defined types
				if (IgnoreList.IsIgnored (rule, e.CurrentType))
					continue;

				currentRule = rule;
				rule.CheckType (e.CurrentType);
			}
		}

		protected virtual void OnMethod (RunnerEventArgs e)
		{
			OnEvent (AnalyzeMethod, e);

			foreach (IMethodRule rule in method_rules) {
				defectCountBeforeCheck = Defects.Count;
				// stop if we reach the user defined defect limit
				if (defectCountBeforeCheck >= DefectsLimit)
					break;

				// ignore if the visibility does not match user selection 
				ApplicabilityScope scope = rule.ApplicabilityScope;
				if ((scope != ApplicabilityScope.All) && !VisibilityCheck (scope, e.CurrentMethod.IsVisible ()))
					continue;

				// ignore the rule on some user defined methods
				if (IgnoreList.IsIgnored (rule, e.CurrentMethod))
					continue;

				currentRule = rule;
				rule.CheckMethod (e.CurrentMethod);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Return RuleResult.Failure is the number of defects has grown since 
		/// the rule Check* method was called or RuleResult.Success otherwise</returns>
		public RuleResult CurrentRuleResult {
			get {
				return (Defects.Count > defectCountBeforeCheck) ?
					RuleResult.Failure : RuleResult.Success;
			}
		}

		/// <summary>
		/// For all assemblies, every modules in each assembly, every 
		/// type in each module, every methods in each type call all
		/// applicable rules.
		/// </summary>
		public virtual void Run ()
		{
			RunnerEventArgs runner_args = new RunnerEventArgs (this);

			foreach (AssemblyDefinition assembly in assemblies) {
				currentTarget = (IMetadataTokenProvider) assembly;
				runner_args.CurrentAssembly = assembly;
				OnAssembly (runner_args);

				foreach (ModuleDefinition module in assembly.Modules) {
					currentTarget = (IMetadataTokenProvider) module;
					runner_args.CurrentModule = module;
					OnModule (runner_args);

					foreach (TypeDefinition type in module.GetAllTypes ()) {
						currentTarget = (IMetadataTokenProvider) type;
						runner_args.CurrentType = type;
						OnType (runner_args);

						foreach (MethodDefinition method in type.Methods) {
							currentTarget = (IMetadataTokenProvider) method;
							runner_args.CurrentMethod = method;
							OnMethod (runner_args);
						}
					}
				}
			}
			// don't report them if we hit an exception after analysis is completed (e.g. in reporting)
			currentRule = null;
			currentTarget = null;
		}

		public virtual void TearDown ()
		{
			// last chance to report defects
			foreach (Rule rule in rules) {
				currentRule = rule;
				rule.TearDown ();
			}

			currentRule = null;

			if ((engine_dependencies != null) && (engine_dependencies.Length >= 0)) {
				foreach (EngineDependencyAttribute eda in engine_dependencies)
					ec.Unsubscribe (eda.EngineType);
			}

			ec.TearDown ();
		}

		// This is for unit tests.
		public virtual void TearDown (IRule rule)
		{
			currentRule = rule;
			rule.TearDown ();
			currentRule = null;
		}
	}
}
