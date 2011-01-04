//
// gd2i - Gendarme's Defects To Ignore List
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010-2011 Novell, Inc (http://www.novell.com)
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

using NDesk.Options;

namespace Gendarme.Tools {

	[SuppressMessage ("Gendarme.Rules.Maintainability", "AvoidLackOfCohesionOfMethodsRule", Justification = "kiss")]
	sealed class DefectsToIgnoreList {

		Dictionary<string, HashSet<string>> entries = new Dictionary<string, HashSet<string>> ();

		[SuppressMessage ("Gendarme.Rules.Smells", "AvoidSwitchStatementsRule", Justification = "kiss")]
		[SuppressMessage ("Gendarme.Rules.Performance", "AvoidRepetitiveCallsToPropertiesRule",
			Justification = "reader.Name and Has Attributes are called very often (on purpose) inside loops")]
		void ReadDefects (string filename)
		{
			using (XmlTextReader reader = new XmlTextReader (filename)) {
				Dictionary<string, string> full_names = new Dictionary<string, string> ();
				// look for 'gendarme-output/rules/rule/'
				while (reader.Read () && (reader.Name != "rule"));
				do {
					full_names.Add (reader ["Name"], "R: " + reader.ReadInnerXml ());
				} while (reader.Read () && reader.HasAttributes);

				// look for 'gendarme-output/results/
				while (reader.Read () && (reader.Name != "results"));

				HashSet<string> targets = null;
				while (reader.Read ()) {
					if (!reader.HasAttributes)
						continue;

					switch (reader.Name) {
					case "rule":
						targets = new HashSet<string> ();
						entries.Add (full_names [reader ["Name"]], targets);
						break;
					case "target":
						string target = reader ["Name"];
						if (target.IndexOf (' ') != -1)
							targets.Add ("M: " + target);
						else
							targets.Add ("T: " + target);
						break;
					}
				}
			}
		}

		[SuppressMessage ("Gendarme.Rules.Smells", "AvoidSwitchStatementsRule", Justification = "kiss")]
		void ReadIgnoreList (string filename)
		{
			HashSet<string> entries_for_rule = null;
			int line_no = 0;
			string rule = String.Empty;

			using (StreamReader sr = new StreamReader (filename)) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine ();
					line_no++;
					if (String.IsNullOrEmpty (line))
						continue;

					char type = line [0];
					switch (type) {
					case 'R':
						// rule
						rule = line.Trim ();
						if (!entries.TryGetValue (rule, out entries_for_rule)) {
							entries_for_rule = new HashSet<string> ();
							entries.Add (rule, entries_for_rule);
						}
						break;
					case '#':
						// comment - ignore
						break;
					case 'A':
					case 'T':
					case 'M':
					case 'N':
						if (rule.Length == 0) {
							if (syntax_errors_warnings) {
								Console.Error.WriteLine ("line #{0}: Entry '{1}' has no rule previously defined.",
									line_no, line);
							}
							break;
						}
						string metadata = line.Trim ();
						// metadata - remove from list
						if (!entries_for_rule.Remove (metadata) && extra_defects_warnings) {
							Console.Error.WriteLine ("line #{0}: Entry '{1}' for rule {2}' was not reported as a defect.",
								line_no, line, rule);
						}
						break;
					default:
						// unknown - report if warn is true
						if (syntax_errors_warnings) {
							Console.Error.WriteLine ("line #{0}: Invalid syntax for '{0}'.",
								line_no, line);
						}
						break;
					}
				}
			}
		}

		string BuildIgnoreData ()
		{
			if (entries.Count == 0)
				return String.Empty;

			StringBuilder ignore_data = new StringBuilder ();
			foreach (KeyValuePair<string, HashSet<string>> entry in entries) {
				if (entry.Value.Count == 0)
					continue;
				ignore_data.AppendLine (entry.Key);
				foreach (string metadata in entry.Value) {
					ignore_data.AppendLine (metadata);
				}
				ignore_data.AppendLine ();
			}
			return ignore_data.ToString ();
		}

		void WriteIgnoreList (string filename)
		{
			string data = BuildIgnoreData ();
			if (data.Length == 0)
				return;

			using (StreamWriter sw = File.CreateText (filename)) {
				sw.Write (data);
			}
		}

		void AppendToIgnoreList (string filename)
		{
			string data = BuildIgnoreData ();
			if (data.Length == 0)
				return;

			using (StreamWriter sw = File.AppendText (filename)) {
				sw.WriteLine ("# begin extra entries generated from defects (added on {0})", DateTime.Now);
				sw.WriteLine ();
				sw.Write (data);
				sw.WriteLine ("# end extra entries generated from defects");
			}
		}

		bool extra_defects_warnings;
		bool syntax_errors_warnings;
		bool help;
		bool version;
		bool quiet;
		string defects;
		string ignores;

		static void Header ()
		{
			Assembly a = Assembly.GetExecutingAssembly ();
			Version v = a.GetName ().Version;
			if (v.ToString () != "0.0.0.0") {
				Console.WriteLine ("gd2i v{0}", v);
			} else {
				Console.WriteLine ("gd2i - Development Snapshot");
			}

			object [] attr = a.GetCustomAttributes (typeof (AssemblyCopyrightAttribute), false);
			if (attr.Length > 0)
				Console.WriteLine (((AssemblyCopyrightAttribute) attr [0]).Copyright);

			Console.WriteLine ();
		}

		static void Help ()
		{
			Console.WriteLine ("Usage: gd2i defects.xml list.ignore [--extra-defects-warnings] [--syntax-check] [--quiet] [--version] [--help]");
			Console.WriteLine ("Where");
			Console.WriteLine (" defects.xml\tThe list of defects (XML) produced by Gendarme on your project.");
			Console.WriteLine (" list.ignore\tThe file listing ignored defects entries for your project.");
			Console.WriteLine (" --extra-check\t[optional] Report ignore entries not in the defect list.");
			Console.WriteLine (" --syntax-check\t[optional] Report syntax error found in 'list.ignore' file.");
			Console.WriteLine (" --quiet\t[optional] Minimize output tro stdout.");
			Console.WriteLine (" --version\tDisplay the tool's version number.");
			Console.WriteLine (" --help\t\tShow help about the command-line options.");
			Console.WriteLine ();
		}

		byte Parse (string [] args)
		{
			var p = new OptionSet () {
				{ "syntax-check",	v => syntax_errors_warnings = v != null },
				{ "extra-check",	v => extra_defects_warnings = v != null },
				{ "quiet",		v => quiet = v != null },
				{ "version",		v => version = v != null },
				{ "h|?|help",		v => help = v != null },
			};

			List<string> files = p.Parse (args);
			if (files.Count != 2)
				return (byte) 1;

			defects = files [0];
			ignores = files [1];
			return (byte) 0;
		}

		[SuppressMessage ("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", 
			Justification = "single reporting point for all failures inside the tool")]
		byte Execute (string [] args)
		{
			byte result = 1;
			try {
				result = Parse (args);

				bool parsed = (result == 0);
				if (!quiet || !parsed)
					Header ();
				if (!parsed) {
					Console.WriteLine ("Invalid command-line parameters");
					Console.WriteLine ();
				}
				if (help || !parsed)
					Help ();

				if (parsed && !help && !version) {
					// read defects XML file and build an ignore list from it
					ReadDefects (defects);
					if (File.Exists (ignores)) {
						// read existing 'ignore-list' and remove them from the ignore list
						ReadIgnoreList (ignores);
						// don't touch the original (sort, comment...) but append new ignore entries
						AppendToIgnoreList (ignores);
					} else {
						WriteIgnoreList (ignores);
					}
				}
			}
			catch (Exception e) {
				Console.WriteLine ("Unhandled exception caught: {0}", e);
				result = 2;
			}
			return result;
		}

		static int Main (string [] args)
		{
			return new DefectsToIgnoreList ().Execute (args);
		}
	}
}

