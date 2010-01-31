//
// BasicIgnoreList
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;

using Mono.Cecil;

using Gendarme.Framework.Rocks;

namespace Gendarme.Framework {

	/// <summary>
	/// Basic ignore list implementation.
	/// </summary>
	public class BasicIgnoreList : IIgnoreList {

		sealed class Metadata {
			List<AssemblyDefinition> assemblies;
			List<TypeDefinition> types;
			List<MethodDefinition> methods;

			public List<AssemblyDefinition> Assemblies {
				get {
					if (assemblies == null)
						assemblies = new List<AssemblyDefinition> ();
					return assemblies;
				}
			}

			public List<TypeDefinition> Types {
				get {
					if (types == null)
						types = new List<TypeDefinition> ();
					return types;
				}
			}

			public List<MethodDefinition> Methods {
				get {
					if (methods == null)
						methods = new List<MethodDefinition> ();
					return methods;
				}
			}
		}

		private SortedList<string, Metadata> list;
		private IRunner runner;

		// note: we should keep statistics here
		// e.g. # of times a rule is ignored because it's inactive
		// e.g. # of times a rule is ignored because of users directives

		public BasicIgnoreList (IRunner runner)
		{
			this.runner = runner;
		}

		private Metadata GetMetadata (string rule, bool create)
		{
			if (rule == null)
				throw new ArgumentNullException ("rule");

			if (list == null)
				list = new SortedList<string, Metadata> ();
			else if (list.ContainsKey (rule))
				return list [rule];

			if (!create)
				return null;

			// ensure rule exists
			foreach (IRule value in runner.Rules) {
				if (rule == value.FullName) {
					Metadata meta = new Metadata ();
					list.Add (rule, meta);
					return meta;
				}
			}
			return null;
		}

		private void Add (string rule, AssemblyDefinition assembly)
		{
			Metadata m = GetMetadata (rule, true);
			if (m != null)
				m.Assemblies.Add (assembly);
		}

		private void Add (string rule, TypeDefinition type)
		{
			Metadata m = GetMetadata (rule, true);
			if (m != null)
				m.Types.Add (type);
		}

		private void Add (string rule, MethodDefinition method)
		{
			Metadata m = GetMetadata (rule, true);
			if (m != null)
				m.Methods.Add (method);
		}

		protected bool AddAssembly (string rule, string assembly)
		{
			bool found = false;

			foreach (AssemblyDefinition definition in runner.Assemblies) {
				// check either the full name or only the name (as the version number will likely
				// change and makes the fullname less useful in a separate ignore file)
				if ((definition.Name.FullName == assembly) || (definition.Name.Name == assembly)) {
					Add (rule, definition);
					found = true; // same assembly can exist elsewhere
				}
			}
			return found;
		}

		protected bool AddType (string rule, string type)
		{
			bool found = false;
			
			foreach (AssemblyDefinition assembly in runner.Assemblies) {
				foreach (ModuleDefinition module in assembly.Modules) {
					TypeDefinition definition = module.Types [type];
					if (definition != null) {
						Add (rule, definition);
						found = true; // same type can exist elsewhere
					}
				}
			}
			return found;
		}

		protected bool AddMethod (string rule, string method)
		{
			bool found = false;

			foreach (AssemblyDefinition assembly in runner.Assemblies) {
				foreach (ModuleDefinition module in assembly.Modules) {
					foreach (TypeDefinition type in module.Types) {
						foreach (MethodDefinition definition in type.AllMethods ()) {
							if (method == definition.ToString ()) {
								Add (rule, definition);
								found = true; // same method can exist elsewhere
							}
						}
					}
				}
			}
			return found;
		}

		private static bool CheckRule (IRule rule)
		{
			return ((rule == null) || !rule.Active);
		}

		public bool IsIgnored (IRule rule, AssemblyDefinition assembly)
		{
			// Note that the Runner tearing_down code may call us with nulls.
			if (assembly == null)
				return false;
				
			if (CheckRule (rule))
				return true;

			Metadata data = GetMetadata (rule.FullName, false);
			if (data == null)
				return false;
			return data.Assemblies.Contains (assembly);
		}

		public bool IsIgnored (IRule rule, TypeDefinition type)
		{
			// Note that the Runner tearing_down code may call us with nulls.
			if (type == null)
				return false;
				
			if (CheckRule (rule))
				return true;

			Metadata data = GetMetadata (rule.FullName, false);
			if (data == null)
				return false;
			if (data.Types.Contains (type))
				return true;
			return (data.Assemblies.Contains (type.Module.Assembly));
		}

		public bool IsIgnored (IRule rule, MethodDefinition method)
		{
			// Note that the Runner tearing_down code may call us with nulls.
			if (method == null)
				return false;
				
			if (CheckRule (rule))
				return true;

			Metadata data = GetMetadata (rule.FullName, false);
			if (data == null)
				return false;
			if (data.Methods.Contains (method))
				return true;
			if (data.Types.Contains (method.DeclaringType.Resolve ()))
				return true;
			return (data.Assemblies.Contains (method.DeclaringType.Module.Assembly));
		}
	}
}
