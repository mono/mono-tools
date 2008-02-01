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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using Mono.Cecil;
using Gendarme.Framework;
using Gendarme.Console.Writers;

class ConsoleRunner : Runner {

	private const string defaultConfiguration = "rules.xml";
	private const string defaultRuleSet = "default";

	private string config;
	private string set;
	private Hashtable assemblies;
	private string format;
	private string output;

	private static Assembly assembly;
	private bool quiet;

	static Assembly Assembly {
		get {
			if (assembly == null)
				assembly = Assembly.GetExecutingAssembly ();
			return assembly;
		}
	}

	static string GetFullPath (string filename)
	{
		if (Path.GetDirectoryName (filename) != String.Empty)
			return filename;
		return Path.Combine (Path.GetDirectoryName (Assembly.Location), filename);
	}

	static string GetNext (string[] args, int index, string defaultValue)
	{
		if ((args == null) || (index < 0) || (index >= args.Length))
			return defaultValue;
		return args [index];
	}
	
	// name can be
	// - a filename (a single assembly)
	// - a mask (*, ?) for multiple assemblies
	// - a special file (@) containing a list of assemblies
	void AddFiles (string name)
	{
		if ((name == null) || (name.Length == 0))
			return;
			
		if (name.StartsWith ("@")) {
			// note: recursive (can contains @, masks and filenames)
			using (StreamReader sr = File.OpenText (name.Substring (1))) {
				while (sr.Peek () >= 0) {
					AddFiles (sr.ReadLine ());
				}
			}
		} else if (name.IndexOfAny (new char[] { '*', '?' }) >= 0) {
			string dirname = Path.GetDirectoryName (name);
			if (dirname.Length == 0)
				dirname = "."; // assume current directory
			string [] files = Directory.GetFiles (dirname, Path.GetFileName (name));
			foreach (string file in files) {
				assemblies.Add (Path.GetFullPath (file), null);
			}
		} else {
			assemblies.Add (Path.GetFullPath (name), null);
		}
	}

	bool ParseOptions (string[] args)
	{
		// defaults
		config = GetFullPath (defaultConfiguration);
		set = defaultRuleSet;
		assemblies = new Hashtable ();

		// TODO - we probably want (i.e. later) the possibility to 
		// include/exclude certain rules from executing
		for (int i=0; i < args.Length; i++) {
			switch (args [i]) {
			case "--config":
				config = GetNext (args, ++i, defaultConfiguration);
				break;
			case "--set":
				set = GetNext (args, ++i, defaultRuleSet);
				break;
			case "--debug":
				debug = true;
				break;
			case "--quiet":
				quiet = true;
				break;
			case "--help":
				return false;
			case "--log":
				format = "text";
				output = GetNext (args, ++i, String.Empty);
				break;
			case "--xml":
				format = "xml";
				output = GetNext (args, ++i, String.Empty);
				break;
			case "--html":
				format = "html";
				output = GetNext (args, ++i, String.Empty);
				break;
			default:
				AddFiles (args[i]);
				break;
			}
		}
		return (assemblies.Count > 0);
	}

	bool LoadCustomParameters (XmlElement ruleset) {
		foreach (XmlElement parameter in ruleset.SelectNodes ("parameter")) {
			try {
				if (!parameter.HasAttribute ("name"))
					throw new XmlException ("The attribute name can't be found");
				if (!parameter.HasAttribute ("value"))
					throw new XmlException ("The attribute value can't be found");
				if (!parameter.HasAttribute ("rule"))
					throw new XmlException ("The attribute rule can't be found");
					
				string name = GetAttribute (parameter, "name", String.Empty);
				int value = 0;
				try {
					value = Int32.Parse (GetAttribute (parameter, "value", String.Empty));
				} 
				catch (Exception exception) {
					throw new XmlException ("The value for the value field should be an integer.", exception);
				}
				string ruleName = GetAttribute (parameter, "rule", String.Empty);

				ApplyCustomParameterToRule (ruleName, name, value);
			}
			catch (Exception e) {
				Console.WriteLine ("Error reading parameters{0}Details: {1}", Environment.NewLine, e);
				return false;
			}
		}
		return true;
	}

	static string GetAttribute (XmlElement xel, string name, string defaultValue)
	{
		XmlAttribute xa = xel.Attributes [name];
		if (xa == null)
			return defaultValue;
		return xa.Value;
	}

	bool LoadConfiguration ()
	{
		XmlDocument doc = new XmlDocument ();
		doc.Load (config);
		if (doc.DocumentElement.Name != "gendarme")
			return false;

		bool result = false;
		foreach (XmlElement ruleset in doc.DocumentElement.SelectNodes("ruleset")) {
			if (ruleset.Attributes["name"].Value != set)
				continue;
			foreach (XmlElement assembly in ruleset.SelectNodes("rules")) {
				string include = GetAttribute (assembly, "include", "*");
				string exclude = GetAttribute (assembly, "exclude", String.Empty);
				string from = GetFullPath (GetAttribute (assembly, "from", String.Empty));
				try {
					int n = LoadRulesFromAssembly (from, include, exclude);
					result = (result || (n > 0));
				}
				catch (Exception e) {
					Console.WriteLine ("Error reading rules{1}Details: {0}", e, Environment.NewLine);
					return false;
				}
			}
			if (!LoadCustomParameters (ruleset))
				return false;
		}
		return result;
	}

	void ApplyCustomParameterToRule (string ruleName, string name, int value) 
	{
		IRule rule = GetRule (ruleName);
		if (rule == null)
			throw new ArgumentException (String.Format ("The rule name {0} can't be found in the rules collection", ruleName), "rule");
		PropertyInfo property = rule.GetType ().GetProperty (name);
		if (property == null)
			throw new ArgumentException (String.Format ("The property {0} can't be found in the rule {1}", name, ruleName), "name");
		if (!property.CanWrite)
			throw new ArgumentException (String.Format ("The property {0} can't be written in the rule {1}", name, ruleName), "name");
		object result = property.GetSetMethod ().Invoke (rule, new object[] {value});
	}

	IRule GetRule (string name) 
	{
		IRule result;
		result = GetRuleFromSet (name, Rules.Assembly);
		if (result == null) {
			result = GetRuleFromSet (name, Rules.Module);
			if (result == null) {
				result = GetRuleFromSet (name, Rules.Type);
				if (result == null) {
					result = GetRuleFromSet (name, Rules.Method);
				}
			}
		}
		return result;
	}

	static IRule GetRuleFromSet (string name, RuleCollection rules) 
	{
		foreach (IRule rule in rules) {
			if (String.Compare (name, rule.GetType ().FullName) == 0)
				return rule;
		}
		return null;
	}

	void Header ()
	{
		if (quiet)
			return;

		Assembly a = Assembly.GetExecutingAssembly();
		Version v = a.GetName ().Version;
		if (v.ToString () != "0.0.0.0") {
			Console.WriteLine ("Gendarme v{0}", v);
			object[] attr = a.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
			if (attr.Length > 0)
				Console.WriteLine (((AssemblyCopyrightAttribute) attr [0]).Copyright);
		} else {
			Console.WriteLine ("Gendarme - Development Snapshot");
		}
		Console.WriteLine ();
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
		Console.WriteLine ("  --debug\t\tEnable debugging output.");
		Console.WriteLine ("  assembly\t\tSpecify the assembly to verify.");
		Console.WriteLine ();
	}

	void Write (string text)
	{
		if (!quiet)
			Console.Write (text);
	}

	void WriteLine (string text)
	{
		if (!quiet)
			Console.WriteLine (text);
	}
	
	void WriteLine (string text, params object[] args)
	{
		if (!quiet)
			Console.WriteLine (text, args);
	}

	static void ProcessRules (ConsoleRunner runner)
	{
		runner.Header ();
		string[] assemblies = new string [runner.assemblies.Count];
		runner.assemblies.Keys.CopyTo (assemblies, 0);
		DateTime total = DateTime.UtcNow;
		foreach (string assembly in assemblies) {
			DateTime start = DateTime.UtcNow;
			runner.Write (assembly);
			try {
				AssemblyDefinition ad = AssemblyFactory.GetAssembly (assembly);
				try {
					runner.Process (ad);
					runner.assemblies [assembly] = ad;
					runner.WriteLine (" - completed ({0} seconds).", (DateTime.UtcNow - start).TotalSeconds);
				}
				catch (Exception e) {
					runner.WriteLine (" - error executing rules{0}Details: {1}", Environment.NewLine, e);
				}
			}
			catch (Exception e) {
				runner.WriteLine (" - error processing{0}\tDetails: {1}", Environment.NewLine, e);
			}
		}
		runner. WriteLine ("{0}{1} assemblies processed in {2} seconds.{0}",  Environment.NewLine, runner.assemblies.Count, 
			(DateTime.UtcNow - total).TotalSeconds);
	}

 	static void Report (ConsoleRunner runner)
	{
		IResultWriter writer;
		switch (runner.format) {
		case "xml":
			writer = new XmlResultWriter (runner.output);
			break;
		case "html":
			writer = new HtmlResultWriter (runner.output);
			break;
		default:
			writer = new TextResultWriter (runner.output);
			break;
		}

		writer.Start ();
		writer.Write (runner.assemblies);
		writer.Write (runner.Rules);
		foreach (Violation v in runner.Violations) {
			writer.Write (v);
		}
		writer.End ();
	}

	static int Main (string[] args)
	{
		ConsoleRunner runner = new ConsoleRunner ();

		// runner options and configuration
		
		try {
			if (!runner.ParseOptions (args)) {
				Help ();
				return 1;
			}
			if (!runner.LoadConfiguration ()) {
				Console.WriteLine ("No assembly file were specified.");
				return 1;
			}
		}
		catch (Exception e) {
			Console.WriteLine (e);
			return 1;
		}

		ProcessRules (runner);
		Report (runner);
		
		if (runner.Violations.Count == 0) {
			runner.WriteLine ("No rule's violation were found.");
			return 0;
		}
		return 1;
	}
}
