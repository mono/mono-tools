using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

class Program {

	static void Header (TextWriter tw)
	{
		tw.WriteLine ("= Rules =");
		tw.WriteLine ();
	}

	static void Footer (TextWriter tw)
	{
		tw.WriteLine (@"= Feedback =

Please report any documentation errors, typos or suggestions to the 
[http://groups.google.com/group/gendarme Gendarme Google Group]. Thanks!
[[Category:Gendarme]]");
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
				tw.AppendFormat ("'''{0}'''", code.Value);
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
		foreach (XElement item in list.Elements ("item")) {
			tw.AppendLine ();
			tw.Append ("* ");
			XElement term = item.Element ("term");
			if (term != null) {
				tw.Append ("'''");
				ProcessText (tw, term.Nodes ());
				tw.Append ("''' : ");
			}
			ProcessText (tw, item.Element ("description").Nodes ());
		}
		tw.AppendLine ();
	}

	static void ProcessRules (TextWriter writer, IEnumerable<XElement> rules)
	{
		SortedList<string, StringBuilder> properties = new SortedList<string, StringBuilder> ();
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

				sb.AppendFormat ("==== {0} ===={1}{1}", property, Environment.NewLine);
				ProcessText (sb, item.Nodes ());
				continue;
			}

			name = name.Substring (name.LastIndexOf ('.') + 1);
			rsb.AppendFormat ("=== {0} ===", name);
			rsb.AppendLine ();

//			tw.WriteLine (ProcessSummary (item));
			ProcessText (rsb, item.Nodes ());
			rsb.AppendLine ();
			rsb.AppendLine ();

			foreach (XNode example in member.Elements ("example").Nodes ()) {
				XText node = (example as XText);
				if (node != null) {
					string text = example.ToString ().Replace ("Bad", "'''Bad'''");
					text = text.Replace ("Good", "'''Good'''");
					rsb.AppendFormat ("{0}{1}{1}", text.Trim (), Environment.NewLine);
					continue;
				}

				XElement code = (example as XElement);
				if (code == null)
					continue;
				
				switch (code.Name.LocalName) {
				case "code":
					rsb.AppendFormat ("<csharp>{0}</csharp>{1}{1}", ProcessCode (code.Value),
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
					rsb.AppendLine ("'''Notes'''");
					foreach (XNode element in remarks) {
						rsb.AppendFormat ("* {0}", element.ToString ());
					}
					rsb.AppendLine ();
				}
			}

			StringBuilder psb;
			if (properties.TryGetValue (name, out psb)) {
				rsb.AppendFormat ("'''Configuration'''{0}{0}", Environment.NewLine);
				rsb.AppendFormat ("Some elements of this rule can be customized to better fit your needs.{0}{0}", Environment.NewLine);
				rsb.Append (psb);
				rsb.AppendLine ();
			}

			writer.WriteLine (rsb);
		}
	}

	static void ProcessFile (string filename)
	{
		XDocument doc = XDocument.Load (filename);
		IEnumerable<XElement> members =
			from member in doc.Descendants ("member")
			orderby (string) member.Attribute ("name") ascending
			select member;

		string outname = Path.ChangeExtension (filename, "wiki");
		using (TextWriter tw = File.CreateText (outname)) {
			Header (tw);
			ProcessRules (tw, members);
			Footer (tw);
		}
	}

	static void Main (string [] args)
	{
		string[] files = args;
		if (args.Length == 1)
			files = Directory.GetFiles (".", args [0]);

		foreach (string file in files) {
			ProcessFile (file);
		}
	}
}

