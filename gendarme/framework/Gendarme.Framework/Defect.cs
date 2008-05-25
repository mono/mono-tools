// 
// Gendarme.Framework.Defect
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework.Rocks;

namespace Gendarme.Framework {

	public class Defect {

		private IRule rule;
		private IMetadataTokenProvider target;
		private IMetadataTokenProvider location;
		private Severity severity;
		private Confidence confidence;
		private Instruction ins;
		private string text;
		private string source;

		public Defect (IRule rule, IMetadataTokenProvider target, IMetadataTokenProvider location, Severity severity, Confidence confidence, string text)
		{
			if (rule == null)
				throw new ArgumentNullException ("rule");
			if (target == null)
				throw new ArgumentNullException ("target");
			if (location == null)
				throw new ArgumentNullException ("location");

			this.rule = rule;
			this.target = target;
			this.location = location;
			this.confidence = confidence;
			this.severity = severity;
			this.text = text;
		}

		public Defect (IRule rule, IMetadataTokenProvider target, IMetadataTokenProvider location, Severity severity, Confidence confidence)
			: this (rule, target, location, severity, confidence, String.Empty)
		{
		}

		public Defect (IRule rule, IMetadataTokenProvider target, MethodDefinition location, Instruction ins, Severity severity, Confidence confidence, string text)
			: this (rule, target, location, severity, confidence, text)
		{
			this.ins = ins;
		}

		public Defect (IRule rule, IMetadataTokenProvider target, MethodDefinition location, Instruction ins, Severity severity, Confidence confidence)
			: this (rule, target, location, ins, severity, confidence, String.Empty)
		{
		}

		public AssemblyDefinition Assembly {
			get { return target.GetAssembly (); }
		}

		public Confidence Confidence {
			get { return confidence; }
		}

		public Instruction Instruction {
			get { return ins; }
		}

		public IMetadataTokenProvider Location {
			get { return location; }
		}

		public IRule Rule {
			get { return rule; }
		}

		public Severity Severity {
			get { return severity; }
		}

		public string Source {
			get {
				if (source == null)
					source = Symbols.GetSource (this);
				return source;
			}
		}

		public IMetadataTokenProvider Target {
			get { return target; }
		}

		public string Text {
			get { return text; }
		}
	}
}
