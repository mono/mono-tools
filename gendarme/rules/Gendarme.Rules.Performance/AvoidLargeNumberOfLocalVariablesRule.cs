//
// Gendarme.Rules.Performance.AvoidLargeNumberOfLocalVariablesRule
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
using System.ComponentModel;
using System.Globalization;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule warns when the number of local variables exceed a maximum value (default is
	/// 64). Having a large amount of local variables makes it hard to generate code that 
	/// performs well and, likely, makes the code harder to understand.
	/// </summary>
	/// <remarks>This rule is available since Gendarme 2.0</remarks>

	[Problem ("The number of local variables is so large that the JIT will be unable to properly allocate registers.")]
	[Solution ("Refactor your code to reduce the number of variables or split the method into several methods.")]
	[FxCopCompatibility ("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
	public class AvoidLargeNumberOfLocalVariablesRule : Rule, IMethodRule {

		private const int DefaultMaxVariables = 64;
		private int max_variables = DefaultMaxVariables;

		/// <summary>The maximum number of local variables which methods may have without a defect being reported.</summary>
		/// <remarks>Defaults to 64.</remarks>
		[DefaultValue (DefaultMaxVariables)]
		[Description ("The maximum number of local variables which methods may have without a defect being reported.")]
		public int MaximumVariables {
			get { return max_variables; }
			set { max_variables = value; }
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// ingore methods without body and generated code
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// special case for System.Windows.Forms since it's designer does not
			// mark this code as generated :-(
			if (method.Name == "InitializeComponent") {
				if (method.DeclaringType.Inherits ("System.Windows.Forms", "Form"))
					return RuleResult.DoesNotApply;
			}

			int num = method.Body.Variables.Count;
			if (num <= MaximumVariables)
				return RuleResult.Success;

			string msg = String.Format (CultureInfo.InvariantCulture, 
				"Found {0} local variables (maximum {1}).", num, MaximumVariables);
			Runner.Report (method, Severity.High, Confidence.High, msg);
			return RuleResult.Failure;
		}
	}
}
