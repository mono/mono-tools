// 
// Gendarme's xmldoc2wiki tool
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010-2011 Novell, Inc (http://www.novell.com)
// Copyright (C) 2010 Yuri Stuken
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Text.RegularExpressions;

static class Program {

	static HashSet<string> assembliesIndex = new HashSet<string> ();

	static string outputdir;
	static string version = "git";

	static string CleanUp (this string s)
	{
		// escape special markdown characters
		return Regex.Replace (s, @"([\`\*])", @"\$1");
	}

	static void AppendIndentedLine (StringBuilder sb, string line, int indentation)
	{
		for (int i = 0; i < indentation; i++)
			sb.Append ("    ");
		sb.AppendLine (line);
	}

	static string ProcessCode (string code)
	{
		int indent = 0;
		StringBuilder sb = new StringBuilder ();
		StringReader reader = new StringReader (code);
		string line = reader.ReadLine ();
		while (line != null) {
			line = line.Trim ();
			if (line.EndsWith ("{")) {
				AppendIndentedLine (sb, line, indent++);
			} else if (line.EndsWith ("}")) {
				AppendIndentedLine (sb, line, --indent);
			} else {
				AppendIndentedLine (sb, line, indent);
			}

			line = reader.ReadLine ();
		}
		return sb.ToString ();
	}

	static string ProcessString (string s)
	{
		StringBuilder sb = new StringBuilder (s);
		return ProcessStringBuilder (sb);
	}

	static string ProcessStringBuilder (StringBuilder sb)
	{
		while (Char.IsWhiteSpace (sb [0]))
			sb.Remove (0, 1);
		while (Char.IsWhiteSpace (sb [sb.Length - 1]))
			sb.Remove (sb.Length - 1, 1);

		sb.Replace ("\r", String.Empty);
		sb.Replace ("\n", " ");
		// we don't start nor end with spaces
		for (int i = 1; i < sb.Length - 1; i++) {
			if (Char.IsWhiteSpace (sb [i])) {
				while (Char.IsWhiteSpace (sb [i + 1]))
					sb.Remove (i + 1, 1);
			}
		}
		return sb.ToString ();
	}

	static void WriteText (StringBuilder tw, string s)
	{
		// trim any space(s) at the start of the string
		int i = 0;
		while (Char.IsWhiteSpace (s [i]) && i < s.Length)
			i++;

		s = s.CleanUp ();

		for (; i < s.Length; i++) {
			char c = s [i];
			if (c == '\r')
				continue;
			if (c == '\n')
				c = ' ';
			// compress spaces
			if (Char.IsWhiteSpace (c)) {
				if (i < (s.Length - 1)) {
					char c2 = s [i + 1];
					if (Char.IsWhiteSpace (c2))
						continue;
				}
				tw.Append (' ');
			} else {
				tw.Append (c);
			}
		}
	}

	static void ProcessText (StringBuilder tw, IEnumerable<XNode> nodes)
	{
		bool insert_space = false;
		foreach (XNode node in nodes) {
			XText text = (node as XText);
			if (text != null) {
				string s = text.Value;
				if (insert_space) {
					if (s [0] == ' ')
						tw.Append (' ');
					insert_space = false;
				}
				WriteText (tw, s);
				continue;
			}

			XElement code = (node as XElement);
			if (code == null)
				continue;

			switch (code.Name.LocalName) {
			case "code":
			case "c":
				// bold
				tw.AppendFormat ("**{0}**", code.Value.CleanUp ());
				insert_space = true;
				break;
			case "list":
				ProcessList (tw, code);
				break;
			default:
				WriteText (tw, code.ToString ());
				break;
			}
		}
	}

	static void ProcessList (StringBuilder tw, XElement list)
	{
		tw.AppendLine ();
		foreach (XElement item in list.Elements ("item")) {
			tw.AppendLine ();
			tw.Append ("* ");
			XElement term = item.Element ("term");
			if (term != null) {
				tw.Append ("**");
				ProcessText (tw, term.Nodes ());
				tw.Append ("** : ");
			}
			var description = item.Element ("description");
			if (description != null)
				ProcessText (tw, description.Nodes ());
		}
		tw.AppendLine ();
		tw.AppendLine (); // make sure the following text is not part of the last item on the list
	}

	static void ProcessRules (IEnumerable<XElement> rules, string assembly)
	{
		SortedList<string, StringBuilder> properties = new SortedList<string, StringBuilder> ();
		HashSet<string> rulesIndex = new HashSet<string> ();
		foreach (var member in rules) {
			XElement item = member.Element ("summary");
			if (item == null)
				continue;

			StringBuilder rsb = new StringBuilder ();
			string name = member.Attribute ("name").Value;

			if (name.StartsWith ("P:")) {
				StringBuilder sb;
				int pp = name.LastIndexOf ('.') + 1;
				int pr = name.LastIndexOf ('.', pp - 2) + 1;
				string property = name.Substring (pp);
				string rule = name.Substring (pr, pp - pr - 1);
				if (!properties.TryGetValue (rule, out sb)) {
					sb = new StringBuilder ();
					properties.Add (rule, sb);
				} else
					sb.AppendLine ();

				sb.AppendFormat ("#### {0} {1}{1}", property, Environment.NewLine);
				ProcessText (sb, item.Nodes ());
				continue;
			}


			if (!name.StartsWith ("T:") || !name.EndsWith ("Rule"))
				continue;

			name = name.Substring (name.LastIndexOf ('.') + 1);

			rulesIndex.Add (name);

			string rule_file = String.Format ("{0}{1}{2}{1}{3}.{4}({2}).md", 
				outputdir, Path.DirectorySeparatorChar, version, assembly, name);
			using (TextWriter writer = File.CreateText (rule_file)) {
				rsb.AppendFormat ("# {0}{1}{1}", name, Environment.NewLine);
				rsb.AppendFormat ("Assembly: **[[{0}|{0}({1})]]**<br/>", assembly, version);
				rsb.AppendFormat ("Version: **{0}**{1}{1}", version, Environment.NewLine);
				rsb.AppendLine ("## Description");

				//			tw.WriteLine (ProcessSummary (item));
				ProcessText (rsb, item.Nodes ());
				rsb.AppendLine ();
				rsb.AppendLine ();

				var examples = member.Elements ("example");
				if (examples != null)
					rsb.AppendLine ("## Examples");
				foreach (XNode example in examples.Nodes ()) {
					XText node = (example as XText);
					if (node != null) {
						string text = example.ToString ().CleanUp ().Replace ("Bad", "**Bad**");
						text = text.Replace ("Good", "**Good**");
						rsb.AppendFormat ("{0}{1}{1}", text.Trim (), Environment.NewLine);
						continue;
					}

					XElement code = (example as XElement);
					if (code == null)
						continue;

					switch (code.Name.LocalName) {
					case "code":
						rsb.AppendFormat ("```csharp{1}{0}{1}```{1}{1}", ProcessCode (code.Value),
							Environment.NewLine);
						break;
					case "c":
						rsb.AppendFormat (" {0}{1}{1}", code.Value, Environment.NewLine);
						break;
					}
				}

				XElement container = member.Element ("remarks");
				if (container != null) {
					IEnumerable<XNode> remarks = container.Nodes ();
					if (remarks.Count () > 0) {
						rsb.AppendFormat ("## Notes{0}{0}", Environment.NewLine);
						rsb.Append ("* ");

						ProcessText (rsb, remarks);
						rsb.AppendLine ();
					}
				}

				StringBuilder psb;
				if (properties.TryGetValue (name, out psb)) {
					rsb.AppendFormat ("## Configuration{0}{0}", Environment.NewLine);
					rsb.AppendFormat ("Some elements of this rule can be customized to better fit your needs.{0}{0}", Environment.NewLine);
					rsb.Append (psb);
					rsb.AppendLine ();
				}

				writer.WriteLine (rsb);

				if (version == "git") {
					writer.WriteLine ();
					writer.WriteLine (@"## Source code

You can browse the latest [[source code|https://github.com/mono/mono-tools/tree/master/gendarme/rules/{0}/{1}.cs]] of this rule on github.com", assembly, name);
				}
			}
		}

		var rulesList =
			from rule in rulesIndex
			orderby rule
			select rule;

		string assembly_index = String.Format ("{0}{1}{2}{1}{3}({2}).md", 
			outputdir, Path.DirectorySeparatorChar, version, assembly);

		using (TextWriter writer = File.CreateText (assembly_index)) {
			writer.WriteLine ("# {0} Rules", assembly);
			writer.WriteLine ();
			writer.WriteLine ("The following ({0}) rules are available in version [[{1}|Gendarme.Rules({1})]] of {2}:", 
				rulesList.Count (), version, assembly);
			writer.WriteLine ();
			foreach (var rule in rulesList) {
				writer.WriteLine ("* [[{0}|{1}.{0}({2})]]  ", rule, assembly, version);
			}

			if (version == "git") {
				writer.WriteLine ();
				writer.WriteLine (@"## Source code

You can browse the latest [[source code|https://github.com/mono/mono-tools/tree/master/gendarme/rules/{0}]] of this assembly on github.com", assembly);
			}

			writer.WriteLine ();
		}
	}

	static void ProcessFile (string filename)
	{
		XDocument doc = XDocument.Load (filename);
		IEnumerable<XElement> members =
			from member in doc.Descendants ("member")
			orderby (string)member.Attribute ("name") ascending
			select member;

		string assembly = Path.GetFileName (
			(from a in doc.Descendants ("assembly")
			 select a.Element ("name").Value).FirstOrDefault ());

		if (assembly == null || !assembly.StartsWith ("Gendarme"))
			return;

		assembliesIndex.Add (assembly);
		ProcessRules (members, assembly); 
	}

	static void Main (string [] args)
	{
		string [] files;
		if (args.Length < 1) {
			Console.WriteLine ("Usage: xmldoc2wiki filepattern [--out outputdir] [--version version]");
			return;
		}

		outputdir = ".";
		version = "git";

		List<string> filenames = new List<string> ();
		for (int i = 0; i < args.Length; i++) {
			string arg = args [i];
			switch (arg) {
			case "--version":
				version = args [++i];
				break;
			case "--out":
				outputdir = args [++i];
				break;
			default:
				int index = arg.LastIndexOfAny (new char [] { '/', '\\' });
				string dir, pattern;
				if (index >= 0) {
					dir = arg.Substring (0, index);
					pattern = arg.Substring (index + 1);
				} else {
					dir = ".";
					pattern = arg;
				}

				files = Directory.GetFiles (dir, pattern);
				foreach (string file in files) {
					filenames.Add (file);
				}
				break;
			}
		}

		string subdir = Path.Combine (outputdir, version);
		if (!Directory.Exists (subdir))
			Directory.CreateDirectory (subdir);

		foreach (string file in filenames) {
			Console.WriteLine ("processing {0}", file);
			ProcessFile (file);
		}

		CreateVersionIndex ();
		CreateFooterFile ();
	}

	static void CreateVersionIndex ()
	{
		string rules_index = String.Format ("{0}{1}{2}{1}Gendarme.Rules({2}).md", 
			outputdir, Path.DirectorySeparatorChar, version);

		using (TextWriter writer = File.CreateText (rules_index)) {
			writer.WriteLine ("# Gendarme Rules Documentation Index");
			writer.WriteLine ();

			var assemblies =
				from assembly in assembliesIndex
				orderby assembly
				select assembly;

			writer.WriteLine ("The following ({0}) assemblies are available in version {1}:", 
				assemblies.Count (), version);
			writer.WriteLine ();

			foreach (var assembly in assemblies) {
				writer.WriteLine ("* [[{0}|{0}({1})]]  ", assembly, version);
			}

			if (version == "git") {
				writer.WriteLine ();
				writer.WriteLine (@"## Source code

* Latest [[source code|https://github.com/mono/mono-tools/tree/master/gendarme/rules/]] for Gendarme's rules is available on github.com
* Documentation is produced using gendarme's `xmldoc2wiki` (unsupported) tool which [[source code|https://github.com/mono/mono-tools/tree/master/gendarme/tools/unsupported/xmldoc2wiki]] is also available on github.com");
			}

			writer.WriteLine ();
		}
	}

	static void CreateFooterFile ()
	{
		string footer = String.Format ("{0}{1}{2}{1}_Footer.md", outputdir, Path.DirectorySeparatorChar, version);

		using (TextWriter writer = File.CreateText (footer)) {
			writer.WriteLine (@"## Feedback

Note that this page was autogenerated ({0}) based on the `xmldoc` comments inside the rules source code and cannot be edited from this wiki.
Please report any documentation errors, typos or suggestions to the 
[[Gendarme Google Group|http://groups.google.com/group/gendarme]]. Thanks!", DateTime.Now);
 		}
	}
}
