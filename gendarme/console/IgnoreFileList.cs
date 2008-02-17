//
// IgnoreFileList - Ignore defects based on a file descriptions
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;

namespace Gendarme {

	public class IgnoreFileList : BasicIgnoreList {

		private string rule;

		public IgnoreFileList (IRunner runner, string fileName)
			: base (runner)
		{
			if (!String.IsNullOrEmpty (fileName) && File.Exists (fileName)) {
				Parse (fileName);
			}
		}

		private void Parse (string fileName)
		{
			using (StreamReader sr = new StreamReader (fileName)) {
				string s = sr.ReadLine ();
				while (s != null) {
					ProcessLine (s);
					s = sr.ReadLine ();
				}
			}
		}

		private void ProcessLine (string line)
		{
			if (line.Length < 1)
				return;

			switch (line [0]) {
			case '#': // comment
				break;
			case 'R': // rule
				rule = line.Substring (line.LastIndexOf (' ') + 1);
				break;
			case 'A': // assembly
				AddAssembly (rule, line.Substring (2).Trim ());
				break;
			case 'T': // type (no space allowed)
				AddType (rule, line.Substring (line.LastIndexOf (' ') + 1));
				break;
			case 'M': // method
				AddMethod (rule, line.Substring (2).Trim ());
				break;
			default:
				Console.WriteLine ("Bad ignore entry : '{0}'", line);
				break;
			}
		}
	}
}
