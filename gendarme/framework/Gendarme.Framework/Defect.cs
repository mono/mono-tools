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

		public Defect (IRule rule, IMetadataTokenProvider target, IMetadataTokenProvider location, Instruction ins, Severity severity, Confidence confidence, string text)
			: this (rule, target, location, severity, confidence, text)
		{
			// this ctor is usable only for MethodDefinition
			if (!(location is MethodDefinition))
				throw new ArgumentException ("Only MethodDefinition can be used with this constructor.", "location");
			this.ins = ins;
		}

		public AssemblyDefinition Assembly {
			get { return target.GetAssembly (); }
		}

		public Confidence Confidence {
			get { return confidence; }
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

		private static Instruction ExtractFirst (TypeDefinition type)
		{
			if (type == null)
				return null;
			foreach (MethodDefinition ctor in type.Constructors) {
				Instruction ins = ExtractFirst (ctor);
				if (ins != null)
					return ins;
			}
			return null;
		}

		private static Instruction ExtractFirst (MethodDefinition method)
		{
			if ((method == null) || !method.HasBody)
				return null;
			Instruction ins = method.Body.Instructions [0];
			return (ins.SequencePoint != null) ? ins : null;
		}

		private TypeDefinition FindTypeFromLocation ()
		{
			MethodDefinition method = (location as MethodDefinition);
			if (method != null)
				return (method.DeclaringType as TypeDefinition);

			FieldDefinition field = (location as FieldDefinition);
			if (field != null)
				return (field.DeclaringType as TypeDefinition);

			ParameterDefinition parameter = (location as ParameterDefinition);
			if (parameter != null)
				return (parameter.Method.DeclaringType as TypeDefinition);

			return (location as TypeDefinition);
		}

		private MethodDefinition FindMethodFromLocation ()
		{
			ParameterDefinition parameter = (location as ParameterDefinition);
			if (parameter != null)
				return (parameter.Method as MethodDefinition);

			return (location as MethodDefinition);
		}

		public string Source {
			get {
				if (ins != null) {
					// try to find the closed sequence point for this instruction
					Instruction search = ins;
					while (search != null) {
						if (search.SequencePoint != null) {
							// real details were provided
							return String.Format (CultureInfo.InvariantCulture, "{0}({1},{2})",
								search.SequencePoint.Document.Url,
								search.SequencePoint.StartLine,
								search.SequencePoint.StartColumn);
						}
						search = search.Previous;
					}
					// no details, we only have the IL offset to report
					return String.Format (CultureInfo.InvariantCulture, "debugging information unavailable, IL offset 0x{0:x4}", ins.Offset);
				}

				// rule didn't provide an Instruction but we do our best to
				// find something since this is our only link to the source code

				Instruction candidate;
				TypeDefinition type = null;

				// MethodDefinition, ParameterDefinition
				//	return the method source file with (appromixate) line number
				MethodDefinition method = FindMethodFromLocation ();
				if (method != null) {
					candidate = ExtractFirst (method);
					if (candidate != null) {
						// we approximated the results (line - 1, no column)
						// to get (close) to the method definition
						return String.Format (CultureInfo.InvariantCulture, "{0}({1})",
							candidate.SequencePoint.Document.Url,
							candidate.SequencePoint.StartLine - 1);
					}
					// we may still be lucky to find the (a) source file for the type itself
					type = (method.DeclaringType as TypeDefinition);
				}

				// TypeDefinition, FieldDefinition
				//	return the type source file (based on the first ctor)
				if (type == null)
					type = FindTypeFromLocation ();
				candidate = ExtractFirst (type);
				if (candidate != null) {
					// we report only the source file of the first ctor (that reported something)
					return (candidate.SequencePoint.Document.Url);
				}

				return String.Empty;
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
