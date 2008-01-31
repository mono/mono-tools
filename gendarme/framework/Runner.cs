//
// Gendarme.Framework.Runner class
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Collections;
using System.Reflection;

using Mono.Cecil;

namespace Gendarme.Framework {

	public abstract class Runner {

		private Rules rules;
		private ViolationCollection violations;
		private static MessageCollection failure = new MessageCollection ();
		protected bool debug = false;

		public Rules Rules {
			get {
				if (rules == null)
					rules = new Rules ();
				return rules;
			}
		}

		public ViolationCollection Violations {
			get {
				if (violations == null)
					violations = new ViolationCollection ();
				return violations;
			}
		}

		public bool Debug {
			get {
				return debug;
			}
		}

		public MessageCollection RuleSuccess {
			get {
				return null;
			}
		}

		public MessageCollection RuleFailure {
			get {
				return failure;
			}
		}

		private static IRule CreateRuleFromType (Type type)
		{
			return (IRule) Activator.CreateInstance (type);
		}

		private static bool IsContainedInRuleSet (string rule, string mask)
		{
			string[] ruleSet = mask.Split ('|');
			foreach (string entry in ruleSet) {
				if (String.Compare (rule, entry.Trim ()) == 0)
					return true;
			}
			return false;
		}

		public int LoadRulesFromAssembly (string assembly, string includeMask, string excludeMask)
		{
			int total = 0;
			Assembly a = Assembly.LoadFile (Path.GetFullPath (assembly));
			foreach (Type t in a.GetTypes ()) {
				if (t.IsAbstract || t.IsInterface)
					continue;
				
				if (includeMask != "*")
					if (!IsContainedInRuleSet (t.Name, includeMask))
						continue;
				
				if ((excludeMask != null) && (excludeMask.Length > 0)) 
					if (IsContainedInRuleSet (t.Name, excludeMask))
						continue;

				LoadRules (typeof (IAssemblyRule), t, Rules.Assembly, ref total);
				LoadRules (typeof (IModuleRule), t, Rules.Module, ref total);
				LoadRules (typeof (ITypeRule), t, Rules.Type, ref total);
				LoadRules (typeof (IMethodRule), t, Rules.Method, ref total);
			}
			return total;
		}

		static void LoadRules (Type rule, Type type, RuleCollection rules, ref int count)
		{
			if (!rule.IsAssignableFrom (type))
				return;

			rules.Add (CreateRuleFromType (type));
			count++;
		}

		public void Process (AssemblyDefinition assembly)
		{
			CheckAssembly (assembly);

			foreach (ModuleDefinition module in assembly.Modules) {
				CheckModule (module);

				foreach (TypeDefinition type in module.Types)
					CheckType (type);
			}
		}

		void CheckAssembly (AssemblyDefinition assembly)
		{
			foreach (IAssemblyRule rule in Rules.Assembly)
				ProcessMessages (rule.CheckAssembly (assembly, this), rule, assembly);
		}

		void CheckModule (ModuleDefinition module)
		{
			foreach (IModuleRule rule in Rules.Module)
				ProcessMessages (rule.CheckModule (module, this), rule, module);
		}

		void CheckType (TypeDefinition type)
		{
			foreach (ITypeRule rule in Rules.Type)
				ProcessMessages (rule.CheckType (type, this), rule, type);

			CheckMethods (type);
		}

		void CheckMethods (TypeDefinition type)
		{
			CheckMethods (type, type.Constructors);
			CheckMethods (type, type.Methods);
		}

		void CheckMethods (TypeDefinition type, ICollection methods)
		{
			foreach (MethodDefinition method in methods)
				foreach (IMethodRule rule in Rules.Method)
					ProcessMessages (rule.CheckMethod (method, this), rule, method);
		}

		void ProcessMessages (MessageCollection messages, IRule rule, object target)
		{
			if (messages == RuleSuccess)
				return;

			Violations.Add (rule, target, messages);
		}
	}
}
