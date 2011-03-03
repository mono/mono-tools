//
// XmlResultWriter
//
// Authors:
//	Christian Birkl <christian.birkl@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//	Jb Evain <jbevain@novell.com>
//	Eric Zeitler <eric.zeitler@gmail.com>
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
using System.Globalization;
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

		private const string AssemblySet = "[across all assemblies analyzed]";

		XmlWriter writer;

		public XmlResultWriter (IRunner runner, string fileName)
			: base (runner, fileName)
		{
			writer = XmlWriter.Create (
				CreateWriterFor (fileName),
				new XmlWriterSettings { Indent = true, CloseOutput = true, CheckCharacters = false });
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
			writer.WriteStartDocument ();
			writer.WriteStartElement ("gendarme-output");
			writer.WriteAttributeString ("date", DateTime.UtcNow.ToString (CultureInfo.InvariantCulture));
		}

		protected override void Write ()
		{
			CreateFiles ();
			CreateRules ();
			CreateDefects ();
		}

		private void CreateFiles ()
		{
			writer.WriteStartElement ("files");
			foreach (AssemblyDefinition assembly in Runner.Assemblies) {
				writer.WriteStartElement ("file");
				writer.WriteAttributeString ("Name", assembly.FullName);
				writer.WriteString (assembly.MainModule.FullyQualifiedName);
				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();
		}

		static string GetRuleType (IRule rule)
		{
			if (rule is IAssemblyRule)
				return "Assembly";
			if (rule is ITypeRule)
				return "Type";
			if (rule is IMethodRule)
				return "Method";
			return "Other";
		}

		private void CreateRules ()
		{
			writer.WriteStartElement ("rules");
			foreach (var rule in Runner.Rules) {
				if (rule.Active) {
					CreateRule (rule, GetRuleType (rule));
				}
			}
			writer.WriteEndElement ();
		}

		void CreateRule (IRule rule, string type)
		{
			writer.WriteStartElement ("rule");
			writer.WriteAttributeString ("Name", rule.Name);
			writer.WriteAttributeString ("Type", type);
			writer.WriteAttributeString ("Uri", rule.Uri.ToString ());
			writer.WriteString (rule.GetType ().FullName);
			writer.WriteEndElement ();
		}

		private void CreateDefects ()
		{
			var query = from n in Runner.Defects
				    group n by n.Rule into a
				    orderby a.Key.Name
				    select new {
					    Rule = a.Key,
					    Value = from o in a
						    group o by o.Target into r
						    orderby (r.Key.GetAssembly () == null ? String.Empty : r.Key.GetAssembly ().Name.FullName)
						    select new {
							    Target = r.Key,
							    Value = r
						    }
				    };

			writer.WriteStartElement ("results");
			foreach (var value in query) {
				writer.WriteStartElement ("rule");
				CreateRuleDetails (value.Rule);
				foreach (var v2 in value.Value) {
					writer.WriteStartElement ("target");
					CreateTargetDetails (v2.Target);
					foreach (Defect defect in v2.Value) {
						CreateElement (defect);
					}
					writer.WriteEndElement ();
				}
				writer.WriteEndElement ();
			}
			writer.WriteEndElement ();
		}

		void CreateRuleDetails (IRule rule)
		{
			writer.WriteAttributeString ("Name", rule.Name);
			writer.WriteAttributeString ("Uri", rule.Uri.ToString ());
			writer.WriteElementString ("problem", rule.Problem);
			writer.WriteElementString ("solution", rule.Solution);
		}

		void CreateTargetDetails (IMetadataTokenProvider target)
		{
			AssemblyDefinition assembly = target.GetAssembly ();

			writer.WriteAttributeString ("Name", target.ToString ());
			writer.WriteAttributeString ("Assembly", assembly == null ? AssemblySet : assembly.Name.FullName);
		}

		void CreateElement (Defect defect)
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
			writer.WriteEndDocument ();
			writer.Close ();
		}

		[ThreadModel (ThreadModel.SingleThread)]
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
