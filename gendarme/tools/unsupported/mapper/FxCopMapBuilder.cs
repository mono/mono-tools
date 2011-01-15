//
// FxCopMapBuilder.cs
//
// Authors:
//	Yuri Stuken <stuken.yuri@gmail.com>
//
// Copyright (C) 2010 Yuri Stuken
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
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Linq;
using Mono.Cecil;
using System.Text;

namespace FxCopMapBuilder {

	static class FxCopMapBuilder {

		static Dictionary<FxCopRule, List<string>> FxCopToGendarme = new Dictionary<FxCopRule, List<string>> ();
		static Dictionary<string, List<FxCopRule>> GendarmeToFxCop = new Dictionary<string, List<FxCopRule>> ();

		static void Main (string [] args)
		{
			string dirname = ".", fxCopFile = "FxCopToGendarme", gendarmeFile = "GendarmeToFxCop";
			bool error = false, include = false, xmloutput = true;

			try {
				dirname = args [0];
				xmloutput = !(args [1] == "/target:wiki");
				fxCopFile = args [2];
				gendarmeFile = args [3];
				include = (args [4] == "/I");
			}
			catch (IndexOutOfRangeException e) {
				// nothing to do
			}

			string ext = (xmloutput) ? "xml" : "md";
			
			fxCopFile = Path.ChangeExtension (fxCopFile, ext);
			gendarmeFile = Path.ChangeExtension (gendarmeFile, ext);

			if (!Directory.Exists (dirname))
				error = true;

			if (error) {
				Console.WriteLine ("Usage: fxcopmapbuilder [dirname] [/target:wiki] [fxcopmapfile] [gendarmemapfile] [/I]");
				Console.WriteLine ();
				Console.WriteLine ("\t/I\tIncludes Gendarme rules which were not mapped to any of FxCop rules to the output");
			} else {
				BuildMap (dirname, include);
				BuildXml (fxCopFile, gendarmeFile, xmloutput);
			}
		}

		private static void BuildMap (string dirname, bool include)
		{
			string [] files = Directory.GetFiles (dirname, Path.GetFileName ("Gendarme.Rules.*.dll"));
			if (files.Length == 0)
				Console.WriteLine ("Unable to find any matching assemblies in a specified directory");

			foreach (string file in files) {
				AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly (file);

				foreach (TypeDefinition type in assembly.MainModule.Types) {
					// we do not need to proceed with such types
					if (!type.HasCustomAttributes || type.IsAbstract || type.IsInterface)
						continue;

					// check if Gendarme.Framework.Rule exists in inheritance chain
					// checks only for two levels, is there something like isSubclassOf?
					if (type.BaseType == null ||
						(type.BaseType.FullName != "Gendarme.Framework.Rule")) {
						TypeDefinition t = type.BaseType.Resolve ();
						if (t.BaseType == null ||
							(t.BaseType.FullName != "Gendarme.Framework.Rule"))
							continue;
					}

					foreach (var attribute in type.CustomAttributes) {
						if (attribute.AttributeType.FullName !=
							"Gendarme.Framework.FxCopCompatibilityAttribute")
							continue;
						if (!attribute.HasConstructorArguments)
							continue;
						FxCopRule rule = new FxCopRule (
							attribute.ConstructorArguments [0].Value.ToString (),
							attribute.ConstructorArguments [1].Value.ToString ());
						if (FxCopToGendarme.ContainsKey (rule))
							FxCopToGendarme [rule].Add (type.FullName);
						else
							FxCopToGendarme.Add (rule, new List<string> { type.FullName });
						if (GendarmeToFxCop.ContainsKey (type.FullName))
							GendarmeToFxCop [type.FullName].Add (rule);
						else
							GendarmeToFxCop.Add (type.FullName, new List<FxCopRule> { rule });
					}

					// add gendarme rule to the dictionary even if it 
					// was not mapped with any fxcop rule
					if (include && !GendarmeToFxCop.ContainsKey (type.FullName))
						GendarmeToFxCop.Add (type.FullName, new List<FxCopRule> ());
				}
			}
		}

		private static void BuildXml (string fxCopToGendarmeFilename, string gendarmeToFxCopFilename, bool xmloutput)
		{
			// Writing FxCop to Gendarme rules map
			XElement root = new XElement ("rules");
			foreach (var pair in FxCopToGendarme) {
				XElement rule = new XElement ("rule",
							new XAttribute ("type", "fxcop"),
							new XAttribute ("category", pair.Key.Category),
							new XAttribute ("id",
								(!String.IsNullOrEmpty (pair.Key.Name)) ?
									pair.Key.Id + ":" + pair.Key.Name : pair.Key.Id));
				foreach (var grule in pair.Value) {
					string category = grule.Substring (0, grule.LastIndexOf ('.'));
					string id = grule.Substring (grule.LastIndexOf ('.') + 1);
					rule.Add (new XElement ("rule",
							new XAttribute ("type", "gendarme"),
							new XAttribute ("category", category),
							new XAttribute ("id", id)));
				}
				root.Add (rule);

			}

			SortXml (root);
			if (xmloutput)
				new XDocument (root).Save (fxCopToGendarmeFilename);
			else
				using (StreamWriter output = new StreamWriter (fxCopToGendarmeFilename))
					output.Write (BuildWikiTextFromXml(root));

			// Writing Gendarme to FxCop rules map
			root = new XElement ("rules");
			foreach (var pair in GendarmeToFxCop) {
				string category = pair.Key.Substring (0, pair.Key.LastIndexOf ('.'));
				string id = pair.Key.Substring (pair.Key.LastIndexOf ('.') + 1);
				XElement rule = new XElement ("rule",
							new XAttribute ("type", "gendarme"),
							new XAttribute ("category", category),
							new XAttribute ("id", id));
				foreach (var fxcrule in pair.Value) {
					rule.Add (new XElement ("rule",
							new XAttribute ("type", "fxcop"),
							new XAttribute ("category", fxcrule.Category),
							new XAttribute ("id",
								(!String.IsNullOrEmpty (fxcrule.Name)) ?
									fxcrule.Id + ":" + fxcrule.Name : fxcrule.Id)));
				}
				root.Add (rule);
			}

			SortXml (root);
			if (xmloutput)
				new XDocument (root).Save (gendarmeToFxCopFilename);
			else
				using (StreamWriter output = new StreamWriter (gendarmeToFxCopFilename))
					output.Write (BuildWikiTextFromXml (root));
		}

		private static XElement SortXml (XElement node)
		{
			node.ReplaceNodes (
				from element in node.Elements ()
				orderby element.Attribute ("category").Value,
					element.Attribute ("id").Value
				select SortXml(element));
			return node;
		}

		private static StringBuilder BuildWikiTextFromXml (XElement rootNode)
		{
			StringBuilder sb = new StringBuilder ();
			string category = String.Empty;
			foreach (var element in rootNode.Elements ()) {
				if (element.Attribute ("category").Value != category) { 
					category = element.Attribute ("category").Value;
					sb.AppendLine(String.Format("## {0}",  category));
				}
				sb.AppendLine (String.Format ("### {0}", BuildWikiLink(element)));
				foreach (var rule in element.Elements ())
					sb.AppendLine(String.Format("* {0}", BuildWikiLink(rule)));
				sb.AppendLine ();

			}
			return sb;
		}

		private static string BuildWikiLink (XElement rule)
		{
			if (rule.Attribute ("type").Value == "fxcop") {
				string [] idName = rule.Attribute ("id").Value.Split (':');
				return String.Format (
					"[[{0}: {1}|http://social.msdn.microsoft.com/Search/en-US?query={2} {0} {1}]]",
					idName [0], idName [1], rule.Attribute ("category").Value);
			}
			if (rule.Attribute ("type").Value == "gendarme") {
				return String.Format ("[[{1}|{0}.{1}{2}]]", rule.Attribute ("category").Value, 
					rule.Attribute ("id").Value, "(2.10)");
			}
			return String.Empty;
		}
	}
}
