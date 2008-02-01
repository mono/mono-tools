//
// Gendarme.Framework.RuleInformationManager class
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Xml;

namespace Gendarme.Framework {

	public class RuleInformationManager {

		private static Hashtable rules;
		private static Hashtable infos;

		static RuleInformationManager ()
		{
		}

		private RuleInformationManager ()
		{
		}

		static private XmlDocument LoadAssemblyRulesInformations (string filename)
		{
			XmlDocument doc = null;
			if (rules == null) {
				rules = new Hashtable ();
			} else {
				doc = (XmlDocument) rules[filename];
			}

			if (doc == null) {
				doc = new XmlDocument ();
				if (File.Exists (filename)) {
					doc.Load (filename);
				}
				rules[filename] = doc;
			}
			return doc;
		}

		static private string GetAttribute (XmlElement xel, string name)
		{
			XmlAttribute xa = xel.Attributes[name];
			if (xa == null)
				return String.Empty;
			return xa.Value;
		}

		static private string GetSubElement (XmlElement xel, string name)
		{
			if (xel.ChildNodes.Count > 0) {
				foreach (XmlElement child in xel.ChildNodes) {
					if (child.Name == name)
						return child.InnerText;
				}
			}
			return String.Empty;
		}

		static private RuleInformation LoadRuleInformations (XmlDocument doc, string name)
		{
			RuleInformation ri = null;
			if (doc.DocumentElement != null) {
				foreach (XmlElement xel in doc.DocumentElement) {
					if ((xel.Name == "rule") && (GetAttribute (xel, "Type") == name)) {
						ri = new RuleInformation ();
						ri.Name = GetAttribute (xel, "Name");
						ri.Uri = GetAttribute (xel, "Uri");
						ri.Problem = GetSubElement (xel, "problem");
						ri.Solution = GetSubElement (xel, "solution");
					}
				}
			}

			return (ri == null) ? RuleInformation.Empty : ri;
		}

		static public RuleInformation GetRuleInformation (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("rule");

			object o = null;
			if (infos == null) {
				infos = new Hashtable ();
			} else {
				o = infos[type.AssemblyQualifiedName];
			}

			if (o != null)
				return (o as RuleInformation);

			string fname = Path.ChangeExtension (type.Assembly.Location, ".xml");
			if (rules == null) {
				rules = new Hashtable ();
			}

			XmlDocument doc = (XmlDocument) rules[fname];
			if (doc == null) {
				doc = LoadAssemblyRulesInformations (fname);
			}

			RuleInformation ri = LoadRuleInformations (doc, type.AssemblyQualifiedName);
			infos[type.AssemblyQualifiedName] = ri;
			return ri;
		}

		static public RuleInformation GetRuleInformation (IRule rule)
		{
			if (rule == null)
				throw new ArgumentNullException ("rule");

			return GetRuleInformation (rule.GetType ());
		}
	}
}
