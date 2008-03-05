//
// TextResultWriter
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Mono.Cecil;

using Gendarme.Framework;

namespace Gendarme {

	public class TextResultWriter : ResultWriter, IDisposable {

		private TextWriter writer;
		private bool need_closing;

		public TextResultWriter (IRunner runner, string fileName)
			: base (runner, fileName)
		{
			if ((fileName == null) || (fileName.Length == 0))
				writer = System.Console.Out;
			else {
				writer = new StreamWriter (fileName);
				need_closing = true;
			}
		}

		protected override void Write ()
		{
			int index = 0;

			var query = from n in Runner.Defects
				    orderby n.Severity
				    select n;

			foreach (Defect defect in query) {
				IRule rule = defect.Rule;

				writer.WriteLine ("{0}. {1}", ++index, rule.Name);
				writer.WriteLine ();
				writer.WriteLine ("Problem: {0}", rule.Problem);
				writer.WriteLine ();
				writer.WriteLine ("Details [Severity: {0}, Confidence: {1}]", defect.Severity, defect.Confidence);
				writer.WriteLine ("* Target: {0}", defect.Target);
				writer.WriteLine ("* Location: {0}", defect.Location);
				if (!String.IsNullOrEmpty (defect.Text))
					writer.WriteLine ("* {0}", defect.Text);
				writer.WriteLine ();
				writer.WriteLine ("Solution: {0}", rule.Solution);
				writer.WriteLine ();
				writer.WriteLine ("More info available at: {0}", rule.Uri.ToString ());
				writer.WriteLine ();
				writer.WriteLine ();
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (need_closing) {
					writer.Dispose ();
				}
			}
		}
	}
}
