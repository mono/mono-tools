//
// Gendarme.Rules.Performance.AvoidMethodWithLargeMaximumStackSizeRule
//
// Authors:
//	Jb Evain <jbevain@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Performance {

	/// <summary>
	/// This rule fires if a method has a large maximum stack size (default is
	/// 100). Having a large maximum stack size makes it hard to generate code that 
	/// performs well and, likely, makes the code harder to understand.
	/// </summary>
	/// <remarks>This rule is available since Gendarme 2.6</remarks>

	[Problem ("The method has a large max stack, which is a sign of complex code.")]
	[Solution ("Refactor your code to reduce the size of the method.")]
	public class AvoidMethodWithLargeMaximumStackSizeRule : Rule, IMethodRule {

		private int max_stack_size = 100;

		public int MaximumStackSize {
			get { return max_stack_size; }
			set { max_stack_size = value; }
		}

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// ingore methods without body and generated code
			if (!method.HasBody || method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			int num = method.Body.MaxStackSize;
			if (num <= MaximumStackSize)
				return RuleResult.Success;

			string msg = String.Format (CultureInfo.InvariantCulture, 
				"Found {0} maximum stack size (maximum {1}).", num, MaximumStackSize);
			Runner.Report (method, Severity.High, Confidence.High, msg);
			return RuleResult.Failure;
		}
	}
}
