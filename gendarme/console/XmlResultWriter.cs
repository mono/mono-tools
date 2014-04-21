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
using System.Collections.Generic;

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
            //Built-in .net encoding for XML strings doesn't appear to handle all cases, so
            //we need an alternative approach.
            writer.WriteRaw (XmlTextEncoder.Encode (defect.Text));
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

        /// <summary>
        /// Borrowed from mkropat's .NET-Snippets:
        /// https://github.com/mkropat/.NET-Snippets/blob/master/XmlTextEncoder.cs
        /// Encodes data so that it can be safely embedded as text in XML documents.
        /// </summary>
        public class XmlTextEncoder : TextReader
        {
            public static string Encode (string s)
            {
                using (var stream = new StringReader (s))
                using (var encoder = new XmlTextEncoder (stream))
                {
                    return encoder.ReadToEnd ();
                }
            }

            /// <param name="source">The data to be encoded in UTF-16 format.</param>
            /// <param name="filterIllegalChars">It is illegal to encode certain
            /// characters in XML. If true, silently omit these characters from the
            /// output; if false, throw an error when encountered.</param>
            public XmlTextEncoder (TextReader source, bool filterIllegalChars = true)
            {
                _source = source;
                _filterIllegalChars = filterIllegalChars;
            }

            readonly Queue<char> _buf = new Queue<char> ();
            readonly bool _filterIllegalChars;
            readonly TextReader _source;

            public override int Peek ()
            {
                PopulateBuffer ();
                if (_buf.Count == 0) return -1;
                return _buf.Peek ();
            }

            public override int Read ()
            {
                PopulateBuffer ();
                if (_buf.Count == 0) return -1;
                return _buf.Dequeue ();
            }

            void PopulateBuffer ()
            {
                const int endSentinel = -1;
                while (_buf.Count == 0 && _source.Peek () != endSentinel)
                {
                    // Strings in .NET are assumed to be UTF-16 encoded [1].
                    var c = (char)_source.Read ();
                    if (Entities.ContainsKey (c))
                    {
                        // Encode all entities defined in the XML spec [2].
                        foreach (var i in Entities [c]) _buf.Enqueue (i);
                    }
                    else if (!(0x0 <= c && c <= 0x8) &&
                             !new[] { 0xB, 0xC }.Contains (c) &&
                             !(0xE <= c && c <= 0x1F) &&
                             !(0x7F <= c && c <= 0x84) &&
                             !(0x86 <= c && c <= 0x9F) &&
                             !(0xD800 <= c && c <= 0xDFFF) &&
                             !new[] { 0xFFFE, 0xFFFF }.Contains (c))
                    {
                        // Allow if the Unicode codepoint is legal in XML [3].
                        _buf.Enqueue (c);
                    }
                    else if (char.IsHighSurrogate (c) &&
                             _source.Peek () != endSentinel &&
                             char.IsLowSurrogate ((char)_source.Peek ()))
                    {
                        // Allow well-formed surrogate pairs [1].
                        _buf.Enqueue (c);
                        _buf.Enqueue ((char)_source.Read ());
                    }
                    else if (!_filterIllegalChars)
                    {
                        // Note that we cannot encode illegal characters as entity
                        // references due to the "Legal Character" constraint of
                        // XML [4]. Nor are they allowed in CDATA sections [5].
                        throw new ArgumentException (
                            String.Format ("Illegal character: '{0:X}'", (int)c));
                    }
                }
            }

            static readonly Dictionary<char, string> Entities =
                new Dictionary<char, string> {
                { '"', "&quot;" }, { '&', "&amp;"}, { '\'', "&apos;" },
                { '<', "&lt;" }, { '>', "&gt;" },
            };

            // References:
            // [1] http://en.wikipedia.org/wiki/UTF-16/UCS-2
            // [2] http://www.w3.org/TR/xml11/#sec-predefined-ent
            // [3] http://www.w3.org/TR/xml11/#charsets
            // [4] http://www.w3.org/TR/xml11/#sec-references
            // [5] http://www.w3.org/TR/xml11/#sec-cdata-sect
        }
	}
}
