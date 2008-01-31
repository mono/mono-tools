//
// Gendarme.Framework.Rules class
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

	public class Rules {

		private RuleCollection assemblyRules;
		private RuleCollection moduleRules;
		private RuleCollection typeRules;
		private RuleCollection methodRules;

		internal Rules ()
		{
		}

		public RuleCollection Assembly {
			get {
				if (assemblyRules == null)
					assemblyRules = new RuleCollection ();
				return assemblyRules;
			}
		}

		public RuleCollection Module {
			get {
				if (moduleRules == null)
					moduleRules = new RuleCollection ();
				return moduleRules;
			}
		}

		public RuleCollection Type {
			get {
				if (typeRules == null)
					typeRules = new RuleCollection ();
				return typeRules;
			}
		}

		public RuleCollection Method {
			get {
				if (methodRules == null)
					methodRules = new RuleCollection ();
				return methodRules;
			}
		}
	}
}
