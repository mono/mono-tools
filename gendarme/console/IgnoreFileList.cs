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

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme {

	public class IgnoreFileList : BasicIgnoreList {

		private string current_rule;
		private string currentFileName;
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
			if (String.IsNullOrEmpty (fileName))
				return;

			if (File.Exists (fileName) && !files.Contains (fileName)) {
				files.Push (fileName);
			}
			else {
				var directory = Path.GetDirectoryName (currentFileName);
				if (!string.IsNullOrEmpty (directory) && !fileName.StartsWith (directory)){
					Push (Path.Combine (directory, fileName));
				}
			}
		}

		private void Parse ()
		{
			char [] buffer = new char [4096];
			while (files.Count > 0) {
				currentFileName = files.Pop ();
				using (StreamLineReader sr = new StreamLineReader (currentFileName)) {
					while (!sr.EndOfStream) {
						int length = sr.ReadLine (buffer, 0, buffer.Length);
						ProcessLine (buffer, length);
					}
				}
			}
			currentFileName = null;
			Resolve();
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
				current_rule = GetString (buffer, length);
				break;
			case 'A': // assembly - we support Name, FullName and *
				string target = GetString (buffer, length);
				if (target == "*") {
					foreach (AssemblyDefinition assembly in Runner.Assemblies) {
						Add (assemblies, current_rule, assembly.Name.FullName);
					}
				} else {
					Add (assemblies, current_rule, target);
				}
				break;
			case 'T': // type (no space allowed)
				Add (types, current_rule, GetString (buffer, length));
				break;
			case 'M': // method
				Add (methods, current_rule, GetString (buffer, length));
				break;
			case 'N': // namespace - special case (no need to resolve)
				base.Add (current_rule, NamespaceDefinition.GetDefinition (GetString (buffer, length)));
				break;
			case '@': // include file
				Push (GetString (buffer, length));
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
			HashSet<string> rules;

			foreach (AssemblyDefinition assembly in Runner.Assemblies) {
				if (assemblies.TryGetValue (assembly.Name.FullName, out rules)) {
					AddList (assembly, rules);
				}
				if (assemblies.TryGetValue (assembly.Name.Name, out rules)) {
					AddList (assembly, rules);
				}

				foreach (ModuleDefinition module in assembly.Modules) {
					foreach (TypeDefinition type in module.GetAllTypes ()) {
						if (types.TryGetValue (type.GetFullName (), out rules)) {
							AddList (type, rules);
						}

						if (type.HasMethods) {
							foreach (MethodDefinition method in type.Methods) {
								if (methods.TryGetValue (method.GetFullName (), out rules)) {
									AddList (method, rules);
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
}
