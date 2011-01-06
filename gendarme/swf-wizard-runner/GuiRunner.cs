//
// GuiRunner.cs: A SWF-based Wizard Runner for Gendarme
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
using System.Reflection;
using System.Text;

using Gendarme.Framework;
using Gendarme.Framework.Engines;

using Mono.Cecil;

namespace Gendarme {

	[EngineDependency (typeof (SuppressMessageEngine))]
	public class GuiRunner : Runner {

		private Wizard wizard;
		private StringBuilder unexpected = new StringBuilder ();
		private StringBuilder warnings = new StringBuilder ();

		private static TypeFilter RuleTypeFilter = new TypeFilter (RuleFilter);

		public GuiRunner (Wizard form)
		{
			wizard = form;
			IgnoreList = new BasicIgnoreList (this);
		}

		public string Error {
			get { return unexpected.ToString (); }
		}

		public string Warnings {
			get { return warnings.ToString (); }
		}

		private static bool RuleFilter (Type type, object interfaceName)
		{
			return (type.ToString () == (interfaceName as string));
		}

		private void LoadRulesFromAssembly (string assemblyName)
		{
			AssemblyName aname = AssemblyName.GetAssemblyName (Path.GetFullPath (assemblyName));
			Assembly a = Assembly.Load (aname);
			foreach (Type t in a.GetTypes ()) {
				if (t.IsAbstract || t.IsInterface)
					continue;

				if (t.FindInterfaces (RuleTypeFilter, "Gendarme.Framework.IRule").Length > 0) {
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
				switch (info.Name) {
				case "Mono.Cecil.dll":
				case "Mono.Cecil.Pdb.dll":
				case "Mono.Cecil.Mdb.dll":
				case "Gendarme.Framework.dll":
					continue;
				}

				LoadRulesFromAssembly (info.FullName);
			}
		}

		public void Execute ()
		{
			try {
				unexpected.Length = 0;

				Initialize ();
				Run ();
				TearDown ();
			}
			catch (Exception e) {
				if (CurrentRule != null)
					unexpected.AppendFormat ("Rule:\t{0}{1}{1}", CurrentRule, Environment.NewLine);
				if (CurrentTarget != null)
					unexpected.AppendFormat ("Target:\t{0}{1}{1}", CurrentTarget, Environment.NewLine);
				unexpected.AppendFormat ("Stack trace: {0}", e);
			}
		}

		protected override void OnAssembly (RunnerEventArgs e)
		{
			// update wizard UI on the main, i.e. UI, thread
			wizard.BeginInvoke ((Action) (() => wizard.PreAssemblyUpdate (e)));
			base.OnAssembly (e);
		}

		protected override void OnType (RunnerEventArgs e)
		{
			base.OnType (e);
			wizard.BeginInvoke ((Action) (() => wizard.PostTypeUpdate (e)));
		}

		public void Warn (string warning)
		{
			warnings.AppendLine (warning);
		}
	}
}
