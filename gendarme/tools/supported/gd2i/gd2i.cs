//
// gd2i - Gendarme's Defects To Ignore List
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Xml;

namespace Gendarme.Tools {

	static class DefectsToIgnoreList {

		static Dictionary<string, HashSet<string>> ignore_list = new Dictionary<string, HashSet<string>> ();

		static void ReadDefects (string filename)
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

				HashSet<string> defects = null;
				while (reader.Read ()) {
					if (!reader.HasAttributes)
						continue;

					switch (reader.Name) {
					case "rule":
						defects = new HashSet<string> ();
						ignore_list.Add (full_names [reader ["Name"]], defects);
						break;
					case "target":
						string target = reader ["Name"];
						if (target.IndexOf (' ') != -1)
							defects.Add ("M: " + target);
						else
							defects.Add ("T: " + target);
						break;
					}
				}
			}
		}

		static void ReadIgnoreList (string filename)
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
						if (!ignore_list.TryGetValue (rule, out entries_for_rule)) {
							entries_for_rule = new HashSet<string> ();
							ignore_list.Add (rule, entries_for_rule);
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

		static string BuildIgnoreData ()
		{
			if (ignore_list.Count == 0)
				return String.Empty;

			StringBuilder ignore_data = new StringBuilder ();
			foreach (KeyValuePair<string, HashSet<string>> entry in ignore_list) {
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

		static void WriteIgnoreList (string filename)
		{
			string data = BuildIgnoreData ();
			if (data.Length == 0)
				return;

			using (StreamWriter sw = File.CreateText (filename)) {
				sw.Write (data);
			}
		}

		static void AppendToIgnoreList (string filename)
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

		static bool extra_defects_warnings;
		static bool syntax_errors_warnings;

		static void Main (string [] args)
		{
			// todo - use Options.cs
			string defects = args [0];
			string ignores = args [1];

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
}

