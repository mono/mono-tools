//
// XmlResultWriter
//
// Authors:
//	Christian Birkl <christian.birkl@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
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
using System.Linq;
using System.Text;
using System.Xml;

using Mono.Cecil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme {

	public class XmlResultWriter : ResultWriter, IDisposable {

		private XmlTextWriter writer;

		public XmlResultWriter (IRunner runner, string fileName)
			: base (runner, fileName)
		{
			if ((fileName == null) || (fileName.Length == 0))
				writer = new XmlTextWriter (System.Console.Out);
			else
				writer = new XmlTextWriter (fileName, Encoding.UTF8);
		}

		protected override void Start ()
		{
			writer.Formatting = Formatting.Indented;
			writer.WriteProcessingInstruction ("xml", "version='1.0'");
			writer.WriteStartElement ("gendarme-output");
			writer.WriteAttributeString ("date", DateTime.UtcNow.ToString ());
		}

		protected override void Write ()
		{
			WriteFiles ();
			WriteRules ();
			WriteDefects ();
		}

		private void WriteFiles ()
		{
			writer.WriteStartElement ("files");
			foreach (AssemblyDefinition assembly in Runner.Assemblies) {
				writer.WriteStartElement ("file");
				writer.WriteAttributeString ("Name", assembly.Name.FullName);
				writer.WriteString (assembly.MainModule.Image.FileInformation.FullName);
				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();
		}

		private void WriteRules ()
		{
			writer.WriteStartElement ("rules");
			foreach (IRule rule in Runner.Rules) {
				if (rule is IAssemblyRule)
					WriteRule (rule, "Assembly");
				if (rule is ITypeRule)
					WriteRule (rule, "Type");
				if (rule is IMethodRule)
					WriteRule (rule, "Method");
			}
			writer.WriteEndElement ();
		}

		private void WriteRule (IRule rule, string type)
		{
			writer.WriteStartElement ("rule");
			writer.WriteAttributeString ("Name", rule.Name);
			writer.WriteAttributeString ("Type", type);
			writer.WriteAttributeString ("Uri", rule.Uri.ToString ());
			writer.WriteString (rule.GetType ().FullName);
			writer.WriteEndElement ();
		}

		private void WriteDefects ()
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

			writer.WriteStartElement ("results");
			foreach (var value in query) {
				writer.WriteStartElement ("rule");
				WriteRuleDetails (value.Rule);
				foreach (var v2 in value.Value) {
					writer.WriteStartElement ("target");
					WriteTargetDetails (v2.Target);
					foreach (Defect defect in v2.Value) {
						WriteDefect (defect);
					}
					writer.WriteEndElement ();
				}
				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();
		}

		private void WriteRuleDetails (IRule rule)
		{
			writer.WriteAttributeString ("Name", rule.Name);
			writer.WriteAttributeString ("Uri", rule.Uri.ToString ());
			writer.WriteElementString ("problem", rule.Problem);
			writer.WriteElementString ("solution", rule.Solution);
		}

		private void WriteTargetDetails (IMetadataTokenProvider target)
		{
			writer.WriteAttributeString ("Name", target.ToString ());
			writer.WriteAttributeString ("Assembly", target.GetAssembly ().Name.FullName);
		}

		private void WriteDefect (Defect defect)
		{
			writer.WriteStartElement ("defect");
			writer.WriteAttributeString ("Severity", defect.Severity.ToString ());
			writer.WriteAttributeString ("Confidence", defect.Confidence.ToString ());
			writer.WriteAttributeString ("Location", defect.Location.ToString ());
			writer.WriteAttributeString ("Source", defect.Source);
			writer.WriteString (defect.Text);
			writer.WriteEndElement ();
		}

		protected override void Finish ()
		{
			writer.WriteEndElement ();
			writer.Flush ();
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (writer != null) {
					(writer as IDisposable).Dispose ();
					writer = null;
				}
			}
		}
	}
}
