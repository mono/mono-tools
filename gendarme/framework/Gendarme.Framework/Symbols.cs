// 
// Gendarme.Framework.Symbols
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

	public static class Symbols {

		// http://blogs.msdn.com/jmstall/archive/2005/06/19/FeeFee_SequencePoints.aspx
		private const int PdbHiddenLine = 0xFEEFEE;

		private static string AlmostEqualTo = new string (new char [] { '\u2248' });

		private static Instruction ExtractFirst (TypeDefinition type)
		{
			if (type == null)
				return null;
			foreach (MethodDefinition method in type.Methods) {
				Instruction ins = ExtractFirst (method);
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
			// note that the first instruction often does not have a sequence point
			while (ins != null && ins.SequencePoint == null)
				ins = ins.Next;
				
			return (ins != null && ins.SequencePoint != null) ? ins : null;
		}

		private static TypeDefinition FindTypeFromLocation (IMetadataTokenProvider location)
		{
			MethodDefinition method = (location as MethodDefinition);
			if (method != null)
				return (method.DeclaringType as TypeDefinition);

			FieldDefinition field = (location as FieldDefinition);
			if (field != null)
				return (field.DeclaringType as TypeDefinition);

			ParameterDefinition parameter = (location as ParameterDefinition);
			if (parameter != null)
				return FindTypeFromLocation (parameter.Method);

			return (location as TypeDefinition);
		}

		private static MethodDefinition FindMethodFromLocation (IMetadataTokenProvider location)
		{
			ParameterDefinition parameter = (location as ParameterDefinition);
			if (parameter != null)
				return (parameter.Method as MethodDefinition);

			MethodReturnType return_type = (location as MethodReturnType);
			if (return_type != null)
				return (return_type.Method as MethodDefinition);

			return (location as MethodDefinition);
		}

		private static string FormatSequencePoint (SequencePoint sp, bool exact)
		{
			return FormatSequencePoint (sp.Document.Url, sp.StartLine, sp.StartColumn, exact);
		}
		
		// It would probably be a good idea to move this formatting into
		// the reporting layer. The XML formatter would ideally not do
		// any formatting at all so that tools could extract the line
		// and column information without complex parsing.
		//
		// We might also want to allow some sort of customization of the
		// formatting used by the text reporter. For example, most editors
		// on the Mac have direct support for paths like foo/bar.cs:10 
		// which include line numbers and foo/bar.cs:10:5 for paths which
		// include line and column.
		private static string FormatSequencePoint (string document, int line, int column, bool exact)
		{
			string sline = (line == PdbHiddenLine) ? "unavailable" : line.ToString (CultureInfo.InvariantCulture);

			// MDB (mono symbols) does not provide any column information (so we don't show any)
			// there's also no point in showing a column number if we're not totally sure about the line
			if (exact && (column > 0))
				return String.Format (CultureInfo.InvariantCulture, "{0}({1},{2})", document, sline, column);

			return String.Format (CultureInfo.InvariantCulture, "{0}({2}{1})", document, sline,
				exact ? String.Empty : AlmostEqualTo);
		}

		private static string GetSource (Instruction ins)
		{
			// try to find the closed sequence point for this instruction
			Instruction search = ins;
			bool feefee = false;
			while (search != null) {
				// find the first entry, going backward, with a SequencePoint
				if (search.SequencePoint != null) {
					// skip entries that are hidden (0xFEEFEE)
					if (search.SequencePoint.StartLine != PdbHiddenLine)
						return FormatSequencePoint (search.SequencePoint, feefee);
					// but from here on we're not 100% sure about line numbers
					feefee = true;
				}

				search = search.Previous;
			}
			// no details, we only have the IL offset to report
			return String.Format (CultureInfo.InvariantCulture, "debugging symbols unavailable, IL offset 0x{0:x4}", ins.Offset);
		}
		
		static private string FormatSource (Instruction candidate)
		{
			int line = candidate.SequencePoint.StartLine;
			// we approximate (line - 1, no column) to get (closer) to the definition
			// unless we have the special 0xFEEFEE value (used in PDB for hidden source code)
			if (line != PdbHiddenLine)
				line--;
			return FormatSequencePoint (candidate.SequencePoint.Document.Url, line, 0, false);
		}

		static public string GetSource (Defect defect)
		{
			if (defect == null)
				return String.Empty;

			if (defect.Instruction != null)
				return GetSource (defect.Instruction);

			// rule didn't provide an Instruction but we do our best to
			// find something since this is our only link to the source code

			Instruction candidate;
			TypeDefinition type = null;

			// MethodDefinition, ParameterDefinition
			//	return the method source file with (approximate) line number
			MethodDefinition method = FindMethodFromLocation (defect.Location);
			if (method != null) {
				candidate = ExtractFirst (method);
				if (candidate != null) 
					return FormatSource (candidate);

				// we may still be lucky to find the (a) source file for the type itself
				type = (method.DeclaringType as TypeDefinition);
			}

			// TypeDefinition, FieldDefinition
			//	return the type source file (based on the first ctor)
			if (type == null)
				type = FindTypeFromLocation (defect.Location);
			candidate = ExtractFirst (type);
			if (candidate != null)
				return FormatSource (candidate);

			return String.Empty;
		}
	}
}
