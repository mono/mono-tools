//
// GuiRunner.cs: A SWF-based Wizard Runner for Gendarme
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
using System.IO;
using System.Reflection;

using Gendarme.Framework;

using Mono.Cecil;

namespace Gendarme {

	public class GuiRunner : Runner, IIgnoreList {

		private Wizard wizard;

		public GuiRunner (Wizard form)
		{
			wizard = form;
			IgnoreList = this;
		}

		private static bool RuleFilter (Type type, object interfaceName)
		{
			return (type.ToString () == (interfaceName as string));
		}

		private void LoadRulesFromAssembly (string assemblyName)
		{
			Assembly a = Assembly.LoadFile (Path.GetFullPath (assemblyName));
			foreach (Type t in a.GetTypes ()) {
				if (t.IsAbstract || t.IsInterface)
					continue;

				if (t.FindInterfaces (new TypeFilter (RuleFilter), "Gendarme.Framework.IRule").Length > 0) {
					Rules.Add ((IRule) Activator.CreateInstance (t));
				}
			}
		}

		public void LoadRules ()
		{
			// load every dll to check for rules...
			string dir = Path.GetDirectoryName (typeof (IRule).Assembly.Location);
			FileInfo [] files = new DirectoryInfo (dir).GetFiles ("*.dll");
			foreach (FileInfo info in files) {
				// except for a few, well known, ones
				switch (info.FullName) {
				case "Mono.Cecil.dll":
				case "Mono.Cecil.Pdb.dll":
				case "Mono.Cecil.Mdb.dll":
				case "Gendarme.Framework.dll":
					continue;
				}

				LoadRulesFromAssembly (info.FullName);
			}
		}

		protected override void OnAssembly (RunnerEventArgs e)
		{
			// update wizard UI on the main, i.e. UI, thread
			wizard.BeginInvoke ((Action) (() => wizard.PreUpdate (e)));
			base.OnAssembly (e);
			wizard.BeginInvoke ((Action) (() => wizard.PostUpdate (e)));
		}


		// Ignore List is not supported by the Wizard runner

		public bool IsIgnored (IRule rule, AssemblyDefinition assembly)
		{
			return !rule.Active;
		}

		public bool IsIgnored (IRule rule, TypeDefinition type)
		{
			return !rule.Active;
		}

		public bool IsIgnored (IRule rule, MethodDefinition method)
		{
			return !rule.Active;
		}
	}
}
