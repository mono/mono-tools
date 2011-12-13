//
// IgnoreFileList - Ignore defects based on a file descriptions
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008-2011 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme {

	public class IgnoreFileList : BasicIgnoreList {

		private List<string> current_rules = new List<string>();

		private Dictionary<string, HashSet<string>> assemblies = new Dictionary<string, HashSet<string>> ();
		private Dictionary<string, HashSet<string>> types = new Dictionary<string, HashSet<string>> ();
		private Dictionary<string, HashSet<string>> methods = new Dictionary<string, HashSet<string>> ();
		private Stack<string> files = new Stack<string> ();

		public IgnoreFileList (IRunner runner, string fileName)
			: base (runner)
		{
			Push (fileName);
			Parse ();
		}

		private void Push (string fileName)
		{
			if (!String.IsNullOrEmpty (fileName) && File.Exists (fileName) && !files.Contains (fileName)) {
				files.Push (fileName);
			}
		}

		private void Parse ()
		{
			char [] buffer = new char [4096];
			while (files.Count > 0) {
				string fileName = files.Pop ();
				using (StreamLineReader sr = new StreamLineReader (fileName)) {
					while (!sr.EndOfStream) {
						int length = sr.ReadLine (buffer, 0, buffer.Length);
						ProcessLine (buffer, length);
					}
				}
			}
			Resolve ();
			TearDown ();
		}

		static private void Add (IDictionary<string, HashSet<string>> list, string rule, string target)
		{
			HashSet<string> rules;

			if (!list.TryGetValue (target, out rules)) {
				rules = new HashSet<string> ();
				list.Add (target, rules);
			}

			rules.Add (rule);
		}

		static string GetString (char [] buffer, int length)
		{
			// skip the 'type' + ':' characters when looking for whitespace separator(s)
			int start = 2;
			while (Char.IsWhiteSpace (buffer [start]) && (start < buffer.Length))
				start++;

			int end = length;
			while (Char.IsWhiteSpace (buffer [end]) && (end >= start))
				end--;

			return new string (buffer, start, end - start);
		}

		private void ProcessLine (char [] buffer, int length)
		{
			if (length < 1)
				return;

			switch (buffer [0]) {
			case '#': // comment
				break;
			case 'R': // rule
				string current_rule_glob = GetString (buffer, length);

				foreach (IRule rule in Runner.Rules) {
					if (rule.FullName.GlobMatch(current_rule_glob)) {
						current_rules.Add(rule.FullName);
					}
				}
				break;
			case 'A': // assembly - we support Name, FullName and *
				string target = GetString (buffer, length);
				foreach (string current_rule in current_rules) {
						Add(assemblies, current_rule, target);
				}
				break;
			case 'T': // type (no space allowed)
				foreach (string current_rule in current_rules) {
					Add(types, current_rule, GetString(buffer, length));
				}
				break;
			case 'M': // method
				foreach (string current_rule in current_rules) {
					Add(methods, current_rule, GetString(buffer, length));
				}
				break;
			case 'N': // namespace - special case (no need to resolve)
				foreach (string current_rule in current_rules) {
					base.Add(current_rule, NamespaceDefinition.GetDefinition(GetString(buffer, length)));
				}
				break;
			case '@': // include file
				files.Push (GetString (buffer, length));
				break;
			default:
				Console.Error.WriteLine ("Bad ignore entry : '{0}'", new string (buffer));
				break;
			}
		}

		private void AddList (IMetadataTokenProvider metadata, IEnumerable<string> rules)
		{
			foreach (string rule in rules) {
				base.Add (rule, metadata);
			}
		}

		// scan the analyzed code a single time looking for targets
		private void Resolve ()
		{
			foreach (AssemblyDefinition assembly in Runner.Assemblies) {
				AssemblyDefinition assembly1 = assembly;
				foreach (var rules in assemblies
							.Where(x => assembly1.Name.Name.GlobMatch(x.Key))
							.Select(x => x.Value)) {
					AddList(assembly, rules);
				}
				foreach (var rules in assemblies
							.Where(x => assembly1.Name.FullName.GlobMatch(x.Key))
							.Select(x => x.Value)) {
					AddList(assembly, rules);
				}

				foreach (ModuleDefinition module in assembly.Modules) {
					foreach (TypeDefinition type in module.GetAllTypes ()) {
						TypeDefinition type1 = type;
						foreach (var rules in types
									.Where(x => type1.GetFullName().GlobMatch(x.Key))
									.Select(x => x.Value)) {
							AddList(type, rules);
						}

						if (type.HasMethods) {
							foreach (MethodDefinition method in type.Methods) {
								MethodDefinition method1 = method;
								foreach (var rules in methods
											.Where(x => method1.GetFullName().GlobMatch(x.Key))
											.Select(x => x.Value)) {
									AddList(method, rules);
								}
							}
						}
					}
				}
			}
		}

		private void TearDown ()
		{
			assemblies.Clear ();
			types.Clear ();
			methods.Clear ();
		}
	}

	public static class StringExtensions
	{
		// Returns true if the globPattern matches the given string, where any "*" characters in the glob pattern are expanded to a reges .*
		public static bool GlobMatch(this string str, string globPattern)
		{
			if (globPattern.IndexOf('*') < 0)
				return str.Equals(globPattern, StringComparison.InvariantCulture);
			return Regex.IsMatch(str, "^" + Regex.Escape(globPattern).Replace(@"\*", @".*") + "$");
		}
	}
}
