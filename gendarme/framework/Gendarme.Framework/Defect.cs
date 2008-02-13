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
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Framework {

	// implementation of Defect classes must be immutable
	abstract public class Defect {

		IRule rule;
		string text;

		protected Defect (IRule rule, string text)
		{
			this.rule = rule;
			this.text = text;
		}

		public IRule Rule {
			get { return rule; }
		}

		public string Text {
			get { return text; }
		}

		abstract public AssemblyDefinition Assembly { get; }

		abstract public Confidence Confidence { get; }

		abstract public string Location { get; }

		abstract public Severity Severity { get; }

		abstract public object Target { get; }
	}

	public class Defect<T> : Defect {

		private T location;
		private Severity severity;
		private Confidence confidence;
		private Instruction ins;

		public Defect (IRule rule, T location, Severity severity, Confidence confidence, string text)
			: base (rule, text)
		{
			this.location = location;
			this.confidence = confidence;
			this.severity = severity;
		}

		public Defect (IRule rule, T location, Instruction ins, Severity severity, Confidence confidence, string text)
			: this (rule, location, severity, confidence, text)
		{
			// this ctor is usable only for MethodDefinition
			if (!(location is MethodDefinition))
				throw new ArgumentException ("Only MethodDefinition can be used with this constructor.", "location");
			this.ins = ins;
		}

		public override AssemblyDefinition Assembly {
			get {
				AssemblyDefinition ad = (location as AssemblyDefinition);
				if (ad != null)
					return ad;

				/*ModuleDefinition md = (location as ModuleDefinition);
				if (md != null)
					return md.Assembly;*/

				TypeDefinition td = (location as TypeDefinition);
				if (td != null)
					return td.Module.Assembly;

				MethodDefinition md = (location as MethodDefinition);
				if (md != null)
					return md.DeclaringType.Module.Assembly;

				FieldDefinition fd = (location as FieldDefinition);
				if (fd != null)
					return fd.DeclaringType.Module.Assembly;

				ParameterDefinition pd = (location as ParameterDefinition);
				if (pd != null)
					return pd.Method.DeclaringType.Module.Assembly;

				return null;
			}
		}

		public override Confidence Confidence {
			get { return confidence; }
		}

		public override string Location {
			get {
				if (ins == null)
					return Assembly.Name.FullName;

				MethodDefinition method = (location as MethodDefinition);
				StringBuilder sb = new StringBuilder ();

				// try to find to most
				Instruction search = ins;
				while (search != null) {
					if (search.SequencePoint != null) {
						sb.AppendFormat ("{0}({1},{2})", search.SequencePoint.Document.Url,
							search.SequencePoint.StartLine, search.SequencePoint.StartColumn);
						return sb.ToString ();
					}
					search = search.Previous;
				}

				sb.Append (method.ToString ());
				sb.AppendFormat (":(0x{0:x4})", ins.Offset);
				return sb.ToString ();
			}
		}

		public override Severity Severity {
			get { return severity; }
		}

		public override object Target {
			get { return location; }
		}
	}
}
