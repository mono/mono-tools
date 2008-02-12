// 
// Gendarme.Framework.Runner
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Framework {

	abstract public class Runner : IRunner {

		Dictionary<IRule, string> ignore_list = new Dictionary <IRule, string> ();
		Collection<Defect> defect_list = new Collection<Defect> ();

		Collection<IRule> rules = new Collection<IRule> ();
		Dictionary<string, AssemblyDefinition> assemblies = new Dictionary<string, AssemblyDefinition> ();
		private int verbose_level;

		public event EventHandler<RunnerEventArgs> AnalyzeAssembly;	// ??? ProcessAssembly ???
		public event EventHandler<RunnerEventArgs> AnalyzeModule;
		public event EventHandler<RunnerEventArgs> AnalyzeType;
		public event EventHandler<RunnerEventArgs> AnalyzeMethod;

		public Collection<IRule> Rules {
			get { return rules; }
		}

		public Dictionary<string,AssemblyDefinition> Assemblies {
			get { return assemblies; }
		}

		public Collection<Defect> Defects {
			get { return defect_list; }
		}

		public int VerbosityLevel {
			get { return verbose_level; }
			set { verbose_level = value; }
		}


		// once every assembly are loaded *and* all the rules are known -> we initialized all rules.
		// this ensure that the list of assemblies is available at rule initialization time
		// which allows caching information and treating the assemblies as "a set"
		public void Initialize ()
		{
			foreach (AssemblyDefinition assembly in assemblies.Values) {
				try {
					assembly.MainModule.LoadSymbols ();
				}
				catch (COMException) {
					// this happens if a PDB is missing
				}
			}

			foreach (Rule rule in rules) {
				try {
					rule.Initialize (this);
				}
				catch (Exception e) {
					// if something goes wrong in initialization we desactivate the rule
					if (VerbosityLevel > 0)
						Console.WriteLine (e);
					rule.Active = false;
				}
			}
		}

		public bool IsIgnored (IRule rule, AssemblyDefinition assembly)
		{
			if ((rule == null) || !rule.Active)
				return true;
			if (assembly == null)
				throw new ArgumentNullException ("assembly");

			return false; // (ignore_list.Contains (rule, method.ToString ()));
		}

		public bool IsIgnored (IRule rule, TypeDefinition type)
		{
			if ((rule == null) || !rule.Active)
				return true;
			if (type == null)
				throw new ArgumentNullException ("type");

			// check for type itself (full match)
			// otherwise check for assembly
			return IsIgnored (rule, type.Module.Assembly);
		}

		public bool IsIgnored (IRule rule, MethodDefinition method)
		{
			if ((rule == null) || !rule.Active)
				return true;
			if (method == null)
				throw new ArgumentNullException ("method");

			// check for method itself (full match)
			// otherwise check for its type
			return IsIgnored (rule, (method.DeclaringType as TypeDefinition));
		}

		public void Report (Defect defect)
		{
			if (defect == null)
				throw new ArgumentNullException ("defect");

			defect_list.Add (defect);
		}

		public void Report (IRule rule, AssemblyDefinition assembly, Severity severity, Confidence confidence, string message)
		{
			defect_list.Add (new Defect<AssemblyDefinition> (rule, assembly, severity, confidence, message));
		}

		public void Report (IRule rule, TypeDefinition type, Severity severity, Confidence confidence, string message)
		{
			defect_list.Add (new Defect<TypeDefinition> (rule, type, severity, confidence, message));
		}

		public void Report (IRule rule, FieldDefinition field, Severity severity, Confidence confidence, string message)
		{
			defect_list.Add (new Defect<FieldDefinition> (rule, field, severity, confidence, message));
		}

		public void Report (IRule rule, MethodDefinition method, Severity severity, Confidence confidence, string message)
		{
			defect_list.Add (new Defect<MethodDefinition> (rule, method, severity, confidence, message));
		}

		public void Report (IRule rule, MethodDefinition method, Instruction ins, Severity severity, Confidence confidence, string message)
		{
			defect_list.Add (new Defect<MethodDefinition> (rule, method, ins, severity, confidence, message));
		}

		public void Report (IRule rule, ParameterDefinition parameter, Severity severity, Confidence confidence, string message)
		{
			defect_list.Add (new Defect<ParameterDefinition> (rule, parameter, severity, confidence, message));
		}


		private void OnEvent (EventHandler<RunnerEventArgs> handler, RunnerEventArgs e)
		{
			if (handler != null)
				handler (this, e);
		}

		// protected since a higher-level (e.g. GUI) runner might want to override
		// them to update it's user interface
		protected virtual void OnAssembly (RunnerEventArgs e)
		{
			OnEvent (AnalyzeAssembly, e);
		}

		protected virtual void OnModule (RunnerEventArgs e)
		{
			OnEvent (AnalyzeModule, e);
		}

		protected virtual void OnType (RunnerEventArgs e)
		{
			OnEvent (AnalyzeType, e);
		}

		protected virtual void OnMethod (RunnerEventArgs e)
		{
			OnEvent (AnalyzeMethod, e);
		}
/*
		private void ProcessAssemblies (RunnerEventArgs args)
		{
			foreach (AssemblyDefinition assembly in assemblies) {
				args.CurrentAssembly = assembly;
				OnAssembly (args);

				foreach (IAssemblyRule rule in rules) {
					if (IsIgnored (rule, assembly.ToString ()))
						continue;
					rule.CheckAssembly (assembly);
				}

				ProcessModules (args);
			}
		}

		private void ProcessModules (RunnerEventArgs args)
		{
			foreach (ModuleDefinition module in args.CurrentAssembly.Modules) {
				args.CurrentModule = module;
				OnModule (args);

				// Note: we don't support IModuleRule in this version
				// nor do we ignore on modules

				ProcessTypes (args);
			}
		}

		private void ProcessTypes (RunnerEventArgs args)
		{
			foreach (TypeDefinition type in args.CurrentModule.Types) {
				args.CurrentType = type;
				OnType (args);

				ProcessMethods (args);
			}
		}

		private void ProcessTypes (RunnerEventArgs args)
		{
		}
*/
		public void Run ()
		{
			RunnerEventArgs runner_args = new RunnerEventArgs (this);

			IEnumerable<IAssemblyRule> assembly_rules = rules.OfType<IAssemblyRule> ();
			IEnumerable<ITypeRule> type_rules = rules.OfType<ITypeRule> ();
			IEnumerable<IMethodRule> method_rules = rules.OfType<IMethodRule> ();

			foreach (AssemblyDefinition assembly in assemblies.Values) {
				runner_args.CurrentAssembly = assembly;
				OnAssembly (runner_args);

				foreach (IAssemblyRule rule in assembly_rules) {
					if (IsIgnored (rule, assembly))
						continue;
					rule.CheckAssembly (assembly);
				}

				foreach (ModuleDefinition module in assembly.Modules) {
					runner_args.CurrentModule = module;
					OnModule (runner_args);

					// Since it has not been used in the previous years this version
					// doesn't support IModuleRule nor do we ignore on modules

					foreach (TypeDefinition type in module.Types) {
						runner_args.CurrentType = type;
						OnType (runner_args);

						foreach (ITypeRule rule in type_rules) {
							if (IsIgnored (rule, type))
								continue;
							rule.CheckType (type);
						}

						foreach (MethodDefinition constructor in type.Constructors) {
							runner_args.CurrentMethod = constructor;
							OnMethod (runner_args);

							foreach (IMethodRule rule in method_rules) {
								if (IsIgnored (rule, constructor))
									continue;
								rule.CheckMethod (constructor);
							}
						}

						foreach (MethodDefinition method in type.Methods) {
							runner_args.CurrentMethod = method;
							OnMethod (runner_args);

							foreach (IMethodRule rule in method_rules) {
								if (IsIgnored (rule, method))
									continue;
								rule.CheckMethod (method);
							}
						}
					}
				}
			}
		}
	}
}
