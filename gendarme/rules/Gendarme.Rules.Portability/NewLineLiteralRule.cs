//
// Gendarme.Rules.Portability.NewLineLiteralRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2006-2008 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Text;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

namespace Gendarme.Rules.Portability {

	/// <summary>
	/// This rule warns if methods, including properties, are using the literal 
	/// <c>\r</c> and/or <c>\n</c> for new lines. This isn't portable across operating systems.
	/// To ensure correct cross-platform functionality they should be replaced by 
	/// <c>System.Environment.NewLine</c>.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// Console.WriteLine ("Hello,\nYou should be using Gendarme!");
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// Console.WriteLine ("Hello,{0}You must be using Gendarme!", Environment.NewLine);
	/// </code>
	/// </example>

	[Problem ("The method uses literals for new lines (e.g. \\r\\n) which isn't portable across operating systems.")]
	[Solution ("Replace the literals with Environment.NewLine.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class NewLineLiteralRule : Rule, IMethodRule {

		private static char[] InvalidChar = { '\r', '\n' };

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// methods can be empty (e.g. p/invoke declarations)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Ldstr instructions in this method
			if (!OpCodeEngine.GetBitmask (method).Get (Code.Ldstr))
				return RuleResult.DoesNotApply;

			// rule applies

			foreach (Instruction ins in method.Body.Instructions) {
				// look for a string load
				if (ins.OpCode.Code != Code.Ldstr)
					continue;

				// check the string being referenced by the instruction
				string s = (ins.Operand as string);
				if (String.IsNullOrEmpty (s))
					continue;

				if (s.IndexOfAny (InvalidChar) >= 0) {
					Runner.Report (method, ins, Severity.Low, Confidence.High, FormatStringForDisplay (s));
				}
			}

			return Runner.CurrentRuleResult;
		}

		// Format the string to looks like in the C# source code
		// For example the character \x01 is converted to the "\x01" string
		// This operation avoid crash with special characters when applying the XSL
		// transform to produce the html report
		private static string FormatStringForDisplay (string value)
		{
			StringBuilder result = new StringBuilder ("Found string: \"");
			foreach (char c in value) {
				if (!Char.IsControl (c)) {
					result.Append (c);
					continue;
				}

				// make the invalid char visible on output
				if (c == '\n')
					result.Append ("\\n");
				else if (c == '\r')
					result.Append ("\\r");
				else if (c == '\t')
					result.Append ("\\t");
				else
					result.Append ("\\x").Append (((short) c).ToString ("x", CultureInfo.InvariantCulture));
			}
			return result.Append ("\".").ToString ();
		}
	}
}
