//
// Gendarme.Rules.BadPractice.PreferTryParseRule
//
// Authors:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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

using Gendarme.Framework;
using Gendarme.Framework.Engines;
using Gendarme.Framework.Helpers;

using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Gendarme.Rules.BadPractice {

	/// <summary>
	/// This rule will warn you if a method use a <c>Parse</c> method when an
	/// alternative <c>TryParse</c> method is available. A <c>Parser</c> method,
	/// when using correctly, requires you to deal with multiple exceptions (a 
	/// complete list likely not easily available) or catching all exceptions (bad).
	/// Also the throwing/catching of exceptions can kill performance. 
	/// The <c>TryParse</c> method allow simpler code without the performance penality.
	/// </summary>
	/// <example>
	/// Bad example (no validation):
	/// <code>
	/// bool ParseLine (string line)
	/// {
	///	string values = line.Split (',');
	///	if (values.Length == 3) {
	///		id = Int32.Parse (values [0]);
	///		timestamp = DateTime.Parse (values [1]);
	///		msg = values [2];
	///		return true;
	///	} else {
	///		return false;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Bad example (validation):
	/// <code>
	/// bool ParseLine (string line)
	/// {
	///	string values = line.Split (',');
	///	if (values.Length == 3) {
	///		try {
	///			id = Int32.Parse (values [0]);
	///			timestamp = DateTime.Parse (values [1]);
	///			msg = values [2];
	///			return true;
	///		}
	///		catch {
	///			// catching all exception is bad
	///			return false;
	///		}
	///	} else {
	///		return false;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// bool ParseLine (string line)
	/// {
	///	string values = line.Split (',');
	///	if (values.Length == 3) {
	///		if (!Int32.TryParse (values [0], out id))
	///			return false;
	///		if (!DateTime.TryParse (values [1], out timestamp))
	///			return false;
	///		msg = values [2];
	///		return true;
	///	} else {
	///		return false;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.8</remarks>
	[Problem ("Using a Parse method force you to catch multiple exceptions.")]
	[Solution ("Use the existing TryParse alternative method.")]
	[EngineDependency (typeof (OpCodeEngine))]
	public class PreferTryParseRule : Rule, IMethodRule {

		// keep a cache of types with a <type> <type>::Parse(string s ...)
		// so we know if an alternative TryParse is available
		static Dictionary<TypeReference, bool> has_try_parse = new Dictionary<TypeReference, bool> ();

		static bool HasTryParseMethod (TypeDefinition type)
		{
			bool present = false;

			if (!has_try_parse.TryGetValue (type, out present)) {
				foreach (MethodReference method in type.Methods) {
					if (MethodSignatures.TryParse.Matches (method)) {
						has_try_parse.Add (type, true);
						return true;
					}
				}
			}
			return present;
		}

		static bool InsideTryBlock (MethodDefinition method, Instruction ins)
		{
			MethodBody body = method.Body;
			if (!body.HasExceptionHandlers)
				return false; // no handlers
			
			int offset = ins.Offset;
			foreach (ExceptionHandler eh in body.ExceptionHandlers) {
				// is the call between a "try/catch" or "try/finally"
				if ((offset >= eh.TryStart.Offset) && (offset < eh.TryEnd.Offset))
					return true;
			}
			return false;
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// avoid processing methods that do not call any methods
			if (!OpCodeBitmask.Calls.Intersect (OpCodeEngine.GetBitmask (method)))
				return RuleResult.DoesNotApply;

			foreach (Instruction ins in method.Body.Instructions) {
				Code code = ins.OpCode.Code;
				if ((code != Code.Call) && (code != Code.Callvirt))
					continue;

				MethodReference mr = (ins.Operand as MethodReference);
				if (!MethodSignatures.Parse.Matches (mr))
					continue;

				TypeDefinition declaringType = mr.DeclaringType.Resolve();
				if (declaringType != null && !HasTryParseMethod(declaringType))
					continue;
				
				// if inside a try (catch/finally) block then...
				bool inside_try_block = InsideTryBlock (method, ins);
				// we lower severity (i.e. other cases are more urgent to fix)
				Severity severity = inside_try_block ? Severity.Medium : Severity.High;
				// but since we're do not check all exceptions (and they could differ 
				// between Parse implementations) we also reduce our confidence level
				Confidence confidence = inside_try_block ? Confidence.Normal : Confidence.High;
				Runner.Report (method, ins, severity, confidence);
			}

			return Runner.CurrentRuleResult;
		}
	}
}

