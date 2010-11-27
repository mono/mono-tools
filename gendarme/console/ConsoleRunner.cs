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
using System.Diagnostics;
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

		private string config_file;
		private string rule_set = "default";
		private string html_file;
		private string log_file;
		private string xml_file;
		private string ignore_file;
		private string limit;
		private string severity_filter;
		private string confidence_filter;
		private bool help;
		private bool quiet;
		private bool version;
		private List<string> assembly_names;

		// parse severity filter
		// e.g. Audit,High+ == Audit, High and Critical
		void ParseSeverity ()
		{
			SeverityBitmask.ClearAll ();
			string [] options = severity_filter.ToUpperInvariant ().Split (',');
			foreach (string option in options) {
				Severity severity;

				switch (option) {
				case "AUDIT":
				case "AUDIT+":
				case "AUDIT-":
					severity = Severity.Audit;
					break;
				case "LOW":
				case "LOW+":
				case "LOW-":
					severity = Severity.Low;
					break;
				case "MEDIUM":
				case "MEDIUM+":
				case "MEDIUM-":
					severity = Severity.Medium;
					break;
				case "HIGH":
				case "HIGH+":
				case "HIGH-":
					severity = Severity.High;
					break;
				case "CRITICAL":
				case "CRITICAL+":
				case "CRITICAL-":
					severity = Severity.Critical;
					break;
				case "ALL":
				case "*":
					SeverityBitmask.SetAll ();
					continue;
				default:
					continue;
				}

				char end = option [option.Length - 1];
				if (end == '+') {
					SeverityBitmask.SetDown (severity);
					Console.WriteLine ("SetDown {0} -> {1}", severity, SeverityBitmask);
				} else if (end == '-') {
					SeverityBitmask.SetUp (severity);
					Console.WriteLine ("SetUp {0} -> {1}", severity, SeverityBitmask);
				} else {
					SeverityBitmask.Set (severity);
					Console.WriteLine ("Set {0} -> {1}", severity, SeverityBitmask);
				}
			}
		}

		void ParseConfidence ()
		{
			ConfidenceBitmask.ClearAll ();
			string [] options = confidence_filter.ToUpperInvariant ().Split (',');
			foreach (string option in options) {
				Confidence confidence;

				switch (option) {
				case "LOW":
				case "LOW+":
				case "LOW-":
					confidence = Confidence.Low;
					break;
				case "NORMAL":
				case "NORMAL+":
				case "NORMAL-":
					confidence = Confidence.Normal;
					break;
				case "HIGH":
				case "HIGH+":
				case "HIGH-":
					confidence = Confidence.High;
					break;
				case "TOTAL":
				case "TOTAL+":
				case "TOTAL-":
					confidence = Confidence.Total;
					break;
				case "ALL":
				case "*":
					ConfidenceBitmask.SetAll ();
					continue;
				default:
					continue;
				}

				char end = option [option.Length - 1];
				if (end == '+') {
					ConfidenceBitmask.SetDown (confidence);
				} else if (end == '-') {
					ConfidenceBitmask.SetUp (confidence);
				} else {
					ConfidenceBitmask.Set (confidence);
				}
			}
		}

		byte Parse (string [] args)
		{
			var p = new OptionSet () {
				{ "config=",	v => config_file = v },
				{ "set=",	v => rule_set = v },
				{ "log=",	v => log_file = v },
				{ "xml=",	v => xml_file = v },
				{ "html=",	v => html_file = v },
				{ "ignore=",	v => ignore_file = v },
				{ "limit=",	v => limit = v },
				{ "severity=",	v => severity_filter = v },
				{ "confidence=",v => confidence_filter = v },
				{ "v|verbose",  v => ++VerbosityLevel },
				{ "quiet",	v => quiet = v != null },
				{ "version",	v => version = v != null },
				{ "h|?|help",	v => help = v != null },
			};
			assembly_names = p.Parse (args);

			// if supplied, use the user limit on defects (otherwise 2^31 is used)
			int defects_limit;
			if (String.IsNullOrEmpty (limit) || !Int32.TryParse (limit, out defects_limit))
				defects_limit = Int32.MaxValue;
			DefectsLimit = defects_limit;

			// by default the runner will ignore Audit and Low severity defects
			if (String.IsNullOrEmpty (severity_filter)) {
				SeverityBitmask.SetAll ();
				SeverityBitmask.Clear (Severity.Audit);
				SeverityBitmask.Clear (Severity.Low);
			} else {
				ParseSeverity ();
			}

			// by default the runner will ignore Low confidence defects
			if (String.IsNullOrEmpty (confidence_filter)) {
				ConfidenceBitmask.SetAll ();
				ConfidenceBitmask.Clear (Confidence.Low);
			} else {
				ParseConfidence ();
			}

			return (byte) ((assembly_names.Count > 0) ? 0 : 1);
		}

		// name can be
		// - a filename (a single assembly)
		// - a mask (*, ?) for multiple assemblies
		// - a special file (@) containing a list of assemblies
		byte AddFiles (string name)
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
			string warning = null;

			try {
				string assembly_name = Path.GetFullPath (filename);
				AssemblyDefinition ad = AssemblyDefinition.ReadAssembly (
					assembly_name,
					new ReaderParameters { AssemblyResolver = AssemblyResolver.Resolver });
				Assemblies.Add (ad);
			}
			catch (BadImageFormatException) {
				warning = "Invalid assembly format";
			}
			catch (FileNotFoundException fnfe) {
				// e.g. .netmodules
				warning = fnfe.Message;
			}
			catch (ArgumentException e) {
				warning = e.ToString ();
			}

			// show warning (quiet or not) but continue loading & analyzing assemblies
			if (warning != null) {
				Console.Error.WriteLine ("warning: could not load assembly '{0}', reason: {1}{2}",
					filename, warning, Environment.NewLine);
			}
		}

		byte Report ()
		{
			// re-activate all loaded (i.e. selected) rules since some of them could have
			// turned off themselve while executing but we still want them listed in the report
			foreach (Rule rule in Rules) {
				rule.Active = true;
			}

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

			return (byte) ((0 == Defects.Count) ? 0 : 1);
		}

		byte Execute (string [] args)
		{
			try {
				byte result = Parse (args);
				Header ();
				if (version)
					return 0;

				if ((result != 0) || help) {
					Help ();
					return help ? (byte) 0 : result;
				}

				// load configuration, including rules
				Settings config = new Settings (this, config_file, rule_set);
				// and continue if there's at least one rule to execute
				if (!config.Load () || (Rules.Count < 1)) {
					int validationErrorsCounter = 0;
					foreach (string error in config.ValidationErrors) {
						Console.WriteLine (error);
						validationErrorsCounter++;
					}
					if (validationErrorsCounter == 0)
						Console.WriteLine ("Configuration parameters does not match any known rule.");
					return 3;
				}

				foreach (string name in assembly_names) {
					result = AddFiles (name);
					if (result != 0)
						return result;
				}

				IgnoreList = new IgnoreFileList (this, ignore_file);

				// now that all rules and assemblies are know, time to initialize
				Initialize ();
				// before analyzing the assemblies with the rules
				Run ();
				// and winding down properly
				TearDown ();

				return Report ();

			} catch (IOException e) {
				if (0 == VerbosityLevel) {
					Console.Error.WriteLine ("ERROR: {0}", e.Message);
					return 2;
				} else {
					WriteUnhandledExceptionMessage (e);
					return 4;
				}

			} catch (Exception e) {
				WriteUnhandledExceptionMessage (e);
				return 4;
			}
		}

		private void WriteUnhandledExceptionMessage (Exception e)
		{
			Console.WriteLine ();
			Console.WriteLine ("An uncaught exception occured. Please fill a bug report at https://bugzilla.novell.com/");
			if (CurrentRule != null)
				Console.WriteLine ("Rule:\t{0}", CurrentRule);
			if (CurrentTarget != null)
				Console.WriteLine ("Target:\t{0}", CurrentTarget);
			Console.WriteLine ("Stack trace: {0}", e);
		}

		private Stopwatch total = new Stopwatch ();
		private Stopwatch local = new Stopwatch ();
		
		private static string TimeToString (TimeSpan time)
		{
			if (time >= TimeSpan.FromMilliseconds (100))
				return string.Format ("{0:0.0} seconds", time.TotalSeconds);
			else
				return "<0.1 seconds";
		}

		public override void Initialize ()
		{
			if (!quiet) {
				Console.Write ("Initialization");
				total.Start ();
				local.Start ();
			}
			
			base.Initialize ();

			if (!quiet) {
				local.Stop ();
				Console.WriteLine (": {0}", TimeToString (local.Elapsed));
				local.Reset ();
			}
		}

		public override void Run ()
		{
			if (Assemblies.Count == 0) {
				Console.WriteLine ("No assemblies were specified to be analyzed.");
				return;
			}

			base.Run ();

			if (!quiet)
				local.Stop ();
		}

		public override void TearDown ()
		{
			if (!quiet) {
				Console.WriteLine (": {0}", TimeToString (local.Elapsed));
				local.Start ();
				local.Reset ();
			}
			
			base.TearDown ();

			if (!quiet) {
				local.Stop ();
				total.Stop ();
				Console.WriteLine ("TearDown: {0}", TimeToString (local.Elapsed));
				Console.WriteLine ();
				if (Assemblies.Count == 1)
					Console.WriteLine ("One assembly processed in {0}.",
						TimeToString (total.Elapsed));
				else
					Console.WriteLine ("{0} assemblies processed in {1}.",
						Assemblies.Count, TimeToString (total.Elapsed));

				string hint = string.Empty;
				if (null != log_file || null != xml_file || null != html_file) {
					List<string> files = new List<string> (new string [] { log_file, xml_file, html_file });
					files.RemoveAll (string.IsNullOrEmpty);
					hint = string.Format ("Report{0} written to: {1}.",
						(files.Count > 1) ? "s" : string.Empty,
						string.Join (",", files.Select (file => string.Format ("`{0}'", file)).ToArray ()));
				}

				if (Defects.Count == 0)
					Console.WriteLine ("No defect found. {0}", hint);
				else if (Defects.Count == 1)
					Console.WriteLine ("One defect found. {0}", hint);
				else
					Console.WriteLine ("{0} defects found. {1}", Defects.Count, hint);
			}
		}

		protected override void OnAssembly (RunnerEventArgs e)
		{
			if (!quiet) {
				if (local.IsRunning) {
					local.Stop ();
					Console.WriteLine (": {0}", TimeToString (local.Elapsed));
					local.Reset ();
				}
			
				// next assembly
				Console.Write (Path.GetFileName (e.CurrentAssembly.MainModule.FullyQualifiedName));
				local.Start ();
			}
			
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
			} else {
				Console.WriteLine ("Gendarme - Development Snapshot");
			}

			object [] attr = a.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
			if (attr.Length > 0)
				Console.WriteLine (((AssemblyCopyrightAttribute) attr [0]).Copyright);

			Console.WriteLine ();
		}

		private static Assembly runner_assembly;

		static public Assembly Assembly {
			get {
				if (runner_assembly == null)
					runner_assembly = Assembly.GetExecutingAssembly ();
				return runner_assembly;
			}
		}

		static void Help ()
		{
			Console.WriteLine ("Usage: gendarme [--config file] [--set ruleset] [--{log|xml|html} file] assemblies");
			Console.WriteLine ("Where");
			Console.WriteLine ("  --config file\t\tSpecify the rule sets and rule settings. Default is 'rules.xml'.");
			Console.WriteLine ("  --set ruleset\t\tSpecify a rule set from configfile. Default is 'default'.");
			Console.WriteLine ("  --log file\t\tSave the report to the specified file.");
			Console.WriteLine ("  --xml file\t\tSave the report, as XML, to the specified file.");
			Console.WriteLine ("  --html file\t\tSave the report, as HTML, to the specified file.");
			Console.WriteLine ("  --ignore file\t\tDo not report defects listed in the specified file.");
			Console.WriteLine ("  --limit N\t\tStop reporting after N defects are found.");
			Console.WriteLine ("  --severity [all | [[audit | low | medium | high | critical][+|-]]],...");
			Console.WriteLine ("\t\t\tFilter defects for the specified severity levels.");
			Console.WriteLine ("\t\t\tDefault is 'medium+'");
			Console.WriteLine ("  --confidence [all | [[low | normal | high | total][+|-]],...");
			Console.WriteLine ("\t\t\tFilter defects for the specified confidence levels.");
			Console.WriteLine ("\t\t\tDefault is 'normal+'");
			Console.WriteLine ("  --quiet\t\tUsed to disable progress and other information which is normally written to stdout.");
			Console.WriteLine ("  --v\t\t\tWhen present additional progress information is written to stdout (can be used multiple times).");
			Console.WriteLine ("  assemblies\t\tSpecify the assemblies to verify.");
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
		/// 4 if an uncaught exception occured</returns>
		static int Main (string [] args)
		{
			return new ConsoleRunner ().Execute (args);
		}
	}
}
