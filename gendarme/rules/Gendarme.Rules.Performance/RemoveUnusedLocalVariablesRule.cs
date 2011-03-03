//
// Gendarme.Rules.Performance.RemoveUnusedLocalVariablesRule
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
using System.Collections;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule looks for unused local variables inside methods. This can leads to larger 
	/// code (IL) size and longer JIT time, but note that some optimizing compilers
	/// can remove the locals so they won't be reported even if you can still see them in 
	/// the source code. This could also be a typo in the source were a value is assigned
	/// to the wrong variable.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// bool DualCheck ()
	/// {
	///	bool b1 = true;
	///	bool b2 = CheckDetails ();
	///	if (b2) {
	///		// typo: a find-replace changed b1 into b2
	///		b2 = CheckMoreDetails ();
	///	}
	///	return b2 &amp;&amp; b2;
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// bool DualCheck ()
	/// {
	///	bool b1 = true;
	///	bool b2 = CheckDetails ();
	///	if (b2) {
	///		b1 = CheckMoreDetails ();
	///	}
	///	return b1 &amp;&amp; b2;
	/// }
	/// </code>
	/// </example>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("This methods contains unused local variables.")]
	[Solution ("Remove unused variables to reduce the method's size.")]
	[FxCopCompatibility ("Microsoft.Performance", "CA1804:RemoveUnusedLocals")]
	public class RemoveUnusedLocalVariablesRule : Rule, IMethodRule {

		// it does not make sense to allocate less than 16 bytes, 16 * 8
		private const int DefaultLength = (16 << 3);

		private BitArray used;

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// this rule cannot execute if debugging information is not available
			Runner.AnalyzeModule += delegate (object o, RunnerEventArgs e) {
				Active = e.CurrentModule.HasSymbols;
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// rule does not apply to external methods (e.g. p/invokes)
			// and generated code (by compilers or tools)
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			MethodBody body = method.Body;
			if (!body.HasVariables)
				return RuleResult.Success;

			var variables = body.Variables;
			int count = variables.Count;

			if (used == null) {
				used = new BitArray (Math.Max (DefaultLength, count));
			} else if (count > used.Length) {
				used = new BitArray (count);
			}
			used.SetAll (false);

			foreach (Instruction ins in body.Instructions) {
				VariableDefinition vd = ins.GetVariable (method);
				if (vd != null)
					used [vd.Index] = true;
			}

			for (int i = 0; i < count; i++) {
				if (!used [i]) {
					// sometimes the compilers generates some locals without really
					// using them (e.g. assign only a constant). In this case we need
					// to determine if the variable is "genuine" or a compiler
					// (*) seen in a while (true) loop over a switch
					VariableDefinition variable = variables [i];
					if (variable.IsGeneratedName ())
						continue;

					string s = String.Format (CultureInfo.InvariantCulture, "Variable '{0}' of type '{1}'", 
						variable.Name, variable.VariableType.GetFullName ());
					Runner.Report (method, Severity.Low, Confidence.Normal, s);
				}
			}

			return Runner.CurrentRuleResult;
		}
	}
}
