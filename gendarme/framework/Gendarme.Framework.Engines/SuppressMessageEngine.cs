// 
// Gendarme.Framework.Engines.SuppressMessageEngine
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
//

using System;
using System.Collections;
using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework.Rocks;

namespace Gendarme.Framework.Engines {

	public class SuppressMessageEngine : Engine {

		const string SuppressMessage = "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute";

		static Type fxCopCompatibility = typeof (FxCopCompatibilityAttribute);
		private Dictionary<string, string> mapper;

		public override void Initialize (EngineController controller)
		{
			base.Initialize (controller);
			controller.BuildingAssembly += new EventHandler<EngineEventArgs> (OnAssembly);

			mapper = new Dictionary<string,string> ();
			foreach (IRule rule in Controller.Runner.Rules) {
				Type type = rule.GetType ();
				object [] attrs = type.GetCustomAttributes (fxCopCompatibility, true);
				if (attrs.Length == 0)
					continue;
				// one Gendarme rule can be mapped to several FxCop rules
				// one FxCop rules can be split across several Gendarme rules
				foreach (FxCopCompatibilityAttribute attr in attrs) {
					mapper.Add (attr.Category + "." + attr.CheckId, rule.FullName);
				}
			}
		}

		void OnAssembly (object sender, EngineEventArgs e)
		{
			// we only need to check the custom attributes if [SuppressMessage] is referenced (note: won't work for mscorlib)
			AssemblyDefinition assembly = (sender as AssemblyDefinition);
			if (assembly.MainModule.HasTypeReference (SuppressMessage)) {
				Controller.BuildingCustomAttributes += new EventHandler<EngineEventArgs> (OnCustomAttributes);
			} else {
				Controller.BuildingCustomAttributes -= new EventHandler<EngineEventArgs> (OnCustomAttributes);
			}
		}

		static bool TryGetPropertyArgument (ICustomAttribute attribute, string name, out CustomAttributeArgument argument)
		{
			foreach (var namedArg in attribute.Properties) {
				if (namedArg.Name == name) {
					argument = namedArg.Argument;
					return true;
				}
			}

			argument = default (CustomAttributeArgument);
			return false;
		}

		void OnCustomAttributes (object sender, EngineEventArgs e)
		{
			ICustomAttributeProvider cap = (sender as ICustomAttributeProvider);
			if (!cap.HasCustomAttributes)
				return;

			Dictionary<string, HashSet<string>> targets = null;

			// deal with Target only for global (asembly-level) attributes
			bool global = (sender is AssemblyDefinition);

			foreach (CustomAttribute ca in cap.CustomAttributes) {
				if (ca.AttributeType.FullName != SuppressMessage)
					continue;

				var arguments = ca.ConstructorArguments;
				string category = (string) arguments [0].Value;
				string checkId = (string) arguments [1].Value;
				if (String.IsNullOrEmpty (category) || String.IsNullOrEmpty (checkId))
					continue;

				IMetadataTokenProvider token = (sender as IMetadataTokenProvider);
				string name = category + "." + checkId;

				// map from FxCop - otherwise keep the Gendarme syntax
				string mapped_name;
				if (!mapper.TryGetValue (name, out mapped_name))
					mapped_name = name;

				// FIXME: Scope ? "member", "resource", "module", "type", "method", or "namespace"

				string target = null;
				if (global) {
					CustomAttributeArgument targetArgument;
					if (TryGetPropertyArgument (ca, "Target", out targetArgument))
						target = (string) targetArgument.Value;
				}

				if (String.IsNullOrEmpty (target)) {
					Controller.Runner.IgnoreList.Add (mapped_name, token);
					// continue loop - [SuppressMessage] has AllowMultiple == true
					continue;
				}

				// we do not want to look for each target individually since we do not know
				// what they represent. Running the "big" loop one time is more than enough

				HashSet<string> list = null;
				if (targets == null)
					targets = new Dictionary<string,HashSet<string>> ();

				// inner types syntax fix
				target = target.Replace ('+', '/');
				// method/member syntax fix
				target = target.Replace (".#", "::");

				if (!targets.TryGetValue (target, out list)) {
					list = new HashSet<string> ();
					targets.Add (target, list);
				}
				list.AddIfNew (mapped_name);
			}

			if (targets != null)
				Resolve (targets);
		}

		// scan the analysis code a single time looking for targets
		private void Resolve (Dictionary<string,HashSet<string>> targets)
		{
			HashSet<string> rules;
			// scan all code and look for targets
			foreach (AssemblyDefinition assembly in Controller.Runner.Assemblies) {
				// TODO ...
				foreach (ModuleDefinition module in assembly.Modules) {
					// TODO ...
					foreach (TypeDefinition type in module.GetAllTypes ()) {
						if (targets.TryGetValue (type.FullName, out rules))
							Add (type, rules);

						if (type.HasMethods) {
							foreach (MethodDefinition method in type.Methods)
								ResolveMethod (method, targets);
						}
					}
				}
			}
		}

		private void ResolveMethod (IMetadataTokenProvider method, IDictionary<string, HashSet<string>> targets)
		{
			HashSet<string> rules;

			string m = method.ToString ();
			m = m.Substring (m.IndexOf (' ') + 1);

			if (targets.TryGetValue (m, out rules))
				Add (method, rules);
		}

		private void Add (IMetadataTokenProvider metadata, IEnumerable<string> rules)
		{
			foreach (string rule in rules) {
				Controller.Runner.IgnoreList.Add (rule, metadata);
			}
		}
	}
}
