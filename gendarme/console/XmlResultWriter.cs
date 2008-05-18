//
// XmlResultWriter
//
// Authors:
//	Christian Birkl <christian.birkl@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//  Jb Evain <jbevain@novell.com>
//
// Copyright (C) 2006 Christian Birkl
// Copyright (C) 2006, 2008 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme {

	public class XmlResultWriter : ResultWriter, IDisposable {

		XmlWriter writer;
		XElement root;

		public XmlResultWriter (IRunner runner, string fileName)
			: base (runner, fileName)
		{
			writer = XmlWriter.Create (
				CreateWriterFor (fileName),
				new XmlWriterSettings { Indent = true, CloseOutput = true });
		}

		static TextWriter CreateWriterFor (string fileName)
		{
			if (String.IsNullOrEmpty (fileName))
				return System.Console.Out;
			else
				return new StreamWriter (fileName, false, Encoding.UTF8);
		}

		protected override void Start ()
		{
			root = new XElement ("gendarme-output",
				new XAttribute ("date", DateTime.UtcNow.ToString ()));
		}

		protected override void Write ()
		{
			root.Add (
				CreateFiles (),
				CreateRules (),
				CreateDefects ());
		}

		private XElement CreateFiles ()
		{
			return new XElement ("files",
				from AssemblyDefinition assembly in Runner.Assemblies
				select new XElement ("file",
					new XAttribute ("Name", assembly.Name.FullName),
					new XText (assembly.MainModule.Image.FileInformation.FullName)));
		}

		static string GetRuleType (IRule rule)
		{
			if (rule is IAssemblyRule)
				return "Assembly";
			if (rule is ITypeRule)
				return "Type";
			if (rule is IMethodRule)
				return "Method";

			throw new NotSupportedException ("RuleType not supported: " + rule.GetType ());
		}

		private XElement CreateRules ()
		{
			return new XElement ("rules",
				from rule in Runner.Rules
				select CreateRule (rule, GetRuleType (rule)));
		}

		static XElement CreateRule (IRule rule, string type)
		{
			return new XElement ("rule",
				new XAttribute ("Name", rule.Name),
				new XAttribute ("Type", type),
				new XAttribute ("Uri", rule.Uri.ToString ()),
				new XText (rule.GetType ().FullName));
		}

		private XElement CreateDefects ()
		{
			var query = from n in Runner.Defects
				    orderby n.Assembly.Name.FullName, n.Rule.Name
				    group n by n.Rule into a
				    select new {
					    Rule = a.Key,
					    Value = from o in a
						    group o by o.Target into r
						    select new {
							    Target = r.Key,
							    Value = r
						    }
				    };

			return new XElement ("results",
				from value in query
				select new XElement ("rule",
					CreateRuleDetails (value.Rule),
					from v2 in value.Value
					select new XElement ("target",
						CreateTargetDetails (v2.Target),
						from Defect defect in v2.Value
						select CreateDefect (defect))));
		}

		static XObject [] CreateRuleDetails (IRule rule)
		{
			return new XObject [] {
				new XAttribute ("Name", rule.Name),
				new XAttribute ("Uri", rule.Uri.ToString ()),
				new XElement ("problem", rule.Problem),
				new XElement ("solution", rule.Solution) };
		}

		static XObject [] CreateTargetDetails (IMetadataTokenProvider target)
		{
			return new XObject [] {
				new XAttribute ("Name", target.ToString ()),
				new XAttribute ("Assembly", target.GetAssembly ().Name.FullName) };
		}

		static XElement CreateDefect (Defect defect)
		{
			return new XElement ("defect",
				new XAttribute ("Severity", defect.Severity.ToString ()),
				new XAttribute ("Confidence", defect.Confidence.ToString ()),
				new XAttribute ("Location", defect.Location.ToString ()),
				new XAttribute ("Source", defect.Source),
				new XText (defect.Text));
		}

		protected override void Finish ()
		{
			var document = new XDocument (
				new XDeclaration ("1.0", "utf-8", "yes"),
				root);

			document.Save (writer);
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposing)
				return;

			if (writer != null) {
				(writer as IDisposable).Dispose ();
				writer = null;
			}
		}
	}
}
