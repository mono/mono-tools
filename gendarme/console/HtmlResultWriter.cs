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
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

using Gendarme.Framework;

namespace Gendarme {

	public class HtmlResultWriter : ResultWriter {

		private string temp_filename;

		public HtmlResultWriter (IRunner runner, string fileName)
			: base (runner, fileName)
		{
			temp_filename = Path.GetTempFileName ();
		}

		protected override void Write()
		{
			using (XmlResultWriter writer = new XmlResultWriter (Runner, temp_filename)) {
				writer.Report ();
			}
		}

		protected override void Finish ()
		{
			// load XSL file from embedded resource
			using (Stream s = Helpers.GetStreamFromResource ("gendarme.xsl")) {
				if (s == null)
					throw new InvalidDataException ("Could not locate XSL style sheet inside resources.");
				// process the XML result with the XSL file
				XslCompiledTransform xslt = new XslCompiledTransform ();
				using (XmlTextReader xmlReader = new XmlTextReader (s))
					xslt.Load (xmlReader);
				xslt.Transform (temp_filename, FileName);
			}
		}

		[ThreadModel (ThreadModel.SingleThread)]
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (File.Exists (temp_filename))
					File.Delete (temp_filename);
			}
		}
	}
}
