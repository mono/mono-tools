//
// Gendarme Console Runner
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005-2008 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Text;
using System.Xml;

using Mono.Cecil;

using Gendarme.Framework;

using NDesk.Options;

namespace Gendarme {

	public class ConsoleRunner : Runner {

		private string config_file = "rules.xml";
		private string rule_set = "*";
		private string html_file;
		private string log_file;
		private string xml_file;
		private bool help;
		private bool quiet;
		private List<string> assembly_names;

		int Parse (string [] args)
		{
			var p = new OptionSet () {
				{ "config=",	v => config_file = v },
				{ "set=",	v => rule_set = v },
				{ "log=",	v => log_file = v },
				{ "xml=",	v => xml_file = v },
				{ "html=",	v => html_file = v },
				{ "v|verbose",  v => ++VerbosityLevel },
				{ "quiet",	v => quiet = v != null },
				{ "h|?|help",	v => help = v != null },
			};
			assembly_names = p.Parse (args);
			return (assembly_names.Count > 0) ? 0 : 1;
		}

		// name can be
		// - a filename (a single assembly)
		// - a mask (*, ?) for multiple assemblies
		// - a special file (@) containing a list of assemblies
		int AddFiles (string name)
		{
			if (String.IsNullOrEmpty (name))
				return 0;

			if (name.StartsWith ("@", StringComparison.OrdinalIgnoreCase)) {
				// note: recursive (can contains @, masks and filenames)
				using (StreamReader sr = File.OpenText (name.Substring (1))) {
					while (sr.Peek () >= 0) {
						AddFiles (sr.ReadLine ());
					}
				}
			} else if (name.IndexOfAny (new char [] { '*', '?' }) >= 0) {
				string dirname = Path.GetDirectoryName (name);
				if (dirname.Length == 0)
					dirname = "."; // assume current directory
				string [] files = Directory.GetFiles (dirname, Path.GetFileName (name));
				foreach (string file in files) {
					AddAssembly (file);
				}
			} else {
				AddAssembly (name);
			}
			return 0;
		}

		void AddAssembly (string filename)
		{
			string assembly_name = Path.GetFullPath (filename);
			AssemblyDefinition ad = AssemblyFactory.GetAssembly (assembly_name);
			(ad as IAnnotationProvider).Annotations.Add ("filename", assembly_name);
			Assemblies.Add (ad);
		}

		static string GetFullPath (string filename)
		{
			if (Path.GetDirectoryName (filename).Length > 0)
				return filename;
			return Path.Combine (Path.GetDirectoryName (Assembly.Location), filename);
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
				if (String.Compare (rule, entry.Trim ()) == 0)
					return true;
			}
			return false;
		}

		public static bool RuleFilter (Type type, object interfaceName)
		{
			return (type.ToString () == (interfaceName as string));
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

				if (t.FindInterfaces (new TypeFilter (RuleFilter), "Gendarme.Framework.IRule").Length > 0) {
					Rules.Add ((IRule) Activator.CreateInstance (t));
					total++;
				}
			}
			return total;
		}

		bool LoadConfiguration ()
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

		int Report ()
		{
			// generate text report (default, to console, if xml and html aren't specified)
			if ((log_file != null) || ((xml_file == null) && (html_file == null))) {
				using (TextResultWriter writer = new TextResultWriter (this, log_file)) {
					writer.Report ();
				}
			}

			// generate XML report
			if (xml_file != null) {
				using (XmlResultWriter writer = new XmlResultWriter (this, xml_file)) {
					writer.Report ();
				}
			}

			// generate HTML report
			if (html_file != null) {
				using (HtmlResultWriter writer = new HtmlResultWriter (this, html_file)) {
					writer.Report ();
				}
			}
			return 0;
		}

		int Execute (string [] args)
		{
			try {
				int result = Parse (args);
				if ((result != 0) || help) {
					Help ();
					return result;
				}

				Header ();

				// load configuration, including rules, and continue if
				// there's at least one rule to execute
				if (!LoadConfiguration () || (Rules.Count < 1))
					return 3;

				foreach (string name in assembly_names) {
					result = AddFiles (name);
					if (result != 0)
						return result;
				}

				// now that all rules and assemblies are know, time to initialize
				Initialize ();
				// before analizing the assemblies with the rules
				Run ();

				return Report ();
			}
			catch (Exception e) {
				Console.WriteLine ("Uncatched exception occured. Please fill a bug report.");
				Console.WriteLine ("Rule:\t{0}", CurrentRule);
				Console.WriteLine ("Target:\t{0}", CurrentTarget);
				Console.WriteLine ("Stack trace: {0}", e);
				return 4;
			}
		}

		private DateTime timer = DateTime.MinValue;

		public override void Run ()
		{
			DateTime start = DateTime.UtcNow;
			base.Run ();
			DateTime end = DateTime.UtcNow;
			Console.WriteLine (": {0} seconds.", (end - timer).TotalSeconds);
			Console.WriteLine ();
			Console.WriteLine ("{0}{1} assemblies processed in {2} seconds.{0}",
				Environment.NewLine, Assemblies.Count, (DateTime.UtcNow - start).TotalSeconds);
		}

		protected override void OnAssembly (RunnerEventArgs e)
		{
			if (timer != DateTime.MinValue)
				Console.WriteLine (": {0} seconds.", (DateTime.UtcNow - timer).TotalSeconds);
			// next assembly
			Console.Write ((e.CurrentAssembly as IAnnotationProvider).Annotations ["filename"]);
			timer = DateTime.UtcNow;
			base.OnAssembly (e);
		}

		void Header ()
		{
			if (quiet)
				return;

			Assembly a = Assembly.GetExecutingAssembly ();
			Version v = a.GetName ().Version;
			if (v.ToString () != "0.0.0.0") {
				Console.WriteLine ("Gendarme v{0}", v);
				object [] attr = a.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
				if (attr.Length > 0)
					Console.WriteLine (((AssemblyCopyrightAttribute) attr [0]).Copyright);
			} else {
				Console.WriteLine ("Gendarme - Development Snapshot");
			}
			Console.WriteLine ();
		}

		private static Assembly assembly;

		static Assembly Assembly {
			get {
				if (assembly == null)
					assembly = Assembly.GetExecutingAssembly ();
				return assembly;
			}
		}

		static void Help ()
		{
			Console.WriteLine ("Usage: gendarme [--config file] [--set ruleset] [--{log|xml|html} file] assembly");
			Console.WriteLine ("Where");
			Console.WriteLine ("  --config file\t\tSpecify the configuration file. Default is 'rules.xml'.");
			Console.WriteLine ("  --set ruleset\t\tSpecify the set of rules to verify. Default is '*'.");
			Console.WriteLine ("  --log file\t\tSave the text output to the specified file.");
			Console.WriteLine ("  --xml file\t\tSave the output, as XML, to the specified file.");
			Console.WriteLine ("  --html file\t\tSave the output, as HTML, to the specified file.");
			Console.WriteLine ("  --quiet\t\tDisplay minimal output (results) from the runner.");
			Console.WriteLine ("  --v\t\tEnable debugging output (use multiple time to augment verbosity).");
			Console.WriteLine ("  assembly\t\tSpecify the assembly to verify.");
			Console.WriteLine ();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns>0 for success, 
		/// 1 if some defects are found, 
		/// 2 if some parameters are bad,
		/// 3 if a problem is related to the xml configuration file
		/// 4 if an uncatched exception occured</returns>
		static int Main (string [] args)
		{
			return new ConsoleRunner ().Execute (args);
		}
	}
}
