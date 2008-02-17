//
// Gendarme Console Settings
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
using System.IO;
using System.Reflection;
using System.Xml;

using Gendarme.Framework;

namespace Gendarme {

	public class Settings {

		private IRunner runner;
		private string config_file;
		private string rule_set;

		public Settings (IRunner runner, string configurationFile, string ruleSet)
		{
			this.runner = runner;
			this.config_file = configurationFile;
			this.rule_set = ruleSet;
		}

		static string GetFullPath (string filename)
		{
			if (Path.GetDirectoryName (filename).Length > 0)
				return filename;
			return Path.Combine (Path.GetDirectoryName (ConsoleRunner.Assembly.Location), filename);
		}

		static string GetAttribute (XmlElement xel, string name, string defaultValue)
		{
			XmlAttribute xa = xel.Attributes [name];
			if (xa == null)
				return defaultValue;
			return xa.Value;
		}

		private static bool IsContainedInRuleSet (string rule, string mask)
		{
			string [] ruleSet = mask.Split ('|');
			foreach (string entry in ruleSet) {
				if (String.Compare (rule, entry.Trim (), StringComparison.OrdinalIgnoreCase) == 0)
					return true;
			}
			return false;
		}

		private static bool RuleFilter (Type type, object interfaceName)
		{
			return (type.ToString () == (interfaceName as string));
		}

		private int LoadRulesFromAssembly (string assembly, string includeMask, string excludeMask)
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

				if (t.FindInterfaces (new TypeFilter (RuleFilter), "Gendarme.Framework.IRule").Length > 0) {
					runner.Rules.Add ((IRule) Activator.CreateInstance (t));
					total++;
				}
			}
			return total;
		}

		public bool Load ()
		{
			XmlDocument doc = new XmlDocument ();
			doc.Load (config_file);
			if (doc.DocumentElement.Name != "gendarme")
				return false;

			bool result = false;
			foreach (XmlElement ruleset in doc.DocumentElement.SelectNodes ("ruleset")) {
				if (ruleset.Attributes ["name"].Value != rule_set)
					continue;
				foreach (XmlElement assembly in ruleset.SelectNodes ("rules")) {
					string include = GetAttribute (assembly, "include", "*");
					string exclude = GetAttribute (assembly, "exclude", String.Empty);
					string from = GetFullPath (GetAttribute (assembly, "from", String.Empty));

					int n = LoadRulesFromAssembly (from, include, exclude);
					result = (result || (n > 0));
				}
			}
			return result;
		}
	}
}
