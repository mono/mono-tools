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
using System.Collections;
using System.IO;

using Gendarme.Framework;

namespace Gendarme.Console.Writers {

	public class TextResultWriter : IResultWriter {

		private TextWriter writer;
		private bool need_closing;
		private int index;

		public TextResultWriter (string output)
		{
			if ((output == null) || (output.Length == 0))
				writer = System.Console.Out;
			else {
				writer = new StreamWriter (output);
				need_closing = true;
			}
		}

		public void Start ()
		{
			index = 0;
		}

		public void End ()
		{
			if (need_closing)
				writer.Close ();
		}

		public void Write (IDictionary assemblies)
		{
		}

		public void Write (Rules rules)
		{
		}

		public void Write (Violation v)
		{
			RuleInformation ri = RuleInformationManager.GetRuleInformation (v.Rule);
			writer.WriteLine ("{0}. {1}", ++index, ri.Name);
			writer.WriteLine ();
			writer.WriteLine ("Problem: {0}", String.Format (ri.Problem, v.Violator));
			writer.WriteLine ();
			if (v.Messages != null && v.Messages.Count > 0) {
				writer.WriteLine ("Details:");
				foreach (Message message in v.Messages) {
					writer.WriteLine ("  {0}", message);
				}
				writer.WriteLine ();
			}
			writer.WriteLine ("Solution: {0}", String.Format (ri.Solution, v.Violator));
			writer.WriteLine ();
			string url = ri.Uri;
			if (url.Length > 0) {
				writer.WriteLine ("More info available at: {0}", url);
				writer.WriteLine ();
			}
			writer.WriteLine ();
		}
	}
}
