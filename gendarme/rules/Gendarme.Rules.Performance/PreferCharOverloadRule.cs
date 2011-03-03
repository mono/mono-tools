//
// Gendarme.Rules.Performance.PreferCharOverloadRule
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
using System.Collections.Generic;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	// rule request from
	// https://bugzilla.novell.com/show_bug.cgi?id=406889
	
	/// <summary>
	/// This rule looks for calls to <c>String</c> methods that use <b>String</b>
	/// parameters when a <c>Char</c> parameter could have been used. Using the
	/// <c>Char</c> overload is preferred because it will be faster. 
	///
	/// Note, however, that this may result in subtly different behavior on versions of
	/// .NET before 4.0: the string overloads do a culture based comparison using
	/// <c>CultureInfo.CurrentCulture</c> and the char methods do an ordinal
	/// comparison (a simple compare of the character values). This can result in
	/// a change of behavior (for example the two can produce different results when
	/// precomposed characters are used). If this is important it is best to use an
	/// overload that allows StringComparison or CultureInfo to be explicitly specified
	/// see [http://msdn.microsoft.com/en-us/library/ms973919.aspx#stringsinnet20_topic4] 
	/// for more details.
	///
	/// With .NET 4.0 <c>String</c>'s behavior will change and the various methods
	/// will be made more consistent. In particular the comparison methods will be changed
	/// so that they all default to doing an ordinal comparison.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// if (s.IndexOf (":") == -1) {
	///	Console.WriteLine ("no separator found");
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// if (s.IndexOf (':') == -1) {
	///	Console.WriteLine ("no separator found");
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.4</remarks>

	[Problem ("This code is calling a string-based overload when a char-based overload could be used.")]
	[Solution ("Replace the string parameters with chararacter parameters.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class PreferCharOverloadRule : Rule, IMethodRule {

		static string GetString (Instruction parameter)
		{
			if ((parameter == null) || (parameter.OpCode.Code != Code.Ldstr))
				return String.Empty;
			return (parameter.Operand as string);
		}

		void Report (MethodDefinition method, Instruction ins, Confidence confidence, MemberReference call, string parameter)
		{
			string msg = String.Format (CultureInfo.InvariantCulture, "Prefer the use of: {0}('{1}'...);", 
				call.Name, parameter);
			Runner.Report (method, ins, Severity.Medium, confidence, msg);
		}

		void CheckIndexOf (MethodDefinition method, MethodReference call, Instruction ins)
		{
			// check that first parameter is a string of length equal to one
			string p1 = GetString (ins.TraceBack (method, -1));
			if (p1.Length != 1)
				return;

			IList<ParameterDefinition> pdc = call.Parameters;
			int last = pdc.Count;
			if (!pdc [last - 1].ParameterType.IsNamed ("System", "StringComparison")) {
				// confidence is normal because it's possible that the code expects a
				// culture sensitive comparison (but that will break in .NET 4).
				Report (method, ins, Confidence.Normal, call, p1);
				return;
			}

			// we try to find out what's the StringComparison
			Instruction sc = ins.TraceBack (method, -last);
			switch (sc.OpCode.Code) {
			case Code.Ldc_I4_4:
				// if it's StringComparison.Ordinal (4) then it's identical to what a Char would do
				Report (method, ins, Confidence.High, call, p1);
				break;
			case Code.Ldc_I4_5:
				// if it's StringComparison.OrdinalIgnoreCase (5) then it's identical as long as the Char is not case sensitive
				if (p1 == p1.ToLowerInvariant () && p1 == p1.ToUpperInvariant ()) {
					Report (method, ins, Confidence.High, call, p1);
				}
				break;
			}
			// otherwise the Char overload is not usable as a direct replacement
		}

		void CheckReplace (MethodDefinition method, Instruction ins)
		{
			string p1 = GetString (ins.TraceBack (method, -1));
			if (p1.Length != 1)
				return;

			string p2 = GetString (ins.TraceBack (method, -2));
			if (p2.Length != 1)
				return;

			string msg = String.Format (CultureInfo.InvariantCulture, 
				"Prefer the use of: Replace('{0}','{1}');", p1, p2);
			// confidence is higher since there's no StringComparison to consider
			Runner.Report (method, ins, Severity.Medium, Confidence.High, msg);
		}

		static bool CheckFirstParameterIsString (IMethodSignature method)
		{
			return (method.HasParameters && method.Parameters [0].ParameterType.IsNamed ("System", "String"));
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule apply only if the method has a body (e.g. p/invokes, icalls don't)
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// is there any Call or Callvirt instructions in the method ?
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				if ((ins.OpCode.Code != Code.Call) && (ins.OpCode.Code != Code.Callvirt))
					continue;

				MethodReference call = (ins.Operand as MethodReference);
				if (!call.DeclaringType.IsNamed ("System", "String"))
					continue;

				switch (call.Name) {
				case "IndexOf":
				case "LastIndexOf":
					// 3 overloads each - parameters are identical (between them)
					// and the String (or Char) is always the first one
					if (CheckFirstParameterIsString (call))
						CheckIndexOf (method, call, ins);
					break;
				case "Replace":
					// both parameters needs to be length == 1
					if (CheckFirstParameterIsString (call))
						CheckReplace (method, ins);
					break;
				}
				// String.Split could be a candidate but it is unlikely we
				// would be able to guess (often) the content of the arrays
			}
			return Runner.CurrentRuleResult;
		}
	}
}
