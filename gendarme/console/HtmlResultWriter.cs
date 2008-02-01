//
// HtmlResultWriter.cs
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
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
using System.Collections;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

using Gendarme.Framework;

namespace Gendarme.Console.Writers {

	public class HtmlResultWriter : IResultWriter {

		private XmlResultWriter writer;
		private string temp_filename;
		private string final_filename;

		public HtmlResultWriter (string output)
		{
			final_filename = output;
			temp_filename = Path.GetTempFileName ();
			writer = new XmlResultWriter (temp_filename);
		}

		public void Start ()
		{
			writer.Start ();
		}

		public void End ()
		{
			try {
				writer.End ();
				// load XSL file from embedded resource
				using (Stream s = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("gendarme.xsl")) {
					// process the XML result with the XSL file
					XslCompiledTransform xslt = new XslCompiledTransform ();
					xslt.Load (new XmlTextReader (s));
					xslt.Transform (temp_filename, final_filename);
				}
			}
			finally {
				File.Delete (temp_filename);
			}
		}

		public void Write (IDictionary assemblies)
		{
			writer.Write (assemblies);
		}

		public void Write (Rules rules)
		{
			writer.Write (rules);
		}

		public void Write (Violation v)
		{
			writer.Write (v);
		}
	}
}
