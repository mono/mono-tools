//
// Gendarme.Rules.Correctness.UseValueInPropertySetterRule
//
// Authors:
//	Lukasz Knop <lukasz.knop@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Lukasz Knop
// Copyright (C) 2007-2008 Novell, Inc (http://www.novell.com)
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
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	/// <summary>
	/// This rule ensures all setter properties uses the value argument passed to the property.
	/// </summary>
	/// <example>
	/// Bad example:
	/// <code>
	/// public bool Active {
	///	get {
	///		return active;
	///	}
	///	// this can take a long time to figure out if the default value for active
	///	// is false (since most people will use the property to set it to true)
	///	set {
	///		active = true;
	///	}
	/// }
	/// </code>
	/// </example>
	/// <example>
	/// Good example:
	/// <code>
	/// public bool Active {
	///	get {
	///		return active;
	///	}
	///	set {
	///		active = value;
	///	}
	/// }
	/// </code>
	/// </example>

	[Problem ("This property setter doesn't use 'value'.")]
	[Solution ("The setter should use 'value' or, if unneeded, you should consider removing the setter to reduce possible confusion.")]
	public class UseValueInPropertySetterRule : Rule, IMethodRule {

		public override void Initialize (IRunner runner)
		{
			base.Initialize (runner);

			// avoid checking all methods unless the type has some properties
			Runner.AnalyzeType += delegate (object o, RunnerEventArgs e) {
				Active = e.CurrentType.HasProperties;
			};
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			//Skip the test, instead of flooding messages
			//in stubs or empty setters.
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// rule applies to setters methods
			if (!method.IsSetter)
				return RuleResult.DoesNotApply;

			// rule applies
			bool flow = false;
			bool empty = true;
			foreach (Instruction instruction in method.Body.Instructions) {

				ParameterDefinition pd = instruction.GetParameter (method);
				if (pd != null) {
					empty = false;
					if (pd.Index == 0) // value
						return RuleResult.Success;
					continue;
				}

				switch (instruction.OpCode.Code) {
				// check if the IL simply throws an exception
				case Code.Throw:
					if (!flow)
						return RuleResult.Success;
					empty = false;
					break;
				case Code.Nop:
					break;
				case Code.Ret:
					flow = true;
					break;
				default:
					empty = false;
					// lots of thing can occurs before the throw
					// e.g. loading the string (ldstr)
					//	or calling a method to translate this string
					FlowControl fc = instruction.OpCode.FlowControl;
					flow |= ((fc != FlowControl.Next) && (fc != FlowControl.Call));
					// but as long as the flow continue uninterruped to the throw
					// we consider this a simple throw
					break;
				}
			}

			if (empty)
				return RuleResult.Success;

			Runner.Report (method, Severity.High, Confidence.Total);
			return RuleResult.Failure;
		}
	}
}
