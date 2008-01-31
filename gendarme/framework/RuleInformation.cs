//
// Gendarme.Framework.RuleInformation class
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
using System.Text;

namespace Gendarme.Framework {

	public class RuleInformation {

		static private RuleInformation empty = new RuleInformation ();

		private string name;
		private string uri;
		private string problem;
		private string solution;

		public RuleInformation ()
		{
			name = String.Empty;
			uri = String.Empty;
			problem = String.Empty;
			solution = String.Empty;
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public string Uri {
			get { return uri; }
			set { uri = value; }
		}

		public string Problem {
			get { return problem; }
			set { problem = value; }
		}

		public string Solution {
			get { return solution; }
			set { solution = value;	}
		}

		static public RuleInformation Empty {
			get { return empty; }
		}
	}
}
