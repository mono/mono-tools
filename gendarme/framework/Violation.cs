//
// Gendarme.Framework.Violation class
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2005-2006,2008 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;

namespace Gendarme.Framework {

	public struct Violation {

		private IRule rule;
		private object violator;
		private MessageCollection messages;

		public Violation (IRule rule, object violator, MessageCollection messages)
		{
			this.rule = rule;
			this.violator = violator;
			this.messages = messages;
		}

		public AssemblyDefinition Assembly {
			get {
				AssemblyDefinition ad = (Violator as AssemblyDefinition);
				if (ad != null)
					return ad;
					
				ModuleDefinition md = (Violator as ModuleDefinition);
				if (md != null)
					return md.Assembly;

				TypeDefinition td = (Violator as TypeDefinition);
				if (td != null)
					return td.Module.Assembly;
					
				MethodDefinition nd = (Violator as MethodDefinition);
				if (nd != null)
					return nd.DeclaringType.Module.Assembly;

				return null;
			}
		}

		public MessageCollection Messages {
			get { return messages; }
		}

		public IRule Rule {
			get { return rule; }
		}

		public object Violator {
			get { return violator; }
		}
	}
}
